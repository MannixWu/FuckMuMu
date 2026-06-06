@echo off
setlocal
set CSC=%windir%\Microsoft.NET\Framework64\v4.0.30319\csc.exe
if not exist "%CSC%" (
  echo Cannot find C# compiler: %CSC%
  exit /b 1
)
if not exist FuckMuMu\bin mkdir FuckMuMu\bin
"%CSC%" /target:exe /out:FuckMuMu\bin\FuckMuMu.exe FuckMuMu\Program.cs
if errorlevel 1 exit /b 1
echo Build complete.
