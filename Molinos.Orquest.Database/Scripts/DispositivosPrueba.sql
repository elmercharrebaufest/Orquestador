IF NOT EXISTS (SELECT 1 FROM dbo.Dispositivo WHERE Codigo = 'BALDM01')
BEGIN

	INSERT INTO dbo.Dispositivo (Codigo, Descripcion)
	VALUES ('BALDM01', 'Cabezal Dummy')

	INSERT INTO dbo.[ConfigDispositivo](Id, ClaseDriver)
	VALUES ((SELECT Id from dbo.Dispositivo WHERE Codigo = 'BALDM01'), 'Molinos.Orquest.DriversImpl.DriverCabezalIPDummy, Molinos.Orquest.DriversImpl')

	INSERT INTO dbo.[ConfigCabezal](Id, DireccionIp, Puerto, PosDesde, PosHasta, LongFrase, CarInicioFrase, ComandoPeso, ComandoCereo, CantLecPesoEstable, MaxCantLecPesoEstable, IntLecPesoEstable, IntLecCereo, TimeoutLectura)
	VALUES ((SELECT c.Id from dbo.Dispositivo d INNER JOIN dbo.ConfigDispositivo c ON d.Id = c.Id WHERE d.Codigo = 'BALDM01'), '10.10.10.1', 111, 0, 0, 1, CHAR(2),'P', 'Z', 3, 20, 10, 10, 5000)

END

IF NOT EXISTS (SELECT 1 FROM dbo.Dispositivo WHERE Codigo = 'BALEM01')
BEGIN

	INSERT INTO dbo.Dispositivo (Codigo, Descripcion)
	VALUES ('BALEM01', 'Cabezal Demanda Emulador')

	INSERT INTO dbo.[ConfigDispositivo](Id, ClaseDriver)
	VALUES ((SELECT Id from dbo.Dispositivo WHERE Codigo = 'BALEM01'), 'Molinos.Orquest.DriversImpl.DriverCabezalIPPorDemanda, Molinos.Orquest.DriversImpl')

	INSERT INTO dbo.[ConfigCabezal](Id, DireccionIp, Puerto, PosDesde, PosHasta, LongFrase, CarInicioFrase, ComandoPeso, ComandoCereo, CantLecPesoEstable, MaxCantLecPesoEstable, IntLecPesoEstable, IntLecCereo, TimeoutLectura)
	VALUES ((SELECT c.Id from dbo.Dispositivo d INNER JOIN dbo.ConfigDispositivo c ON d.Id = c.Id WHERE d.Codigo = 'BALEM01'), '192.168.100.135', 1470, 1, 8, 30, CHAR(2),'P', 'Z', 3, 20, 10, 10, 5000)

END

IF NOT EXISTS (SELECT 1 FROM dbo.Dispositivo WHERE Codigo = 'BALEM02')
BEGIN

	INSERT INTO dbo.Dispositivo (Codigo, Descripcion)
	VALUES ('BALEM02', 'Cabezal Continuo Emulador')

	INSERT INTO dbo.[ConfigDispositivo](Id, ClaseDriver)
	VALUES ((SELECT Id from dbo.Dispositivo WHERE Codigo = 'BALEM02'), 'Molinos.Orquest.DriversImpl.DriverCabezalIPContinuo, Molinos.Orquest.DriversImpl')

	INSERT INTO dbo.[ConfigCabezal](Id, DireccionIp, Puerto, PosDesde, PosHasta, LongFrase, CarInicioFrase, ComandoPeso, ComandoCereo, CantLecPesoEstable, MaxCantLecPesoEstable, IntLecPesoEstable, IntLecCereo, TimeoutLectura)
	VALUES ((SELECT c.Id from dbo.Dispositivo d INNER JOIN dbo.ConfigDispositivo c ON d.Id = c.Id WHERE d.Codigo = 'BALEM02'), '192.168.100.135', 1470, 5, 10, 16, CHAR(2),'P', 'Z', 3, 20, 10, 10, 5000)

