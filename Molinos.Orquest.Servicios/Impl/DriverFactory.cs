using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Molinos.Orquest.Dominio;
using Molinos.Orquest.Dominio.Entidades;
using Molinos.Orquest.Drivers;
using Molinos.Orquest.DriversImpl;
using Ninject;
using Ninject.Extensions.Logging;

namespace Molinos.Orquest.Servicios.Impl
{
    public class DriverFactory : IDriverFactory
    {
        private readonly IKernel kernel;
        private readonly ILogger log;

        public DriverFactory(IKernel kernel, ILogger log)
        {
            this.kernel = kernel;
            this.log = log;
        }

        public TDriver Driver<TDriver>(Dispositivo dispositivo) where TDriver : class, IDriver
        {
            var claseDriver = ClaseDriver(dispositivo);

            var driver = kernel.Get(claseDriver) as TDriver;
            if (driver == null)
            {
                var error = string.Format("El tipo del driver '{0}' no concuerda con el tipo del dispositivo {1}",
                                          dispositivo.Configuracion.ClaseDriver, dispositivo.Codigo);
                log.Error(error);

                throw new TipoDriverIncorrectoException(error, dispositivo.Codigo, dispositivo.Configuracion.ClaseDriver);
            }
            driver.Log = kernel.Get<ILoggerFactory>().GetLogger(driver.GetType());
            if (!driver.TipoDispositivo.IsInstanceOfType(dispositivo.Configuracion))
            {
                var error = string.Format("El driver {0} para el dispositivo {1} no es un driver que admita el tipo de dispositivo {2}",
                                          dispositivo.Configuracion.ClaseDriver,
                                          dispositivo.Codigo,
                                          dispositivo.Configuracion.GetType());

                log.Error(error);
                throw new TipoDispositivoIncorrectoException(error, dispositivo.Codigo, dispositivo.Configuracion.GetType().Name);
            }

            driver.Inicializar(dispositivo.Codigo, dispositivo.Configuracion);
            return driver;
        }

        public TDriver DriverLogico<TDriver>(Dispositivo dispositivo, IDriver driverFisico) where TDriver : class, IDriver
        {
            var driver = Driver<TDriver>(dispositivo);
            ((IDriverLogico)driver).DriverFisico = driverFisico;
            return driver;
        }

        private Type ClaseDriver(Dispositivo dispositivo)
        {
            var claseDriver = Type.GetType(dispositivo.Configuracion.ClaseDriver);
            if (claseDriver == null)
            {
                var error = string.Format("No se encontró la clase del driver '{0}' configurado en el dispositivo {1}",
                                          dispositivo.Configuracion.ClaseDriver, dispositivo.Codigo);
                log.Error(error);
                throw new DriverNoEncontradoException(error, dispositivo.Codigo, dispositivo.Configuracion.ClaseDriver);
            }
            return claseDriver;
        }

        public IList<string> DriversDisponibles<TDriver>() where TDriver : class, IDriver
        {
            return AssembliesConDrivers()
                .SelectMany(a => a.GetTypes().Where(t => t.GetInterfaces().Contains(typeof (TDriver))), (a, t) => NombreClase(t))
                .ToList();
        }

        private IEnumerable<Assembly> AssembliesConDrivers()
        {
            var nombresAssemblies = new string[0];
            var listaAssemplies = ConfigurationManager.AppSettings["AssembliesDrivers"];
            if (!string.IsNullOrEmpty(listaAssemplies))
            {
                nombresAssemblies = listaAssemplies.Split(',');
            }
            var lista = nombresAssemblies.Select(CargarAssembly).Where(a => a != null).ToList();
            lista.Add(typeof(DriverBase).Assembly);
            return lista;
        }

        private Assembly CargarAssembly(string nombre)
        {
            try
            {
                return Assembly.Load(nombre.Trim());
            }
            catch (Exception e)
            {
                log.Error(e, "NO se pudo cargar el assemply de drivers: {0}", nombre);
                return null;
            }
        }

        private string NombreClase(Type type)
        {
            return new Regex(", Version=.*").Replace(type.AssemblyQualifiedName, "");
        }
    }
}
