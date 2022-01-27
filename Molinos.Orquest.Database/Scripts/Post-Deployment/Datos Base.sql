/*
Post-Deployment Script Template							
--------------------------------------------------------------------------------------
 This file contains SQL statements that will be appended to the build script.		
 Use SQLCMD syntax to include a file in the post-deployment script.			
 Example:      :r .\myfile.sql								
 Use SQLCMD syntax to reference a variable in the post-deployment script.		
 Example:      :setvar TableName MyTable							
               SELECT * FROM [$(TableName)]					
--------------------------------------------------------------------------------------
*/

-- Rol
if not exists(select 1 from rol where descripcion = 'Administrador General') begin insert into Rol(Descripcion) values ('Administrador General') end;

IF NOT EXISTS (select 1 from RolPermisoOrquestador where PermisoOrquestador = 1 and Rol_Id = (select id from rol where descripcion = 'Administrador General')) BEGIN INSERT INTO RolPermisoOrquestador([Rol_Id],[PermisoOrquestador]) VALUES ((select id from rol where descripcion = 'Administrador General'), 1); END
IF NOT EXISTS (select 1 from RolPermisoOrquestador where PermisoOrquestador = 2 and Rol_Id = (select id from rol where descripcion = 'Administrador General')) BEGIN INSERT INTO RolPermisoOrquestador([Rol_Id],[PermisoOrquestador]) VALUES ((select id from rol where descripcion = 'Administrador General'), 2); END
IF NOT EXISTS (select 1 from RolPermisoOrquestador where PermisoOrquestador = 3 and Rol_Id = (select id from rol where descripcion = 'Administrador General')) BEGIN INSERT INTO RolPermisoOrquestador([Rol_Id],[PermisoOrquestador]) VALUES ((select id from rol where descripcion = 'Administrador General'), 3); END
IF NOT EXISTS (select 1 from RolPermisoOrquestador where PermisoOrquestador = 4 and Rol_Id = (select id from rol where descripcion = 'Administrador General')) BEGIN INSERT INTO RolPermisoOrquestador([Rol_Id],[PermisoOrquestador]) VALUES ((select id from rol where descripcion = 'Administrador General'), 4); END
IF NOT EXISTS (select 1 from RolPermisoOrquestador where PermisoOrquestador = 5 and Rol_Id = (select id from rol where descripcion = 'Administrador General')) BEGIN INSERT INTO RolPermisoOrquestador([Rol_Id],[PermisoOrquestador]) VALUES ((select id from rol where descripcion = 'Administrador General'), 5); END
IF NOT EXISTS (select 1 from RolPermisoOrquestador where PermisoOrquestador = 6 and Rol_Id = (select id from rol where descripcion = 'Administrador General')) BEGIN INSERT INTO RolPermisoOrquestador([Rol_Id],[PermisoOrquestador]) VALUES ((select id from rol where descripcion = 'Administrador General'), 6); END
IF NOT EXISTS (select 1 from RolPermisoOrquestador where PermisoOrquestador = 7 and Rol_Id = (select id from rol where descripcion = 'Administrador General')) BEGIN INSERT INTO RolPermisoOrquestador([Rol_Id],[PermisoOrquestador]) VALUES ((select id from rol where descripcion = 'Administrador General'), 7); END
IF NOT EXISTS (select 1 from RolPermisoOrquestador where PermisoOrquestador = 8 and Rol_Id = (select id from rol where descripcion = 'Administrador General')) BEGIN INSERT INTO RolPermisoOrquestador([Rol_Id],[PermisoOrquestador]) VALUES ((select id from rol where descripcion = 'Administrador General'), 8); END
IF NOT EXISTS (select 1 from RolPermisoOrquestador where PermisoOrquestador = 9 and Rol_Id = (select id from rol where descripcion = 'Administrador General')) BEGIN INSERT INTO RolPermisoOrquestador([Rol_Id],[PermisoOrquestador]) VALUES ((select id from rol where descripcion = 'Administrador General'), 9); END
IF NOT EXISTS (select 1 from RolPermisoOrquestador where PermisoOrquestador = 10 and Rol_Id = (select id from rol where descripcion = 'Administrador General')) BEGIN INSERT INTO RolPermisoOrquestador([Rol_Id],[PermisoOrquestador]) VALUES ((select id from rol where descripcion = 'Administrador General'), 10); END
IF NOT EXISTS (select 1 from RolPermisoOrquestador where PermisoOrquestador = 11 and Rol_Id = (select id from rol where descripcion = 'Administrador General')) BEGIN INSERT INTO RolPermisoOrquestador([Rol_Id],[PermisoOrquestador]) VALUES ((select id from rol where descripcion = 'Administrador General'), 11); END
IF NOT EXISTS (select 1 from RolPermisoOrquestador where PermisoOrquestador = 12 and Rol_Id = (select id from rol where descripcion = 'Administrador General')) BEGIN INSERT INTO RolPermisoOrquestador([Rol_Id],[PermisoOrquestador]) VALUES ((select id from rol where descripcion = 'Administrador General'), 12); END
IF NOT EXISTS (select 1 from RolPermisoOrquestador where PermisoOrquestador = 13 and Rol_Id = (select id from rol where descripcion = 'Administrador General')) BEGIN INSERT INTO RolPermisoOrquestador([Rol_Id],[PermisoOrquestador]) VALUES ((select id from rol where descripcion = 'Administrador General'), 13); END
IF NOT EXISTS (select 1 from RolPermisoOrquestador where PermisoOrquestador = 14 and Rol_Id = (select id from rol where descripcion = 'Administrador General')) BEGIN INSERT INTO RolPermisoOrquestador([Rol_Id],[PermisoOrquestador]) VALUES ((select id from rol where descripcion = 'Administrador General'), 14); END
IF NOT EXISTS (select 1 from RolPermisoOrquestador where PermisoOrquestador = 15 and Rol_Id = (select id from rol where descripcion = 'Administrador General')) BEGIN INSERT INTO RolPermisoOrquestador([Rol_Id],[PermisoOrquestador]) VALUES ((select id from rol where descripcion = 'Administrador General'), 15); END
IF NOT EXISTS (select 1 from RolPermisoOrquestador where PermisoOrquestador = 16 and Rol_Id = (select id from rol where descripcion = 'Administrador General')) BEGIN INSERT INTO RolPermisoOrquestador([Rol_Id],[PermisoOrquestador]) VALUES ((select id from rol where descripcion = 'Administrador General'), 16); END
IF NOT EXISTS (select 1 from RolPermisoOrquestador where PermisoOrquestador = 17 and Rol_Id = (select id from rol where descripcion = 'Administrador General')) BEGIN INSERT INTO RolPermisoOrquestador([Rol_Id],[PermisoOrquestador]) VALUES ((select id from rol where descripcion = 'Administrador General'), 17); END
IF NOT EXISTS (select 1 from RolPermisoOrquestador where PermisoOrquestador = 18 and Rol_Id = (select id from rol where descripcion = 'Administrador General')) BEGIN INSERT INTO RolPermisoOrquestador([Rol_Id],[PermisoOrquestador]) VALUES ((select id from rol where descripcion = 'Administrador General'), 18); END