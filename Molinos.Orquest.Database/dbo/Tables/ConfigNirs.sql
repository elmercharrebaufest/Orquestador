CREATE TABLE [dbo].[ConfigNirs] (
    [Id]             INT NOT NULL,
    [DireccionIp]    NVARCHAR (50) NOT NULL,
    [Puerto]         INT NOT NULL,
    [TimeoutLectura] INT NOT NULL, 
    [CaracteresABorrar]    NVARCHAR (20) NULL,
    CONSTRAINT [PK_dbo.ConfigNirs] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_dbo.ConfigNirs_dbo.ConfigId] FOREIGN KEY ([Id]) REFERENCES [dbo].[ConfigDispositivo] ([Id])
);


GO
