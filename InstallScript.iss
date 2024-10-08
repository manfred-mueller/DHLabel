; Script generated by the Inno Setup Script Wizard.
; SEE THE DOCUMENTATION FOR DETAILS ON CREATING INNO SETUP SCRIPT FILES!

#define MyAppName "DHLabel"
#define MyAppVersion "1.5.2"
#define MyAppExeName MyAppName + ".exe"
#define MyAppPublisher "NASS e.K."
#define MyAppURL "https://www.nass-ek.de"
#define ProgramFiles GetEnv("ProgramFiles")

[Setup]
; NOTE: The value of AppId uniquely identifies this application. Do not use the same AppId value in installers for other applications.
; (To generate a new GUID, click Tools | Generate GUID inside the IDE.)
AppId={{66E110ED-DFB6-4A7D-891E-63652FDBD51C}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}
DefaultDirName={autopf}\{#MyAppName}
DisableDirPage=yes
DefaultGroupName={#MyAppName}
DisableProgramGroupPage=yes
LicenseFile=D:\Dokumente\gpl_de.txt
; Uncomment the following line to run in non administrative install mode (install for current user only.)
;PrivilegesRequired=lowest
OutputDir=Program\bin\Release
OutputBaseFilename={#MyAppName}-Setup-{#MyAppVersion}
SetupIconFile=D:\Bilder\nass-ek.ico
UninstallDisplayIcon={app}\{#MyAppExeName},0
;Begin adjustments for showing the logo
DisableWelcomePage=False
WizardImageFile=D:\Bilder\wz_nass-ek.bmp
WizardSmallImageFile=D:\Bilder\wz_leer_small.bmp
;End adjustments for showing the logo
Compression=lzma
SolidCompression=yes
WizardStyle=modern
ChangesAssociations = yes
SignTool=CertumVS

[Languages]
Name: "german"; MessagesFile: "compiler:Languages\German.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}";
Name: "{commondesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon;

[Registry]
Root: HKCR; Subkey: ".pdf";                             ValueData: "{#MyAppName}";          Flags: uninsdeletevalue; ValueType: string;  ValueName: ""
Root: HKCR; Subkey: "{#MyAppName}";                     ValueData: "Program {#MyAppName}";  Flags: uninsdeletekey;   ValueType: string;  ValueName: ""
Root: HKCR; Subkey: "{#MyAppName}\DefaultIcon";         ValueData: "{app}\{#MyAppExeName},0";                        ValueType: string;  ValueName: ""
Root: HKCR; Subkey: "{#MyAppName}\shell\open\command";  ValueData: """{app}\{#MyAppExeName}"" %1";                   ValueType: string;  ValueName: ""

[Files]
Source: "Program\bin\Release\DHLabel.exe"; DestDir: "{app}"; DestName: "{#MyAppExeName}"; Flags: confirmoverwrite
Source: "Program\bin\Release\AutoUpdater.NET.dll"; DestDir: "{app}"
Source: "Program\bin\Release\Microsoft.mshtml.dll"; DestDir: "{app}"
Source: "Program\bin\Release\Microsoft.Web.WebView2.Core.dll"; DestDir: "{app}"
Source: "Program\bin\Release\Microsoft.Web.WebView2.WinForms.dll"; DestDir: "{app}"
Source: "Program\bin\Release\Spire.Pdf.dll"; DestDir: "{app}"

[Code]

function GetUninstallString(): String;
var
  sUnInstPath: String;
  sUnInstallString: String;
begin
  sUnInstPath := ExpandConstant('Software\Microsoft\Windows\CurrentVersion\Uninstall\{#emit SetupSetting("AppId")}_is1');
  sUnInstallString := '';
  if not RegQueryStringValue(HKLM, sUnInstPath, 'UninstallString', sUnInstallString) then
    RegQueryStringValue(HKCU, sUnInstPath, 'UninstallString', sUnInstallString);
  Result := sUnInstallString;
end;


{ ///////////////////////////////////////////////////////////////////// }
function IsUpgrade(): Boolean;
begin
  Result := (GetUninstallString() <> '');
end;


{ ///////////////////////////////////////////////////////////////////// }
function UnInstallOldVersion(): Integer;
var
  sUnInstallString: String;
  iResultCode: Integer;
begin
{ Return Values: }
{ 1 - uninstall string is empty }
{ 2 - error executing the UnInstallString }
{ 3 - successfully executed the UnInstallString }

  { default return value }
  Result := 0;

  { get the uninstall string of the old app }
  sUnInstallString := GetUninstallString();
  if sUnInstallString <> '' then begin
    sUnInstallString := RemoveQuotes(sUnInstallString);
    if Exec(sUnInstallString, '/SILENT /NORESTART /SUPPRESSMSGBOXES','', SW_HIDE, ewWaitUntilTerminated, iResultCode) then
      Result := 3
    else
      Result := 2;
  end else
    Result := 1;
end;

{ ///////////////////////////////////////////////////////////////////// }
procedure CurStepChanged(CurStep: TSetupStep);
begin
  if (CurStep=ssInstall) then
  begin
    if (IsUpgrade()) then
    begin
      UnInstallOldVersion();
    end;
  end;
end;
