CREATE TABLE [dbo].[Rol] (
    [Id]                         INT            IDENTITY (1, 1) NOT NULL,
    [Descripcion]                NVARCHAR (40)  NOT NULL,
    CONSTRAINT [PK_dbo.Rol] PRIMARY KEY CLUSTERED ([Id] ASC),
);

GO