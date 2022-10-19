using MediMonitor.Service.Interfaces;

using System;

namespace MediMonitor.Service.Exceptions
{
    public class NoSessionException : Exception
    {
        public NoSessionException(IResultViewModel result)
            : base(result.Error)
        {
            Result = result;
        }

        public IResultViewModel Result { get; }
    }
}
