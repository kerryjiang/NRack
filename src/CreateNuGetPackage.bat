set msbuild="%ProgramFiles(x86)%\MSBuild\14.0\Bin\msbuild.exe"


%msbuild% NRack.build /t:BuildAndPack

pause