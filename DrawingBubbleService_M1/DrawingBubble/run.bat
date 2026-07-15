@echo off
:: ─────────────────────────────────────────────────────────────────────────────
:: DrawingBubbleService — Windows startup script
:: Usage:  run.bat [port]    (default port: 8000)
:: ─────────────────────────────────────────────────────────────────────────────
setlocal

set PORT=%1
if "%PORT%"=="" set PORT=8000

:: run.bat lives in the DrawingBubble\ subfolder but main.py /
:: requirements.txt / the live .env all live one level up. Resolve
:: SCRIPT_DIR to the parent so uvicorn can import "main" and so we
:: load the .env that main.py actually reads.
set "SCRIPT_DIR=%~dp0.."
:: Collapse "...\DrawingBubble\.." to its canonical absolute path
for %%I in ("%SCRIPT_DIR%") do set "SCRIPT_DIR=%%~fI"

echo.
echo ╔══════════════════════════════════════════════════════╗
echo ║       Drawing Bubble Detection Service v4.0          ║
echo ╚══════════════════════════════════════════════════════╝
echo.

:: ── Check Python ─────────────────────────────────────────────────────────────
where python >nul 2>&1
if errorlevel 1 (
    echo ERROR: Python not found. Install Python 3.12 and add it to PATH.
    pause
    exit /b 1
)

for /f "tokens=*" %%V in ('python -c "import sys; print(f'{sys.version_info.major}.{sys.version_info.minor}')"') do set PY_VER=%%V
echo Python: %PY_VER%

:: ── Check .env ────────────────────────────────────────────────────────────────
if not exist "%SCRIPT_DIR%\.env" (
    if exist "%SCRIPT_DIR%\.env.example" (
        echo.
        echo WARNING: .env not found. Creating from .env.example ...
        copy "%SCRIPT_DIR%\.env.example" "%SCRIPT_DIR%\.env" >nul
        echo ^>^>^> Edit %SCRIPT_DIR%\.env and set BUBBLE_API_KEY before production use!
        echo.
    ) else (
        echo ERROR: .env file not found.
        pause
        exit /b 1
    )
)

:: ── Check dependencies ────────────────────────────────────────────────────────
echo Checking dependencies ...
python -c "import cv2, numpy, rapidocr_onnxruntime, fastapi, uvicorn" >nul 2>&1
if errorlevel 1 (
    echo.
    echo Some packages missing. Installing from requirements.txt ...
    python -m pip install -r "%SCRIPT_DIR%\requirements.txt" --quiet
    if errorlevel 1 (
        echo ERROR: pip install failed.
        pause
        exit /b 1
    )
)
echo Dependencies OK

:: ── Start server ──────────────────────────────────────────────────────────────
echo.
echo Starting API server on http://0.0.0.0:%PORT%
echo Swagger UI:  http://localhost:%PORT%/docs
echo Health:      http://localhost:%PORT%/api/health
echo.
echo Press Ctrl+C to stop.
echo.

cd /d "%SCRIPT_DIR%"
python -m uvicorn main:app ^
    --host 0.0.0.0 ^
    --port %PORT% ^
    --workers 1 ^
    --log-level info

if errorlevel 1 (
    echo.
    echo Server stopped with an error.
    pause
)
endlocal
