CREATE TABLE [dbo].[ConfigMeteorologica]
(
	[Id]             INT NOT NULL,
    [Ruta] NCHAR(50) NOT NULL, 
    [TiempoActivacion] INT NULL, 
    CONSTRAINT [PK_dbo.ConfigMeteorologica] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_dbo.ConfigMeteorologica_dbo.ConfigId] FOREIGN KEY ([Id]) REFERENCES [dbo].[ConfigDispositivo] ([Id])
)
