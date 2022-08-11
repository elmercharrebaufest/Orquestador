using Molinos.Orquest.Dominio;
using Molinos.Orquest.Dominio.Entidades;
using Molinos.Orquest.Dominio.Resultados;
using Molinos.Orquest.Drivers;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Molinos.Orquest.DriversImpl
{
    public class DriverBarreraGrupo : DriverBase, IDriverBarrera
    {
        private ConfigGrupoBarrera configGrupoBarrera;
        private ConfigBarrera configBarreraArriba;
        private ConfigBarrera configBarreraAbajo;
        private ConfigSensor configSensorPrimerCruce;
        private ConfigSensor configSensorSegundoCruce;
        private ConfigSensor configSensorArriba;
        private ConfigSensor configSensorAbajo;

        private bool ejecutandoApertura;
        private bool entroAlTramoBalanza;
        private bool pasoPorSegundoSensor;

        public override Type TipoDispositivo => throw new NotImplementedException();

        public void Abrir()
        {
            var claseDriver = Type.GetType(configBarreraArriba.ClaseDriver);
            var obj = (IDriverBarrera)Activator.CreateInstance(claseDriver);
            obj.Abrir();
            throw new NotImplementedException();
        }

        public void AbrirMaestro()
        {
            throw new NotImplementedException();
        }

        public void Cerrar()
        {
            throw new NotImplementedException();
        }

        public override void Inicializar(string codigo, ConfigDispositivo configuracion)
        {
            configGrupoBarrera = (ConfigGrupoBarrera)configuracion;
            configBarreraArriba = configGrupoBarrera.BarreraArriba;
            configBarreraAbajo = configGrupoBarrera.BarreraAbajo;
            configSensorArriba = configGrupoBarrera.SensorArriba;
            configSensorAbajo = configGrupoBarrera.SensorAbajo;
            configSensorPrimerCruce = configGrupoBarrera.SensorPrimerCruce;
            configSensorSegundoCruce = configGrupoBarrera.SensorSegundoCruce;
        }

        public override void VerificarDispositivo()
        {
            throw new NotImplementedException();
        }

        private Type GetClaseDriver(Dispositivo dispositivo)
        {
            var claseDriver = Type.GetType(dispositivo.Configuracion.ClaseDriver);
            if (claseDriver == null)
            {
                var error = string.Format("No se encontró la clase del driver '{0}' configurado en el dispositivo {1}",
                                          dispositivo.Configuracion.ClaseDriver, dispositivo.Codigo);
                throw new DriverNoEncontradoException(error, dispositivo.Codigo, dispositivo.Configuracion.ClaseDriver);
            }
            return claseDriver;
        }
    }
}