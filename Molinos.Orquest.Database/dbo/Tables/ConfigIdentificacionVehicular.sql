CREATE TABLE [dbo].[ConfigIdentificacionVehicular]
(
    [Id]                        INT             NOT NULL IDENTITY(1,1),
    [Nombre]                    NVARCHAR(100)   NOT NULL,
    [Codigo]                    NVARCHAR(50)    NOT NULL,
    [ConfigLectorTarjetas_Id]   INT             NULL,
    [ConfigSensorVehicular_Id]  INT             NULL,
    [ConfigSensorPresencia_Id]  INT             NULL,
    [MaxReintentosFoto]         INT             NOT NULL DEFAULT 3,
    [DelayEntreReintentosMs]    INT             NOT NULL DEFAULT 500,
    [Activo]                    BIT             NOT NULL DEFAULT 1,
    [TomadoPor_Id]              INT             NULL,
    CONSTRAINT [PK_dbo.ConfigIdentificacionVehicular] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_ConfigIdentificacionVehicular_ConfigLectorTarjetas] FOREIGN KEY ([ConfigLectorTarjetas_Id])
        REFERENCES [dbo].[ConfigLectorTarjetas] ([Id]),
    CONSTRAINT [FK_ConfigIdentificacionVehicular_ConfigSensorVehicular] FOREIGN KEY ([ConfigSensorVehicular_Id])
        REFERENCES [dbo].[ConfigSensor] ([Id]),
    CONSTRAINT [FK_ConfigIdentificacionVehicular_ConfigSensorPresencia] FOREIGN KEY ([ConfigSensorPresencia_Id])
        REFERENCES [dbo].[ConfigSensor] ([Id]),
    CONSTRAINT [FK_ConfigIdentificacionVehicular_Orquestador] FOREIGN KEY ([TomadoPor_Id])
        REFERENCES [dbo].[Orquestador] ([Id])
)
