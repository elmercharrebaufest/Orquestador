CREATE TABLE [dbo].[Usuario] (
    [Id]                         INT            IDENTITY (1, 1) NOT NULL,
    [NombreUsuario]                NVARCHAR (40)  NOT NULL,
    [Apellido]               NVARCHAR (40)  NOT NULL,
    [Nombre]               NVARCHAR (40)  NOT NULL
    CONSTRAINT [PK_dbo.Usuario] PRIMARY KEY CLUSTERED ([Id] ASC),
	CONSTRAINT [UK_dbo.Usuario] UNIQUE ([NombreUsuario])
);

GO
