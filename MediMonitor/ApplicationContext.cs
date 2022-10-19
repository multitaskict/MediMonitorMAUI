using MediMonitor.Enums;
using MediMonitor.Service.Models;
using MediMonitor.Service.Web;

using System.Net;

namespace MediMonitor;

public class ApplicationContext
{
    /// <summary>
    /// Application Context
    /// </summary>
    /// <param name="version">Version of the App</param>
    /// <param name="appVersionType">The type of version.</param>
    /// <param name="appName">The name of the app.</param>
    public ApplicationContext(string version, AppVersionType appVersionType, string appName)
    {
        AppVersion = version;
        Connection = new Connection(version, appName, DeviceInfo.Platform.ToString());

        TestVersion = true;
        AppVersionType = appVersionType;
    }

    /// <summary>
    /// The Version of the App.
    /// </summary>
    public string AppVersion { get; private set; }

    /// <summary>
    /// The connection of the app.
    /// </summary>
    public Connection Connection { get; private set; }

    /// <summary>
    /// The type of AppVersion
    /// </summary>
    public AppVersionType AppVersionType { get; set; }

    /// <summary>
    /// Should the Testversion be accessed?
    /// </summary>
    public bool TestVersion { get; set; }

    /// <summary>
    /// The signed in <see cref="User"/>.
    /// </summary>
    public User User { get; set; }

    /// <summary>
    /// The context of the webapplication
    /// </summary>
    public MedicijnVerstrekking MedicijnVerstrekking { get; set; }

    public string SessionCookie
    {
        get
        {
            if (MedicijnVerstrekking?.Cookies == null)
                return string.Empty;

            if (MedicijnVerstrekking.Cookies.OfType<Cookie>().Any(c => c.Name == "MediMonitorSession"))
                return MedicijnVerstrekking.Cookies["MediMonitorSession"].Value;

            else
                return string.Empty;
        }
    }

}