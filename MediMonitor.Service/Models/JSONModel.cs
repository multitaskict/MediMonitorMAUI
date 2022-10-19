using System;
using System.Collections.Generic;
using System.Text;

namespace MediMonitor.Service.Models
{
    public class JSONModel
    {
        public Uri Uri { get; set; }    

        public string Data { get; set; }


        public override string ToString()
        {
            return Uri.PathAndQuery;
        }
    }
}
