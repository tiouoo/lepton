@echo off
setlocal
cd /d "%~dp0.." || exit /b 1
git submodule sync --recursive && git submodule update --init --recursive && git submodule update --remote --recursive --merge
exit /b %errorlevel%
