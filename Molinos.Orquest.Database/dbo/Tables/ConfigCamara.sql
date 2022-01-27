CREATE TABLE [dbo].[ConfigCamara]
(
	[Id]             INT NOT NULL,
	[Uri]            NVARCHAR(100) NOT NULL,
    [TimeoutLectura] INT NOT NULL DEFAULT 5000,
	[MargenDerecho] INT NULL,
	[MargenIzquierdo] INT NULL, 
	[MargenSuperior] INT NULL, 
	[MargenInferior] INT NULL, 
	[RotateFlipType] INT NULL, 
    [NombreUsuario] NVARCHAR(40) NULL, 
    [Contrasenia] NVARCHAR(32) NULL, 
    [UrlStreaming] NVARCHAR(200) NULL,
	[DireccionIp]    NVARCHAR (50) NULL,
    [Puerto]         INT NULL,
	[LongFrase]    INT NULL,
    CONSTRAINT [PK_dbo.ConfigCamara] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_dbo.ConfigCamara_dbo.ConfigId] FOREIGN KEY ([Id]) REFERENCES [dbo].[ConfigDispositivo] ([Id])
)
