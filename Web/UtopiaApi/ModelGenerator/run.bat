REM: note that the '-sprocs' option is turned on

DbMetal.exe -provider=MySql -database:utopiamaindb -server:utopiamaindb.db.8691439.hostedresource.com -user:utopiamaindb -password:p934n11nfF -namespace:UtopiaApi.Models -dbml:UtopiaApi.dbml

DbMetal.exe -provider=MySql -code:Model.cs UtopiaApi.dbml -namespace:UtopiaApi.Models
pause