@echo off & setlocal EnableDelayedExpansion

echo Making tree-sitter-lua on WINDOWS

cd /d "%~dp0"

set "OUTDIR=..\..\out"

:: ---- Detect Visual Studio via vswhere ----
set "VSWHERE=%ProgramFiles(x86)%\Microsoft Visual Studio\Installer\vswhere.exe"

if not exist "%VSWHERE%" (
    echo Error: vswhere.exe not found. Is Visual Studio installed?
    exit /b 1
)

for /f "usebackq tokens=*" %%i in (`"%VSWHERE%" -latest -property installationPath`) do (
    set "VS_INSTALL_DIR=%%i"
)

if not defined VS_INSTALL_DIR (
    echo Error: No Visual Studio installation found.
    exit /b 1
)

echo Using Visual Studio: !VS_INSTALL_DIR!

call "!VS_INSTALL_DIR!\Common7\Tools\VsDevCmd.bat" -arch=amd64
if errorlevel 1 (
    echo Error: Failed to initialize VS Developer environment.
    exit /b 1
)

:: ---- Build ----
if "%1"=="clean" goto :clean

if not exist "%OUTDIR%" mkdir "%OUTDIR%"

nmake /f Makefile clean >nul 2>&1
nmake /f Makefile
if errorlevel 1 (
    echo Error: Build failed.
    exit /b 1
)

echo.
echo Build succeeded: %OUTDIR%\tree-sitter-lua.dll
goto :eof

:clean
nmake /f Makefile clean >nul 2>&1
echo Clean done.
