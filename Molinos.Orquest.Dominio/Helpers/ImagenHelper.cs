using System;
using System.IO;
using System.Net;

namespace Molinos.Orquest.Dominio.Helpers
{
    public static class ImagenHelper
    {
        public static (byte[] imagen, string contentType) ConvertirUrlAByte(string url, int timeoutMs, NetworkCredential credentials = null)
        {
            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Timeout = timeoutMs;

            if (credentials != null)
                request.Credentials = credentials;

            using (var response = (HttpWebResponse)request.GetResponse())
            {
                if (!response.ContentType.StartsWith("image", StringComparison.OrdinalIgnoreCase))
                    return (null, null);

                using (var stream = response.GetResponseStream())
                using (var ms = new MemoryStream())
                {
                    stream.CopyTo(ms);
                    return (ms.ToArray(), response.ContentType);
                }
            }
        }
    }
}
