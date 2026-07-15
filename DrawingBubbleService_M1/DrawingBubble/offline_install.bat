@echo off
:: ─────────────────────────────────────────────────────────────────────────────
:: DrawingBubbleService — Offline installation helper (Windows)
::
:: Step 1 (internet machine):
::   offline_install.bat download
::   → downloads all wheels into .\wheels\
::
:: Step 2 (offline machine, copy the whole folder):
::   offline_install.bat install
::   → installs from .\wheels\ with no internet
:: ─────────────────────────────────────────────────────────────────────────────
setlocal

set "SCRIPT_DIR=%~dp0"
if "%SCRIPT_DIR:~-1%"=="\" set "SCRIPT_DIR=%SCRIPT_DIR:~0,-1%"
set "WHEEL_DIR=%SCRIPT_DIR%\wheels"
set "REQ=%SCRIPT_DIR%\requirements.txt"
set ACTION=%1
if "%ACTION%"=="" set ACTION=install

if /I "%ACTION%"=="download" goto :download
if /I "%ACTION%"=="install" goto :install
echo Usage: offline_install.bat [download^|install]
exit /b 1

:download
echo Downloading wheels to %WHEEL_DIR% ...
if not exist "%WHEEL_DIR%" mkdir "%WHEEL_DIR%"
python -m pip download -r "%REQ%" --dest "%WHEEL_DIR%" --prefer-binary
if errorlevel 1 (
    echo ERROR: Download failed.
    pause & exit /b 1
)
echo.
echo Done. Copy the entire DrawingBubbleService\ folder to the offline machine
echo then run:  offline_install.bat install
goto :end

:install
if not exist "%WHEEL_DIR%" (
    echo ERROR: wheels\ directory not found.
    echo Run "offline_install.bat download" on an internet machine first.
    pause & exit /b 1
)
echo Installing from %WHEEL_DIR% (offline) ...
python -m pip install --no-index --find-links="%WHEEL_DIR%" -r "%REQ%"
if errorlevel 1 (
    echo ERROR: Installation failed.
    pause & exit /b 1
)
echo.
echo Installation complete.
goto :end

:end
endlocal
