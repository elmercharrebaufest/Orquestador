CREATE TABLE [dbo].[ConfigJsonToIotBox]
(
	[Id] INT NOT NULL PRIMARY KEY, 
    [Json_Id] INT NULL, 
    [NumeroSalida] INT NULL, 
    CONSTRAINT [FK_ConfigJsonToIotBox_FormatosJson] FOREIGN KEY (Json_Id) REFERENCES FormatosJson(Id)
)
