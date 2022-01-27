CREATE TABLE [dbo].[ConfigPantallaPuestoDeVianda]
(
	[Id] INT NOT NULL,
	[ConfigPuestoDeVianda_Id] INT NOT NULL, 

    CONSTRAINT [PK_dbo.ConfigPantallaPuestoDeVianda] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_dbo.ConfigPantallaPuestoDeVianda_dbo.ConfigId] FOREIGN KEY ([Id]) REFERENCES [dbo].[ConfigDispositivo] ([Id]),
    CONSTRAINT [FK_dbo.ConfigPantallaPuestoDeVianda_dbo.ConfigPuestoDeVianda] FOREIGN KEY ([ConfigPuestoDeVianda_Id]) REFERENCES [dbo].[ConfigPuestoDeVianda] ([Id])
)
