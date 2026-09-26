@echo off
powershell.exe -NoProfile -ExecutionPolicy Bypass -File "%~dp0Tools\Play-CharacterWorkshop.ps1"
if errorlevel 1 pause
