using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Web;
using System.Web.Mvc;
using Molinos.Orquest.Dominio.Recursos;

namespace Molinos.Orquest.Web.Atributos
{
    public class CharValidationAttribute : ValidationAttribute, IClientValidatable
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value != null)
            {
                var ascii = Encoding.ASCII;
                var car = HttpContext.Current.Server.UrlDecode(value.ToString()) ?? string.Empty;
                var asciiBytes = ascii.GetBytes(car.ToCharArray());
                // Convert the new byte[] into a char[] and then into a string. 
                var asciiChars = new char[ascii.GetCharCount(asciiBytes, 0, asciiBytes.Length)];
                ascii.GetChars(asciiBytes, 0, asciiBytes.Length, asciiChars, 0);

                if (asciiChars.Length > 1)
                {
                    return new ValidationResult(String.Format(Textos.Char_ErrorCantidad, validationContext.DisplayName));
                }

                if (asciiChars.Length == 0)
                {
                    return new ValidationResult(String.Format(Textos.Char_ErrorTipo, validationContext.DisplayName));
                }
                return ValidationResult.Success;
            }

            return new ValidationResult(String.Format(Textos.Char_ErrorTipo, validationContext.DisplayName));
        }

        public IEnumerable<ModelClientValidationRule> GetClientValidationRules(ModelMetadata metadata, ControllerContext context)
        {
            var modelClientValidationRule = new ModelClientValidationRule
            {
                ValidationType = "charvalidation",
                ErrorMessage = string.Format(Textos.Char_ErrorCantidad, metadata.DisplayName)
            };
            yield return modelClientValidationRule;
        }
    }
}