@echo off
setlocal
python "%~dp0normalize-lf.py"
exit /b %errorlevel%
