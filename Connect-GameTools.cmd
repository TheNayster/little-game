@echo off
powershell.exe -NoProfile -ExecutionPolicy Bypass -File "%~dp0Connect-GameTools.ps1" %*
if errorlevel 1 (
  echo.
  echo Connection did not complete. The message above explains what is missing.
)
pause
