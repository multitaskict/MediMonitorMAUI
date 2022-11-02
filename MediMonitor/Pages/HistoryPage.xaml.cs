using MediMonitor.Resources;
using MediMonitor.Service.Data;
using MediMonitor.Service.Exceptions;

namespace MediMonitor.Pages;

public partial class HistoryPage : ContentPage
{
    private readonly SurveyService surveyService;
    private readonly SyncService syncService;

    public HistoryPage()
    {
        InitializeComponent();

        surveyService = new SurveyService(App.Database, App.ApplicationContext.User);
        syncService = new SyncService(App.Database, App.ApplicationContext.User);

        var syncToolBarItem = new ToolbarItem
        {
            IconImageSource = new FontImageSource
            {
                FontFamily = "FontAwesomeSolid",
                Glyph = FontAwesome.Regular.ArrowsRotate,
                Color = Colors.Black
            }
        };

        ToolbarItems.Add(syncToolBarItem);
        syncToolBarItem.Clicked += SyncToolBarItem_Clicked;
    }

    private async void SyncToolBarItem_Clicked(object sender, EventArgs e)
    {
        if (Connectivity.NetworkAccess != NetworkAccess.Internet)
        {
            await DisplayAlert(AppResources.Sync, AppResources.Not_connected, AppResources.Cancel);
            return;
        }

        try
        {
            var allToSync = await surveyService.GetSurveysAsync(s => s.SyncDateTime == null || s.SyncDateTime < s.ModifiedDateTime);

            var lastSync = await syncService.GetLastSync();

            var surveyResult = await App.ApplicationContext.Connection.SyncSurveysAsync(App.ApplicationContext.MedicijnVerstrekking,
                App.Database, App.ApplicationContext.User, allToSync, lastSync?.SyncDateTime);

            await ReloadData();

            await DisplayAlert(AppResources.Sync, AppResources.Sync_Complete, AppResources.OK);
        }
        catch (NoSessionException)
        {
            await DisplayAlert(AppResources.Sync, AppResources.NoSessionEx, AppResources.Back);

            Preferences.Remove("User_Id");
            Preferences.Remove("Session_Cookie");

            await Shell.Current.GoToAsync("//SignIn", true);
        }
        catch (Exception ex)
        {
            var errorText = AppResources.ResourceManager.GetString(ex.Message);
            await DisplayAlert(AppResources.Sync, errorText ?? ex.Message, AppResources.OK);
        }
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await ReloadData();

    }

    private async Task ReloadData()
    {
        tableSectionSurveys.Clear();
        tableSectionCurrentSurveys.Clear();

        var allSurveys = await surveyService.GetSurveysAsync();

        var surveys = allSurveys.Where(s => s.DateTime.Date != DateTime.Today).OrderByDescending(s => s.DateTime);

        var today = allSurveys.SingleOrDefault(s => s.DateTime.Date == DateTime.Today);

        if (today != null)
        {
            var cell = new TextCell
            {
                Text = "Score: " + today.Score,
                Detail = string.Format(AppResources.FullDateFormat, today.DateTime)
            };

            tableSectionCurrentSurveys.Add(cell);

            foreach (var survey in surveys)
            {
                var tCell = new TextCell
                {
                    Text = "Score: " + survey.Score,
                    Detail = string.Format(AppResources.FullDateFormat, survey.DateTime)
                };

                tableSectionSurveys.Add(tCell);
            }
        }
        else
        {
            var textCell = new TextCell { Text = AppResources.Enter_today_s_score };

            tableSectionCurrentSurveys.Add(textCell);
        }
    }
}