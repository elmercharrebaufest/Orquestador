CREATE TABLE [dbo].[ConfigPantalla]
(
	[Id] INT NOT NULL, 
    [UrlSCATO] VARCHAR(500) NOT NULL, 
    [UrlFTP] VARCHAR(500) NOT NULL, 
    [TiempoDeRefresco] INT NOT NULL,
	[NombreUsuario] NVARCHAR(40) NULL, 
    [Contrasenia] NVARCHAR(32) NULL
	CONSTRAINT [PK_dbo.ConfigPantalla] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_dbo.ConfigPantalla_dbo.ConfigId] FOREIGN KEY ([Id]) REFERENCES [dbo].[ConfigPantalla] ([Id])
)
