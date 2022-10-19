﻿using MediMonitor.Enums;    
using MediMonitor.Pages;
using MediMonitor.Service.Data;
using MediMonitor.Service.Exceptions;
using MediMonitor.Service.Web;

using System.Net;

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
        Database = new AppData(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "MediMonitor.db3"), "??");

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
        //
        //window.Created += Window_Created;
        //
        return window;
    }


    private async void LoadApp(string requestPage = "survey")
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
                                Preferences.Remove("User_Id");

                                Preferences.Remove("Session_Cookie");

                                await Shell.Current.GoToAsync("//SignIn", true);

                                return;
                            }
                        }
                    }

                    await Shell.Current.GoToAsync("//Main/Survey", true);
                }
            }
            else
            {
                await Shell.Current.GoToAsync("//SignIn", true);
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

    public static ApplicationContext ApplicationContext { get; private set; }
    
    public static AppData Database { get; private set; }

    private static string url = Connection.TestUrl;
}
