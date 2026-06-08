CREATE TABLE [dbo].[SuscripcionIdentificacionVehicular] (
    [Id] INT IDENTITY (1, 1) NOT NULL,
    [ConfigIdentificacionVehicular_Id] INT NOT NULL,
    [CodigoEvento] NVARCHAR (200) NOT NULL,
    [RutaAccesoSuscriptor] NVARCHAR (500) NOT NULL,
    CONSTRAINT [PK_dbo.SuscripcionIdentificacionVehicular] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_dbo.SuscripcionIdentificacionVehicular_dbo.ConfigIdentificacionVehicular_Id] FOREIGN KEY ([ConfigIdentificacionVehicular_Id]) REFERENCES [dbo].[ConfigIdentificacionVehicular] ([Id]),
    CONSTRAINT [UK_dbo.SuscripcionIdentificacionVehicular] UNIQUE ([ConfigIdentificacionVehicular_Id], [CodigoEvento], [RutaAccesoSuscriptor])
);
