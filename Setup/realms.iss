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
Source: ..\Utopia\Realms\Realms.Client\bin\Release\*; DestDir: {app}; Excludes: *.dds,*.pdb,*.vshost.*,*.exe.config,Newtonsoft.Json.xml,Ninject.xml,NLog.xml,protobuf-net.xml,SharpDX.D3DCompiler.xml,SharpDX.Direct3D11.xml,SharpDX.DXGI.xml,SharpDX.XAudio2.xml,SharpDX.xml; Flags: ignoreversion recursesubdirs createallsubdirs

[Setup]
VersionInfoVersion=1.0.0
VersionInfoCompany=Utopia
VersionInfoDescription=Utopia
VersionInfoCopyright=Fabian Ceressia, Vladislav Pozdnyakov, 2012-2013
VersionInfoProductName=Utopia realms
MinVersion=0,6.0.6000
AppName=Utopia: Realms
AppVerName=Utopia: Realms, 1.0.0
AppPublisher=April32
AppPublisherURL=http://april32.com
AppSupportURL=http://utopiarealms.com
AppUpdatesURL=http://utopiarealms.com
LicenseFile=EULA.txt
DisableProgramGroupPage=true
ShowLanguageDialog=auto
DefaultDirName={pf}\Utopia Realms\
SetupIconFile=Utopia.ico
WizardImageFile=setupTitle.bmp
WizardSmallImageFile=setupSmall.bmp
UninstallDisplayIcon=Utopia.ico

[Icons]
Name: {group}\Utopia Realms; Filename: {app}\Launcher.exe

[Run]
Filename: {app}\Launcher.exe; Description: {cm:LaunchProgram,Utopia Realms}; Flags: nowait postinstall skipifsilent

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
		
		ProcessMsgPage.Hide;
  end;
end;
