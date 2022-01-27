namespace Molinos.Orquest.Web.Models
{
    public class ResultadoPruebaModel
    {
        public string Message { get; private set; }
        public bool Error { get; private set; }

        public ResultadoPruebaModel(string message, bool error)
        {
            Message = message;
            Error = error;
        }
    }
}