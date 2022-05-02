CREATE TABLE [dbo].[Dispositivo] (
    [Id]           INT            IDENTITY (1, 1) NOT NULL,
    [Codigo]       NVARCHAR (50) NOT NULL,
    [Descripcion]  NVARCHAR (255) NULL,
    [TomadoPor_Id] INT            NULL,
	[Activo] BIT NOT NULL DEFAULT 1, 
    [Concentrador_Id] INT NULL, 
    [EsConcentrador] BIT NOT NULL DEFAULT 0, 
	[EstadoCorrecto] BIT NOT NULL DEFAULT 0, 
    [ServerFijo] NVARCHAR (255) NULL,
    CONSTRAINT [PK_dbo.Dispositivo] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_dbo.Dispositivo_dbo.Orquestador_TomadoPor_Id] FOREIGN KEY ([TomadoPor_Id]) REFERENCES [dbo].[Orquestador] ([Id]),
	CONSTRAINT [FK_dbo.Dispositivo_dbo.Dispositivo_Concentrador_Id] FOREIGN KEY ([Concentrador_Id]) REFERENCES [dbo].[Dispositivo] ([Id]), 
    CONSTRAINT [UK_dbo.Dispositivo_Codigo] UNIQUE ([Codigo]), 

);

GO
CREATE NONCLUSTERED INDEX ndx_Codigo_TomadoPor_Id_Activo
ON [dbo].[Dispositivo] (Codigo,TomadoPor_Id,Activo)
INCLUDE (Id)
GO
