
$folder = ".\Assets\1.Scripts"

Get-ChildItem -Path $folder -Recurse -File | ForEach-Object {
    $content = Get-Content $_.FullName -Raw
    Set-Content -Path $_.FullName -Value $content -NoNewline
    Write-Host "Saved:" $_.FullName
}

Write-Host "Done."