using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Molinos.Orquest.Dominio.Dtos;
using Molinos.Orquest.Dominio.Entidades;
using Molinos.Orquest.Drivers;
using HtmlAgilityPack;
using System.Linq;

namespace Molinos.Orquest.DriversImpl
{
    public class DriverMeteorologica : DriverBase, IDriverMeteorologica
    {
        private IDriverItc driverItc;
     

        private ConfigMeteorologica configMeteorologica;
        
        public override Type TipoDispositivo
        {
            get { return typeof(ConfigMeteorologica); }
        }

        public override void Inicializar(string codigo, ConfigDispositivo configuracion)
        {
            configMeteorologica = (ConfigMeteorologica)configuracion;
        }

        public override void VerificarDispositivo()
        {
            if (!Directory.Exists(configMeteorologica.Ruta))
            {
                throw new ConexionDispositivoDriverException("");
            }
        }

        public void Abrir()
        {
        
        }
        public List<MeteorologicaDto> ObtenerGraficosMeteorologicos()
        {
            var resultado = new List<MeteorologicaDto>();
            try
            {
                string configRutaMeteorologica = configMeteorologica.Ruta.Trim();
                if (Directory.Exists(configRutaMeteorologica))
                {

                    var imagenes = Directory.GetFiles(configRutaMeteorologica, "*.gif");
                    var archivo = File.ReadAllText(Directory.GetFiles(configRutaMeteorologica, "*.htm").GetValue(0).ToString());

                    var html = new HtmlDocument();
                    var node = HtmlNode.CreateNode(archivo.Replace("\r\n", string.Empty));
                    html.DocumentNode.AppendChild(node);

                    var aux = html.DocumentNode.SelectNodes("//html/body/div"); // descartas los que son de otra cosa
                    var fechaHtml = aux[1];
                   aux.Remove(9); aux.Remove(0); aux.Remove(0);
                    var data = new List<string>();
                    var tablas = new List<string>();

                    foreach (var info in aux)
                    {
                        data.Add(Regex.Replace(info.InnerText, @"\s{2,}", " "));
                        tablas.Add(info.OuterHtml);
                    }
                    resultado.Add(ObtenerFecha(fechaHtml));
                    for (int i = 0; i < tablas.Count; i++)
                    {
                        var descripcion = Regex.Match(data.ElementAt(i), @"([A-Z]+\s?[A-Z]+\s?[A-Z]+[^a-z0-9\W])").Value;

                        var detalleAux = data.ElementAt(i).Split(new string[] { descripcion.Split(' ').GetValue(0).ToString() }, StringSplitOptions.RemoveEmptyEntries).GetValue(1).ToString()
                            .Split(new string[] { "Actual", "M�nima M�xima", "Diaria", "A las", "Mensual", "Anual", "Temperatura y Viento", "Temperatura y Humedad", "Velocidad", "Del Sector", "M�ximas" }, System.StringSplitOptions.RemoveEmptyEntries);
                        
                        var resultadoDetalle = new List<string>();

                        for (int j = 0; j < detalleAux.Length; j++)
                        {
                            var rgx = new Regex("([0-9]+.)?[0-9]+(a|p|)");
                            var valores = string.Join(" ", rgx.Matches(detalleAux[j]).Cast<Match>());
                            foreach (var detalles in valores.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries))
                            {
                                resultadoDetalle.Add(detalles);
                            }
                        }

                        resultadoDetalle = AgregarUnidadesYColumnas(descripcion, resultadoDetalle);

                        var resultados = new MeteorologicaDto
                        {
                            Descripcion = descripcion,
                            Detalle = resultadoDetalle,
                            Imagenes = new List<byte[]>()
                        };

                        foreach (var img in imagenes)
                        {
                            if (tablas.ElementAt(i).Contains(Path.GetFileName(img)))
                            {
                                resultados.Imagenes.Add(File.ReadAllBytes(img));
                            }
                        }

                        resultado.Add(resultados);
                    }

                    return resultado;                    
                }
                else
                {
                    throw new DirectoryNotFoundException(
                      string.Format("El dispositivo no se encuentra"));
                }
            }
            catch (IOException e)
            {
                throw new DirectoryNotFoundException(
                      string.Format("El dispositivo no se encuentra"), e);
            }

        }
        public IDriver DriverFisico
        {
            set { driverItc = (IDriverItc) value; }
        }

