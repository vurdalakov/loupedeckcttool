@echo off

set solutionDir=%1
set config=%2

set zip=%solutionDir%..\ext\7za.exe 
set bin=%solutionDir%..\bin\%config%

set zipfile=%bin%\LoupedeckCt.zip
if exist %zipfile% del %zipfile%

set source=.\..\..\bin\%config%
echo source=%bin%

set target=%temp%\LoupedeckCtTool
if exist %target% rd /s /q %target%
md %target%

copy %bin%\LoupedeckCtTool.exe %target%
copy %bin%\LoupedeckCtTool.exe.config %target%
copy %bin%\Microsoft.Management.Infrastructure.dll %target%
copy %bin%\System.Management.Automation.dll %target%

%zip% a -tzip %zipfile% %target%
