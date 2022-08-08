CREATE TABLE [dbo].[ConfigGrupoBarrera]
(
	[Id] INT NOT NULL, 
    [BarreraArriba_Id] INT NOT NULL, 
    [BarreraAbajo_Id] INT NOT NULL, 
    [SensorArriba_Id] INT NOT NULL, 
    [SensorAbajo_Id] INT NOT NULL, 
    [SensorPrimerCruce_Id] INT NOT NULL, 
    [SensorSegundoCruce_Id] INT NOT NULL,
    CONSTRAINT [PK_dbo.ConfigGrupoBarrera] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_dbo.ConfigGrupoBarrera.ConfigId] FOREIGN KEY ([Id]) REFERENCES [dbo].[ConfigDispositivo] ([Id]),
	CONSTRAINT [FK_dbo.ConfigGrupoBarrera.SensorArribaId] FOREIGN KEY ([SensorArriba_Id]) REFERENCES [dbo].[ConfigSensor] ([Id]),
    CONSTRAINT [FK_dbo.ConfigGrupoBarrera.SensorAbajoId] FOREIGN KEY ([SensorAbajo_Id]) REFERENCES [dbo].[ConfigSensor] ([Id]),
    CONSTRAINT [FK_dbo.ConfigGrupoBarrera.SensorPrimerCruceId] FOREIGN KEY ([SensorPrimerCruce_Id]) REFERENCES [dbo].[ConfigSensor] ([Id]),
    CONSTRAINT [FK_dbo.ConfigGrupoBarrera.SensorSegundoCruceId] FOREIGN KEY ([SensorSegundoCruce_Id]) REFERENCES [dbo].[ConfigSensor] ([Id]),
    CONSTRAINT [FK_dbo.ConfigGrupoBarrera.BarreraArribaId] FOREIGN KEY ([BarreraArriba_Id]) REFERENCES [dbo].[ConfigBarrera] ([Id]),
    CONSTRAINT [FK_dbo.ConfigGrupoBarrera.BarreraAbajoId] FOREIGN KEY ([BarreraAbajo_Id]) REFERENCES [dbo].[ConfigBarrera] ([Id]),
)
