<#
.SYNOPSIS
    Veroeffentlicht DHLabel: bauen, Setup pruefen, Release auf GitHub anlegen.

.DESCRIPTION
    Ersetzt den bisherigen Weg ueber das eingecheckte Setup (vgl. den alten
    Workflow .github/workflows/release.yml, der auf das Einchecken der
    Setup-EXE reagiert hat). Das Installationspaket bleibt lokal und wird als
    Release-Asset hochgeladen - das Repository bleibt schlank.

    Da das Release mit dem persoenlichen Token der GitHub CLI angelegt wird,
    loest es den Workflow .github/workflows/winget.yml aus. (Ein Release, das
    innerhalb einer Action mit GITHUB_TOKEN erstellt wird, taete das nicht.)

.NOTES
    Voraussetzung: GitHub CLI installiert und angemeldet (gh auth login).
    Die Version wird aus Program\Properties\AssemblyInfo.cs gelesen - sie ist
    die einzige Stelle, an der sie gepflegt werden muss (neben SetupScript.iss
    und version.xml, deren Uebereinstimmung dieses Skript selbst prueft).

    DHLabel ist ein klassisches (Non-SDK) .NET-Framework-Projekt mit
    packages.config statt PackageReference und einem PostBuildEvent
    (Program\DHLabel.csproj), der nach jedem Release|x64-Build automatisch
    signiert (sign.cmd) und das Setup erzeugt (iscc SetupScript.iss) - ein
    separater "BuildInstaller"-Zielaufruf ist hier daher nicht noetig.

    Nach einem erfolgreichen Release werden EXE und Setup zusaetzlich per
    avUpload (eigenes Projekt, siehe E:\Windows\avUpload) beim
    Avast-Whitelisting eingereicht, damit die neue Version nicht faelschlich
    als Virus erkannt wird. avUpload unterstuetzt dafuer bereits einen
    Silent-Modus (--silent Datei1 [Datei2 ...]); die SFTP-Zugangsdaten liegen
    DPAPI-verschluesselt in der Registry dieses Rechners (einmalig ueber die
    avUpload-GUI gespeichert) - es ist also keine Code-Aenderung an avUpload
    noetig. Schlaegt die Einreichung fehl, wird nur gewarnt - der GitHub-
    Release ist davon unabhaengig und bleibt bestehen.

.EXAMPLE
    .\publish-release.ps1
    .\publish-release.ps1 -DryRun       # alles bauen, aber kein Release anlegen
    .\publish-release.ps1 -PreRelease   # Release als Vorabversion - WinGet bleibt aussen vor
    .\publish-release.ps1 -NoClean      # ohne vollstaendigen Neubau (nur fuer Probelaeufe)
    .\publish-release.ps1 -SkipAvast    # ohne Avast-Whitelisting-Einreichung
#>

[CmdletBinding()]
param(
    [switch] $DryRun,

    # Legt das Release als Vorabversion an. GitHub meldet dann das Ereignis
    # "prereleased" statt "released" - der WinGet-Workflow horcht auf "released"
    # und laeuft folglich nicht an. So laesst sich eine Fassung erst selbst
    # benutzen und spaeter, nach Entfernen der Markierung oder von Hand, an
    # WinGet weiterreichen.
    [switch] $PreRelease,

    # Ueberspringt das Loeschen der Ausgabeordner. Spart beim wiederholten
    # Probelauf die Zeit fuer den vollstaendigen Neubau - fuer ein echtes
    # Release aber nicht zu empfehlen.
    [switch] $NoClean,

    # Ueberspringt die Avast-Whitelisting-Einreichung per avUpload.
    [switch] $SkipAvast,

    # Pfad zu avUpload.exe. Standard passt zur regulaeren avUpload-Installation
    # (InstallScript.iss: DefaultDirName={autopf}\AvUpload).
    [string] $AvUploadExe = (Join-Path $env:ProgramFiles 'AvUpload\avUpload.exe')
)

$ErrorActionPreference = 'Stop'
Set-Location -Path $PSScriptRoot

