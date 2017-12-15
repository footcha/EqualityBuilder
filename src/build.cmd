@echo off

echo Creating Nuget package EqualityBuilder...

set ROOT_DIR=%~dp0
set Configuration=Release
set PROJ_FILE=%ROOT_DIR%build.proj
set /p PackageVersion="Insert version of a packake to be created (i.e. 1.5.4): "

"d:\Program Files (x86)\Microsoft Visual Studio\2017\Enterprise\MSBuild\15.0\Bin\MSBuild.exe" "%PROJ_FILE%" /t:CreatePackage /p:PackageVersion=%PackageVersion% /p:Configuration=%Configuration%
