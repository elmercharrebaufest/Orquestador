CREATE TABLE [dbo].[ConfigItc]
(
	[Id]             INT NOT NULL,
    [DireccionIp]    NVARCHAR (1000) NOT NULL,
    [Puerto]         INT NOT NULL,
	[LongFrase]    INT NOT NULL,
    [TimeoutLectura] INT NOT NULL, 
	[IntervaloPolling] INT NOT NULL, 
    [ComandoEstado] CHAR(1) NOT NULL, 
	[ComandoTarjeta] CHAR(1) NOT NULL,
	[ComandoActivarSalida] CHAR(1) NOT NULL,
	[CarInicioFrase] CHAR(1) NOT NULL,
	[CarFinFrase] CHAR(1) NOT NULL,
    [DelimitadorCampos] CHAR(1) NOT NULL,
	[RespuestaExito] CHAR(1) NOT NULL,
	[RespuestaError] CHAR(1) NOT NULL,
    CONSTRAINT [PK_dbo.ConfigItc] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_dbo.ConfigItc_dbo.ConfigId] FOREIGN KEY ([Id]) REFERENCES [dbo].[ConfigDispositivo] ([Id])
)
