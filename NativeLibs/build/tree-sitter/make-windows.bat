@echo off & setlocal EnableDelayedExpansion

echo Making tree-sitter on WINDOWS

cd /d "%~dp0"

set "OUTDIR=..\..\out\%1"

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

call "!VS_INSTALL_DIR!\Common7\Tools\VsDevCmd.bat"
if errorlevel 1 (
    echo Error: Failed to initialize VS Developer environment.
    exit /b 1
)

:: ---- Build ----
if "%1"=="clean" goto :clean

if not exist "%OUTDIR%" mkdir "%OUTDIR%"

nmake /f Makefile OUTDIR="%OUTDIR%" clean >nul 2>&1
nmake /f Makefile OUTDIR="%OUTDIR%"
if errorlevel 1 (
    echo Error: Build failed.
    exit /b 1
)

echo.
echo Build succeeded: %OUTDIR%\tree-sitter.dll
goto :eof

:clean
nmake /f Makefile OUTDIR="%OUTDIR%" clean >nul 2>&1
echo Clean done.