# Ausgaben von msbuild auf Englisch (1033 = en-US). Zwei Gruende:
#   1. Deutsche Uebersetzungen von Build-Tools sind haeufig fehlerhaft oder
#      unvollstaendig.
#   2. Englische Fehlercodes und Meldungstexte lassen sich nachschlagen;
#      Dokumentation und Suchergebnisse sind durchweg englisch.
# VSLANG statt DOTNET_CLI_UI_LANGUAGE: Hier kommt klassisches msbuild.exe zum
# Einsatz, nicht die dotnet-CLI - DOTNET_CLI_UI_LANGUAGE haette keine Wirkung.
# Gilt nur fuer diesen Skriptlauf, nicht fuer die uebrige Umgebung.
$env:VSLANG = '1033'

# --- 0. Werkzeuge pruefen ---------------------------------------------------
# Frueh statt spaet: Ohne diese Pruefung faellt ein fehlendes Werkzeug erst
# nach mehreren Minuten Bauzeit auf, unmittelbar vor dem Anlegen des Releases.
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

if (-not $DryRun) {
    if (-not (Get-Command gh -ErrorAction SilentlyContinue)) {
        throw "GitHub CLI nicht gefunden. Installieren mit:`n" +
              "  winget install --id GitHub.cli`n" +
              "Danach PowerShell neu starten (PATH) und 'gh auth login' ausfuehren."
    }

    # gh schreibt den Anmeldestatus nach stderr. Zusammen mit
    # $ErrorActionPreference='Stop' macht PowerShell aus jeder umgeleiteten
    # stderr-Zeile einen abbrechenden Fehler - noch bevor der Exit-Code geprueft
    # wird. Deshalb die Praeferenz kurz herabsetzen und allein den Exit-Code werten.
    $prevEap = $ErrorActionPreference
    $ErrorActionPreference = 'SilentlyContinue'
    gh auth status 2>&1 | Out-Null
    $authed = ($LASTEXITCODE -eq 0)
    $ErrorActionPreference = $prevEap

    if (-not $authed) {
        throw "GitHub CLI ist nicht angemeldet. Bitte 'gh auth login' ausfuehren."
    }
}

# --- 1. Version ermitteln ---------------------------------------------------
$assemblyInfo = Get-Content 'Program\Properties\AssemblyInfo.cs' -Raw -Encoding utf8
$version = [regex]::Match($assemblyInfo, 'AssemblyFileVersion\("(\d+\.\d+\.\d+)').Groups[1].Value

if ([string]::IsNullOrWhiteSpace($version)) {
    throw 'Version konnte nicht aus Program\Properties\AssemblyInfo.cs gelesen werden.'
}

$tag   = "v$version"
$setup = "Program\bin\Release\x64\DHLabel-Setup-$version.exe"

Write-Host "Version: $version" -ForegroundColor Cyan
Write-Host "Tag:     $tag"     -ForegroundColor Cyan

# --- 1a. SetupScript.iss und version.xml gegenpruefen -----------------------
# Beide Dateien tragen die Versionsnummer ein zweites Mal von Hand:
# SetupScript.iss bestimmt den Namen der erzeugten Setup-EXE, version.xml
# wird vom eigenen In-App-Updater (Form1.cs) ausgelesen. Weichen sie von
# AssemblyInfo.cs ab, baut entweder die falsche Setup-Datei oder der
# In-App-Updater bleibt dauerhaft auf einem alten Stand haengen - beides ist
# schon vorgekommen.
$setupScriptIss = Get-Content 'SetupScript.iss' -Raw -Encoding utf8
$issVersion = [regex]::Match($setupScriptIss, '#define MyAppVersion "(\d+\.\d+\.\d+)"').Groups[1].Value
if ($issVersion -ne $version) {
    throw "SetupScript.iss (MyAppVersion '$issVersion') stimmt nicht mit " +
          "AssemblyInfo.cs (Version '$version') ueberein. Bitte angleichen."
}

