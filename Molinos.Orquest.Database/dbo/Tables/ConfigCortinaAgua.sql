CREATE TABLE [dbo].[ConfigCortinaAgua]
(
	[Id]             INT NOT NULL,
    [IntervaloPooling]    INT NOT NULL,
    [DireccionDelVientoDesde]   INT NOT NULL,
    [DireccionDelVientoHasta]   INT NOT NULL,
	[NumeroSalida]    INT NOT NULL,
    [TiempoActivacion] INT NULL, 
    [TiempoDeEsperaActivacion] INT NULL, 
    [EstadoAbierta] BIT NOT NULL, 
    [Estacion_Id] INT NULL, 
    CONSTRAINT [PK_dbo.ConfigCortinaAgua] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_dbo.ConfigCortinaAgua_dbo.ConfigId] FOREIGN KEY ([Id]) REFERENCES [dbo].[ConfigDispositivo] ([Id]),
    CONSTRAINT [FK_dbo.ConfigCortinaAgua_dbo.EstacionId] FOREIGN KEY ([Estacion_Id]) REFERENCES [dbo].[ConfigMeteorologica] ([Id])
    
)
