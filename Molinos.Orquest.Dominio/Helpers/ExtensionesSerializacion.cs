using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Xml.Serialization;

namespace Molinos.Orquest.Dominio.Helpers
{
    public static class ExtensionesSerializacion
    {
        public static string ToJson<T>(this T data)
        {
            var serializer = new DataContractJsonSerializer(typeof(T));

            using (var ms = new MemoryStream())
            {
                serializer.WriteObject(ms, data);
                return Encoding.UTF8.GetString(ms.ToArray());
            }
        }

        public static string ToXml<T>(this T data)
        {
            var serializer = new XmlSerializer(typeof(T));

            using (var ms = new MemoryStream())
            {
                serializer.Serialize(ms, data);
                return Encoding.UTF8.GetString(ms.ToArray());
            }
        }

        public static T FromJson<T>(this string data)
        {
            if (string.IsNullOrEmpty(data))
            {
                return default(T);
            }
            var js = new DataContractJsonSerializer(typeof(T));
            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(data)))
            {
                return (T)js.ReadObject(ms);
            }
        }

        public static string Truncate(this string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value))
            {
                return value;
            }
            return value.Length <= maxLength ? value : value.Substring(0, maxLength);
        }

        public static bool IsValidJson(string strInput)
        {
            if (string.IsNullOrWhiteSpace(strInput)) { return false; }
            strInput = strInput.Trim();
            if ((strInput.StartsWith("{") && strInput.EndsWith("}")) || //Si es un object o
                (strInput.StartsWith("[") && strInput.EndsWith("]"))) //  es un array
            {
                try
                {
                    var obj = JToken.Parse(strInput);
                    return true;
                }
                catch (Exception) //some other exception
                {
                    //Console.WriteLine(ex.ToString());
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
    }
}
