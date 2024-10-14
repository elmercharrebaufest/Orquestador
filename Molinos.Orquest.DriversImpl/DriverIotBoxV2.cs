using Molinos.Orquest.Dominio.Dtos;
using Molinos.Orquest.Dominio.Entidades;
using Molinos.Orquest.Dominio.Resultados;
using Molinos.Orquest.Drivers;
using Molinos.Orquest.DriversImpl.Helpers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Molinos.Orquest.DriversImpl
{
	public class DriverIotBoxV2 : DriverBase, IDriverItc
	{
		private bool dispositivoActivo = false;
		private string codigoRasp;
		private ConfigItc config;
		private bool conectado;
		private bool pingOK;
		private TcpCommandClient cliente;
		private bool consultarEstado = true;
		private int delayReconexion;
		private int delayPing;

		private readonly ManualResetEvent finCiclo = new ManualResetEvent(false);

		private bool? falloUltimaConexion;
		private Exception errorUltimaConexion;
		private readonly List<string> eventosSoportados = new List<string> { CodigosEventos.EntradaActivada, CodigosEventos.ErrorConexionDispositivo };
		private readonly IConfiguracionGeneral configuracionGeneral;

		public DriverIotBoxV2(IConfiguracionGeneral configuracionGeneral)
		{
			this.configuracionGeneral = configuracionGeneral;
		}

		public override bool MantenerConectado()
		{
			return true;
		}

		public override IEnumerable<string> EventosSoportados
		{
			get { return eventosSoportados; }
		}

		public override Type TipoDispositivo
		{
			get { return typeof(ConfigItc); }
		}

		public override void Inicializar(string codigo, ConfigDispositivo configuracion)
		{
			codigoRasp = codigo;
			config = (ConfigItc)configuracion;
			dispositivoActivo = true;
			pingOK = true;
			cliente = new TcpCommandClient(config.DireccionIp, config.Puerto, config.LongFrase, config.TimeoutLectura, Log, false);
			conectado = cliente.Conectado;
			delayReconexion = configuracionGeneral.TiempoReintentoReconexion;
			delayPing = configuracionGeneral.TiempoPing;

			Log.Debug("Iniciando Driver de IotBox {0}", codigo);
			// 2 hilos separados, uno consulta con ping y pingresponse y el otro es lectura
			// envio de ping desde el dispositivo fisico y lifetime, si no responde se reconecta teniendo en cuenta el lifetime
			// cada dispositivo tiene su propio lifetime

			Task.Run(async () => await EjecutarCicloDeConsulta());
		}

		private async Task EjecutarCicloDeConsulta()
		{
			while (dispositivoActivo)
			{
				// Si no funciona usar variable con tiempo actual y vuelva a correr 1 segundo despues			
				try
				{
					await ConsultarEstado();
					//Cuando no hay estado anterior se lanza el evento
					if (!falloUltimaConexion.HasValue || falloUltimaConexion.Value)
					{
						//Log.Debug("Conexion reestablecida con la Rasp {0}", codigoRasp);
						Log.Info("Nueva Conexión a Rasp={0}", codigoRasp);
						NotificarEstadoConexion(CodigosEventos.ConexionDispositivoCorrecta);
						falloUltimaConexion = false;
						errorUltimaConexion = null;
					}
				}
				catch (Exception e)
				{
					Log.Error(e, "Error al ConsultarEstado del Rasp {0}", codigoRasp);
					//Cuando no hay estado anterior se lanza el evento
					if (!falloUltimaConexion.HasValue || !falloUltimaConexion.Value)
					{
						Log.Info("Desconexión de Rasp={0}", codigoRasp);
						NotificarEstadoConexion(CodigosEventos.ErrorConexionDispositivo, e);
						falloUltimaConexion = true;
						errorUltimaConexion = e;
					}
					if (dispositivoActivo)
					{
						Thread.Sleep(config.IntervaloPolling);
					}
				}
			}

			finCiclo.Set();
		}

		public override void VerificarDispositivo()
		{
			Log.Info($"ARSCT350-546 - VerificarDispositivo");
			if ((falloUltimaConexion ?? false))
			{
				throw new ConexionDispositivoDriverException("");
			}
		}

		private void NotificarEstadoConexion(string codigoEvento, Exception e = null)
		{
			try
			{
				var notification = new NotificacionEvento
				{
					CodigoDispositivo = codigoRasp,
					CodigoEvento = codigoEvento,
					Datos = e != null ? new Dictionary<string, string>
							{
								{"Error", e.Message},
								{"Detalle", e.StackTrace}
							} : new Dictionary<string, string>()
				};
				OnEventoDriver(new EventoDriverEventArgs { Notificacion = notification });
			}
			catch (Exception ex)
			{
				Log.Error(ex, "Rasp {0}: No se pudo notificar el evento {1}", codigoRasp, codigoEvento);
			}
		}

		private async Task ConsultarEstado()
		{
			List<EntradaDto> jsonResponse = null;
			try
			{
				// Envio de ping
				await EnviarPingPeriodicamente();

				// Lectura de Novedad
				string response;
				try
				{
					response = cliente.LeerNovedad();
				}
				catch (SocketException e)
				{
					Log.Debug($"Error de conexion al leer respuesta: {e.Message}. Intentando un nuevo ping.");
					await ReConectarSiEsNecesario();
					await EnviarPingPeriodicamente();
					response = cliente.LeerNovedad();
				}

				// Intento de parseo de respuesta
				try
				{
					if (response != null && response != "\"ok\"")
					{
						jsonResponse = ParseJsonData(response);

						// Reconexion en caso de que IsConnected devuelva false
						var connected = ConnectionHelper.IsConnected(cliente, pingOK, conectado);
						if (!connected)
						{
							Log.Info("Intentando reconectar con dispositivo. Dispositivo: {0}", codigoRasp);
							cliente.ReConectar();
							conectado = true;
						}
						else if (connected)
						{
							Log.Info("Ping respondido exitosamente.");
						}

						// Reconexion enviada desde el dispostivo
						if (jsonResponse.Any(x => x.Dato == "reconnectDevice"))
						{
							Log.Error($"ARSCT350-599: Reconexion enviada desde {codigoRasp}");
							cliente.ReConectar();
						}
					}
				}
				catch (Exception e)
				{
					//conectado = false;
					Log.Error(e, "Error al parsear respuesta");
				}
			}
			catch (Exception e) when (e.InnerException != null && (e.InnerException is SocketException) && ((SocketException)e.InnerException).ErrorCode == 10060)
			{
				Log.Debug(e, $"{codigoRasp} - Sin novedad");
			}
			catch (Exception e)
			{
				throw new DriverException("Error al Conectar con el dispositivo", e);
			}

			// Filtrar entradas que no sean pingResponse o socketconnected
			List<EntradaDto> entradasFiltradas = null;
			try
			{
				entradasFiltradas = jsonResponse?.Where(x => x.Dato != "ping" && x.Dato != "socketconnected")?.ToList(); // "pingResponse"

				if (entradasFiltradas != null && entradasFiltradas.Any())
				{
					foreach (var entrada in entradasFiltradas)
					{
						NotificarEventoEntrada(entrada.Numero, entrada.Dato, CodigosEventos.EntradaActivada);
					}
				}
			}
			catch (Exception e)
			{
				Log.Error(e, "Error al filtrar entradas o NotificarEventoEntrada para entradas");
			}
		}

		private async Task EnviarPingPeriodicamente()
		{
			try
			{
				ActivarSalida(0, "\"ping\"", "0", false);
			}
			catch
			{
				await ReConectarSiEsNecesario();
				ActivarSalida(0, "\"socketconnected\"", "0", false);
			}
			await Task.Delay(delayPing); // Espera 5 segundos antes de la siguiente ejecución
		}

		public List<EntradaDto> ParseJsonData(string jsonData)
		{
			var entradas = new List<EntradaDto>();

			if (string.IsNullOrWhiteSpace(jsonData))
			{
				return entradas; // Devuelve una lista vacía si jsonData es nulo o está vacío
			}

			jsonData = jsonData.Replace("[", ",").Replace("]", "");
			jsonData = ReplaceFirstCharacterWithBracket(jsonData);
			jsonData = AddBracketToEnd(jsonData);

			try
			{
				entradas = JsonConvert.DeserializeObject<List<EntradaDto>>(jsonData);
			}
			catch (JsonException ex)
			{
				// Manejo de errores en caso de que la deserialización falle
				Log.Error($"Error deserializando el JSON \n{ex.Message}");
			}

			return entradas;
		}

		private static string ReplaceFirstCharacterWithBracket(string input)
		{
			if (string.IsNullOrEmpty(input))
			{
				return "[";
			}

			return "[" + input.Substring(1);
		}

		private static string AddBracketToEnd(string input)
		{
			return input + "]";
		}

		private async Task ReConectarSiEsNecesario()
		{
			if (!cliente.Conectado)
			{
				Log.Info($"Intentando reconectar: {codigoRasp}");
				cliente.ReConectar();
				await Task.Delay(delayReconexion); // Espera 5 segundos antes de la siguiente verificación
			}
		}

		public void ActivarSalida(int salida, string estado, string dato, bool flush = false)
		{
			if (!dispositivoActivo)
			{
				return;
			}
			try
			{
				if (!cliente.Conectado)
				{
					Log.Info($"Reconexion desde ActivarSalida: {codigoRasp}");
					cliente.ReConectar();
				}
				if (consultarEstado)
				{
					consultarEstado = false;
					cliente.EnviarComando("[{\"Tipo\": \"salida\", \"Numero\" : " + salida.ToString(CultureInfo.InvariantCulture) +
					", \"Dato\" : " + (estado == "1" ? "true" : (estado == "0" ? "false" : estado)) +
					", \"Delay\": " + dato + "}]", flush);
					consultarEstado = true;
				}
			}
			catch (Exception e)
			{
				consultarEstado = true;
				Log.Error("Error al Conectar con el dispositivo", e);
				throw new DriverException("Error al Conectar con el dispositivo", e);
			}

		}

		public override void InformarEstado()
		{
			Log.Debug("Informando estado ITC {0}", codigoRasp);
			if (falloUltimaConexion.HasValue && falloUltimaConexion.Value)
			{
				NotificarEstadoConexion(CodigosEventos.ErrorConexionDispositivo, errorUltimaConexion ?? new Exception(CodigosEventos.ErrorConexionDispositivo));
			}
			else
			{
				NotificarEstadoConexion(CodigosEventos.ConexionDispositivoCorrecta);
			}
		}

		private void NotificarEventoEntrada(int entrada, string dato, string codigoEvento)
		{
			try
			{
				Log.Debug("Cambio Estado Entrada: Rasp={0} Entrada={1} Dato={3} Evento={2}", codigoRasp, entrada, codigoEvento, dato);

				var notification = new NotificacionEvento
				{
					CodigoDispositivo = codigoRasp,
					CodigoEvento = codigoEvento,
					Datos = new Dictionary<string, string>
								{
									{"Entrada", entrada.ToString(CultureInfo.InvariantCulture)},
									{"Dato", dato}
								}
				};

				OnEventoDriver(new EventoDriverEventArgs { Notificacion = notification });
			}
			catch (Exception ex)
			{
				Log.Error(ex, "No se pudo notificar el evento ", codigoEvento);
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				dispositivoActivo = false;
				cliente.Desconectar();
				finCiclo.WaitOne();
				finCiclo.Dispose();
			}
		}

		public bool ConsultarEstadoEntrada(int numeroEntrada)
		{
			return false;
		}

		public void DesactivarSalida(int salida, string estado, string dato)
		{
			return;
		}

		public bool ConsultarEstadoActual(int numeroEntrada)
		{
			/// <summary>
			/// Motodo deshabilitado, falta realizar la implementación del lado del concetrador para el envío de un comando el cual permita consultar el estado actual de un sensor enviado como parámetro el código del dispositivo.
			/// </summary>

			return false;
		}

		public void NotificarEstadoActual(int numeroEntrada)
		{
			/// <summary>
			/// Metodo No implementado, falta realizar la implementación del lado del concetrador para el envío de un comando el cual permita consultar el estado actual de un sensor enviado como parámetro el código del dispositivo.
			/// </summary>
		}
	}
}