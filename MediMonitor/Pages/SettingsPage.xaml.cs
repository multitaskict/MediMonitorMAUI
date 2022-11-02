using MediMonitor.Resources;
using MediMonitor.Service.Exceptions;
using MediMonitor.Service.Models;

namespace MediMonitor.Pages;

public partial class SettingsPage : ContentPage
{
	public SettingsPage()
	{
		InitializeComponent();

        signOffCell.Command = new Command(SignOut);
        downloadDataCell.Command = new Command(DownloadData);
        verwijderenCell.Command = new Command(RemoveData);

        appSettingsCell.Command = new Command(OpenAppSettings);

        privacyCell.Command = new Command(OpenPrivacyAgreement);
    }

    public async void SignOut()
    {
        if (await DisplayAlert(AppResources.Sign_off, AppResources.Sign_off_Confirm, AppResources.Yes, AppResources.No))
        {
            Preferences.Remove("User_Id");

            Preferences.Remove("Session_Cookie");

            await Shell.Current.GoToAsync("//SignIn", true);
        }
    }

    public async void DownloadData()
    {
        //Get the id of the current user
        var userId = Preferences.Get("User_Id", -1);

        //Get the user data from the database.
        var surveys = await App.Database.ToListAsync<Survey>(survey => survey.UserId == userId);
        var userInfo = await App.Database.GetByIdAsync<User>(userId);

        //Prepare the JSON
        var userData = new
        {
            Surveys = surveys,
            User = userInfo
        };

        var myData = Service.Web.Connection.GetJson(userData);

        //Create the output file
        var fileName = Path.Combine(FileSystem.CacheDirectory, "userdata.json");

        using (var file = new FileStream(fileName, FileMode.OpenOrCreate))
        using (var fileStreamWriter = new StreamWriter(file))
        {
            await fileStreamWriter.WriteAsync(myData);
        }

        //Share the file to OS.
        var shareFile = new ShareFile(fileName, Service.Web.Connection.JsonContentType);
        await Share.RequestAsync(new ShareFileRequest
        {
            File = shareFile,
            Title = AppResources.Download_data
        });
    }

    public async void RemoveData()
    {
        if (Connectivity.NetworkAccess != NetworkAccess.Internet)
        {
            await DisplayAlert(AppResources.Remove_data, AppResources.Not_connected, AppResources.Cancel);
            return;
        }

        var answer = await DisplayActionSheet(AppResources.Remove_data, AppResources.Cancel, AppResources.Remove_all_data, AppResources.Remove_local_data);

        if (answer == AppResources.Remove_all_data)
        {
            try
            {

                if (await App.ApplicationContext.Connection.DeleteDataAsync(
                    App.ApplicationContext.MedicijnVerstrekking,
                    App.Database,
                    App.ApplicationContext.User))
                {
                    await RemoveLocalData();
                }
                else
                {
                    await DisplayAlert(AppResources.RemoveDataError, AppResources.Remove_data, AppResources.Back);
                }
            }
            catch (NoSessionException)
            {
                await DisplayAlert(AppResources.NoSessionEx, AppResources.Remove_data, AppResources.Back);

                Preferences.Remove("User_Id");
                Preferences.Remove("Session_Cookie");

                await Shell.Current.GoToAsync("//SignIn", true);
            }
        }
        else if (answer == AppResources.Remove_local_data)
        {
            await RemoveLocalData();
        }

        async Task RemoveLocalData()
        {
            //Get the id of the current user
            var userId = Preferences.Get("User_Id", -1);

            //Remove syncs
            await App.Database.TableQuery<Sync>().Where(s => s.UserId == userId).DeleteAsync();

            //Remove surveys
            await App.Database.TableQuery<Survey>().Where(s => s.UserId == userId).DeleteAsync();

            //Remove User
            await App.Database.TableQuery<User>().Where(u => u.Id == userId).DeleteAsync();

            App.ApplicationContext.User = null;

            Preferences.Remove("User_Id");
            Preferences.Remove("Session_Cookie");
            
            await Shell.Current.GoToAsync("//SignIn", true);
        }
    }

    public async void OpenAppSettings()
    {
        AppInfo.ShowSettingsUI();
    }

    public async void OpenPrivacyAgreement()
    {
        await Browser.OpenAsync(AppResources.PrivacyUrl, BrowserLaunchMode.SystemPreferred);
    }
}