if (Test-Path 'version.xml') {
    $versionXml = [xml](Get-Content 'version.xml' -Raw -Encoding utf8)
    $xmlVersion = $versionXml.item.version
    if ($xmlVersion -ne $version) {
        throw "version.xml (Version '$xmlVersion') stimmt nicht mit AssemblyInfo.cs " +
              "(Version '$version') ueberein. Der In-App-Updater wuerde sonst dauerhaft " +
              "auf '$xmlVersion' stehen bleiben. Bitte version.xml aktualisieren " +
              "(Version und Download-URL) und committen."
    }
}

# --- 1b. Arbeitsverzeichnis pruefen -----------------------------------------
# "gh release create" setzt den Tag auf den Stand, der SERVERSEITIG im
# Standard-Branch liegt. Nicht committete oder nicht gepushte Aenderungen
# wuerden dazu fuehren, dass das Release auf veralteten Quellcode zeigt,
# waehrend das Setup den neuen Stand enthaelt.
if (-not $DryRun) {
    $dirty = git status --porcelain
    if ($dirty) {
        Write-Host "`nNicht committete Aenderungen:" -ForegroundColor Yellow
        $dirty | ForEach-Object { Write-Host "  $_" }
        throw 'Bitte erst committen und pushen - sonst zeigt das Release auf veralteten Quellcode.'
    }

    $branch = (git rev-parse --abbrev-ref HEAD).Trim()
    git fetch --quiet origin $branch 2>$null
    $ahead = (git rev-list --count "origin/$branch..HEAD" 2>$null)

    if ($ahead -and [int]$ahead -gt 0) {
        throw "$ahead Commit(s) noch nicht gepusht. Bitte erst 'git push' ausfuehren."
    }
}

# --- 1c. CHANGELOG-Abschnitt pruefen ----------------------------------------
# Frueh statt spaet, aus demselben Grund wie oben: Ein fehlender Abschnitt soll
# nicht erst nach mehreren Minuten Bauzeit auffallen. Bei -DryRun genuegt eine
# Warnung, damit Probelaeufe auch ohne Eintrag funktionieren.
$notes = $null

if (Test-Path 'CHANGELOG.md') {
    $changelog = Get-Content 'CHANGELOG.md' -Raw -Encoding utf8

    # Abschnitt der aktuellen Version: von "## [1.5.9]" bis zur naechsten "## "-Ueberschrift.
    $pattern = '(?ms)^##\s*\[' + [regex]::Escape($version) + '\].*?$(.*?)(?=^##\s|\z)'
    $match   = [regex]::Match($changelog, $pattern)

    if ($match.Success) { $notes = $match.Groups[1].Value.Trim() }
}

if ([string]::IsNullOrWhiteSpace($notes)) {
    $msg = "CHANGELOG.md enthaelt keinen (oder einen leeren) Abschnitt '## [$version]'. " +
           "Bitte Eintrag ergaenzen, committen und pushen."
    if ($DryRun) { Write-Host "`n$msg" -ForegroundColor Yellow }
    else         { throw $msg }
}
else {
    Write-Host "CHANGELOG-Abschnitt fuer $version gefunden." -ForegroundColor Cyan
}

# --- 2. Alte Ausgaben entfernen ---------------------------------------------
# Absichtlich mehr als nur den Ausgabeordner: Auch die Zwischenergebnisse unter
# obj verschwinden, damit kein inkrementeller Bau ein veraltetes Teilstueck
# weiterreicht. Die Versionspruefung weiter unten faengt zwar eine falsche
# Versionsnummer ab - nicht aber alten Code, der zufaellig dieselbe Nummer traegt.
#
# Bewusst NUR die x64-Release-Zweige: Debug-Staende und die AnyCPU-Ordner bleiben
# stehen, damit Visual Studio nach einem Release nicht alles neu uebersetzen muss.
if (-not $NoClean) {
    foreach ($dir in @('Program\bin\Release\x64', 'Program\obj\x64\Release')) {
        if (Test-Path $dir) {
            Write-Host "Entferne $dir" -ForegroundColor DarkGray
            Remove-Item $dir -Recurse -Force
        }
    }
}
else {
    Write-Host "-NoClean: alte Ausgaben bleiben stehen." -ForegroundColor Yellow

    # Zumindest das, was sonst unbemerkt weiterverwendet wuerde: Eine alte
    # Setup-EXE mit demselben Namen wuerde die Pruefung weiter unten
    # bestehen, ohne dass tatsaechlich neu gebaut wurde.
    if (Test-Path $setup) { Remove-Item $setup -Force }
}

