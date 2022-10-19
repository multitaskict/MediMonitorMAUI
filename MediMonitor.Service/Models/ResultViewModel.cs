using MediMonitor.Service.Interfaces;

using System;
namespace MediMonitor.Service.Models
{
    public class ResultViewModel<T> : IResultViewModel
        where T : class
    {

        public string Error { get; set; }

        public bool Success { get; set; }

        public T[] Data { get; set; }

        public int Pages { get; set; }
    }

}
