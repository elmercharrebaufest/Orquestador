CREATE TABLE [dbo].[ConfigSensor]
(
	[Id]	INT NOT NULL,
    [NumeroEntrada] INT NOT NULL, 
    [EstadoActivado] BIT NOT NULL DEFAULT 0, 
    CONSTRAINT [PK_dbo.ConfigSensor] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_dbo.ConfigSensor_dbo.ConfigId] FOREIGN KEY ([Id]) REFERENCES [dbo].[ConfigDispositivo] ([Id])
)
