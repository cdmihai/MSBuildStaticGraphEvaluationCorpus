$msbuildBinaries="E:\projects\msbuild\artifacts\Release\bootstrap\net472\MSBuild\Current\Bin"
$msbuildApp="E:\projects\MSBuildTestProjects\src\msb\bin\Debug\net472\msb.exe"

$invokingScriptDirectory=[System.IO.Path]::GetDirectoryName($MyInvocation.ScriptName)

function BuildWithCacheRoundtrip(){
    BuildWithCacheRoundtrip "false" "$invokingScriptDirectory\caches" $invokingScriptDirectory "csproj"
}

function BuildWithCacheRoundtrip($useConsoleLogger, $cacheRoot, $projectRoot, $projectFileExtension){
    # echo "$msbuildapp $msbuildBinaries -buildWithCacheRoundtrip $invokingScriptDirectory\caches $invokingScriptDirectory"

    & $msbuildapp $msbuildBinaries $useConsolelogger "-buildWithCacheRoundtrip" $cacheRoot $projectRoot $projectFileExtension
}

function BuildSingleProject($useConsoleLogger, $projectFile, $cacheRoot){
    # echo "$msbuildapp $msbuildBinaries -buildWithCacheRoundtrip $invokingScriptDirectory\caches $invokingScriptDirectory"

    & $msbuildapp $msbuildBinaries $useConsoleLogger "-singleProject" $projectFile $cacheRoot
}