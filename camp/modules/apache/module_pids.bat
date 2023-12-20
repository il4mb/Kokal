@echo off

SET "MOD_DIR=E:\Apache24"
SET "PIDS_FILE=%MOD_DIR%\logs\httpd.pid"

for /f "delims=" %%a in (%PIDS_FILE%) do (
	echo %%a
)