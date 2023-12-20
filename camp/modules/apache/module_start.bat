@echo off

SET "MOD_DIR=E:\Apache24"
SET "HTTPD=%MOD_DIR%\bin\httpd.exe"

echo Starting Apache HTTP Server...

start %HTTPD%

timeout /t 3 /nobreak

for /f "delims=" %%a in ('module_pids.bat') do set "PID=%%a"

tasklist /FI "PID eq %PID%" 2>nul | find /i "%PID%" >nul

if errorlevel 1 (
    echo 0
) else (
    echo 1
)