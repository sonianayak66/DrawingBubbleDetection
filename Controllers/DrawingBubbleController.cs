using Dapper;
using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.Spreadsheet;
using ImageMagick;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MPCRS.Models;
using MPCRS.Services;
using MPCRS.Utilities;
using MPCRS.ViewModels;
using Newtonsoft.Json;
using OpenXmlPowerTools;
using System.Data;
using System.Security.Claims;
using static MPCRS.Utilities.Constants;

namespace MPCRS.Controllers
{
    [Authorize]
    public class DrawingBubbleController : Controller
    {
        private readonly DESI_STFE_PRODContext _dbContext;
        private readonly IConfiguration _configuration;
        private readonly MPDapperContext mPDapperContext;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IWebHostEnvironment _environment;
        private readonly BubbleDetectionOptions _bubbleOptions;

        public DrawingBubbleController(
            DESI_STFE_PRODContext dbContext,
            IConfiguration configuration,
            MPDapperContext mPDapperContext,
            IHttpClientFactory httpClientFactory,
            IWebHostEnvironment environment,
            IOptions<BubbleDetectionOptions> bubbleOptions)
        {
            _dbContext = dbContext;
            _configuration = configuration;
            this.mPDapperContext = mPDapperContext;
            _httpClientFactory = httpClientFactory;
            _environment = environment;
            _bubbleOptions = bubbleOptions.Value;
        }

        // Resolves a method id to its config, falling back to the default method.
        private (string methodId, BubbleDetectionMethodOptions opts) ResolveMethod(string requested)
        {
            if (_bubbleOptions?.Methods == null || _bubbleOptions.Methods.Count == 0)
                return (null, null);

            string id = !string.IsNullOrWhiteSpace(requested) && _bubbleOptions.Methods.ContainsKey(requested)
                ? requested
                : _bubbleOptions.DefaultMethod;

            if (string.IsNullOrWhiteSpace(id) || !_bubbleOptions.Methods.TryGetValue(id, out var opts))
                return (null, null);

            return (id, opts);
        }

        private int GetCurrentUserDbKey()
        {
            var sid = User.FindFirst(ClaimTypes.Sid)?.Value;
            return int.TryParse(sid, out int key) ? key : 0;
        }

        private static readonly HashSet<string> AllowedDrawingExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".png", ".jpg", ".jpeg", ".bmp", ".pdf", ".tif", ".tiff"
        };

        private static string SafeFileComponent(string raw, string fallback = "drawing")
        {
            if (string.IsNullOrWhiteSpace(raw)) return fallback;
            var invalid = Path.GetInvalidFileNameChars().ToHashSet();
            var clean = new string(raw.Trim()
                .Select(c => invalid.Contains(c) || c == '/' || c == '\\' ? '-' : c)
                .Where(c => !char.IsControl(c))
                .ToArray());
            clean = clean.Replace("..", "-").Trim('.', ' ', '-');
            return string.IsNullOrWhiteSpace(clean) ? fallback : clean;
        }

        // ── Python Service Health Check (diagnostic) ─────────────

        [HttpGet]
        [ClaimRequirement(UserPermissions.DrawingBubble_Read)]
        public async Task<IActionResult> PythonStatus(string method = null)
        {
            var methodIds = method != null
                ? new[] { method }
                : _bubbleOptions.Methods.Keys.ToArray();

            var results = new List<object>();
            foreach (var id in methodIds)
            {
                var (ok, message) = await PythonProcessManager.CheckHealthAsync(id);
                results.Add(new
                {
                    methodId = id,
                    displayName = _bubbleOptions.Methods.TryGetValue(id, out var m) ? m.DisplayName : id,
                    success = ok,
                    processRunning = PythonProcessManager.IsRunning(id),
                    message,
                    pythonOutput = PythonProcessManager.GetRecentOutput(id),
                });
            }
            return Json(new { success = true, methods = results });
        }

        // ── Upload Page ──────────────────────────────────────────

        [ClaimRequirement(UserPermissions.DrawingBubble_Read)]
        public IActionResult Index(string method = null)
        {
            var vm = new DrawingBubbleIndexVM();

            using (var connection = mPDapperContext.CreateConnection())
            {
                vm.Parts = connection.Query<PartDropdownItem>(
                    "SELECT Engine_Part_Dbkey, Draw_part_no, Description, Revision FROM Engine_Parts_Master WHERE ISNULL(is_active,1) = 1 ORDER BY Draw_part_no"
                ).ToList();
            }

            vm.DetectionMethods = _bubbleOptions.Methods
                .Select(kv => new DetectionMethodItem { Id = kv.Key, DisplayName = kv.Value.DisplayName })
                .ToList();

            // Allow navbar entries to deep-link straight to a specific
            // method by passing ?method=<id>. Falls back to the global
            // DefaultMethod from appsettings when not supplied or when
            // the supplied id isn't configured. This is what lets the
            // "Bubble Annotation" navbar link land here with the
            // auto-annotate option pre-selected in the dropdown.
            vm.DefaultMethod =
                !string.IsNullOrWhiteSpace(method) && _bubbleOptions.Methods.ContainsKey(method)
                    ? method
                    : _bubbleOptions.DefaultMethod;

            ViewBag.RevisionList = Masters.RevisionList();
            // Use the explicit "Index" view name so the BubbleAnnotation
            // action below (which delegates here) picks up Index.cshtml
            // instead of looking for BubbleAnnotation.cshtml.
            return View("Index", vm);
        }

        // Convenience landing for the "Bubble Annotation" navbar entry —
        // lands on the same Index page with auto-annotate pre-selected,
        // and also flips the page title so it's clear which mode the
        // user is in. The view itself is shared with detect.
        [ClaimRequirement(UserPermissions.DrawingBubble_Read)]
        public IActionResult BubbleAnnotation()
        {
            ViewData["Title"] = "Bubble Annotation";
            return Index(method: "auto-annotate");
        }

        // AJAX — get revision when part is selected
        [HttpGet]
        public IActionResult GetPartRevision(int enginePartDbkey)
        {
            using (var connection = mPDapperContext.CreateConnection())
            {
                var part = connection.QueryFirstOrDefault<PartDropdownItem>(
                    "SELECT Draw_part_no, Revision, Description FROM Engine_Parts_Master WHERE Engine_Part_Dbkey = @key",
                    new { key = enginePartDbkey }
                );
                if (part == null) return Json(new { success = false });
                return Json(new { success = true, revision = part.Revision, description = part.Description });
            }
        }

