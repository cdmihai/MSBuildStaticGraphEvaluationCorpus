param (
    [string]$Configuration = "Release"
)

. "$PSScriptRoot\Common.ps1"

& "$PSScriptRoot\BuildRepos.ps1" -configuration $Configuration -BuildMSBuild -BuildSdk -RedirectEnvironmentToBuildOutputs

rm -Recurse -Force "$sourceDirectory\msb\bin"
rm -Recurse -Force "$sourceDirectory\msb\obj"

& "$env:MSBuildBootstrapExe" /restore "$sourceDirectory\msb\msb.csproj" /p:"Configuration=$Configuration"

$env:GraphTestApp = "$sourceDirectory\msb\bin\$Configuration\net472\msb.exe"
