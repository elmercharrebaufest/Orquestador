CREATE TABLE [dbo].[ConfigPuestoDeVianda] (
    [Id]           INT            NOT NULL,
    [Sector] NVARCHAR(50) NOT NULL, 
    CONSTRAINT [PK_dbo.ConfigPuestoDeVianda] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_dbo.ConfigPuestoDeVianda_dbo.ConfigId] FOREIGN KEY ([Id]) REFERENCES [dbo].[ConfigDispositivo] ([Id])
);
GO


