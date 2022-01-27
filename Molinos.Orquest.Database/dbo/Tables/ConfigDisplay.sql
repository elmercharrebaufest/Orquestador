CREATE TABLE [dbo].[ConfigDisplay]
(
	[Id]             INT NOT NULL,
    [NumeroSalida]    INT NOT NULL,
    [TiempoActivacion]    INT NOT NULL,
    [Url] NVARCHAR(100) NOT NULL,
    CONSTRAINT [PK_dbo.ConfigDisplay] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_dbo.ConfigDisplay_dbo.ConfigId] FOREIGN KEY ([Id]) REFERENCES [dbo].[ConfigDispositivo] ([Id])
)