END

IF NOT EXISTS (SELECT 1 FROM dbo.Dispositivo WHERE Codigo = 'HUMDM01')
BEGIN

	INSERT INTO dbo.Dispositivo (Codigo, Descripcion)
	VALUES ('HUMDM01', 'Humedímetro Dummy')

	INSERT INTO dbo.[ConfigDispositivo](Id, ClaseDriver)
	VALUES ((SELECT Id from dbo.Dispositivo WHERE Codigo = 'HUMDM01'), 'Molinos.Orquest.DriversImpl.DriverHumedimetroDummy, Molinos.Orquest.DriversImpl')

	INSERT INTO dbo.[ConfigHumedimetro](Id, DireccionIp, Puerto, ComandoHumedad, LongFrase, TimeoutLectura, DelimitadorCampos, PosicionCampoHumedad)
	VALUES ((SELECT c.Id from dbo.Dispositivo d INNER JOIN dbo.ConfigDispositivo c ON d.Id = c.Id WHERE d.Codigo = 'HUMDM01'), '10.10.10.1', 1470, 'H', 113, 10000, ',', 3)

END

IF NOT EXISTS (SELECT 1 FROM dbo.Dispositivo WHERE Codigo = 'HUMDM02')
BEGIN

	INSERT INTO dbo.Dispositivo (Codigo, Descripcion)
	VALUES ('HUMDM02', 'Humedímetro Dummy por Eventos')

	INSERT INTO dbo.[ConfigDispositivo](Id, ClaseDriver)
	VALUES ((SELECT Id from dbo.Dispositivo WHERE Codigo = 'HUMDM02'), 'Molinos.Orquest.DriversImpl.DriverHumedimetroDummyPorEventos, Molinos.Orquest.DriversImpl')

	INSERT INTO dbo.[ConfigHumedimetro](Id, DireccionIp, Puerto, ComandoHumedad, LongFrase, TimeoutLectura, DelimitadorCampos, PosicionCampoHumedad)
	VALUES ((SELECT c.Id from dbo.Dispositivo d INNER JOIN dbo.ConfigDispositivo c ON d.Id = c.Id WHERE d.Codigo = 'HUMDM02'), '10.10.10.1', 1470, 'H', 113, 10000, ',', 3)

END

IF NOT EXISTS (SELECT 1 FROM dbo.Dispositivo WHERE Codigo = 'HUMEM01')
BEGIN

	INSERT INTO dbo.Dispositivo (Codigo, Descripcion)
	VALUES ('HUMEM01', 'Humedímetro Emulador')

	INSERT INTO dbo.[ConfigDispositivo](Id, ClaseDriver)
	VALUES ((SELECT Id from dbo.Dispositivo WHERE Codigo = 'HUMEM01'), 'Molinos.Orquest.DriversImpl.DriverHumedimetro, Molinos.Orquest.DriversImpl')

	INSERT INTO dbo.[ConfigHumedimetro](Id, DireccionIp, Puerto, ComandoHumedad, LongFrase, TimeoutLectura, DelimitadorCampos, PosicionCampoHumedad)
	VALUES ((SELECT c.Id from dbo.Dispositivo d INNER JOIN dbo.ConfigDispositivo c ON d.Id = c.Id WHERE d.Codigo = 'HUMEM01'), '192.168.100.135', 1470, 'H', 113, 10000, ',', 3)

END

IF NOT EXISTS (SELECT 1 FROM dbo.Dispositivo WHERE Codigo = 'HUMEM02')
BEGIN

	INSERT INTO dbo.Dispositivo (Codigo, Descripcion)
	VALUES ('HUMEM02', 'Humedímetro Emulador con Eventos')

	INSERT INTO dbo.[ConfigDispositivo](Id, ClaseDriver)
	VALUES ((SELECT Id from dbo.Dispositivo WHERE Codigo = 'HUMEM02'), 'Molinos.Orquest.DriversImpl.DriverHumedimetroPorEventos, Molinos.Orquest.DriversImpl')

	INSERT INTO dbo.[ConfigHumedimetro](Id, DireccionIp, Puerto, ComandoHumedad, LongFrase, TimeoutLectura, DelimitadorCampos, PosicionCampoHumedad)
	VALUES ((SELECT c.Id from dbo.Dispositivo d INNER JOIN dbo.ConfigDispositivo c ON d.Id = c.Id WHERE d.Codigo = 'HUMEM02'), '192.168.100.135', 1470, 'H', 113, 1000000, ',', 3)

