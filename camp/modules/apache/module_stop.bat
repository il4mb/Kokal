@echo off

for /f "delims=" %%a in ('module_pids.bat') do set "PID=%%a"

echo Stopping the task with PID %PID%...

taskkill /F /PID %PID%

if errorlevel 1 (
    echo 0
) else (
    echo 1
)