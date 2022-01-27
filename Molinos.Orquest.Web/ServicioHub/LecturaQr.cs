using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Molinos.Orquest.Web.ServicioHub
{
    public class LecturaQr
    {
        public string CodigoItc { get; set; }
        public string CodigoDispositivo { get; set; }
        public string CodigoMolinete { get; set; }
        public string QR { get; set; }
        public string Lector { get; set; }
    }
}