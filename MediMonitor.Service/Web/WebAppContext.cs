using System;

namespace MediMonitor.Service.Web
{
    /// <summary>
    /// Application Context from the WebApp.  Should be requested using <see cref="Connection"/>.
    /// </summary>
    public class WebAppContext
    {
        /// <summary>
        /// Version of the WebApp
        /// </summary>
        public string Version { get; set; }

        /// <summary>
        /// Branche of the WebApp (the version of the Database)
        /// </summary>
        public string Branche { get; set; }
    }
}
