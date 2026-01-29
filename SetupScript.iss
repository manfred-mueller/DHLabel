; DHLabel – Winget-konformer Installer (ProgramData-Variante)

#define MyAppName "DHLabel"
#define MyAppVersion "1.5.4"
#define MyAppExeName MyAppName + ".exe"
#define MyAppPublisher "NASS e.K."
#define MyAppURL "https://www.nass-ek.de"

[Setup]
AppId={{66E110ED-DFB6-4A7D-891E-63652FDBD51C}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}

; 🔑 WINGET-ENTSCHEIDUNGEN
PrivilegesRequired=lowest
DefaultDirName={commonappdata}\{#MyAppName}

DisableDirPage=yes
DisableProgramGroupPage=yes
DisableWelcomePage=yes
DisableFinishedPage=yes

LicenseFile=D:\Dokumente\gpl_de.txt

OutputDir=Program\bin\Release
OutputBaseFilename={#MyAppName}-Setup-{#MyAppVersion}

SetupIconFile=D:\Bilder\nass-ek.ico
UninstallDisplayIcon={app}\{#MyAppExeName},0

Compression=lzma
SolidCompression=yes
WizardStyle=modern
SignTool=Certum

[Languages]
Name: "german"; MessagesFile: "compiler:Languages\German.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}";
Name: "{commondesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon;

[Files]
; Haupt-EXE (Costura → managed DLLs eingebettet)
Source: "Program\bin\Release\DHLabel.exe"; DestDir: "{app}"; Flags: ignoreversion

; Native SkiaSharp DLLs – architekturabhängig
Source: "Program\bin\Release\x64\libSkiaSharp.dll"; DestDir: "{app}"; Check: Is64BitInstallMode; Flags: ignoreversion
Source: "Program\bin\Release\x86\libSkiaSharp.dll"; DestDir: "{app}"; Check: not Is64BitInstallMode; Flags: ignoreversion

; WebView2 native loader (immer win-x86 korrekt)
Source: "Program\bin\Release\runtimes\win-x86\native\WebView2Loader.dll"; DestDir: "{app}"; Flags: ignoreversion

[Registry]
; Nur per-User-Registry – erlaubt & korrekt
Root: HKCU; Subkey: "Software\{#MyAppName}"; Flags: uninsdeletekey

[Code]

function GetUninstallString(): String;
var
  sUnInstPath: String;
  sUnInstallString: String;
begin
  sUnInstPath :=
    ExpandConstant('Software\Microsoft\Windows\CurrentVersion\Uninstall\{#emit SetupSetting("AppId")}_is1');
  sUnInstallString := '';
  if not RegQueryStringValue(HKCU, sUnInstPath, 'UninstallString', sUnInstallString) then
    RegQueryStringValue(HKLM, sUnInstPath, 'UninstallString', sUnInstallString);
  Result := sUnInstallString;
end;

function IsUpgrade(): Boolean;
begin
  Result := (GetUninstallString() <> '');
end;

procedure UnInstallOldVersion();
var
  sUnInstallString: String;
  iResultCode: Integer;
begin
  sUnInstallString := GetUninstallString();
  if sUnInstallString <> '' then
  begin
    sUnInstallString := RemoveQuotes(sUnInstallString);
    Exec(
      sUnInstallString,
      '/VERYSILENT /SUPPRESSMSGBOXES /NORESTART /SP-',
      '',
      SW_HIDE,
      ewWaitUntilTerminated,
      iResultCode
    );
  end;
end;

procedure CurStepChanged(CurStep: TSetupStep);
begin
  if (CurStep = ssInstall) and IsUpgrade() then
    UnInstallOldVersion();
end;