# --- 3. Bauen ----------------------------------------------------------------
Write-Host "`nStelle NuGet-Pakete wieder her..." -ForegroundColor Cyan
& $nuget restore 'DHLabel.sln'
if ($LASTEXITCODE -ne 0) {
    throw "nuget restore ist mit Code $LASTEXITCODE fehlgeschlagen."
}

Write-Host "`nVeroeffentliche..." -ForegroundColor Cyan
# Der PostBuildEvent in Program\DHLabel.csproj ruft nach erfolgreichem Build
# automatisch sign.cmd und iscc (SetupScript.iss) auf - ein separater
# "BuildInstaller"-Schritt ist hier anders als bei SDK-Projekten nicht noetig.
# /p:Platform=x64 ist noetig: Ohne diese Angabe nimmt MSBuild den Standard
# "AnyCPU" und legt das Kompilat nach Program\bin\Release\ statt
# Program\bin\Release\x64\.
& $msbuild 'DHLabel.sln' /p:Configuration=Release /p:Platform=x64 /v:minimal /m

# WICHTIG: PowerShell wertet Rueckgabewerte externer Programme nicht als Fehler
# aus - $ErrorActionPreference = 'Stop' greift bei msbuild und iscc nicht. Ohne
# diese Pruefung liefe das Skript nach einem Compilerfehler einfach weiter und
# meldete erst spaeter "Setup wurde nicht erzeugt", was an der falschen Stelle
# suchen laesst.
if ($LASTEXITCODE -ne 0) {
    throw "msbuild ist mit Code $LASTEXITCODE fehlgeschlagen. Fehlermeldungen stehen oben."
}

if (-not (Test-Path $setup)) {
    throw "Kompilierung lief durch, aber das Setup fehlt: $setup`n" +
          "Vermutlich ist iscc nicht gelaufen oder hat abgebrochen - siehe den " +
          "PostBuildEvent in Program\DHLabel.csproj und die Versionspruefung oben."
}

$size = [math]::Round((Get-Item $setup).Length / 1MB, 1)
Write-Host "`nSetup erstellt: $setup ($size MB)" -ForegroundColor Green

# --- 4. Gegenprobe: steckt wirklich die richtige Version drin? --------------
$exe = 'Program\bin\Release\x64\DHLabel.exe'
$exeVersion = (Get-Item $exe).VersionInfo.FileVersion
Write-Host "Exe-Dateiversion: $exeVersion"

if (-not $exeVersion.StartsWith($version)) {
    throw "Die veroeffentlichte Exe meldet $exeVersion, erwartet wurde $version."
}

if ($DryRun) {
    Write-Host "`n-DryRun: Release wird nicht angelegt." -ForegroundColor Yellow
    exit 0
}

# --- 5. Release anlegen und Asset hochladen ---------------------------------
# Existiert der Tag schon, wird das Release ersetzt - so ist ein zweiter
# Anlauf nach einem Fehlschlag gefahrlos moeglich.
# Auch hier schreibt gh nach stderr; dieselbe Herabsetzung wie beim Anmeldestatus,
# damit ein "Release existiert nicht" nicht faelschlich als Skriptfehler durchschlaegt.
$prevEap = $ErrorActionPreference
$ErrorActionPreference = 'SilentlyContinue'
gh release view $tag 2>&1 | Out-Null
$exists = ($LASTEXITCODE -eq 0)
$ErrorActionPreference = $prevEap

if ($exists) {
    Write-Host "`nRelease $tag existiert bereits - wird geloescht." -ForegroundColor Yellow
    gh release delete $tag --yes --cleanup-tag
}

