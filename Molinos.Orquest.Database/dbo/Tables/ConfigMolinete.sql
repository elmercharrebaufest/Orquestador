CREATE TABLE [dbo].[ConfigMolinete]
(
	[Id] INT NOT NULL, 
    [DireccionUrl] NVARCHAR(300) NOT NULL,
    [TimeoutHabilitacion] INT NOT NULL,
    [IntervaloPooling] INT NOT NULL,
	CONSTRAINT [PK_dbo.ConfigMolinete] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_dbo.ConfigMolinete_dbo.ConfigId] FOREIGN KEY ([Id]) REFERENCES [dbo].[ConfigDispositivo] ([Id])

)
