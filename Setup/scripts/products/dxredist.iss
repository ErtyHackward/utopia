[CustomMessages]
dxredist_title=DirectX End-User Runtimes (June 2010)
dxredist_size=0 - 95.6 MB

[Code]
procedure dxredist();
var
	version: cardinal;
	
begin
	AddProductWithError('dxwebsetup.exe',
		'/Q',
		CustomMessage('dxredist_title'),
		CustomMessage('dxredist_size'),
		'http://google.com', -9);
end;