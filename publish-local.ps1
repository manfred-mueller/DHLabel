<#
.SYNOPSIS
    Baut DHLabel lokal (Release|x64) inkl. Signieren und Setup - ohne Release/Upload.

.DESCRIPTION
    Entspricht einem normalen Release-Build in Visual Studio: msbuild baut
    DHLabel.sln in der Konfiguration Release|x64. Der PostBuildEvent in
    Program\DHLabel.csproj signiert danach automatisch die EXE (sign.cmd)
    und erzeugt das Setup (iscc SetupScript.iss) - hier ist dafuer kein
    eigener Schritt noetig.

    DHLabel ist ein klassisches (Non-SDK) .NET-Framework-Projekt mit
    packages.config statt PackageReference. Daher reicht "msbuild" allein
    nicht wie bei "dotnet publish" - die NuGet-Pakete muessen vorher per
    "nuget restore" geholt werden.

    Fuer einen vollstaendigen Release mit GitHub-Upload siehe publish-release.ps1.

.EXAMPLE
    .\publish-local.ps1
#>

[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'
Set-Location -Path $PSScriptRoot

# Ausgaben auf Englisch (VSLANG, nicht DOTNET_CLI_UI_LANGUAGE - hier kommt
# klassisches msbuild.exe zum Einsatz) - leichter nachschlagbare
# Fehlermeldungen, siehe ausfuehrliche Begruendung in publish-release.ps1.
$env:VSLANG = '1033'

# --- Werkzeuge suchen --------------------------------------------------------
# Weder msbuild.exe noch nuget.exe liegen zuverlaessig im PATH, ausser man
# arbeitet in einer "Developer PowerShell for VS". vswhere.exe wird von jedem
# Visual-Studio-Installer mitgeliefert und kennt den tatsaechlichen Pfad.
function Find-MSBuild {
    $cmd = Get-Command msbuild.exe -ErrorAction SilentlyContinue
    if ($cmd) { return $cmd.Source }

    $vswhere = "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe"
    if (Test-Path $vswhere) {
        $path = & $vswhere -latest -prerelease -requires Microsoft.Component.MSBuild `
            -find 'MSBuild\**\Bin\MSBuild.exe' | Select-Object -First 1
        if ($path) { return $path }
    }

    throw "MSBuild nicht gefunden. Entweder in einer 'Developer PowerShell for VS' " +
          "ausfuehren oder Visual Studio (mit .NET-Desktopentwicklung) installieren."
}

function Find-NuGet {
    $cmd = Get-Command nuget.exe -ErrorAction SilentlyContinue
    if ($cmd) { return $cmd.Source }

    $vswhere = "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe"
    if (Test-Path $vswhere) {
        $vsPath = & $vswhere -latest -property installationPath
        if ($vsPath) {
            $bundled = Join-Path $vsPath 'Common7\IDE\CommonExtensions\Microsoft\NuGet\NuGet.exe'
            if (Test-Path $bundled) { return $bundled }
        }
    }

    throw "nuget.exe nicht gefunden. Installieren mit:`n" +
          "  winget install --id Microsoft.NuGet`n" +
          "Danach PowerShell neu starten (PATH)."
}

$msbuild = Find-MSBuild
$nuget   = Find-NuGet

Write-Host "Stelle NuGet-Pakete wieder her..." -ForegroundColor Cyan
& $nuget restore 'DHLabel.sln'
if ($LASTEXITCODE -ne 0) {
    throw "nuget restore ist mit Code $LASTEXITCODE fehlgeschlagen."
}

Write-Host "`nBaue DHLabel (Release|x64)..." -ForegroundColor Cyan
& $msbuild 'DHLabel.sln' /p:Configuration=Release /p:Platform=x64 /v:minimal /m

# WICHTIG: PowerShell wertet Rueckgabewerte externer Programme nicht als Fehler
# aus - $ErrorActionPreference = 'Stop' greift bei msbuild nicht von selbst.
if ($LASTEXITCODE -ne 0) {
    throw "msbuild ist mit Code $LASTEXITCODE fehlgeschlagen. Fehlermeldungen stehen oben."
}

$exe = 'Program\bin\Release\x64\DHLabel.exe'
if (Test-Path $exe) {
    Write-Host "`nFertig: $exe" -ForegroundColor Green
}
else {
    Write-Host "`nBuild durchgelaufen, aber $exe fehlt - bitte oben pruefen." -ForegroundColor Yellow
}
