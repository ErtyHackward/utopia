[CustomMessages]
crt2013_title=Visual C++ Redistributable Packages for Visual Studio 2013
crt2013_size=7 MB

[Code]
const
	crt2013x86_url = 'http://download.microsoft.com/download/2/E/6/2E61CFA4-993B-4DD4-91DA-3737CD5CD6E3/vcredist_x86.exe';
	crt2013x64_url = 'http://download.microsoft.com/download/2/E/6/2E61CFA4-993B-4DD4-91DA-3737CD5CD6E3/vcredist_x64.exe';

procedure crt2013();
var
	version: cardinal;
	
begin
	RegQueryDWordValue(HKLM, 'Software\Microsoft\DevDiv\VC\Servicing\12.0\RuntimeMinimum', 'Install', version);
	if version <> 1 then
	begin
		if IsWin64 then
			AddProduct('vcredist_x64.exe',
					'/quiet /norestart',
					CustomMessage('crt2013_title'),
					CustomMessage('crt2013_size'),
					crt2013x64_url)
		else
			AddProduct('vcredist_x86.exe',
					'/quiet /norestart',
					CustomMessage('crt2013_title'),
					CustomMessage('crt2013_size'),
					crt2013x86_url);
	end;
end;