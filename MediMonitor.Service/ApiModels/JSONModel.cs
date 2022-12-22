using System;

namespace MediMonitor.Service.ApiModels
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
