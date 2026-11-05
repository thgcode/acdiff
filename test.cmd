@echo off
setlocal
cd /d "%~dp0"
set "CSC=%WINDIR%\Microsoft.NET\Framework64\v4.0.30319\csc.exe"
if not exist "%CSC%" set "CSC=%WINDIR%\Microsoft.NET\Framework\v4.0.30319\csc.exe"
"%CSC%" /nologo /out:ParserTests.exe Diff.cs ParserTests.cs
if errorlevel 1 exit /b 1
ParserTests.exe
exit /b %errorlevel%
