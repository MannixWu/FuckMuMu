@echo off
setlocal
set CSC=%windir%\Microsoft.NET\Framework64\v4.0.30319\csc.exe
set FWDIR=%windir%\Microsoft.NET\Framework64\v4.0.30319
if not exist "%CSC%" (
  echo Cannot find C# compiler: %CSC%
  exit /b 1
)
if not exist FuckMuMu\bin mkdir FuckMuMu\bin
"%CSC%" /target:exe /out:FuckMuMu\bin\FuckMuMu.exe -win32icon:icon.ico -r:"%FWDIR%\System.Windows.Forms.dll" -r:"%FWDIR%\System.Drawing.dll" FuckMuMu\Program.cs
if errorlevel 1 exit /b 1
echo Build complete.
