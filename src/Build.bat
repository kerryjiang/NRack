@echo off

set fdir=%ProgramFiles(x86)%\MSBuild\14.0
set msbuild="%fdir%\bin\msbuild.exe"


FOR /F "tokens=*" %%G IN ('DIR /B /AD /S obj') DO RMDIR /S /Q "%%G"
%msbuild% NDock.sln /p:Configuration=Release /t:Clean;Rebuild /p:OutputPath=..\bin\Net40

reg query "HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\.NETFramework\v4.0.30319\SKUs\.NETFramework,Version=v4.5" 2>nul
if errorlevel 0 (
    FOR /F "tokens=*" %%G IN ('DIR /B /AD /S obj') DO RMDIR /S /Q "%%G"
	%msbuild% NDock-Net45.sln /p:Configuration=Release /t:Clean;Rebuild /p:OutputPath=..\bin\Net45
)

pause