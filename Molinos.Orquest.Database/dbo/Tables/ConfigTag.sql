CREATE TABLE [dbo].[ConfigTag]
(
	[Id] INT NOT NULL, 
    [Query] NVARCHAR(MAX) NOT NULL, 
    [Timeout] INT NULL,  
    CONSTRAINT [FK_ConfigTag_Dispositivo] FOREIGN KEY (Id) REFERENCES Dispositivo(id),  
    CONSTRAINT [FK_dbo.ConfigTag_dbo.ConfigId] FOREIGN KEY ([Id]) REFERENCES [dbo].[ConfigDispositivo] ([Id]),
)
