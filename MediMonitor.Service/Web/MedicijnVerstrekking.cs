using System;
using System.Net;

namespace MediMonitor.Service.Web
{
    public class MedicijnVerstrekking
    {

        public static MedicijnVerstrekking Instance => new MedicijnVerstrekking();

        private MedicijnVerstrekking()
        {
            Cookies = new CookieCollection();
        }

        /// <summary>
        /// The Uri where the Medicijnverstrekking application is located
        /// </summary>
        private Uri uri;

        /// <summary>
        /// The Url where the Medicijnverstrekking application is located
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// Get <see cref="Url"/> as <see cref="Uri"/>.
        /// </summary>
        public Uri UriUrl
        {
            get
            {
                if (uri != null)
                    return uri;

                return uri = new Uri(Url);
            }
        }

        /// <summary>
        /// Collection of cookies. (for account management)
        /// </summary>
        public CookieCollection Cookies { get; set; }

        /// <summary>
        /// The Version of the application.
        /// </summary>
        public WebAppVersion Version { get; set; }

        /// <summary>
        /// The current GebruikerId, -1 if the Gebruiker is not looged on.
        /// </summary>
        public int GebruikerId { get; set; }

        /// <summary>
        /// The current app mode.
        /// </summary>
        public string AppMode { get; set; }

    }
}
