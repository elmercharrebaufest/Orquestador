CREATE TABLE [dbo].[UsuarioRol] (
    [Usuario_Id]      INT NOT NULL,
    [Rol_Id] INT NOT NULL,
    CONSTRAINT [PK_dbo.UsuarioRol] PRIMARY KEY CLUSTERED ([Usuario_Id] ASC, [Rol_Id] ASC),
    CONSTRAINT [FK_dbo.UsuarioRol_dbo.Usuario_Usuario_Id] FOREIGN KEY ([Usuario_Id]) REFERENCES [dbo].[Usuario] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_dbo.UsuarioRol_dbo.Rol_Rol_Id] FOREIGN KEY ([Rol_Id]) REFERENCES [dbo].[Rol] ([Id]) ON DELETE CASCADE
);


GO
CREATE NONCLUSTERED INDEX [IX_Usuario_Id]
    ON [dbo].[UsuarioRol]([Usuario_Id] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_Rol_Id]
    ON [dbo].[UsuarioRol]([Rol_Id] ASC);

