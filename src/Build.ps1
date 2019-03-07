param (
    [string]$Configuration = "Release"
)

& "$PSScriptRoot\BuildRepos.ps1" -configuration $Configuration -BuildMSBuild -BuildSdk -RedirectEnvironmentToBuildOutputs

rm -Recurse -Force "$PSScriptRoot\msb\bin"
rm -Recurse -Force "$PSScriptRoot\msb\obj"

& "$env:MSBuildBootstrapExe" /restore "$PSScriptRoot\msb\msb.csproj" /p:"Configuration=$Configuration"

$env:GraphTestApp = "$PSScriptRoot\msb\bin\$Configuration\net472\msb.exe"
