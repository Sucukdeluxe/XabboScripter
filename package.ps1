$ErrorActionPreference = "Stop"
$root = $PSScriptRoot

$csproj = Get-Content "$root\src\Xabbo.Scripter\Xabbo.Scripter.csproj" -Raw
if ($csproj -notmatch '<Version>([^<]+)</Version>') { throw "Version not found in Xabbo.Scripter.csproj" }
$ver = $Matches[1]
Write-Host "Packaging XabboScripter $ver"

$pub = "$root\publish-final"
dotnet publish "$root\src\Xabbo.Scripter" -c Release -f net6.0-windows -r win-x64 --self-contained false -p:PublishReadyToRun=true -o $pub
if ($LASTEXITCODE -ne 0) { throw "dotnet publish failed" }

$stage = "$root\pkg\XabboScripter_$ver"
if (Test-Path "$root\pkg") { Remove-Item "$root\pkg" -Recurse -Force }
New-Item -ItemType Directory -Force "$stage\extension" | Out-Null
Set-Content -Path "$stage\command.txt" -NoNewline -Value '["cmd","/C","Xabbo.Scripter.exe","-c","{cookie}","-p","{port}","-f","{filename}",">","NUL","2>&1"]'
Copy-Item "$pub\*" "$stage\extension\" -Recurse -Force

$zip = "$root\XabboScripter_$ver.zip"
Compress-Archive -Path $stage -DestinationPath $zip -Force
Write-Host "Package: $zip"
Write-Host "Install: open the zip, drag XabboScripter_$ver into G-Earth's Extensions folder."
