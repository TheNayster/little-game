@echo off
powershell.exe -NoProfile -ExecutionPolicy Bypass -File "%~dp0Tools\Launch\Play-CharacterWorkshop.ps1"
if errorlevel 1 pause
