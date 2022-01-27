CREATE TABLE [dbo].[ConfigCabezal] (
    [Id]           INT            NOT NULL,
    [DireccionIp]  NVARCHAR (50) NOT NULL,
    [Puerto]       INT            NOT NULL,
    [PosDesde]     INT            NOT NULL,
    [PosHasta]    INT            NOT NULL,
    [LongFrase]    INT            NOT NULL,
	[CarInicioFrase] VARCHAR(10) NOT NULL,
    [ComandoPeso]  VARCHAR(2) NOT NULL,
    [ComandoCereo] VARCHAR(2) NOT NULL,
    [CantLecPesoEstable] INT NOT NULL,
	[MaxCantLecPesoEstable] INT NOT NULL,  
    [IntLecPesoEstable] INT NOT NULL, 
    [IntLecCereo] INT NOT NULL, 
    [TimeoutLectura] INT NOT NULL, 
    [DigitosDecimales] INT NOT NULL DEFAULT 0, 
    CONSTRAINT [PK_dbo.ConfigCabezal] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_dbo.ConfigCabezal_dbo.ConfigId] FOREIGN KEY ([Id]) REFERENCES [dbo].[ConfigDispositivo] ([Id])
);
GO


