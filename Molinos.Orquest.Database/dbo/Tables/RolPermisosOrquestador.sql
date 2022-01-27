CREATE TABLE [dbo].[RolPermisoOrquestador] (
	[Id]                         INT            IDENTITY (1, 1) NOT NULL,
    [Rol_Id]      INT NOT NULL,
    [PermisoOrquestador] INT NOT NULL,
    CONSTRAINT [PK_dbo.RolPermisoOrquestador] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_dbo.RolPermisoOrquestador_dbo.Rol_Rol_Id] FOREIGN KEY ([Rol_Id]) REFERENCES [dbo].[Rol] ([Id]) ON DELETE CASCADE,
);


GO
CREATE NONCLUSTERED INDEX [IX_Rol_Id]
    ON [dbo].[RolPermisoOrquestador]([Rol_Id] ASC);
