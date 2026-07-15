@echo off
REM ============================================================
REM  Bubble Detection Method 1 - Offline install
REM  Rebuilds .venv from the bundled wheels folder. Run once
REM  after copying this folder to a new machine. No internet
REM  required.
REM
REM  Python path is read from ..\appsettings.json (PythonPath:Path).
REM  Override by passing the Python install folder as arg 1:
REM      install.bat "D:\Python312"
REM ============================================================
setlocal

set "SCRIPT_DIR=%~dp0"
if "%SCRIPT_DIR:~-1%"=="\" set "SCRIPT_DIR=%SCRIPT_DIR:~0,-1%"

set "PYTHON_FOLDER=%~1"
if not "%PYTHON_FOLDER%"=="" goto have_python

set "APPSETTINGS=%SCRIPT_DIR%\..\appsettings.json"
if not exist "%APPSETTINGS%" goto missing_appsettings

for /f "usebackq delims=" %%V in (`powershell -NoProfile -ExecutionPolicy Bypass -File "%SCRIPT_DIR%\read_python_path.ps1" -AppSettingsPath "%APPSETTINGS%"`) do set "PYTHON_FOLDER=%%V"

if "%PYTHON_FOLDER%"=="" goto missing_python_config

:have_python
set "PYTHON_EXE=%PYTHON_FOLDER%\python.exe"
if not exist "%PYTHON_EXE%" goto missing_python_exe

echo Using Python: %PYTHON_EXE%
echo.
echo [1/3] Creating virtualenv at "%SCRIPT_DIR%\.venv" ...
if exist "%SCRIPT_DIR%\.venv" rmdir /S /Q "%SCRIPT_DIR%\.venv"
"%PYTHON_EXE%" -m venv "%SCRIPT_DIR%\.venv"
if errorlevel 1 goto venv_failed

echo [2/3] Installing packages from "%SCRIPT_DIR%\wheels" ...
"%SCRIPT_DIR%\.venv\Scripts\python.exe" -m pip install --no-index --find-links="%SCRIPT_DIR%\wheels" -r "%SCRIPT_DIR%\requirements.txt"
if errorlevel 1 goto pip_failed

echo [3/3] Done.
if not exist "%SCRIPT_DIR%\.env" (
    echo.
    echo WARNING: .env file missing. Create one with a BUBBLE_API_KEY
    echo matching appsettings.json BubbleDetection.Methods.method1.ApiKey
)
echo.
echo Method 1 venv ready. The ASP.NET app will launch this service on startup.
endlocal
exit /b 0

:missing_appsettings
echo ERROR: Cannot find "%SCRIPT_DIR%\..\appsettings.json"
echo Pass the Python folder explicitly:  install.bat "C:\path\to\Python312"
endlocal
exit /b 1

:missing_python_config
echo ERROR: PythonPath:Path not set in appsettings.json and no arg given.
endlocal
exit /b 1

:missing_python_exe
echo ERROR: python.exe not found at "%PYTHON_EXE%"
echo Pass the correct Python 3.12 folder:  install.bat "C:\path\to\Python312"
endlocal
exit /b 1

:venv_failed
echo ERROR: venv creation failed.
endlocal
exit /b 1

:pip_failed
echo ERROR: offline install failed.
endlocal
exit /b 1
