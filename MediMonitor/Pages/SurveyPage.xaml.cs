using MediMonitor.Resources;
using MediMonitor.Service.Data;
using MediMonitor.Service.Exceptions;
using MediMonitor.Service.Models;
using MediMonitor.Service.Web;
using MediMonitor.ViewModels;

namespace MediMonitor.Pages;

public partial class SurveyPage : ContentPage
{

    private readonly SurveyService surveyService;

    public Survey Survey { get; private set; }

    public SurveyViewModel SurveyModel { get; private set; }

    public Connection Connection { get; private set; }

    public SurveyPage()
    {
        InitializeComponent();

        surveyService = new SurveyService(App.Database, App.ApplicationContext.User);

        var submitToolBarItem = new ToolbarItem
        {
            IconImageSource = new FontImageSource
            {
                FontFamily = "FontAwesomeSolid",
                Glyph = FontAwesome.Regular.PaperPlane,
                Color = Colors.Black
            }
        };

        submitToolBarItem.Clicked += SubmitToolBarItem_ClickedAsync; ;
        ToolbarItems.Add(submitToolBarItem);

        entryScore.TextChanged += EntryScore_TextChanged;

        Connection = App.ApplicationContext.Connection;

    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        Survey = await surveyService.GetSurveyForDateAsync(DateTime.Today);

        SurveyModel = new SurveyViewModel(Survey);

        BindingContext = SurveyModel;
    }

    private async void SubmitToolBarItem_ClickedAsync(object sender, EventArgs e)
    {
        await SendAsync();
    }

    private void EntryScore_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (entryScore.Text == string.Empty)
        {
            SurveyModel.Score = null;
        }

        var scoreClass = SurveyModel.GetErrors("Score").OfType<string>().Any() ? "numberEntryError" : "numberEntryCorrect";
        entryScore.StyleClass = new List<string> { scoreClass };
    }

    private async void submitButton_Clicked(object sender, EventArgs e)
    {
        await SendAsync();
    }

    private async Task SendAsync()
    {
        if (!submitButton.IsEnabled)
            return;

        if (SurveyModel.GetErrors("Score").OfType<string>().Any() || SurveyModel.Score.HasValue == false)
        {
            await DisplayAlert(AppResources.Survey, AppResources.Survey_Score_Info, AppResources.Back);
            return;
        }

        submitButton.IsEnabled = false;

        Survey.DateTime = SurveyModel.DateTime;
        Survey.Score = SurveyModel.Score;

        await surveyService.SaveAsync(Survey);

        try
        {
            if (Connectivity.NetworkAccess == NetworkAccess.Internet)
            {
                var result = await Connection.SendSurveyAsync(App.ApplicationContext.MedicijnVerstrekking, Survey);
                if (!result.Success)
                {
                    var errorText = AppResources.ResourceManager.GetString(result.Error);
                    await DisplayAlert(AppResources.Survey, errorText ?? result.Error, AppResources.OK);
                }
                else
                {
                    if (result.Data != null)
                    {
                        var syncDateTime = result.Data[0].SyncDateTime;

                        if (syncDateTime.HasValue)
                        {
                            await surveyService.SaveSyncAsync(new[] { Survey }, syncDateTime.Value);

                            await DisplayAlert(AppResources.Survey, AppResources.Survey_sent, AppResources.OK);
                        }
                    }
                }
            }
            else
            {
                await DisplayAlert(AppResources.Survey, AppResources.Survey_saved, AppResources.OK);
            }
        }
        catch (NoSessionException)
        {
            await DisplayAlert(AppResources.Survey, AppResources.NoSessionEx, AppResources.Back);

            Preferences.Remove("User_Id");
            Preferences.Remove("Session_Cookie");

            await Shell.Current.GoToAsync("//SignIn", true);
        }
        finally
        {
            //Re-enable the button
            submitButton.IsEnabled = true;
        }
    }
}