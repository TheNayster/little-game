@echo off
powershell.exe -NoProfile -ExecutionPolicy Bypass -File "%~dp0Tools\Launch\Play-SharedGarden.ps1"
if errorlevel 1 pause
