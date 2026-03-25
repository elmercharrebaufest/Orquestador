using System.Collections.Generic;

namespace Molinos.Orquest.Web.Models
{
    public class PruebaItcModel
    {
        public string CodigoItc { get; set; }
        public string DireccionIpItc { get; set; }
        public int PuertoItc { get; set; }
        public List<PruebaDispositivoModel> Lectores { get; set; }
        public List<PruebaDispositivoModel> LectoresQr { get; set; }
        public List<PruebaDispositivoModel> Sensores { get; set; }
        public List<PruebaDispositivoModel> Barreras { get; set; }
        public List<PruebaDispositivoModel> Displays { get; set; }
        public List<PruebaDispositivoModel> CortinaAgua { get; set; }
        public List<PruebaDispositivoModel> Tags { get; set; }
        public List<PruebaDispositivoModel> SensoresVehiculares { get; set; }
    }
}
