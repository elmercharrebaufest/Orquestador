using System;

namespace Molinos.Orquest.Servicios.Impl
{
    public class ControlDeNotificacionesDispositivo : IControlDeNotificacionesDispositivo
    {
        private DateTime init;
        private readonly int maxNotificacions;
        private TimeSpan minuto;

        private long counter;
        

        public ControlDeNotificacionesDispositivo(int maxNotificacions)
        {
            counter = 0;
            TotalCounter = 0;
            init = DateTime.Now;
            TotalInit = init;
            minuto = TimeSpan.FromMinutes(1);
            this.maxNotificacions = maxNotificacions;
        }

        public long TotalCounter { get; private set; }

        public DateTime TotalInit { get; private set; }

        public bool NewValue()
        {
            var result = false;
            counter++;

            TotalCounter++;
            if (TotalCounter >= long.MaxValue) TotalCounter = 0;

            if (counter >= maxNotificacions)
            {
                result = GetElapsedTime() <= minuto;
                Reset();
            }

            return result;
        }

        private void Reset()
        {
            init = DateTime.Now;
            counter = 0;
        }

        private TimeSpan GetElapsedTime() => DateTime.Now.Subtract(init);
    }
} 