# --- 6. Release-Notizen aus dem CHANGELOG ----------------------------------
# Der gepflegte CHANGELOG-Abschnitt ist fuer Leser deutlich nuetzlicher als
# eine automatisch erzeugte Liste von Commit-Titeln. Ermittelt und geprueft
# wurde er bereits in Schritt 1c - hier kommt ein echtes Release nur mit
# vorhandenem Abschnitt an.
$notesFile = [System.IO.Path]::GetTempFileName()
Set-Content -Path $notesFile -Value $notes -Encoding utf8
Write-Host "Release-Notizen aus CHANGELOG.md uebernommen." -ForegroundColor Cyan

$kind = if ($PreRelease) { 'Vorabversion' } else { 'Release' }
Write-Host "`nLege $kind $tag an..." -ForegroundColor Cyan

# Argumente sammeln statt die Aufrufe zu verdoppeln - so kann keine Variante
# beim Aendern vergessen werden.
$ghArgs = @('release', 'create', $tag, $setup, '--title', "DHLabel $tag")

$ghArgs += @('--notes-file', $notesFile)

if ($PreRelease) { $ghArgs += '--prerelease' }

gh @ghArgs
$ghExit = $LASTEXITCODE

Remove-Item $notesFile -Force -ErrorAction SilentlyContinue

# Auch hier gilt: externe Programme lassen PowerShell nicht von selbst abbrechen.
if ($ghExit -ne 0) {
    throw "gh release create ist mit Code $ghExit fehlgeschlagen. " +
          "Angemeldet? Pruefe mit 'gh auth status'."
}

if ($PreRelease) {
    Write-Host "`nFertig - als Vorabversion angelegt." -ForegroundColor Green
    Write-Host "Der Workflow 'WinGet veroeffentlichen' laeuft NICHT an." -ForegroundColor Yellow
    Write-Host "Zum Nachreichen spaeter eines von beidem:" -ForegroundColor Yellow
    Write-Host "  gh release edit $tag --prerelease=false      # loest den Workflow aus"
    Write-Host "  gh workflow run winget.yml -f tag=$tag          # von Hand anstossen"
}
else {
    Write-Host "`nFertig. Der Workflow 'WinGet veroeffentlichen' laeuft jetzt an." -ForegroundColor Green
}

# --- 7. Avast-Whitelisting ---------------------------------------------------
# avUpload reicht EXE und Setup beim Avast-Whitelisting-Server ein, damit die
# neue Version dort nicht faelschlich als Virus erkannt wird. Das ist dem
# eigentlichen Release nachgelagert und unabhaengig davon - ein Fehler hier
# (z.B. SFTP nicht erreichbar) soll das bereits angelegte Release nicht
# ungeschehen machen, deshalb nur eine Warnung statt throw.
#
# Die SFTP-Zugangsdaten holt sich avUpload selbst aus der Windows-Registry
# (HKCU, DPAPI-verschluesselt) - einmalig ueber die avUpload-GUI auf diesem
# Rechner gespeichert. Auf einem anderen Rechner oder einem GitHub-Actions-
# Runner waere dieser Schritt ohne Weiteres NICHT lauffaehig.
if (-not $SkipAvast) {
    if (-not (Test-Path $AvUploadExe)) {
        Write-Host "`navUpload nicht gefunden unter '$AvUploadExe' - Avast-Whitelisting uebersprungen." -ForegroundColor Yellow
        Write-Host "Pfad mit -AvUploadExe angeben oder mit -SkipAvast unterdruecken." -ForegroundColor Yellow
    }
    else {
        Write-Host "`nReiche EXE und Setup beim Avast-Whitelisting ein..." -ForegroundColor Cyan
        & $AvUploadExe --silent $exe $setup

        # Wie bei gh/msbuild: PowerShell wertet den Exit-Code eines externen
        # Programms nicht von selbst als Fehler aus.
        if ($LASTEXITCODE -ne 0) {
            Write-Host "avUpload ist mit Code $LASTEXITCODE fehlgeschlagen - das Release bleibt bestehen." -ForegroundColor Yellow
        }
        else {
            Write-Host "Avast-Whitelisting-Einreichung erfolgreich." -ForegroundColor Green
        }
    }
}
