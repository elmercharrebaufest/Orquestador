CREATE TABLE [dbo].[ConfigLectorQr]
(
	[Id]           INT            NOT NULL,
    [Lector] NVARCHAR(5) NOT NULL,
    CONSTRAINT [PK_dbo.ConfigLectorQr] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_dbo.ConfigLectorQr_dbo.ConfigId] FOREIGN KEY ([Id]) REFERENCES [dbo].[ConfigDispositivo] ([Id])
)
