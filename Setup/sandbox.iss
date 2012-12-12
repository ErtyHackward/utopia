#include "scripts\products.iss"
#include "scripts\dotnetfx40.iss"

[Languages]
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
