@echo off
setlocal enabledelayedexpansion
cd /d "%~dp0"

set "MSBUILD="

rem --- VS Build Tools 2022 ---
if exist "%ProgramFiles(x86)%\Microsoft Visual Studio\2022\BuildTools\MSBuild\Current\Bin\MSBuild.exe" (
    set "MSBUILD=%ProgramFiles(x86)%\Microsoft Visual Studio\2022\BuildTools\MSBuild\Current\Bin\MSBuild.exe"
)

rem --- VS Community 2022 ---
if not defined MSBUILD if exist "%ProgramFiles%\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe" (
    set "MSBUILD=%ProgramFiles%\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe"
)

rem --- VS Professional 2022 ---
if not defined MSBUILD if exist "%ProgramFiles%\Microsoft Visual Studio\2022\Professional\MSBuild\Current\Bin\MSBuild.exe" (
    set "MSBUILD=%ProgramFiles%\Microsoft Visual Studio\2022\Professional\MSBuild\Current\Bin\MSBuild.exe"
)

rem --- VS Enterprise 2022 ---
if not defined MSBUILD if exist "%ProgramFiles%\Microsoft Visual Studio\2022\Enterprise\MSBuild\Current\Bin\MSBuild.exe" (
    set "MSBUILD=%ProgramFiles%\Microsoft Visual Studio\2022\Enterprise\MSBuild\Current\Bin\MSBuild.exe"
)

rem --- VS 2019 ---
if not defined MSBUILD if exist "%ProgramFiles(x86)%\Microsoft Visual Studio\2019\BuildTools\MSBuild\Current\Bin\MSBuild.exe" (
    set "MSBUILD=%ProgramFiles(x86)%\Microsoft Visual Studio\2019\BuildTools\MSBuild\Current\Bin\MSBuild.exe"
)
if not defined MSBUILD if exist "%ProgramFiles(x86)%\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe" (
    set "MSBUILD=%ProgramFiles(x86)%\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe"
)

if not defined MSBUILD (
    echo [ERROR] MSBuild not found.
    echo.
    echo Please install Visual Studio 2022 Build Tools first:
    echo   Download: https://aka.ms/vs/17/release/vs_BuildTools.exe
    echo   During install, check the ".NET desktop build tools" workload.
    echo.
    echo Or via Chocolatey:
    echo   choco install visualstudio2022buildtools -y --params "--add Microsoft.VisualStudio.Workload.ManagedDesktopBuildTools --includeRecommended"
    echo.
    pause
    exit /b 1
)

echo Using: !MSBUILD!
echo.
echo Restoring NuGet packages and building Release...
echo.
"!MSBUILD!" "%~dp0WinSockPacketEditor.sln" /p:Configuration=Release /restore /m /v:minimal

if errorlevel 1 (
    echo.
    echo [ERROR] Build failed.
    pause
    exit /b 1
)

echo.
echo [OK] Build succeeded.
echo Output: %~dp0WinsockPacketEditor\bin\Release\
pause
