using System;

namespace MediMonitor.Service.Exceptions
{
    public class QrCodeException : Exception
    {
        public string Key { get; private set; }

        public QrCodeException(string key, string message)
            : base(message)
        {
            Key = key;
        }
    }
}
