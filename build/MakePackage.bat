@echo off

rem get xFusion.SCCMPlugin.FusionDirector.PluginUI.dll version
set dllpath=%WORKSPACE%
call set dllpath=%%dllpath:\=\\%%
echo dllpath is: %dllpath%
set dllfile=%dllpath%\\build\\Output\\xFusion.SCCMPlugin.FusionDirector.PluginUI.dll
echo dllfile is: %dllfile%

FOR /F "tokens=2 delims==" %%I IN (
  'wmic datafile where "name='%dllfile%'" get version /format:list'
) DO SET "CurrentVersion=%%I"

echo CurrentVersion is: %CurrentVersion%

rem get mainVersion
set CurrentVersionFor=%CurrentVersion%
set /a i=1
:loop
for /f "tokens=1* delims=." %%a in ("%CurrentVersionFor%") do (
	 set /a i=%i%+1
	 @echo i=%i%
	 
	 if %i% == 1 ( set mainVersion1=%%a)
	 if %i% == 2 ( set mainVersion2=%%a)
	 if %i% == 3 ( set mainVersion3=%%a)
	 
	 set CurrentVersionFor=%%b
)
if defined CurrentVersionFor goto :loop

set mainVersion=%mainVersion1%.%mainVersion2%.%mainVersion3%
echo mainVersion=%mainVersion%

rem do package
set SetupScriptPath=%WORKSPACE%\build\Setup.nsi
echo execute %SetupScriptPath%
%NSIS_PATH% /DVERSION=%mainVersion% "%SetupScriptPath%"

Pause