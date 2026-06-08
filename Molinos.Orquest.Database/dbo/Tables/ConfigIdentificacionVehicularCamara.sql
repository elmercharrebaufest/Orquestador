CREATE TABLE [dbo].[ConfigIdentificacionVehicularCamara]
(
    [Id]                                INT     NOT NULL IDENTITY(1,1),
    [ConfigIdentificacionVehicular_Id]  INT     NOT NULL,
    [ConfigCamara_Id]                   INT     NOT NULL,
    CONSTRAINT [PK_dbo.ConfigIdentificacionVehicularCamara] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_ConfigIdentificacionVehicularCamara_ConfigIdentificacionVehicular] FOREIGN KEY ([ConfigIdentificacionVehicular_Id])
        REFERENCES [dbo].[ConfigIdentificacionVehicular] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_ConfigIdentificacionVehicularCamara_ConfigCamara] FOREIGN KEY ([ConfigCamara_Id])
        REFERENCES [dbo].[ConfigCamara] ([Id])
)
