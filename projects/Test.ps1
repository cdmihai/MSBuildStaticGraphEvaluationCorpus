param([string]$projectExtension)

$invocationDirectory=(pwd).Path
$msbuildBinaries=$env:MSBuildBootstrapBinDirectory
$msbuildApp=$env:GraphTestApp

echo "Using msbuild binaries from: $msbuildBinaries"
echo "Using test app binaries from: $msbuildApp"

function Combine([string]$root, [string]$subdirectory)
{
    return [System.IO.Path]::Combine($root, $subdirectory)
}

function BuildWithCacheRoundtripDefault([string] $projectRoot, [string] $projectFileExtension){

    BuildWithCacheRoundtrip "true" $projectRoot "$projectRoot\caches" $projectFileExtension
}

function BuildWithCacheRoundtrip([string] $useConsoleLogger, [string] $projectRoot, [string] $cacheRoot, [string] $projectFileExtension){
    & $msbuildapp $msbuildBinaries $useConsolelogger "-buildWithCacheRoundtrip" $projectRoot $cacheRoot $projectFileExtension
}

function BuildwithBuildManager([string] $projectRoot, [string] $projectFileExtension){
    & $msbuildapp $msbuildBinaries "true" "-buildWithBuildManager" $projectRoot $projectFileExtension
}

function BuildSingleProject($useConsoleLogger, $projectFile, $cacheRoot){
    # echo "$msbuildapp $msbuildBinaries -buildWithCacheRoundtrip $invokingScriptDirectory\caches $invokingScriptDirectory"

    & $msbuildapp $msbuildBinaries $useConsoleLogger "-singleProject" $projectFile $cacheRoot
}

function PrintHeader([string]$text)
{
    $line = "=========================$text========================="
    Write-Output ("=" * $line.Length)
    Write-Output $line
    Write-Output ("=" * $line.Length)
}

function SetupTestProject([string]$projectRoot)
{
    echo "Cleaning $projectRoot"

    Remove-Item -Force -Recurse "$projectRoot\**\bin"
    Remove-Item -Force -Recurse "$projectRoot\**\obj"

    $setupScript = Combine $projectRoot "setup.ps1"

    if (Test-Path $setupScript)
    {
        echo "running $setupScript"
        & $setupScript
    }
}

function TestProject([string] $projectRoot, [string] $projectExtension)
{
    PrintHeader "BuildManager: $projectRoot"
    SetupTestProject $projectRoot
    BuildWithBuildManager $projectRoot $projectExtension

    if ($LASTEXITCODE -ne 0)
    {
        exit
    }

    PrintHeader "Cache roundtrip: $projectRoot"
    SetupTestProject $projectRoot
    BuildWithCacheRoundtripDefault $projectRoot $projectExtension

    if ($LASTEXITCODE -ne 0)
    {
        exit
    }
}

if ($projectExtension) {
    TestProject $invocationDirectory $projectExtension
    exit
}

$workingProjectRoots = @{
    "$PSScriptRoot\non-sdk\working" = "proj";
    "$PSScriptRoot\sdk\working" = "csproj"
}

$brokenProjects = @("oldWPF1", "oldWPF-new-2")

foreach ($directoryWithProjects in $workingProjectRoots.Keys)
{
    $projectExtension = $workingProjectRoots[$directoryWithProjects]

    foreach ($projectRoot in Get-ChildItem -Directory $directoryWithProjects)
    {
        if ($brokenProjects.Contains($projectRoot.Name))
        {
            PrintHeader "Skipping $($projectRoot.FullName)"
            continue;
        }

        TestProject $projectRoot.FullName $projectExtension
    }
}