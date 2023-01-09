using MediMonitor.Pages;
using MediMonitor.Resources;
using MediMonitor.Service.Data;
using MediMonitor.Service.Models;
using MediMonitor.Service.Web;

namespace MediMonitor;

public partial class MainPage : ContentPage
{
    private bool disconnected = false;
    private readonly Connection connection;

    public MainPage()
    {
        InitializeComponent();

        connection = App.ApplicationContext.Connection;
    }

    private async void buttonManualEntry_Clicked(object sender, EventArgs e)
    {
        try
        {
            var code = await DisplayPromptAsync(AppResources.Manual_Entry, AppResources.Manual_Entry_Info, AppResources.OK, AppResources.Cancel, null, -1, Keyboard.Text, "");

            if (!string.IsNullOrWhiteSpace(code))
            {
                await SignIn(code);
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert(
                AppResources.Sign_In_Error,
                AppResources.ResourceManager.GetString(ex.InnerException?.Message) ?? ex.InnerException.Message,
                AppResources.Back
            );
        }
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        await Connect();
    }

    public string Copyright => $"Multitask ICT b.v. © {DateTime.Now:yyyy}";

    internal async Task Connect()
    {
        if (Connectivity.NetworkAccess == NetworkAccess.Internet)
        {
            var mv = App.ApplicationContext.MedicijnVerstrekking = await connection.GetContextAsync(App.ApplicationContext.MedicijnVerstrekking.Url);
            labelVersion.Text = mv.Version.ToString();
            disconnected = false;
        }
        else
        {
            labelVersion.Text = "n/a";
            disconnected = true;
        }

        labelAppVersion.Text = $"{AppResources.Version}: {App.ApplicationContext.AppVersion}";
        lblTest.IsVisible = App.ApplicationContext.AppVersionType != Enums.AppVersionType.Production;
    }

    private async void buttonScanQR_Clicked(object sender, EventArgs e)
    {
        if (disconnected || Connectivity.NetworkAccess != NetworkAccess.Internet)
        {
            await DisplayAlert("MediMonitor", AppResources.Not_connected, AppResources.Back);
            return;
        }

        if(DeviceInfo.Platform == DevicePlatform.WinUI)
        {
            await DisplayAlert("MediMonitor", "Not supported on Windows!", "Back");
            return;
        }

        var barcodeScanner = new BarcodeScanner();

        await Navigation.PushModalAsync(barcodeScanner);



        //try
        //{
            

        //    var scanner = new MobileBarcodeScanner
        //    {
        //        CancelButtonText = AppResources.Cancel,
        //        FlashButtonText = AppResources.Flash
        //    };

        //    var result = await scanner.Scan();

        //    if (result != null)
        //    {
        //        await SignIn(result.Text);
        //    }
        //}
        //catch (Exception ex)
        //{
        //    await DisplayAlert(
        //        AppResources.Sign_In_Error,
        //        AppResources.ResourceManager.GetString(ex.InnerException?.Message) ?? ex.InnerException.Message,
        //        AppResources.Back
        //    );
        //}

    }
     
    private async Task SignIn(string code)
    {
        try
        {
            if (code.Equals("demo", StringComparison.CurrentCultureIgnoreCase))
            {
                code = "T00000000-M0000-C000";
            }

            var qrCodeCheck = new QrCodeCheck(code);
            if (!qrCodeCheck.IsValid())
            {
                throw new Exception("Unsupported_QR");
            }

            var qrCode = (await App.Database.ToListAsync<QrCode>(qrc => qrc.Code == code)).SingleOrDefault() ?? new QrCode { Code = code };

            User testUser = null;

            testUser = await connection.SignInAsync(App.ApplicationContext.MedicijnVerstrekking, (c) => App.ApplicationContext.MedicijnVerstrekking.Cookies.Add(c), qrCode, App.Database);

            qrCode.UserId = testUser.Id;

            await App.Database.SaveAsync(qrCode);

            if (testUser != null)
            {
                App.ApplicationContext.User = testUser;
                Preferences.Set("User_Id", testUser.Id);

                if (!string.IsNullOrWhiteSpace(App.ApplicationContext.SessionCookie))
                {
                    Preferences.Set("Session_Cookie", App.ApplicationContext.SessionCookie);
                }

                var surveyService = new SurveyService(App.Database, testUser);
                var lastSurveyNotSynced = await surveyService.GetLastNotSynced();

                //Check the last sync
                var lastSync = await new SyncService(App.Database, testUser).GetLastSync();
                if (lastSync == null || lastSurveyNotSynced != null)
                {
                    var surveys = await surveyService.GetSurveysAsync();
                    await connection.SyncSurveysAsync(App.ApplicationContext.MedicijnVerstrekking, App.Database, testUser, surveys, null);
                }

                await Shell.Current.GoToAsync("//Main/Survey", true);
            }
        }
        catch (Exception ex)
        {
            throw new Exception(AppResources.ResourceManager.GetString(ex.Message), ex);
        }
    }

    private void buttonAppSettings_Clicked(object sender, EventArgs e)
    {
        AppInfo.ShowSettingsUI();
    }

    private async void buttonPrivacyAgreement_Clicked(object sender, EventArgs e)
    {
        await Browser.OpenAsync(AppResources.PrivacyUrl, BrowserLaunchMode.SystemPreferred);
    }
}

