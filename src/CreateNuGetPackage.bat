set msbuild="%ProgramFiles(x86)%\MSBuild\14.0\Bin\msbuild.exe"


%msbuild% NDock.build /t:BuildAndPack

pause