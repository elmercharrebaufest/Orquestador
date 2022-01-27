CREATE TABLE [dbo].[ConfigBarrera]
(
	[Id]             INT NOT NULL,
    [NumeroSalida]    INT NOT NULL,
    [EstadoAbierta]   BIT NOT NULL,
	[TiempoActivacion]    INT NOT NULL,
    [Sensor_Id] INT NULL, 
    [TiempoEsperaReintento] INT NULL, 
    [TiempoMaximoEsperaReintento] INT NULL, 
    CONSTRAINT [PK_dbo.ConfigBarrera] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_dbo.ConfigBarrera_dbo.ConfigId] FOREIGN KEY ([Id]) REFERENCES [dbo].[ConfigDispositivo] ([Id]),
	CONSTRAINT [FK_dbo.ConfigBarrera_dbo.SensorId] FOREIGN KEY ([Sensor_Id]) REFERENCES [dbo].[ConfigSensor] ([Id])
)