        private List<string> AgregarUnidadesYColumnas(string descripcion, List<string> detalles)
        {
            if (descripcion == "HUMEDAD" || descripcion == "TEMPERATURA")
            {
                detalles = detalles.Select(x => (!x.Contains("a") && !x.Contains("p")) ? x + (descripcion == "HUMEDAD" ? " %" : " °C") : x).ToList();
                detalles.Insert(0, "Actual");
                var columnas = new List<string> { "Diaria", "A las", "Mensual", "Anual" };
                for (int i = 2, j = 0; j < columnas.Count; i += 3, j++)
                {
                    detalles.Insert(i, columnas[j]);
                }
                var columnasFaltantes = new List<string> { "Máxima", "Mínima", "", "" };
                for (int k = 0; k < columnasFaltantes.Count; k++)
                {
                    detalles.Insert(2, columnasFaltantes[k]);
                }
            }
            else if (descripcion == "VIENTO")
            {
                detalles.Insert(0, "Velocidad");
                detalles.Insert(2, "Del Sector");
                detalles = detalles.Select((x, i) => i != 3 ? ((!x.Contains("a") && !x.Contains("p")) ? x + " km/h" : x) : "S (" + x + ")").ToList();
                detalles.Insert(4, "Máximas");
                detalles.Insert(4, "");
                var columnas = new List<string> { "Diaria", "A las", "Mensual", "Anual" };
                for (int i = 6, j = 0; j < columnas.Count; i += 2, j++)
                {
                    detalles.Insert(i, columnas[j]);
                }
            }
            else if (descripcion == "LLUVIA")
            {
                detalles = detalles.Select((x, i) => i != 2 ? x + " mm" : x + " mm/h").ToList();

                var columnas = new List<string> { "Diaria", "Intesidad", "Mensual", "Anual" };
                for (int i = 0, j = 0; j < columnas.Count; i += 2, j++)
                {
                    detalles.Insert(i, columnas[j]);
                }
            }
            else if (descripcion == "SENSACION TERMICA")
            {
                detalles = detalles.Select(x => x + " °C").ToList();
                detalles.Insert(0, "Temperatura y Viento");
                detalles.Insert(1, "Actual");
                detalles.Insert(3, "Temperatura y Humedad");
                detalles.Insert(4, "Actual");
            }
            else if (descripcion == "PRESION BAROMETRICA")
            {
                detalles = detalles.Select(x => x + " hPa").ToList();
                detalles.Insert(0, "Actual");
            }
            else
            {
                detalles = detalles.Select(x => x + " °C").ToList();
                detalles.Insert(0, "Actual");
            }

            return detalles;
        }

        public MeteorologicaDto ObtenerFecha(HtmlNode htmlNode)
        {
            var fecha = Regex.Match(htmlNode.InnerText,
                @"[0-9][0-9]\/[0-9][0-9]\/[0-9][0-9]").Value;
            var hora = Regex.Match(htmlNode.InnerText,
                @"[0-9]?[0-9]:[0-9][0-9][p,a]").Value;

            hora = hora.EndsWith("p") ? hora.Replace("p", " PM") : hora.Replace("a", " AM");

            var horaParseada = DateTime.ParseExact(fecha, "dd/MM/yy", null).AddHours(DateTime.Parse(hora).Hour);

            return new MeteorologicaDto() 
            {
                Descripcion = "FECHA",
                Detalle = new List<string>() { horaParseada.ToString("dd-MM-yyyy HH:mm"), horaParseada.ToString("dd-MM-yyyy HH:mm") },
                Imagenes = new List<byte[]>()
            };
        }

        public decimal ObtenerDireccionViento()
        {
            var test = ObtenerGraficosMeteorologicos();
            decimal data = -1;

            var itemViento = test.Where(x => x.Descripcion == "VIENTO").FirstOrDefault();

            if (itemViento != null)
            {
                decimal.TryParse(Regex.Match(itemViento.Detalle[3], @"\d+").Value, out data);
            }
            return data;
        }
    }
}
