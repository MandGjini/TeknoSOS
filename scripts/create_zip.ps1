Add-Type -AssemblyName System.IO.Compression.FileSystem
$src = (Resolve-Path 'publish').Path
$out = Join-Path (Split-Path $src) "TeknoSOS-published-$(Get-Date -Format yyyyMMddHHmmss).zip"
[System.IO.Compression.ZipFile]::CreateFromDirectory($src, $out)
Write-Output $out
