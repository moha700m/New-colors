$ErrorActionPreference = "Stop"
$dist = Join-Path $PSScriptRoot "dist\MohammedLab-ColorVision"
if (Test-Path $dist) { Remove-Item $dist -Recurse -Force }
dotnet publish "$PSScriptRoot\MohammedLab.ColorVision.csproj" -c Release -r win-x64 --self-contained true -p:PublishReadyToRun=true -o $dist
Copy-Item "$PSScriptRoot\LICENSE.txt" $dist
