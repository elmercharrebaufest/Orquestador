CREATE TABLE [dbo].[ConfigJsonToIotBox]
(
	[Id] INT NOT NULL PRIMARY KEY, 
    [FormatoJson] INT NULL, 
    [NumeroSalida] INT NULL, 
    CONSTRAINT [FK_ConfigJsonToIotBox_FormatosJson] FOREIGN KEY (FormatoJson) REFERENCES FormatosJson(Id)
)
