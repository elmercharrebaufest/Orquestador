CREATE TABLE [dbo].[ConfigHumedimetro] (
    [Id]             INT NOT NULL,
    [DireccionIp]    NVARCHAR (50) NOT NULL,
    [Puerto]         INT NOT NULL,
    [ComandoHumedad] CHAR NULL,
	[LongFrase]    INT NOT NULL,
    [TimeoutLectura] INT NOT NULL, 
    [DelimitadorCampos] CHAR NOT NULL, 
    [PosicionCampoHumedad] INT NOT NULL, 
    [PosicionCampoPesoHectolitrico] INT NOT NULL DEFAULT 4, 
    CONSTRAINT [PK_dbo.ConfigHumedimetro] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_dbo.ConfigHumedimetro_dbo.ConfigId] FOREIGN KEY ([Id]) REFERENCES [dbo].[ConfigDispositivo] ([Id])
);


GO
