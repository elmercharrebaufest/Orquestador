CREATE TABLE [dbo].[ConfigDispositivo]
(
	[Id]  INT  NOT NULL,
	[ClaseDriver]  NVARCHAR (255) NOT NULL,
	CONSTRAINT [PK_dbo.ConfigDispositivo] PRIMARY KEY CLUSTERED ([Id] ASC),
	CONSTRAINT [FK_dbo.ConfigDispositivo_dbo.Dispositivo_Id] FOREIGN KEY ([Id]) REFERENCES [dbo].[Dispositivo] ([Id]), 
)
