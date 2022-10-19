using System;
using System.Collections.Generic;
using System.Text;

namespace MediMonitor.Service.Interfaces
{
    public interface IResultViewModel
    {
        string Error { get; set; }
        bool Success { get; set; }
    }
}
