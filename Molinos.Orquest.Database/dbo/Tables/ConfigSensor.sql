CREATE TABLE [dbo].[ConfigSensor]
(
	[Id]	INT NOT NULL,
    [NumeroEntrada] INT NOT NULL, 
    [EstadoActivado] BIT NOT NULL DEFAULT 0, 
    [Camara_Id] INT NULL, 
    [Accion] INT NULL, 
    CONSTRAINT [PK_dbo.ConfigSensor] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_dbo.ConfigSensor_dbo.ConfigId] FOREIGN KEY ([Id]) REFERENCES [dbo].[ConfigDispositivo] ([Id]),
    CONSTRAINT [FK_dbo.ConfigSensor_dbo.CamaraId] FOREIGN KEY ([Camara_Id]) REFERENCES [dbo].[Dispositivo] ([Id])
)
