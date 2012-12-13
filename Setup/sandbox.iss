#include "scripts\products.iss"
#include "scripts\products\dotnetfx40.iss"
#include "scripts\products\winversion.iss"
#include "scripts\products\fileversion.iss"


[Languages]
Name: en; MessagesFile: compiler:Default.isl
Name: ru; MessagesFile: compiler:Languages\Russian.isl
Name: fr; MessagesFile: compiler:Languages\French.isl

[Files]
Source: ..\Utopia\Sandbox\Sandbox.Client\bin\Release\*; DestDir: {app}; Excludes: *.pdb; Flags: ignoreversion recursesubdirs createallsubdirs

[Setup]
VersionInfoVersion=1.0.0
VersionInfoCompany=Utopia
VersionInfoDescription=Utopia
VersionInfoCopyright=Fabian Ceressia, Vladislav Pozdnyakov, 2012
VersionInfoProductName=Utopia sandbox
MinVersion=0,6.0.6000
AppName=Utopia: Sandbox
AppVerName=1.0.0
AppPublisherURL=http://utopiarealms.com
AppSupportURL=http://utopiarealms.com
AppUpdatesURL=http://utopiarealms.com
LicenseFile=EULA.txt
DisableProgramGroupPage=true
ShowLanguageDialog=auto
DefaultDirName={pf}\Utopia Sandbox\
SetupIconFile=Utopia.ico
WizardImageFile=setupTitle.bmp
WizardSmallImageFile=setupSmall.bmp

[Icons]
Name: {group}\Utopia Sandbox; Filename: {app}\Sandbox.exe

[Run]
Filename: {app}\Sandbox.exe; Description: {cm:LaunchProgram,Utopia Sandbox}; Flags: nowait postinstall skipifsilent

[Code]
function InitializeSetup(): Boolean;
begin
	dotnetfx40();
	Result := true;
end;