END

-- ITC

IF NOT EXISTS (SELECT 1 FROM dbo.Dispositivo WHERE Codigo = 'ITCEM01')
BEGIN

	INSERT INTO dbo.Dispositivo (Codigo, Descripcion, EsConcentrador)
	VALUES ('ITCEM01', 'ITC Emulador', 1)

	INSERT INTO dbo.[ConfigDispositivo](Id, ClaseDriver)
	VALUES ((SELECT Id from dbo.Dispositivo WHERE Codigo = 'ITCEM01'), 'Molinos.Orquest.DriversImpl.DriverItc30, Molinos.Orquest.DriversImpl')

	INSERT INTO dbo.[ConfigItc](Id, DireccionIp, Puerto, LongFrase, TimeoutLectura, IntervaloPolling, CarInicioFrase, CarFinFrase, DelimitadorCampos, ComandoEstado, ComandoTarjeta, ComandoActivarSalida, RespuestaExito, RespuestaError)
	VALUES ((SELECT c.Id from dbo.Dispositivo d INNER JOIN dbo.ConfigDispositivo c ON d.Id = c.Id WHERE d.Codigo = 'ITCEM01'), '192.168.100.117', 1470, 50, 10000, 1000, '<', '>', ' ', 'R', 'U', 'S', 'A', 'F')
END

IF NOT EXISTS (SELECT 1 FROM dbo.Dispositivo WHERE Codigo = 'BAREM01')
BEGIN

	INSERT INTO dbo.Dispositivo (Codigo, Descripcion, Concentrador_Id)
	VALUES ('BAREM01', 'Barrera ITC', (SELECT Id from dbo.Dispositivo WHERE Codigo = 'ITCEM01'))

	INSERT INTO dbo.[ConfigDispositivo](Id, ClaseDriver)
	VALUES ((SELECT Id from dbo.Dispositivo WHERE Codigo = 'BAREM01'), 'Molinos.Orquest.DriversImpl.DriverBarreraItc, Molinos.Orquest.DriversImpl')

	INSERT INTO dbo.[ConfigBarrera](Id, NumeroSalida, EstadoAbierta, TiempoActivacion)
	VALUES ((SELECT c.Id from dbo.Dispositivo d INNER JOIN dbo.ConfigDispositivo c ON d.Id = c.Id WHERE d.Codigo = 'BAREM01'), 1, 1, 20)

END

IF NOT EXISTS (SELECT 1 FROM dbo.Dispositivo WHERE Codigo = 'LECEM01')
BEGIN

	INSERT INTO dbo.Dispositivo (Codigo, Descripcion, Concentrador_Id)
	VALUES ('LECEM01', 'Lector Tarjetas ITC', (SELECT Id from dbo.Dispositivo WHERE Codigo = 'ITCEM01'))

	INSERT INTO dbo.[ConfigDispositivo](Id, ClaseDriver)
	VALUES ((SELECT Id from dbo.Dispositivo WHERE Codigo = 'LECEM01'), 'Molinos.Orquest.DriversImpl.DriverLectorTarjetasItc, Molinos.Orquest.DriversImpl')

	INSERT INTO dbo.[ConfigLectorTarjetas](Id, Lector)
	VALUES ((SELECT c.Id from dbo.Dispositivo d INNER JOIN dbo.ConfigDispositivo c ON d.Id = c.Id WHERE d.Codigo = 'LECEM01'), '1')

END