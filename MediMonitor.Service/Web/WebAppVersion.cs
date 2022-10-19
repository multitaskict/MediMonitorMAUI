using System;
using System.Globalization;

namespace MediMonitor.Service.Web
{
    public class WebAppVersion
    {
        public WebAppVersion(WebAppContext context)
        {

            DatabaseVersion = long.Parse(context.Branche);
            ApplicationDate = DateTime.ParseExact(context.Version, "dd-MM-yyyy HH:mm", CultureInfo.CurrentCulture);
        }

        /// <summary>
        /// The version of the database
        /// </summary>
        public long DatabaseVersion { get; }

        /// <summary>
        /// The date of the applcation build
        /// </summary>
        public DateTime ApplicationDate { get; }

        /// <summary>
        /// Get the current version string.
        /// </summary>
        /// <returns>The current version as string</returns>
        public override string ToString()
        {
            return $"{DatabaseVersion} {ApplicationDate:dd-MM-yyyy HH:mm}";
        }

        /// <summary>
        /// Check if the application database is minal the specified <paramref name="version"/>.
        /// </summary>
        /// <param name="version">The version to check for.</param>
        /// <returns>true if the database is the minimal specified version, otherwise false.</returns>
        public bool CheckMinimalVersion(long version)
        {
            return DatabaseVersion >= version;
        }
    }
}
