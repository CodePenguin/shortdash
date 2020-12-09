#define ApplicationName 'ShortDash'
#ifndef BuildVersion
#define BuildVersion "0.0.0"
#endif

[Setup]
AppId={{A9D5146E-5ABD-4687-B370-6850DD8B995F}
AppName={#ApplicationName}
AppVersion={#BuildVersion}
AppVerName={#ApplicationName} {#BuildVersion}
AppPublisher=ShortDash
AppPublisherURL=https://github.com/CodePenguin/shortdash
AppSupportURL=https://github.com/CodePenguin/shortdash
AppUpdatesURL=https://github.com/CodePenguin/shortdash
DefaultDirName={commonpf}\ShortDash
DefaultGroupName=ShortDash
AllowNoIcons=yes
LicenseFile=license.txt
OutputDir=bin
OutputBaseFilename=ShortDash-win-x64-setup
SetupIconFile=assets\ShortDash.ico
Compression=lzma
SolidCompression=yes
ArchitecturesInstallIn64BitMode=x64
VersionInfoVersion={#BuildVersion}
WizardStyle=modern
AlwaysShowGroupOnReadyPage=True
AlwaysShowDirOnReadyPage=True
DisableDirPage=no
DisableProgramGroupPage=yes
UninstallDisplayIcon={app}\ShortDash.ico

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Files]
Source: "assets\ShortDash.ico"; DestDir: "{app}"; Flags: ignoreversion
Source: "bin\ShortDash-win-x64\ShortDash.Launcher.exe"; DestDir: "{app}"; Flags: ignoreversion; Components: server target
Source: "bin\ShortDash-win-x64\ShortDash.Server\appsettings.json"; DestDir: "{app}\ShortDash.Server"; Flags: ignoreversion; Components: server
Source: "bin\ShortDash-win-x64\ShortDash.Server\ShortDash*"; DestDir: "{app}\ShortDash.Server"; Flags: ignoreversion; Components: server
Source: "bin\ShortDash-win-x64\ShortDash.Server\plugins\*"; DestDir: "{app}\ShortDash.Server\plugins"; Flags: ignoreversion recursesubdirs createallsubdirs; Components: server
Source: "bin\ShortDash-win-x64\ShortDash.Server\wwwroot\*"; DestDir: "{app}\ShortDash.Server\wwwroot"; Flags: ignoreversion recursesubdirs createallsubdirs; Components: server
Source: "bin\ShortDash-win-x64\ShortDash.Target\appsettings.json"; DestDir: "{app}\ShortDash.Target"; Flags: ignoreversion; Components: target
Source: "bin\ShortDash-win-x64\ShortDash.Target\ShortDash*"; DestDir: "{app}\ShortDash.Target"; Flags: ignoreversion; Components: target
Source: "bin\ShortDash-win-x64\ShortDash.Target\plugins\*"; DestDir: "{app}\ShortDash.Target\plugins"; Flags: ignoreversion recursesubdirs createallsubdirs; Components: target
Source: "bin\ShortDash-win-x64\ShortDash.Target\wwwroot\*"; DestDir: "{app}\ShortDash.Target\wwwroot"; Flags: ignoreversion recursesubdirs createallsubdirs; Components: target
Source: "license.txt"; DestDir: "{app}"; Flags: ignoreversion

[Icons]
Name: "{group}\ShortDash Server"; Filename: "{app}\ShortDash.Launcher.exe"; WorkingDir: "{app}"; IconFilename: "{app}\ShortDash.ico"; IconIndex: 0; Parameters: "-b ShortDash.Server"; Components: server
Name: "{group}\ShortDash Target"; Filename: "{app}\ShortDash.Launcher.exe"; WorkingDir: "{app}"; IconFilename: "{app}\ShortDash.ico"; IconIndex: 0; Parameters: "-b ShortDash.Target"; Components: target

[Components]
Name: "server"; Description: "ShortDash Server"; Types: server; Flags: checkablealone
Name: "server/startup"; Description: "Run server on startup"; Types: server
Name: "target"; Description: "ShortDash Target"; Types: custom; Flags: checkablealone
Name: "target/startup"; Description: "Run target on startup"; Types: target

[Types]
Name: "server"; Description: "ShortDash Server"
Name: "target"; Description: "ShortDash Target"
Name: "custom"; Description: "Custom"; Flags: iscustom

[Registry]
Root: "HKLM"; Subkey: "SOFTWARE\Microsoft\Windows\CurrentVersion\Run"; ValueType: string; ValueName: "ShortDash Server"; ValueData: """{app}\ShortDash.Launcher.exe"" -b ShortDash.Server"; Flags: deletevalue uninsdeletevalue; Components: server/startup
Root: "HKLM"; Subkey: "SOFTWARE\Microsoft\Windows\CurrentVersion\Run"; ValueType: string; ValueName: "ShortDash Target"; ValueData: """{app}\ShortDash.Launcher.exe"" -b ShortDash.Target"; Flags: deletevalue uninsdeletevalue; Components: target/startup

[Run]
Filename: "{app}\ShortDash.Launcher.exe"; Parameters: "-b ShortDash.Server"; WorkingDir: "{app}"; Flags: postinstall nowait; Description: "Run ShortDash Server"; Components: server
Filename: "{app}\ShortDash.Launcher.exe"; Parameters: "-b ShortDash.Target"; WorkingDir: "{app}"; Flags: postinstall nowait; Description: "Run ShortDash Target"; Components: target
