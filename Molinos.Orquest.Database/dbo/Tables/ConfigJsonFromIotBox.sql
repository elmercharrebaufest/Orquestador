CREATE TABLE [dbo].[ConfigJsonFromIotBox]
(
	[Id] INT NOT NULL PRIMARY KEY, 
    [NumeroEntrada] INT NULL, 
    CONSTRAINT [FK_ConfigJsonFromIotBox_ConfigDispositivo] FOREIGN KEY (Id) REFERENCES ConfigDispositivo(Id)
)
