@echo off
setlocal
powershell.exe -NoProfile -ExecutionPolicy Bypass -File "%~dp0Setup-Database.ps1"
set "exitCode=%ERRORLEVEL%"
if not "%exitCode%"=="0" (
    echo.
    echo Database setup failed. See the error above.
    pause
)
exit /b %exitCode%
