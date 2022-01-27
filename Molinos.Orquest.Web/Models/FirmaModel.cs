using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.Web;
using Molinos.Orquest.Dominio.Recursos;
using Molinos.Orquest.Web.Atributos;

namespace Molinos.Orquest.Web.Models
{
    public sealed class FirmaModel : IValidatableObject
    {
        public int Id { get; set; }

        [Display(ResourceType = typeof(Textos), Name = "Logo")]
        public Byte[] Logo { get; set; }

        [Display(ResourceType = typeof(Textos), Name = "Icono")]
        public Byte[] Favicon { get; set; }

        [FileSize(50000)]
        [FileTypes("jpg,jpeg,png")]
        public HttpPostedFileBase LogoFile { get; set; }

        [FileSize(5000)]
        [FileTypes("ico")]
        public HttpPostedFileBase FaviconFile { get; set; }


        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (LogoFile != null)
            {
                var imagen = Image.FromStream(LogoFile.InputStream, true, true);
                if (imagen.Height > 50 || imagen.Width > 300)
                {
                    yield return new ValidationResult(string.Format(Textos.Error_Imagen_Grande, new[] { "Logo", imagen.Width.ToString(), imagen.Height.ToString() }));
                }
                LogoFile.InputStream.Position = 0;
            }

            if (FaviconFile != null)
            {
                var imagen = Image.FromStream(FaviconFile.InputStream, true, true);
                if (imagen.Height > 16 || imagen.Width > 16)
                {
                    yield return new ValidationResult(string.Format(Textos.Error_Imagen_Grande, new[] { "Icono", imagen.Width.ToString(), imagen.Height.ToString() }));
                }
                FaviconFile.InputStream.Position = 0;
            }

            
        }

    }
}