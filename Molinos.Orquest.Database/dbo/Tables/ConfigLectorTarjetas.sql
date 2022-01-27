CREATE TABLE [dbo].[ConfigLectorTarjetas]
(
	[Id]	INT NOT NULL,
	[Lector] NVARCHAR(5) NOT NULL,
    CONSTRAINT [PK_dbo.ConfigLectorTarjetas] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_dbo.ConfigLectorTarjetas_dbo.ConfigId] FOREIGN KEY ([Id]) REFERENCES [dbo].[ConfigDispositivo] ([Id])
)
