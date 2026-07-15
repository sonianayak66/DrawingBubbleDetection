using MPCRS.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace MPCRS.Services
{
    /// <summary>
    /// Launches and supervises one Python uvicorn process per configured
    /// bubble detection method (see appsettings.json → BubbleDetection.Methods).
    /// Each method runs in its own folder using its own .venv, on its own port.
    /// </summary>
    public static class PythonProcessManager
    {
        private sealed class ManagedProcess
        {
            public Process Process;
            public StringBuilder Output = new StringBuilder();
            public BubbleDetectionMethodOptions Options;
        }

        private static readonly Dictionary<string, ManagedProcess> _processes = new();
        private static readonly object _lock = new object();

        public static bool IsRunning(string methodId)
        {
            lock (_lock)
            {
                return _processes.TryGetValue(methodId, out var mp)
                    && mp.Process != null
                    && !mp.Process.HasExited;
            }
        }

        public static string GetRecentOutput(string methodId)
        {
            lock (_lock)
            {
                return _processes.TryGetValue(methodId, out var mp) ? mp.Output.ToString() : string.Empty;
            }
        }

        public static IReadOnlyCollection<string> ConfiguredMethods()
        {
            lock (_lock)
            {
                return new List<string>(_processes.Keys);
            }
        }

        public static BubbleDetectionOptions LoadOptions(IConfiguration configuration)
        {
            var opts = new BubbleDetectionOptions();
            configuration.GetSection("BubbleDetection").Bind(opts);
            return opts;
        }

        public static void Start(IConfiguration configuration, IWebHostEnvironment environment)
        {
            var opts = LoadOptions(configuration);
            if (opts.Methods == null || opts.Methods.Count == 0)
            {
                ErrorHandler.LogException(new Exception(
                    "PythonProcessManager: No BubbleDetection.Methods configured. Nothing to start."));
                return;
            }

            string globalPythonFolder = configuration["PythonPath:Path"];

            foreach (var kv in opts.Methods)
            {
                StartOne(kv.Key, kv.Value, environment, globalPythonFolder);
            }
        }

        private static void StartOne(
            string methodId,
            BubbleDetectionMethodOptions m,
            IWebHostEnvironment environment,
            string globalPythonFolder)
        {
            try
            {
                string appDirectory = Path.Combine(environment.ContentRootPath, m.Folder);
                if (!Directory.Exists(appDirectory))
                {
                    appDirectory = Path.Combine(environment.WebRootPath, m.Folder);
                }
                if (!Directory.Exists(appDirectory))
                {
                    ErrorHandler.LogException(new Exception(
                        $"PythonProcessManager[{methodId}]: folder '{m.Folder}' not found under ContentRoot or wwwroot. Skipping."));
                    return;
                }

                // Resolve python.exe — prefer the method's own .venv, fall back to the global Python path.
                string pythonExe = null;
                if (m.UseVenv)
                {
                    string venvExe = Path.Combine(appDirectory, ".venv", "Scripts", "python.exe");
                    if (File.Exists(venvExe)) pythonExe = venvExe;
                }
                if (pythonExe == null)
                {
                    if (string.IsNullOrWhiteSpace(globalPythonFolder))
                    {
                        ErrorHandler.LogException(new Exception(
                            $"PythonProcessManager[{methodId}]: no .venv in '{appDirectory}' and PythonPath:Path is not set."));
                        return;
                    }
                    pythonExe = Path.Combine(globalPythonFolder, "python.exe");
                    if (!File.Exists(pythonExe))
                    {
                        ErrorHandler.LogException(new Exception(
                            $"PythonProcessManager[{methodId}]: python.exe not found at '{pythonExe}'."));
                        return;
                    }
                }

                string envFile = Path.Combine(appDirectory, ".env");
                if (!File.Exists(envFile))
                {
                    ErrorHandler.LogException(new Exception(
                        $"PythonProcessManager[{methodId}]: .env file missing at '{envFile}'. BUBBLE_API_KEY must be set."));
                }

                var mp = new ManagedProcess { Options = m };

                lock (_lock)
                {
                    StopOneLocked(methodId);

                    var startInfo = new ProcessStartInfo
                    {
                        FileName = pythonExe,
                        Arguments = $"-m uvicorn {m.Module} --host 127.0.0.1 --port {m.Port} --workers 1",
                        WorkingDirectory = appDirectory,
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        StandardOutputEncoding = System.Text.Encoding.UTF8,
                        StandardErrorEncoding = System.Text.Encoding.UTF8,
                    };

                    // Force UTF-8 stdout/stderr inside Python so OCR text containing
                    // non-ASCII characters (e.g. Japanese/Chinese glyphs) doesn't
                    // crash the detection with a charmap UnicodeEncodeError.
                    startInfo.Environment["PYTHONIOENCODING"] = "utf-8";
                    startInfo.Environment["PYTHONUTF8"] = "1";

                    mp.Process = new Process { StartInfo = startInfo };
                    mp.Process.OutputDataReceived += (sender, e) => AppendOutput(mp, "OUT", e.Data);
                    mp.Process.ErrorDataReceived += (sender, e) => AppendOutput(mp, "ERR", e.Data);

                    mp.Process.Start();
                    mp.Process.BeginOutputReadLine();
                    mp.Process.BeginErrorReadLine();

                    _processes[methodId] = mp;
                }

                Thread.Sleep(2000);

                lock (_lock)
                {
                    if (mp.Process.HasExited)
                    {
                        ErrorHandler.LogException(new Exception(
                            $"PythonProcessManager[{methodId}]: process CRASHED on startup (exit {mp.Process.ExitCode}). " +
                            $"Python='{pythonExe}', WorkDir='{appDirectory}'. Output:\n{mp.Output}"));
                        _processes.Remove(methodId);
                    }
                    else
                    {
                        ErrorHandler.LogException(new Exception(
                            $"PythonProcessManager[{methodId}]: started OK. PID={mp.Process.Id}, Port={m.Port}, WorkDir='{appDirectory}'."));
                    }
                }
            }
            catch (Exception ex)
            {
                try
                {
                    ErrorHandler.LogException(new Exception(
                        $"PythonProcessManager[{methodId}]: failed to start. Error: {ex.Message}", ex));
                }
                catch { }
            }
        }

        private static void AppendOutput(ManagedProcess mp, string tag, string data)
        {
            if (data == null) return;
            lock (_lock)
            {
                mp.Output.AppendLine($"[{tag}] {data}");
                if (mp.Output.Length > 50000)
                    mp.Output.Remove(0, mp.Output.Length - 30000);
            }
        }

        public static async Task<(bool ok, string message)> CheckHealthAsync(string methodId)
        {
            BubbleDetectionMethodOptions m;
            lock (_lock)
            {
                if (!_processes.TryGetValue(methodId, out var mp))
                    return (false, $"Method '{methodId}' is not configured or not started.");
                m = mp.Options;
            }

            try
            {
                if (!IsRunning(methodId))
                    return (false, $"Python process for '{methodId}' is not running. Output: {GetRecentOutput(methodId)}");

                using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(5) };
                var url = $"{m.BaseUrl.TrimEnd('/')}{m.HealthPath}";
                var response = await client.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var body = await response.Content.ReadAsStringAsync();
                    return (true, $"Healthy. Response: {body}");
                }
                return (false, $"Health check returned HTTP {(int)response.StatusCode}.");
            }
            catch (Exception ex)
            {
                return (false,
                    $"Cannot reach '{methodId}' at {m.BaseUrl}{m.HealthPath}. Error: {ex.Message}. " +
                    $"Recent output: {GetRecentOutput(methodId)}");
            }
        }

        public static void Stop()
        {
            lock (_lock)
            {
                foreach (var key in new List<string>(_processes.Keys))
                {
                    StopOneLocked(key);
                }
                _processes.Clear();
            }
        }

        private static void StopOneLocked(string methodId)
        {
            try
            {
                if (_processes.TryGetValue(methodId, out var mp) && mp.Process != null && !mp.Process.HasExited)
                {
                    mp.Process.Kill(entireProcessTree: true);
                    mp.Process.WaitForExit(3000);
                }
            }
            catch (Exception ex)
            {
                try { ErrorHandler.LogException(ex); } catch { }
            }
        }
    }
}