        // ── Process (called on form submit) ──────────────────────

        [HttpPost]
        [ClaimRequirement(UserPermissions.DrawingBubble_Process)]
        public async Task<IActionResult> Process(int enginePartDbkey, string drawPartNo, string revision, string method, string requestId, IFormFile drawingFile)
        {
            if (drawingFile == null || drawingFile.Length == 0)
                return Json(new { success = false, message = "Please upload a drawing file." });

            if (string.IsNullOrWhiteSpace(drawPartNo) || string.IsNullOrWhiteSpace(revision))
                return Json(new { success = false, message = "Part number and revision are required." });

            var (methodId, methodOpts) = ResolveMethod(method);
            if (methodOpts == null)
                return Json(new { success = false, message = "No detection method configured. Check BubbleDetection settings." });

            // Sanitise the client-supplied request id so it's safe to embed
            // in URLs, log lines, and filenames. The browser generates one
            // before submit (so the live log panel can subscribe before
            // detection starts); fall back to a fresh GUID if missing.
            string baseRequestId = SanitiseRequestId(requestId)
                                   ?? Guid.NewGuid().ToString("N").Substring(0, 8);

            try
            {
                // ── 1. Save uploaded file to wwwroot ─────────────
                string uploadFolder = Path.Combine(_environment.WebRootPath, "Uploads", "DrawingBubbles", enginePartDbkey.ToString());
                Directory.CreateDirectory(uploadFolder);

                string originalExt = Path.GetExtension(drawingFile.FileName).ToLowerInvariant();
                if (!AllowedDrawingExtensions.Contains(originalExt))
                    return Json(new { success = false, message = "Unsupported drawing file type. Accepted: PDF, PNG, JPG, TIFF, BMP." });

                string safePartNo = SafeFileComponent(drawPartNo, "part");
                string safeRevision = SafeFileComponent(revision, "rev");
                string safeFileName = $"{safePartNo}_{safeRevision}_{DateTime.Now:yyyyMMddHHmmss}{originalExt}";
                string originalFilePath = Path.Combine(uploadFolder, safeFileName);

                using (var stream = new FileStream(originalFilePath, FileMode.Create))
                    await drawingFile.CopyToAsync(stream);

                // ── 2. Convert file to image(s) ───────────────────
                // Handles PDF (multi-page), TIFF (multi-page), and direct images
                var imagePaths = ConvertToImages(originalFilePath, uploadFolder, originalExt);

                if (!imagePaths.Any())
                    return Json(new { success = false, message = "Could not convert file to image. Check the file format." });

                // ── 3. Send each page to Python API ───────────────
                var client = _httpClientFactory.CreateClient();
                client.Timeout = TimeSpan.FromMinutes(5);

                var pages = new List<DrawingBubblePageResult>();

                // Per-page log directory — one JSON file per Python request_id.
                string logsFolder = Path.Combine(uploadFolder, "logs");
                Directory.CreateDirectory(logsFolder);
                var pageLogIds = new List<string>();

                bool isAutoAnnotate = string.Equals(methodOpts.Mode, "auto-annotate", StringComparison.OrdinalIgnoreCase);

                foreach (var (imagePath, pageNumber) in imagePaths.Select((p, i) => (p, i + 1)))
                {
                    // The two endpoints return different shapes; we
                    // collect into a unified (items, annotatedB64) pair.
                    string annotatedB64 = null;
                    List<DrawingBubbleInspectionItem> items;

                    // Each page gets its own request_id so the UI's progress
                    // bar and log panel can update per-image. Multi-page PDFs
                    // therefore produce multiple log files.
                    string pageRequestId = imagePaths.Count > 1
                        ? $"{baseRequestId}-p{pageNumber}"
                        : baseRequestId;
                    pageLogIds.Add(pageRequestId);

                    // Capture the buffer's max seq right before this page goes
                    // to Python. The snapshot step uses this as a fallback
                    // when filtering by request_id returns nothing — happens
                    // when the Python service is an older build that ignores
                    // the request_id query parameter.
                    long preDetectHeadSeq = await GetHeadSeqAsync(methodOpts);

                    using (var content = new MultipartFormDataContent())
                    using (var fileStream = new FileStream(imagePath, FileMode.Open, FileAccess.Read))
                    {
                        var fileContent = new StreamContent(fileStream);
                        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/png");
                        content.Add(fileContent, "file", Path.GetFileName(imagePath));

                        // Build the URL — both endpoints accept the same
                        // include_annotated_image + request_id query params.
                        var detectUrl = $"{methodOpts.BaseUrl.TrimEnd('/')}{methodOpts.DetectPath}"
                                      + $"?include_annotated_image=true"
                                      + $"&request_id={Uri.EscapeDataString(pageRequestId)}";
                        var request = new HttpRequestMessage(HttpMethod.Post, detectUrl);
                        request.Headers.Add("X-API-Key", methodOpts.ApiKey);
                        request.Content = content;

                        var response = await client.SendAsync(request);

                        if (!response.IsSuccessStatusCode)
                        {
                            var error = await response.Content.ReadAsStringAsync();
                            await TrySnapshotLogsAsync(methodOpts, pageRequestId, preDetectHeadSeq, logsFolder, drawPartNo, revision, pageNumber);
                            return Json(new { success = false, message = $"Python API error on page {pageNumber}: {error}", requestId = baseRequestId });
                        }

                        var json = await response.Content.ReadAsStringAsync();

                        // Branch on Mode. Both endpoints carry an
                        // annotated_image_base64; only the items list
                        // shape differs. We flatten to a common model
                        // so Results / Save / History don't care which
                        // mode produced the run.
                        if (isAutoAnnotate)
                        {
                            var auto = JsonConvert.DeserializeObject<AutoAnnotateApiResponse>(json);
                            annotatedB64 = auto?.annotated_image_base64;
                            items = (auto?.balloons ?? new List<AutoAnnotateBalloon>())
                                .Select(b => new DrawingBubbleInspectionItem
                                {
                                    Bubble_Number = b.number.ToString(),
                                    Dimension     = b.dimension_text ?? "",
                                    // Auto-annotate generates ground truth, so
                                    // confidence is 1 and nothing needs review.
                                    Confidence    = 1.0m,
                                    Needs_Review  = false,
                                    X_Coordinate  = b.balloon?.cx ?? 0,
                                    Y_Coordinate  = b.balloon?.cy ?? 0,
                                    Radius        = b.balloon?.radius ?? 0,
                                    Is_Manually_Added = false,
                                    Is_Corrected      = false,
                                }).ToList();
                        }
                        else
                        {
                            var apiResult = JsonConvert.DeserializeObject<BubbleDetectionApiResponse>(json);
                            annotatedB64 = apiResult?.annotated_image_base64;
                            items = (apiResult?.bubbles ?? new List<BubbleDetectionResult>())
                                .Select(b => new DrawingBubbleInspectionItem
                                {
                                    Bubble_Number = b.bubble_number,
                                    Dimension     = b.dimension,
                                    Confidence    = b.confidence,
                                    Needs_Review  = b.needs_review,
                                    X_Coordinate  = b.x,
                                    Y_Coordinate  = b.y,
                                    Radius        = b.radius,
                                    Is_Manually_Added = false,
                                    Is_Corrected      = false,
                                }).ToList();
                        }
                    }

                    // Persist the captured logs for this page so they can be
                    // retrieved from the History view later.
                    await TrySnapshotLogsAsync(methodOpts, pageRequestId, preDetectHeadSeq, logsFolder, drawPartNo, revision, pageNumber);

                    // ── 4. Save annotated image returned from Python
                    string annotatedFileName = $"annotated_p{pageNumber}_{safeFileName.Replace(originalExt, ".png")}";
                    string annotatedFilePath = Path.Combine(uploadFolder, annotatedFileName);

                    if (!string.IsNullOrEmpty(annotatedB64))
                    {
                        var imageBytes = Convert.FromBase64String(annotatedB64);
                        await System.IO.File.WriteAllBytesAsync(annotatedFilePath, imageBytes);
                    }

                    // Build relative URLs for view
                    string relativeAnnotated = $"/Uploads/DrawingBubbles/{enginePartDbkey}/{annotatedFileName}";
                    string relativeOriginal = $"/Uploads/DrawingBubbles/{enginePartDbkey}/{Path.GetFileName(imagePath)}";

                    pages.Add(new DrawingBubblePageResult
                    {
                        PageNumber = pageNumber,
                        AnnotatedImageUrl = relativeAnnotated,
                        OriginalImageUrl = relativeOriginal,
                        Bubbles = items,
                    });
                }

                // ── 6. Build ViewModel and store in TempData ──────
                var vm = new DrawingBubbleResultViewModel
                {
                    Engine_Part_Dbkey = enginePartDbkey,
                    Draw_part_no = drawPartNo,
                    Revision = revision,
                    Original_File_Name = drawingFile.FileName,
                    Original_File_Path = $"/Uploads/DrawingBubbles/{enginePartDbkey}/{safeFileName}",
                    Pages = pages,
                    Detection_Method = methodId,
                    Detection_Method_DisplayName = methodOpts.DisplayName,
                };

                TempData["BubbleResult"] = JsonConvert.SerializeObject(vm);

                return Json(new
                {
                    success = true,
                    redirect = Url.Action("Results", "DrawingBubble"),
                    requestId = baseRequestId,
                    pageRequestIds = pageLogIds,
                    enginePartDbkey,
                });
            }
            catch (HttpRequestException)
            {
                return Json(new { success = false, message = "Cannot connect to the bubble detection service. Please ensure the Python service is running.", requestId = baseRequestId });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Processing error: {ex.Message}", requestId = baseRequestId });
            }
        }

