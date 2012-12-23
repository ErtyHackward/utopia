#include "scripts\products.iss"
#include "scripts\products\dotnetfx40.iss"
#include "scripts\products\winversion.iss"
#include "scripts\products\fileversion.iss"


[Languages]
Name: en; MessagesFile: compiler:Default.isl
Name: ru; MessagesFile: compiler:Languages\Russian.isl
Name: fr; MessagesFile: compiler:Languages\French.isl

[Files]
Source: ..\Utopia\Realms\Realms.Client\bin\Release\*; DestDir: {app}; Excludes: *.pdb; Flags: ignoreversion recursesubdirs createallsubdirs

[Setup]
VersionInfoVersion=1.0.0
VersionInfoCompany=Utopia
VersionInfoDescription=Utopia
VersionInfoCopyright=Fabian Ceressia, Vladislav Pozdnyakov, 2012
VersionInfoProductName=Utopia realms
MinVersion=0,6.0.6000
AppName=Utopia: Realms
AppVerName=1.0.0
AppPublisherURL=http://utopiarealms.com
AppSupportURL=http://utopiarealms.com
AppUpdatesURL=http://utopiarealms.com
LicenseFile=EULA.txt
DisableProgramGroupPage=true
ShowLanguageDialog=auto
DefaultDirName={pf}\Utopia Realms\
SetupIconFile=Utopia.ico
WizardImageFile=setupTitle.bmp
WizardSmallImageFile=setupSmall.bmp

[Icons]
Name: {group}\Utopia Realms; Filename: {app}\Launcher.exe

[Run]
Filename: {app}\Launcher.exe; Description: {cm:LaunchProgram,Utopia Realms}; Flags: nowait postinstall skipifsilent

[Code]
function InitializeSetup(): Boolean;
begin
	dotnetfx40();
	Result := true;
end;
