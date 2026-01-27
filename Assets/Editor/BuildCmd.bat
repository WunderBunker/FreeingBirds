@echo off

REM Dossier du script
set SCRIPT_DIR=%~dp0
for %%I in ("%SCRIPT_DIR%\..\..") do set "PROJECT_DIR=%%~fI"

REM Chemin Unity
set UNITY_EXE=C:\Program Files\Unity\Hub\Editor\6000.2.7f2\Editor\Unity.exe


echo SCRIPT_DIR=%SCRIPT_DIR%
echo PROJECT_DIR=%PROJECT_DIR%
echo UNITY_EXE=%UNITY_EXE%

echo === Lancement de Unity ===

REM Build Android
"%UNITY_EXE%" ^
 -quit ^
 -nographics ^
 -batchmode ^
 -projectPath "%PROJECT_DIR%" ^
 -buildTarget Android ^
 -executeMethod BuildScript.BuildAndroid ^
 -incrementVersion ^
 -logFile "%PROJECT_DIR%\build_android.log"

echo === Fin du script ===

pause