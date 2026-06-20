$ErrorActionPreference = "Stop"

$projectRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$appProject = Join-Path $projectRoot "EquipmentProject\EquipmentProject.csproj"
$appOutput = Join-Path $projectRoot "EquipmentProject\bin\Debug\net10.0-windows10.0.19041.0\win-x64"
$appExe = Join-Path $appOutput "EquipmentProject.exe"

dotnet build $appProject -f net10.0-windows10.0.19041.0 -p:UseSharedCompilation=false

if (-not (Test-Path $appExe)) {
    throw "Could not find the built desktop app at: $appExe"
}

Start-Process -FilePath $appExe -WorkingDirectory $appOutput
