CREATE TABLE [dbo].[ConfigImpresoraHasar] (
    [Id]             INT NOT NULL,
    [DireccionIp]    NVARCHAR (50) NOT NULL,
    [Puerto]         INT NOT NULL,
    [TimeoutLectura] INT NOT NULL, 
    CONSTRAINT [PK_dbo.ConfigImpresoraHasar] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_dbo.ConfigImpresoraHasar_dbo.ConfigId] FOREIGN KEY ([Id]) REFERENCES [dbo].[ConfigDispositivo] ([Id])
);
GO
