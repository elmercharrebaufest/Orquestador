using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Molinos.Orquest.Web.ServicioHub
{ //TODO: Deprecar
    public class TransitoMolinete
    {
        public string CodigoMolinete { get; set; }
        public string Tarjeta { get; set; }
        public string Direccion { get; set; }
        public bool Denegado { get; set; }
        public bool SinTransito { get; set; }

    }
}