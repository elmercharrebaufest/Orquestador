CREATE TABLE [dbo].[Suscripcion] (
    [Id] INT IDENTITY (1, 1) NOT NULL,
    [Dispositivo_Id] INT  NOT NULL,
    [CodigoEvento] NVARCHAR (100)  NOT NULL,
    [RutaAccesoSuscriptor] NVARCHAR (255)  NOT NULL,
    [Cantidad] INT NOT NULL, 
    [UltimaSuscripcion] DATETIME NULL, 
    [Persistente] BIT NOT NULL DEFAULT 0, 
    CONSTRAINT [PK_dbo.Suscripcion] PRIMARY KEY CLUSTERED ([Id] ASC),
	CONSTRAINT [FK_dbo.Suscripcion_dbo.Dispositivo_Id] FOREIGN KEY ([Dispositivo_Id]) REFERENCES [dbo].[Dispositivo] ([Id]),
	CONSTRAINT [UK_dbo.Suscripcion] UNIQUE ([Dispositivo_Id], [CodigoEvento], [RutaAccesoSuscriptor])  
);
