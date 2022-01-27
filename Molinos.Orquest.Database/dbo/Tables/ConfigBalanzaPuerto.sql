CREATE TABLE [dbo].[ConfigBalanzaPuerto] (
    [Id]           INT            NOT NULL,
    [DireccionIp]  NVARCHAR (50) NOT NULL,
    [Puerto]       INT            NOT NULL,
    [PosDesde]     INT            NOT NULL,
    [PosHasta]    INT            NOT NULL,
    [TimeoutLectura] INT NOT NULL, 
	[ComandoConsulta] VARCHAR(2) NOT NULL, 
    [ComandoBorrado] VARCHAR(2) NOT NULL,
    [CantidadCaracteresTotal] INT NOT NULL, 
    [CaracterIzquierdaACompletar] VARCHAR(2) NOT NULL, 
    [IntervaloPolling] INT NOT NULL, 
    [LongFrase] INT NULL, 
    CONSTRAINT [PK_dbo.ConfigBalanzaPuerto] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_dbo.ConfigBalanzaPuerto_dbo.ConfigId] FOREIGN KEY ([Id]) REFERENCES [dbo].[ConfigDispositivo] ([Id])
);
GO


