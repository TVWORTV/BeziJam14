$rootFolder = ".\Assets\1.Scripts"

$usings = @(
    "using System;"
    "using System.Collections;"
    "using System.Collections.Generic;"
    "using UnityEngine;"
)

Get-ChildItem -Path $rootFolder -Recurse -Filter *.cs | ForEach-Object {

    $path = $_.FullName
    $content = Get-Content $path

    $newUsings = @()

    foreach ($using in $usings)
    {
        if (-not ($content -contains $using))
        {
            $newUsings += $using
        }
    }

    if ($newUsings.Count -gt 0)
    {
        Write-Host "Updating $path"

        $newContent = @()
        $newContent += $newUsings
        $newContent += ""
        $newContent += $content

        Set-Content $path $newContent
    }
}