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
Source: Deps\dxwebsetup.exe; Flags: dontcopy
Source: ..\Utopia\Sandbox\Sandbox.Client\bin\Release\*; DestDir: {app}; Excludes: *.dds,*.pdb,*.vshost.*,*.exe.config,Newtonsoft.Json.xml,Ninject.xml,NLog.xml,protobuf-net.xml,SharpDX.*.xml,SharpDX.xml; Flags: ignoreversion recursesubdirs createallsubdirs

[Setup]
VersionInfoVersion=1.0.0
VersionInfoCompany=Utopia
VersionInfoDescription=Utopia
VersionInfoCopyright=Fabian Ceressia, Vladislav Pozdnyakov, 2012-2013
VersionInfoProductName=Utopia sandbox
MinVersion=0,6.0.6000
AppName=Utopia: Sandbox
AppVerName=Utopia: Sandbox, 1.0.0
AppPublisher=April32
AppPublisherURL=http://april32.com
AppSupportURL=http://utopiarealms.com
AppUpdatesURL=http://utopiarealms.com
LicenseFile=EULA.txt
DisableProgramGroupPage=true
ShowLanguageDialog=auto
DefaultDirName={pf}\Utopia Sandbox\
SetupIconFile=Utopia.ico
WizardImageFile=setupTitle.bmp
WizardSmallImageFile=setupSmall.bmp
UninstallDisplayIcon=Utopia.ico

[Icons]
Name: {group}\Utopia Sandbox; Filename: {app}\Sandbox.exe

[Run]
Filename: {app}\Sandbox.exe; Description: {cm:LaunchProgram,Utopia Sandbox}; Flags: nowait postinstall skipifsilent

[UninstallDelete]
Type: filesandordirs; Name: "{app}\TexturesPacks\Default\AnimatedTextures\ArrayAnimatedTextures.dds"
Type: filesandordirs; Name: "{app}\TexturesPacks\Default\BiomesColors\ArrayBiomesColors.dds"
Type: filesandordirs; Name: "{app}\TexturesPacks\Default\Particules\ArrayParticules.dds"
Type: filesandordirs; Name: "{app}\TexturesPacks\Default\Terran\ArrayTerran.dds"

Type: dirifempty; Name: "{app}\TexturesPacks\Default\AnimatedTextures\"
Type: dirifempty; Name: "{app}\TexturesPacks\Default\BiomesColors\"
Type: dirifempty; Name: "{app}\TexturesPacks\Default\Particules\"
Type: dirifempty; Name: "{app}\TexturesPacks\Default\Terran\"
Type: dirifempty; Name: "{app}\TexturesPacks\Default\"
Type: dirifempty; Name: "{app}\TexturesPacks\"
Type: dirifempty; Name: "{app}"


[Code]
function InitializeSetup(): Boolean;
begin

	dotnetfx40();
	ExtractTemporaryFile('dxwebsetup.exe');
	dxredist();
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
		
		Exec(ExpandConstant('{app}\PacksOptimize.exe'), ExpandConstant('action=createtexturearray path="{app}\TexturesPacks"'), '', SW_HIDE, ewWaitUntilTerminated, ResultCode);
		
    Exec('netsh', ExpandConstant('advfirewall firewall add rule name="Utopia Sandbox" dir=in action=allow program="{app}\Sandbox.exe" enable=yes'),'', SW_HIDE, ewWaitUntilTerminated, ResultCode);

		ProcessMsgPage.Hide;
  end;
end;