function Combine
{
    [string[]] $argsAsStrings = $args
    return [System.IO.Path]::Combine($argsAsStrings)
}

function CloneOrUpdateRepo([string]$address, [string] $branch, [string] $repoPath, [bool] $updateIfExists = $true)
{
    if ((Test-Path  $repoPath) -and $updateIfExists)
    {
        Push-Location $repoPath

        & git fetch origin

        & git checkout $branch

        & git reset --hard "origin/$branch"

        Pop-Location
    }
    elseif (-not (Test-Path $repoPath))
    {
        & git clone $address $repoPath

        Push-Location $repoPath

        & git fetch origin

        & git checkout $branch

        Pop-Location
    }
}

$repoPath = [System.IO.Path]::GetFullPath((Combine $PSScriptRoot ".."))
$projectsDirectory = Combine $repoPath "projects"
$sourceDirectory = Combine $repoPath "src"