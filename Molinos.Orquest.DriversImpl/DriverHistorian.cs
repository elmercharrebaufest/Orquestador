using Molinos.Orquest.Dominio.Entidades;
using Molinos.Orquest.Dominio.Resultados;
using Molinos.Orquest.Drivers;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace Molinos.Orquest.DriversImpl
{
    public class DriverHistorian : DriverBase, IDriverItc
    {
        private ConfigItc configHistorian;
        private string cliente;

        public override Type TipoDispositivo
        {
            get { return typeof(ConfigItc); }
        }

        public override void Inicializar(string codigo, ConfigDispositivo configuracion)
        {
            configHistorian = (ConfigItc)configuracion;
            cliente = configHistorian.DireccionIp;

            Log.Debug("Iniciando Driver de Historian {0}", codigo);
        }

        public override void VerificarDispositivo()
        {
            if (string.IsNullOrEmpty(cliente))
            {
                throw new ConexionDispositivoDriverException("");
            }
        }

        public ResultadoEjecutarQuery ObtenerDatos(string query, string codigoTag)
        {
            Log.Debug("Se va a ejecutar la query: " + query);
            try
            {
                using (var connection = new SqlConnection(configHistorian.DireccionIp))
                {
                    var command = new SqlCommand(query, connection);
                    List<string> result = new List<string>();
                    Log.Debug("Driver Historian pre-inicializar query");
                    try
                    {
                        connection.Open();
                        Console.WriteLine("Connected");
                        SqlDataReader reader = command.ExecuteReader();
                        Log.Debug("Conection string exitoso", reader);
                        while (reader.Read())
                        {
                            //encapsular para mejora
                            string row = string.Empty;
                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                row += reader.GetValue(i).ToString() + "|";
                            }
                            row = row.Length > 0 ? row.Substring(0, row.Length - 1) : row;
                            result.Add(row);

                            return new ResultadoEjecutarQuery
                            {
                                Mensaje = Mensaje.ResultadoOK(),
                                queryResult = result
                            };
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, "Error durante la ejecución de la query a la bd", configHistorian.DireccionIp, command);
                        throw new ConexionDispositivoDriverException(string.Format("Falló la ejecución de la query al dispositivo {0}", codigoTag), ex);
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error(e, "error durante la conexion a la bd");
                throw new ConexionDispositivoDriverException(string.Format("Falló la conexión al dispositivo {0}", codigoTag), e);
            }

            return new ResultadoEjecutarQuery
            {
                Mensaje = Mensaje.ResultadoOK(),
                queryResult = new List<string>()
            };
        }

        public void ActivarSalida(int salida, string estado, string dato, bool flush = false)
        {
            return;
        }

        public void DesactivarSalida(int salida, string estado, string dato)
        {
            return;
        }

        public bool ConsultarEstadoEntrada(int numeroEntrada)
        {
            return true;
        }

        protected override void Dispose(bool disposing)
        {
        }

        public bool ConsultarEstadoActual(int numeroEntrada)
        {
            return ConsultarEstadoEntrada(numeroEntrada);
        }
    }
}