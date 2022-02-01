CREATE TABLE [dbo].[ConfigComunicador]
(
	[Id]             INT NOT NULL,
    [NumeroSalida]    INT NOT NULL,
    [TiempoMaximoEjecucion] INT NULL, 
    [PuertoDeAudio] INT NULL, 
    CONSTRAINT [PK_dbo.ConfigComunicador] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_dbo.ConfigComunicador_dbo.ConfigId] FOREIGN KEY ([Id]) REFERENCES [dbo].[ConfigDispositivo] ([Id]),
)
