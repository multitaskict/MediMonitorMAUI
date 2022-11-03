using MediMonitor.Enums;    
using MediMonitor.Pages;
using MediMonitor.Service.Data;
using MediMonitor.Service.Exceptions;
using MediMonitor.Service.Web;

using System.Net;

using FileSystem = Microsoft.Maui.Storage.FileSystem;

namespace MediMonitor;


public partial class App : Application
{
    public App()
    {
        InitializeComponent();

        var appVersionType = AppVersionType.Production;

#if DEBUG
        appVersionType = AppVersionType.Development;
#endif 

        ApplicationContext = new ApplicationContext(VersionTracking.CurrentVersion, appVersionType, "Default");
        Database = new AppData(Path.Combine(FileSystem.AppDataDirectory, "MediMonitor.db3"), "??");

        MainPage = new AppShell();
        
    }
    protected override void OnStart()
    {
        base.OnStart();
    }

        protected override Window CreateWindow(IActivationState activationState)
    {
        LoadApp();

        var window = base.CreateWindow(activationState);
        return window;
    }

    private static async void LoadApp(string requestPage = "survey")
    {
        Preferences.Remove("Sleep-Page");

        try
        {
            await Database.Init();

            if (!Database.Outdated)
            {
                if (ApplicationContext.MedicijnVerstrekking == null)
                {
                    ApplicationContext.MedicijnVerstrekking = await ApplicationContext.Connection.GetContextAsync(url);
                }

                var userId = Preferences.Get("User_Id", -1);
                var userService = new UserService(Database);
                var sessionCookie = Preferences.Get("Session_Cookie", "");

                if (userId > 0 && !string.IsNullOrWhiteSpace(sessionCookie))
                {
                    ApplicationContext.MedicijnVerstrekking.Cookies = new CookieCollection {
                        new Cookie("MediMonitorSession", sessionCookie) { Expires = DateTime.Today.AddYears(1) }
                     };

                    var user = await userService.GetById(userId);
                    if(user == null)
                    {
                        GoToLogin();
                        return;
                    }

                    ApplicationContext.User = user;

                    if (Connectivity.NetworkAccess == NetworkAccess.Internet)
                    {
                        //Check last sync
                        var surveyService = new SurveyService(Database, user);
                        var lastSurveyNotSynced = await surveyService.GetLastNotSynced();

                        //Check the last sync
                        if (lastSurveyNotSynced != null)
                        {
                            var surveys = await surveyService.GetSurveysAsync();
                            try
                            {
                                await ApplicationContext.Connection.SyncSurveysAsync(ApplicationContext.MedicijnVerstrekking, Database, user, surveys, null);
                            }
                            catch (NoSessionException)
                            {
                                GoToLogin();
                                return;
                            }
                        }
                    }

                    await Shell.Current.GoToAsync("//Main/Survey", true);
                }
                else
                {
                    await Shell.Current.GoToAsync("//SignIn", true);
                }
            }
        }
        catch (Exception ex)
        {
            if (Shell.Current.CurrentPage is LoadingPage loadingPage)
            {
                loadingPage.ShowError(ex);
            }
        }
    }

    private static async void GoToLogin()
    {
        Preferences.Remove("User_Id");

        Preferences.Remove("Session_Cookie");

        await Shell.Current.GoToAsync("//SignIn", true);

    }

    internal static void Relaunch()
    {
        LoadApp();
    }

    public static ApplicationContext ApplicationContext { get; private set; }
    
    public static AppData Database { get; private set; }

    private static readonly string url = Connection.TestUrl;

}
