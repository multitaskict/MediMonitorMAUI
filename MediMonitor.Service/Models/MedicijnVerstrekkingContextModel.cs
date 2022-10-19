using System;
namespace MediMonitor.Service.Models
{
    public class MedicijnVerstrekkingContextModel
    {
        public string BaseUrl { get; set; }

        public string Controller { get; set; }

        public string Action { get; set; }

        public bool Nachtmodus { get; set; }

        public string Version { get; set; }

        public string Branche { get; set; }

        public string AppMode { get; set; }

        public string VersionString
        {
            get
            {
                return Branche + " - " + Version;
            }
        }

        public int GebruikerId { get; set; }
    }
}
