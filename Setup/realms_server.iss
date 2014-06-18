#include "scripts\products.iss"
#include "scripts\products\dotnetfx40.iss"
#include "scripts\products\dxredist.iss"
#include "scripts\products\winversion.iss"
#include "scripts\products\fileversion.iss"


[Languages]
Name: en; MessagesFile: compiler:Default.isl
Name: ru; MessagesFile: compiler:Languages\Russian.isl
Name: fr; MessagesFile: compiler:Languages\French.isl

[Files]
Source: ..\Utopia\Realms\Realms.Server\bin\Release\*; DestDir: {app}; Excludes: *.dds,*.pdb,*.vshost.*,*.exe.config,Newtonsoft.Json.xml,Ninject.xml,NLog.xml,protobuf-net.xml,SharpDX.*.xml,SharpDX.xml,Config,Effects,GUI; Flags: ignoreversion recursesubdirs createallsubdirs

[Setup]
VersionInfoVersion=1.0.10
VersionInfoCompany=Utopia
VersionInfoDescription=Utopia
VersionInfoCopyright=Fabian Ceressia, Vladislav Pozdnyakov, 2013-2014
VersionInfoProductName=Utopia realms
MinVersion=0,6.0.6000
AppName=Utopia Realms Server
AppVerName=Utopia Realms Server, v4
AppPublisher=April32
AppPublisherURL=http://april32.com
AppSupportURL=http://utopiarealms.com
AppUpdatesURL=http://utopiarealms.com
LicenseFile=EULA.txt
DisableProgramGroupPage=true
ShowLanguageDialog=auto
DefaultDirName={pf}\Utopia Realms Server\
SetupIconFile=..\Media\Utopia2.ico
WizardImageFile=setupTitle.bmp
WizardSmallImageFile=setupSmallRealms.bmp
UninstallDisplayIcon=..\Media\Utopia2.ico
OutputBaseFilename=setup_realms_server

[Icons]
Name: {group}\Utopia Realms Server; Filename: {app}\Realms.Server.exe

[Run]
Filename: {app}\Realms.Server.exe; Description: {cm:LaunchProgram,Utopia Realms Server}; Flags: nowait postinstall skipifsilent

[Code]
function InitializeSetup(): Boolean;
begin
	dotnetfx40();
	Result := true;
end;

procedure CurStepChanged(CurStep: TSetupStep);
var 
	ProcessMsgPage: TOutputProgressWizardPage;
	ResultCode: Integer;
begin
  if CurStep = ssPostInstall then
  begin
    	ProcessMsgPage := CreateOutputProgressPage(CustomMessage('postprocess'),CustomMessage('postprocess'));
		ProcessMsgPage.Show;
		
    Exec('netsh', ExpandConstant('advfirewall firewall add rule name="Utopia Realms Server" dir=in action=allow program="{app}\Realms.Server.exe" enable=yes'),'', SW_HIDE, ewWaitUntilTerminated, ResultCode);
		
		ProcessMsgPage.Hide;
  end;
end;