        // Trim and restrict the client-supplied request id to characters
        // that are safe in URLs and filenames. Returns null if the input
        // sanitises to nothing.
        private static string SanitiseRequestId(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return null;
            var clean = new string(raw.Trim()
                .Where(c => char.IsLetterOrDigit(c) || c == '-' || c == '_')
                .ToArray());
            if (clean.Length > 64) clean = clean.Substring(0, 64);
            return clean.Length == 0 ? null : clean;
        }

        // Read the current max seq in the Python service's ring buffer.
        // Used to bracket each detection run so the snapshot step can fall
        // back to "all records since this point" if request_id filtering
        // misses (older Python builds ignored the request_id query param).
        private async Task<long> GetHeadSeqAsync(BubbleDetectionMethodOptions opts)
        {
            try
            {
                var url = $"{opts.BaseUrl.TrimEnd('/')}{opts.LogsPath}?limit=1";
                var client = _httpClientFactory.CreateClient();
                client.Timeout = TimeSpan.FromSeconds(8);
                var resp = await client.GetAsync(url);
                if (!resp.IsSuccessStatusCode) return 0;
                var json = await resp.Content.ReadAsStringAsync();
                var parsed = JsonConvert.DeserializeObject<dynamic>(json);
                if (parsed?.head_seq != null) return (long)parsed.head_seq;
                // Old Python build without head_seq — pull more entries
                // and pick the max manually.
                url = $"{opts.BaseUrl.TrimEnd('/')}{opts.LogsPath}?limit=2000";
                resp = await client.GetAsync(url);
                if (!resp.IsSuccessStatusCode) return 0;
                json = await resp.Content.ReadAsStringAsync();
                parsed = JsonConvert.DeserializeObject<dynamic>(json);
                long max = 0;
                if (parsed?.entries != null)
                {
                    foreach (var e in parsed.entries)
                    {
                        long s = (long)(e.seq ?? 0);
                        if (s > max) max = s;
                    }
                }
                return max;
            }
            catch
            {
                return 0;
            }
        }

        // Pull the captured log records for `requestId` from the Python
        // service's in-memory ring buffer and write them to disk so they
        // can be replayed from the History view later. Best-effort —
        // failures are swallowed because logging shouldn't block the
        // detection workflow. If filtering by request_id returns nothing
        // (older Python builds), fall back to "everything since the
        // baselineSeq we captured before sending the page".
        private async Task TrySnapshotLogsAsync(
            BubbleDetectionMethodOptions opts,
            string requestId,
            long baselineSeq,
            string logsFolder,
            string drawPartNo,
            string revision,
            int pageNumber)
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                client.Timeout = TimeSpan.FromSeconds(15);

