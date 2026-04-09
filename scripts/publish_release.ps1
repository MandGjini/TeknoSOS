param(
    [string]$Configuration = "Release",
    [string]$Project = "TeknoSOS.WebApp.csproj",
    [string]$Output = "publish"
)

Write-Output "Publishing $Project ($Configuration) to $Output..."

dotnet publish $Project -c $Configuration -o $Output

if ($LASTEXITCODE -ne 0) {
    Write-Error "dotnet publish failed"
    exit $LASTEXITCODE
}

# Create a zip package
$zipPath = "$Output\TeknoSOS-published.zip"
if (Test-Path $zipPath) { Remove-Item $zipPath -Force }
Add-Type -AssemblyName System.IO.Compression.FileSystem
[System.IO.Compression.ZipFile]::CreateFromDirectory((Resolve-Path $Output).Path, $zipPath)

Write-Output "Publish complete. Package: $zipPath"
