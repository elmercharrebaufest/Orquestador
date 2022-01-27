using System.ComponentModel.DataAnnotations;
using System.Web;
using Molinos.Orquest.Dominio.Recursos;

namespace Molinos.Orquest.Web.Atributos
{
    public class FileSizeAttribute : ValidationAttribute
    {
        private readonly int _maxSize;

        public FileSizeAttribute(int maxSize)
        {
            _maxSize = maxSize;
        }

        public override bool IsValid(object value)
        {
            if (value == null) return true;

            return (value as HttpPostedFileBase).ContentLength <= _maxSize;
        }

        public override string FormatErrorMessage(string name)
        {
            return string.Format(Textos.Error_Excede_Espacio, _maxSize);
        }
    }
    
}