"C:\Program Files (x86)\Inno Setup 5\compil32.exe" /cc "realms_server.iss"
REM "sign.bat" this allows us to sign the file if we will have a key :(
"ftp" -s:"upload_realms_server.txt" -n
pause