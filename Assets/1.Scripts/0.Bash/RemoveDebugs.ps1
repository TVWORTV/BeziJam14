$folder = ".\Assets\1.Scripts"

Get-ChildItem -Path $folder -Recurse -Filter *.cs | ForEach-Object {

    $path = $_.FullName
    $content = Get-Content $path -Raw

    $newContent = [regex]::Replace(
        $content,
        '(?ms)^[ \t]*Debug\.Log\s*\(.*?\)[ \t]*;[ \t]*(?:\r?\n)?',
        ''
    )

    if ($newContent -ne $content) {
        Set-Content -Path $path -Value $newContent -Encoding UTF8
        Write-Host "Cleaned: $path"
    }
}

Write-Host "Done."