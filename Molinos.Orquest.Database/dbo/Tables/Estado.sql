CREATE TABLE [dbo].[Estado]
(
	[Id] INT IDENTITY (1, 1) NOT NULL, 
    [Fecha] DateTime NULL
    CONSTRAINT [PK_dbo.Estado] PRIMARY KEY CLUSTERED ([Id] ASC),
); 

GO