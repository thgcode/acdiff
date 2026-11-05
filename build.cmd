@echo off
setlocal
cd /d "%~dp0"
set "CSC=%WINDIR%\Microsoft.NET\Framework64\v4.0.30319\csc.exe"
if not exist "%CSC%" set "CSC=%WINDIR%\Microsoft.NET\Framework\v4.0.30319\csc.exe"
if not exist "%CSC%" (
  echo The .NET Framework 4 compiler was not found.
  exit /b 1
)
for %%I in ("%CSC%") do set "FRAMEWORK=%%~dpI"
"%CSC%" /nologo /target:winexe /platform:anycpu /optimize+ /out:Acdiff.exe /reference:System.Windows.Forms.dll /reference:System.Drawing.dll /reference:"%FRAMEWORK%WPF\UIAutomationProvider.dll" /reference:"%FRAMEWORK%WPF\UIAutomationTypes.dll" Acdiff.cs Diff.cs
exit /b %errorlevel%