                // Try the strict filter first (works on the new Python build).
                var byIdUrl = $"{opts.BaseUrl.TrimEnd('/')}{opts.LogsPath}"
                            + $"?request_id={Uri.EscapeDataString(requestId)}&limit=2000";
                var resp = await client.GetAsync(byIdUrl);
                if (!resp.IsSuccessStatusCode) return;
                var json = await resp.Content.ReadAsStringAsync();
                var parsed = JsonConvert.DeserializeObject<dynamic>(json);
                int count = (int?)parsed?.count ?? 0;

                // Fallback: if the strict filter found nothing, pull every
                // record produced after baselineSeq. This is wider but
                // gives the user something useful to look at later.
                if (count == 0 && baselineSeq > 0)
                {
                    var sinceUrl = $"{opts.BaseUrl.TrimEnd('/')}{opts.LogsPath}"
                                 + $"?since_seq={baselineSeq}&limit=2000";
                    resp = await client.GetAsync(sinceUrl);
                    if (resp.IsSuccessStatusCode)
                    {
                        json = await resp.Content.ReadAsStringAsync();
                        parsed = JsonConvert.DeserializeObject<dynamic>(json);
                        count = (int?)parsed?.count ?? 0;
                    }
                }

                var snapshot = new
                {
                    request_id = requestId,
                    captured_at = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss"),
                    draw_part_no = drawPartNo,
                    revision,
                    page_number = pageNumber,
                    count,
                    entries = parsed?.entries,
                };

