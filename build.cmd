@echo off
set "projectName=UKMDUnlocker"
set "version=0.3.1"
set "buildDir=.\out"
set "target=whyis2plus2-%projectName%-%version%.zip"

dotnet build
if not exist "%buildDir%\" (
    mkdir %buildDir%
)

copy /y ".\bin\Debug\netstandard2.1\%projectName%.dll" "%buildDir%"
copy /y ".\bin\Debug\netstandard2.1\%projectName%.pdb" "%buildDir%"
copy /y ".\README.md" "%buildDir%"
copy /y ".\LICENSE" "%buildDir%"
copy /y ".\icon.png" "%buildDir%"
copy /y ".\manifest.json" "%buildDir%"
7z a -y -tzip -r %target% %CD%\%buildDir%\* | findstr /r /v ".*ing ^$"

exit /b 0
