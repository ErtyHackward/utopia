[CustomMessages]
dxredist_title=DirectX End-User Runtimes (June 2010)
dxredist_size=0 - 95.6 MB

[Code]
procedure dxredist();
var
	version: cardinal;
	
begin
	if not FileExists(ExpandConstant('{sys}\d3dx11_43.dll')) then
		AddProduct('dxwebsetup.exe',
			'/q',
			CustomMessage('dxredist_title'),
			CustomMessage('dxredist_size'),
			'http://google.com');
end;