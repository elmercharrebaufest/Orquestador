CREATE TABLE [dbo].[Orquestador] (
    [Id]            INT            IDENTITY (1, 1) NOT NULL,
    [NombreMaquina] NVARCHAR (255) NOT NULL,
    [RutaAcceso]    NVARCHAR (255) NOT NULL,
    CONSTRAINT [PK_dbo.Orquestador] PRIMARY KEY CLUSTERED ([Id] ASC)
);

