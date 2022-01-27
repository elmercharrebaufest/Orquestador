CREATE TABLE [dbo].[ConfigCartelLed] (
    [Id]             INT NOT NULL,
    [DireccionIp]    NVARCHAR (50) NOT NULL,
    [Puerto]         INT NOT NULL,
	[TimeoutLectura] INT NOT NULL DEFAULT 0,
    [LongFrase]    INT   NOT NULL DEFAULT 0,
	[VelocidadScroll] INT NOT NULL DEFAULT 0,
	[Tipografia] INT NOT NULL DEFAULT 0,
	[ControlBrillo] INT NOT NULL DEFAULT 0,
	[Efecto] INT NOT NULL DEFAULT 0,
    [NumeroTrama] NVARCHAR (2),
    [NumeroVariable] NVARCHAR (2),
    [NumeroPrograma] NVARCHAR (2),
    CONSTRAINT [PK_dbo.ConfigCartelLed] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_dbo.ConfigCartelLed_dbo.ConfigId] FOREIGN KEY ([Id]) REFERENCES [dbo].[ConfigDispositivo] ([Id])
);


GO