                var outPath = Path.Combine(logsFolder, $"{requestId}.json");
                await System.IO.File.WriteAllTextAsync(
                    outPath,
                    JsonConvert.SerializeObject(snapshot, Formatting.Indented));
            }
            catch
            {
                // Logging snapshot is best-effort — never let it break the run.
            }
        }

        // ── Results Page ─────────────────────────────────────────

        [ClaimRequirement(UserPermissions.DrawingBubble_Read)]
        public IActionResult Results()
        {
            if (TempData["BubbleResult"] == null)
                return RedirectToAction("Index");

            var vm = JsonConvert.DeserializeObject<DrawingBubbleResultViewModel>(
                TempData["BubbleResult"].ToString()
            );

            return View(vm);
        }

        // ── Save confirmed bubble data ────────────────────────────

        [HttpPost]
        [ClaimRequirement(UserPermissions.DrawingBubble_Process)]
        public IActionResult Save([FromBody] DrawingBubbleInspectionSaveRequest request)
        {
            if (request == null || !request.Items.Any())
                return Json(new { success = false, message = "No bubble data to save." });

            try
            {
                request.Processed_By = GetCurrentUserDbKey();

                // Serialize items to JSON for SP
                var itemsJson = JsonConvert.SerializeObject(request.Items.Select(i => new
                {
                    bubble_number = i.Bubble_Number,
                    dimension = i.Dimension ?? "",
                    tolerance = i.Tolerance,
                    confidence = i.Confidence,
                    needs_review = i.Needs_Review,
                    is_manually_added = i.Is_Manually_Added,
                    is_corrected = i.Is_Corrected,
                    x = i.X_Coordinate,
                    y = i.Y_Coordinate,
                    radius = i.Radius,
                }));

                using (var connection = mPDapperContext.CreateConnection())
                {
                    var result = connection.QueryFirstOrDefault<dynamic>(
                        "usp_DrawingBubbleInspection_Save",
                        new
                        {
                            request.Engine_Part_Dbkey,
                            request.Draw_part_no,
                            request.Revision,
                            request.Original_File_Name,
                            request.Original_File_Path,
                            request.Annotated_File_Name,
                            request.Annotated_File_Path,
                            request.Total_Bubbles,
                            request.Needs_Review_Count,
                            request.Processed_By,
                            ItemsJson = itemsJson,
                            request.Detection_Method,
                        },
                        commandType: CommandType.StoredProcedure
                    );

                    int newKey = result?.Inspection_Dbkey ?? 0;
                    return Json(new { success = true, inspectionDbkey = newKey, message = "Saved successfully." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Save error: {ex.Message}" });
            }
        }

        // ── History Page ─────────────────────────────────────────

        [ClaimRequirement(UserPermissions.DrawingBubble_Read)]
        public IActionResult History(int? enginePartDbkey)
        {
            var vm = new DrawingBubbleHistoryVM();

            using (var connection = mPDapperContext.CreateConnection())
            {
                vm.Parts = connection.Query<PartDropdownItem>(
                    "SELECT Engine_Part_Dbkey, Draw_part_no, Description, Revision FROM Engine_Parts_Master WHERE ISNULL(is_active,1) = 1 ORDER BY Draw_part_no"
                ).ToList();

                vm.SelectedEnginePartDbkey = enginePartDbkey ?? 0;

                // Always load all records — DataTable handles filtering
                using var multi = connection.QueryMultiple(
                    "usp_DrawingBubbleInspection_GetByPart",
                    new { Engine_Part_Dbkey = 0, Inspection_Dbkey = 0 },
                    commandType: CommandType.StoredProcedure
                );

                vm.Records = multi.Read<DrawingBubbleInspectionVM>().ToList();
                multi.Read<DrawingBubbleInspectionItem>(); // consume RS2
            }

            return View(vm);
        }

        // ── View specific past result ─────────────────────────────

        [ClaimRequirement(UserPermissions.DrawingBubble_Read)]
        public IActionResult ViewResult(int inspectionDbkey)
        {
            using (var connection = mPDapperContext.CreateConnection())
            {
                using var multi = connection.QueryMultiple(
                    "usp_DrawingBubbleInspection_GetByPart",
                    new { Engine_Part_Dbkey = 0, Inspection_Dbkey = inspectionDbkey },
                    commandType: CommandType.StoredProcedure
                );

                var header = multi.ReadFirstOrDefault<DrawingBubbleInspectionVM>();
                var items = multi.Read<DrawingBubbleInspectionItem>().ToList();

                if (header == null) return RedirectToAction("History");

                string displayName = !string.IsNullOrEmpty(header.Detection_Method)
                    && _bubbleOptions?.Methods != null
                    && _bubbleOptions.Methods.TryGetValue(header.Detection_Method, out var m)
                        ? m.DisplayName
                        : header.Detection_Method;

                var vm = new DrawingBubbleResultViewModel
                {
                    Engine_Part_Dbkey = header.Engine_Part_Dbkey,
                    Draw_part_no = header.Draw_part_no,
                    Revision = header.Revision,
                    Original_File_Name = header.Original_File_Name,
                    IsReadOnly = true,
                    Detection_Method = header.Detection_Method,
                    Detection_Method_DisplayName = displayName,
                    Pages = new List<DrawingBubblePageResult>
                    {
                        new DrawingBubblePageResult
                        {
                            PageNumber = 1,
                            AnnotatedImageUrl = header.Annotated_File_Path,
                            Bubbles = items,
                        }
                    }
                };

                return View("Results", vm);
            }
        }

        // ── Delete ────────────────────────────────────────────────

        [HttpPost]
        [ClaimRequirement(UserPermissions.DrawingBubble_Delete)]
        public IActionResult Delete(int inspectionDbkey)
        {
            try
            {
                using (var connection = mPDapperContext.CreateConnection())
                {
                    connection.Execute(
                        "usp_DrawingBubbleInspection_Delete",
                        new { Inspection_Dbkey = inspectionDbkey, Deleted_By = GetCurrentUserDbKey() },
                        commandType: CommandType.StoredProcedure
                    );
                }
                return Json(new { success = true, message = "Record deleted." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // ── Excel Export ──────────────────────────────────────────

        [ClaimRequirement(UserPermissions.DrawingBubble_Read)]
        public IActionResult ExportExcel(int inspectionDbkey)
        {
            using (var connection = mPDapperContext.CreateConnection())
            {
                using var multi = connection.QueryMultiple(
                    "usp_DrawingBubbleInspection_GetByPart",
                    new { Engine_Part_Dbkey = 0, Inspection_Dbkey = inspectionDbkey },
                    commandType: CommandType.StoredProcedure
                );

                var header = multi.ReadFirstOrDefault<DrawingBubbleInspectionVM>();
                var items = multi.Read<DrawingBubbleInspectionItem>().ToList();

                if (header == null) return NotFound();

                using var package = new OfficeOpenXml.ExcelPackage();
                var ws = package.Workbook.Worksheets.Add("Bubble Inspection");

                // Header info rows
                ws.Cells[1, 1].Value = "Part Number"; ws.Cells[1, 2].Value = header.Draw_part_no;
                ws.Cells[2, 1].Value = "Revision"; ws.Cells[2, 2].Value = header.Revision;
                ws.Cells[3, 1].Value = "Processed On"; ws.Cells[3, 2].Value = header.Processed_On.ToString("dd-MMM-yyyy HH:mm");
                ws.Cells[4, 1].Value = "Total Bubbles"; ws.Cells[4, 2].Value = header.Total_Bubbles;

                // Table headers
                int row = 6;
                ws.Cells[row, 1].Value = "Bubble No.";
                ws.Cells[row, 2].Value = "Dimension";
                ws.Cells[row, 3].Value = "Confidence %";
                ws.Cells[row, 4].Value = "Needs Review";
                ws.Cells[row, 5].Value = "Manually Added";
                ws.Cells[row, 6].Value = "Corrected";

                using (var range = ws.Cells[row, 1, row, 6])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(68, 114, 196));
                    range.Style.Font.Color.SetColor(System.Drawing.Color.White);
                }

                row++;
                foreach (var item in items)
                {
                    ws.Cells[row, 1].Value = item.Bubble_Number;
                    ws.Cells[row, 2].Value = item.Dimension;
                    ws.Cells[row, 3].Value = Math.Round((double)item.Confidence * 100, 1);
                    ws.Cells[row, 4].Value = item.Needs_Review ? "Yes" : "No";
                    ws.Cells[row, 5].Value = item.Is_Manually_Added ? "Yes" : "No";
                    ws.Cells[row, 6].Value = item.Is_Corrected ? "Yes" : "No";

                    if (item.Needs_Review)
                    {
                        ws.Cells[row, 1, row, 6].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        ws.Cells[row, 1, row, 6].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(255, 235, 156));
                    }
                    row++;
                }

                ws.Cells[ws.Dimension.Address].AutoFitColumns();

                var fileName = $"BubbleInspection_{header.Draw_part_no}_{header.Revision}.xlsx";
                return File(package.GetAsByteArray(),
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    fileName);
            }
        }

        // ── Private: Convert file to PNG images ───────────────────

        private List<string> ConvertToImages(string filePath, string outputFolder, string ext)
        {
            var imagePaths = new List<string>();
            string baseName = Path.GetFileNameWithoutExtension(filePath);

            // Direct images — return as-is
            if (ext is ".png" or ".jpg" or ".jpeg" or ".bmp")
            {
                imagePaths.Add(filePath);
                return imagePaths;
            }

            // PDF — use PdfiumViewer (no Ghostscript needed)
            if (ext == ".pdf")
            {
                using var document = PdfiumViewer.PdfDocument.Load(filePath);
                for (int i = 0; i < document.PageCount; i++)
                {
                    using var image = document.Render(i, 200, 200, true);
                    string outPath = Path.Combine(outputFolder, $"{baseName}_page{i + 1}.png");
                    image.Save(outPath, System.Drawing.Imaging.ImageFormat.Png);
                    imagePaths.Add(outPath);
                }
                return imagePaths;
            }

            // TIFF — use Magick.NET (works fine, no Ghostscript for TIFF)
            if (ext is ".tif" or ".tiff")
            {
                using var images = new MagickImageCollection();
                images.Read(filePath);

                int pageNum = 1;
                foreach (var image in images)
                {
                    image.Format = MagickFormat.Png;
                    image.Resize(new MagickGeometry(2400, 0));
                    string outPath = Path.Combine(outputFolder, $"{baseName}_page{pageNum}.png");
                    image.Write(outPath);
                    imagePaths.Add(outPath);
                    pageNum++;
                }
                return imagePaths;
            }

            return imagePaths; // unsupported format — empty list
        }

        // ── Logs Viewer (live tail of the Python detection service) ──────

        [HttpGet]
        [ClaimRequirement(UserPermissions.DrawingBubble_Read)]
        public IActionResult Logs(string method = null)
        {
            var (methodId, opts) = ResolveMethod(method);
            ViewBag.MethodId = methodId;
            ViewBag.MethodName = opts?.DisplayName ?? methodId;
            // Use a Dictionary<string,string> instead of anonymous types
            // — Razor views can't access anonymous-type properties via
            // `dynamic` because they're internal to the controller's
            // assembly, throwing RuntimeBinderException on m.Id.
            ViewBag.AllMethods = _bubbleOptions.Methods != null
                ? _bubbleOptions.Methods.ToDictionary(
                    kv => kv.Key,
                    kv => kv.Value.DisplayName ?? kv.Key)
                : new Dictionary<string, string>();
            return View();
        }

        // List the saved log snapshots for a part (newest first).
        // Used by the History page's per-row "Logs" modal so the user
        // can pick any past detection run and replay its log entries.
        [HttpGet]
        [ClaimRequirement(UserPermissions.DrawingBubble_Read)]
        public IActionResult LogsList(int enginePartDbkey)
        {
            string logsFolder = Path.Combine(
                _environment.WebRootPath, "Uploads", "DrawingBubbles",
                enginePartDbkey.ToString(), "logs");

            if (!Directory.Exists(logsFolder))
                return Json(new { count = 0, logs = Array.Empty<LogSnapshotSummary>() });

            var items = new List<LogSnapshotSummary>();
            foreach (var path in Directory.EnumerateFiles(logsFolder, "*.json"))
            {
                try
                {
                    var json = System.IO.File.ReadAllText(path);
                    var parsed = JsonConvert.DeserializeObject<LogSnapshotSummary>(json);
                    if (parsed == null) continue;
                    if (string.IsNullOrEmpty(parsed.request_id))
                        parsed.request_id = Path.GetFileNameWithoutExtension(path);
                    if (string.IsNullOrEmpty(parsed.captured_at))
                        parsed.captured_at = System.IO.File.GetLastWriteTime(path).ToString("yyyy-MM-ddTHH:mm:ss");
                    items.Add(parsed);
                }
                catch
                {
                    // Skip corrupt files — don't fail the whole listing.
                }
            }

            items = items.OrderByDescending(o => o.captured_at).ToList();

            Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
            return Json(new { count = items.Count, logs = items });
        }

        // Lightweight DTO for the LogsList endpoint (and what's serialised
        // into each .json snapshot). Field names match the on-disk shape.
        private class LogSnapshotSummary
        {
            public string request_id { get; set; }
            public string captured_at { get; set; }
            public string draw_part_no { get; set; }
            public string revision { get; set; }
            public int page_number { get; set; }
            public int count { get; set; }
        }

        // Render a single persisted log snapshot as a viewer page.
        [HttpGet]
        [ClaimRequirement(UserPermissions.DrawingBubble_Read)]
        public IActionResult ViewLogs(int enginePartDbkey, string requestId)
        {
            ViewBag.EnginePartDbkey = enginePartDbkey;
            ViewBag.RequestId = SanitiseRequestId(requestId) ?? "";
            return View();
        }

        // Stream the saved snapshot as a self-contained HTML report —
        // mirrors what the in-app log viewer shows (Pipeline Steps card
        // with per-step progress bars + dark log pane). The file embeds
        // its own CSS so it renders identically when opened standalone,
        // even offline.
        [HttpGet]
        [ClaimRequirement(UserPermissions.DrawingBubble_Read)]
        public IActionResult DownloadLogs(int enginePartDbkey, string requestId)
        {
            var safeId = SanitiseRequestId(requestId);
            if (safeId == null)
                return NotFound("Missing or invalid requestId.");

            string logsFolder = Path.Combine(
                _environment.WebRootPath, "Uploads", "DrawingBubbles",
                enginePartDbkey.ToString(), "logs");
            string filePath = Path.Combine(logsFolder, $"{safeId}.json");

            if (!System.IO.File.Exists(filePath))
                return NotFound("Log snapshot not found.");

            var json = System.IO.File.ReadAllText(filePath);
            var parsed = JsonConvert.DeserializeObject<dynamic>(json);

            string html = BuildLogReportHtml(safeId, parsed);
            var bytes = System.Text.Encoding.UTF8.GetBytes(html);
            return File(bytes, "text/html; charset=utf-8", $"detection-{safeId}.html");
        }

        // Build the standalone HTML report. Renders a Pipeline Steps
        // section with per-step progress bars + timing, followed by the
        // full raw log pane. All styling is inlined so the file works
        // when opened from disk with no network access.
        private static string BuildLogReportHtml(string requestId, dynamic snapshot)
        {
            string capturedAt = (string)(snapshot?.captured_at ?? "");
            string partNo     = (string)(snapshot?.draw_part_no ?? "");
            string revision   = (string)(snapshot?.revision ?? "");
            int    pageNumber = (int?)(snapshot?.page_number) ?? 0;

            // Parse [STEP] lines into ordered, named steps with status + ms.
            var stepOrder = new List<string>();
            var stepState = new Dictionary<string, (string status, double? ms, string reason)>();
            var stepRegex = new System.Text.RegularExpressions.Regex(
                @"^\[STEP\]\s+(start|done|skip|fail):\s+(.+?)(?:\s+\|\s+(.*))?$"
            );

            int totalEntries = 0;
            if (snapshot?.entries != null)
            {
                foreach (var e in snapshot.entries)
                {
                    totalEntries++;
                    string msg = (string)(e.message ?? "");
                    var m = stepRegex.Match(msg);
                    if (!m.Success) continue;

                    string kind = m.Groups[1].Value;
                    string name = m.Groups[2].Value.Trim();
                    string tail = m.Groups[3].Success ? m.Groups[3].Value : "";

                    if (!stepState.ContainsKey(name))
                    {
                        stepOrder.Add(name);
                        stepState[name] = ("pending", null, "");
                    }
                    var (status, ms, reason) = stepState[name];

                    if (kind == "start")
                    {
                        if (status == "pending") status = "running";
                    }
                    else if (kind == "done")
                    {
                        status = "done";
                        var msMatch = System.Text.RegularExpressions.Regex.Match(tail, @"ms=([\d.]+)");
                        if (msMatch.Success) ms = double.Parse(msMatch.Groups[1].Value, System.Globalization.CultureInfo.InvariantCulture);
                    }
                    else if (kind == "fail")
                    {
                        status = "failed";
                        var msMatch = System.Text.RegularExpressions.Regex.Match(tail, @"ms=([\d.]+)");
                        if (msMatch.Success) ms = double.Parse(msMatch.Groups[1].Value, System.Globalization.CultureInfo.InvariantCulture);
                    }
                    else if (kind == "skip")
                    {
                        status = "skipped";
                        var rMatch = System.Text.RegularExpressions.Regex.Match(tail, @"reason=(.+)$");
                        if (rMatch.Success) reason = rMatch.Groups[1].Value.Trim();
                    }
                    stepState[name] = (status, ms, reason);
                }
            }

            double maxMs = 0;
            int doneCount = 0, skipCount = 0, failCount = 0, runCount = 0;
            double totalMs = 0;
            foreach (var name in stepOrder)
            {
                var (status, ms, _) = stepState[name];
                if (ms.HasValue && ms.Value > maxMs) maxMs = ms.Value;
                if (status == "done")    { doneCount++; totalMs += ms ?? 0; }
                if (status == "skipped") skipCount++;
                if (status == "failed")  failCount++;
                if (status == "running") runCount++;
            }

            string FormatMs(double? ms) =>
                !ms.HasValue ? "" :
                ms.Value < 1000
                    ? $"{ms.Value:F0} ms"
                    : $"{ms.Value / 1000.0:F2} s";

            string Esc(string s) => System.Net.WebUtility.HtmlEncode(s ?? "");

            string IconFor(string status) => status switch
            {
                "done"    => "<span class='ic ic-done'>✓</span>",
                "running" => "<span class='ic ic-run'>◐</span>",
                "skipped" => "<span class='ic ic-skip'>○</span>",
                "failed"  => "<span class='ic ic-fail'>✗</span>",
                _         => "<span class='ic'>·</span>",
            };

            var sb = new System.Text.StringBuilder();
            sb.Append(@"<!doctype html>
<html lang='en'>
<head>
<meta charset='utf-8'>
<title>Detection Logs — ").Append(Esc(requestId)).Append(@"</title>
<style>
  :root { color-scheme: light; }
  body { margin: 0; font: 14px/1.45 -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif;
         background: #f5f7fa; color: #212529; }
  .wrap { max-width: 1100px; margin: 24px auto; padding: 0 16px; }
  h1 { font-size: 20px; margin: 0 0 4px 0; }
  .rid { color: #6c757d; font: 13px/1 ui-monospace, Menlo, Consolas, monospace; }
  .meta { color: #6c757d; font-size: 13px; margin: 4px 0 18px; }
  .card { background: #fff; border: 1px solid #e3e6ea; border-radius: 6px;
          margin-bottom: 18px; overflow: hidden; }
  .card-header { padding: 10px 14px; border-bottom: 1px solid #e3e6ea;
                 font-weight: 600; background: #f8f9fa; display: flex;
                 align-items: center; gap: 12px; }
  .summary { color: #6c757d; font-weight: 400; font-size: 13px; }
  .step { padding: 8px 14px; border-bottom: 1px solid #f0f1f3; font-size: 13px; }
  .step:last-child { border-bottom: 0; }
  .step .row1 { display: flex; align-items: center; gap: 8px; }
  .step .name { flex: 1; }
  .step .right { font-size: 12px; }
  .step .right.ok { color: #198754; font-weight: 600; }
  .step .right.muted { color: #6c757d; }
  .step .right.fail { color: #dc3545; }
  .bar { height: 4px; background: #e9ecef; border-radius: 2px;
         margin-top: 6px; overflow: hidden; }
  .bar > span { display: block; height: 100%; }
  .bar .b-done { background: #198754; }
  .bar .b-run  { background: #ffc107; }
  .bar .b-skip { background: #adb5bd; }
  .bar .b-fail { background: #dc3545; }
  .ic { display: inline-block; width: 18px; text-align: center; font-weight: 700; }
  .ic-done { color: #198754; }
  .ic-run  { color: #0d6efd; }
  .ic-skip { color: #6c757d; }
  .ic-fail { color: #dc3545; }
  pre.log { background: #0d1117; color: #c9d1d9; margin: 0;
            padding: 12px 16px; max-height: none; overflow: visible;
            font: 12px/1.5 ui-monospace, Menlo, Consolas, monospace;
            white-space: pre-wrap; word-break: break-word; }
  .lvl { font-weight: 700; }
  .lvl-INFO    { color: #58a6ff; }
  .lvl-WARNING { color: #d29922; }
  .lvl-ERROR   { color: #f85149; }
  .lvl-DEBUG   { color: #8b949e; }
  .ts { color: #6e7681; }
  .footer { color: #6c757d; font-size: 12px; text-align: center; margin: 24px 0 8px; }
</style>
</head>
<body>
<div class='wrap'>
  <h1>Detection Logs <span class='rid'>").Append(Esc(requestId)).Append(@"</span></h1>
  <div class='meta'>");

            var bits = new List<string>();
            if (!string.IsNullOrEmpty(partNo))     bits.Add($"Part: {Esc(partNo)}");
            if (!string.IsNullOrEmpty(revision))   bits.Add($"Rev: {Esc(revision)}");
            if (pageNumber > 0)                    bits.Add($"Page {pageNumber}");
            if (!string.IsNullOrEmpty(capturedAt)) bits.Add($"Captured: {Esc(capturedAt)}");
            sb.Append(string.Join("  •  ", bits));
            sb.Append(@"</div>

  <div class='card'>
    <div class='card-header'>
      <span>Pipeline Steps</span>
      <span class='summary'>");
            var summaryBits = new List<string> { $"{doneCount} done" };
            if (runCount  > 0) summaryBits.Add($"{runCount} interrupted");
            if (skipCount > 0) summaryBits.Add($"{skipCount} skipped");
            if (failCount > 0) summaryBits.Add($"{failCount} failed");
            if (totalMs   > 0) summaryBits.Add($"total {FormatMs(totalMs)}");
            sb.Append(Esc(string.Join(" · ", summaryBits)));
            sb.Append(@"</span>
    </div>
");

            if (stepOrder.Count == 0)
            {
                sb.Append("    <div class='step muted'>No step records in this snapshot.</div>\n");
            }
            else
            {
                foreach (var name in stepOrder)
                {
                    var (status, ms, reason) = stepState[name];
                    string barClass = status switch
                    {
                        "done"    => "b-done",
                        "running" => "b-run",
                        "skipped" => "b-skip",
                        "failed"  => "b-fail",
                        _         => "b-skip",
                    };
                    double barPct = (ms.HasValue && maxMs > 0)
                        ? Math.Max(2, Math.Min(100, ms.Value / maxMs * 100))
                        : (status == "running" ? 100 : 0);

                    string rightHtml;
                    if (status == "running")
                        rightHtml = "<span class='right muted'>interrupted</span>";
                    else if (status == "skipped")
                        rightHtml = "<span class='right muted'>skipped" +
                                    (string.IsNullOrEmpty(reason) ? "" : $" ({Esc(reason)})") + "</span>";
                    else if (status == "failed")
                        rightHtml = $"<span class='right fail'>failed at {Esc(FormatMs(ms))}</span>";
                    else if (ms.HasValue)
                        rightHtml = $"<span class='right ok'>{Esc(FormatMs(ms))}</span>";
                    else
                        rightHtml = "<span class='right muted'>—</span>";

                    sb.Append("    <div class='step'>")
                      .Append("<div class='row1'>")
                      .Append(IconFor(status))
                      .Append("<span class='name'>").Append(Esc(name)).Append("</span>")
                      .Append(rightHtml)
                      .Append("</div>")
                      .Append("<div class='bar'><span class='").Append(barClass)
                      .Append("' style='width:").Append(barPct.ToString("F1", System.Globalization.CultureInfo.InvariantCulture)).Append("%'></span></div>")
                      .Append("</div>\n");
                }
            }

            sb.Append(@"  </div>

  <div class='card'>
    <div class='card-header' style='background:#212529; color:#f8f9fa; border-bottom:0;'>
      <span>Live Logs</span>
      <span class='summary' style='color:#adb5bd;'>")
              .Append(totalEntries).Append(" entr").Append(totalEntries == 1 ? "y" : "ies")
              .Append(" · ").Append(stepOrder.Count).Append(@" steps</span>
    </div>
<pre class='log'>");

            if (snapshot?.entries != null)
            {
                foreach (var e in snapshot.entries)
                {
                    string ts  = (string)(e.ts_human ?? "");
                    string lvl = (string)(e.level ?? "INFO");
                    string msg = (string)(e.message ?? "");
                    sb.Append("<span class='ts'>").Append(Esc(ts)).Append("</span> ")
                      .Append("<span class='lvl lvl-").Append(Esc(lvl)).Append("'>")
                      .Append(Esc(lvl)).Append("</span> ")
                      .Append(Esc(msg)).Append('\n');
                }
            }

            sb.Append(@"</pre>
  </div>

  <div class='footer'>Generated by MPCRS Detection Service · request id ")
              .Append(Esc(requestId)).Append(@"</div>
</div>
</body>
</html>");

            return sb.ToString();
        }

        // Read a persisted log snapshot from disk (written at the end of
        // a detection run by Process). Used by the live page after the
        // run completes and by the History view to replay past runs.
        [HttpGet]
        [ClaimRequirement(UserPermissions.DrawingBubble_Read)]
        public IActionResult SavedLogs(int enginePartDbkey, string requestId)
        {
            var safeId = SanitiseRequestId(requestId);
            if (safeId == null)
                return Json(new { count = 0, entries = Array.Empty<object>(), error = "Missing requestId." });

            string logsFolder = Path.Combine(
                _environment.WebRootPath, "Uploads", "DrawingBubbles",
                enginePartDbkey.ToString(), "logs");
            string filePath = Path.Combine(logsFolder, $"{safeId}.json");

            if (!System.IO.File.Exists(filePath))
                return Json(new { count = 0, entries = Array.Empty<object>(), error = "Log file not found." });

            var json = System.IO.File.ReadAllText(filePath);
            Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
            return Content(json, "application/json");
        }

        // JSON proxy to the FastAPI service's /api/logs endpoint.
        // Browsers can't safely send the X-API-Key from JavaScript
        // (and the Python service is on a different port), so the
        // .NET app fetches the logs and forwards them to the page.
        [HttpGet]
        [ClaimRequirement(UserPermissions.DrawingBubble_Read)]
        public async Task<IActionResult> LogsData(
            string method = null,
            string requestId = null,
            long? sinceSeq = null,
            string level = null,
            int limit = 500)
        {
            var (_, opts) = ResolveMethod(method);
            if (opts == null)
            {
                return Json(new { count = 0, entries = Array.Empty<object>(),
                                  error = "No detection method configured." });
            }

            var query = new List<string>();
            if (!string.IsNullOrWhiteSpace(requestId)) query.Add($"request_id={Uri.EscapeDataString(requestId)}");
            if (sinceSeq.HasValue) query.Add($"since_seq={sinceSeq.Value}");
            if (!string.IsNullOrWhiteSpace(level)) query.Add($"level={Uri.EscapeDataString(level)}");
            query.Add($"limit={Math.Clamp(limit, 1, 2000)}");
            var url = $"{opts.BaseUrl.TrimEnd('/')}{opts.LogsPath}?{string.Join("&", query)}";

            // Generous timeout — when Python is mid-detection it's
            // CPU-saturated and the FastAPI event loop can lag 10–15 s
            // before /api/logs is serviced. A short timeout shows up as
            // a noisy "request canceled" error in the live panel even
            // though everything is fine; the next poll succeeds.
            var client = _httpClientFactory.CreateClient();
            client.Timeout = TimeSpan.FromSeconds(30);
            try
            {
                var resp = await client.GetAsync(url);
                if (!resp.IsSuccessStatusCode)
                {
                    return Json(new { count = 0, entries = Array.Empty<object>(),
                                      error = $"Service responded {(int)resp.StatusCode}" });
                }
                var json = await resp.Content.ReadAsStringAsync();
                Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
                return Content(json, "application/json");
            }
            catch (TaskCanceledException)
            {
                // Don't show this in the UI as an error — it just means
                // the Python service was momentarily busy. Return empty
                // and let the next poll catch up.
                return Json(new { count = 0, entries = Array.Empty<object>() });
            }
            catch (Exception ex)
            {
                return Json(new { count = 0, entries = Array.Empty<object>(),
                                  error = $"Cannot reach log service: {ex.Message}" });
            }
        }
    }
}
