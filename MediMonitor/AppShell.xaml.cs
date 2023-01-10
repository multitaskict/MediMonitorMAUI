using MediMonitor.Resources;

namespace MediMonitor;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
    }    

    private async void MenuItem_Clicked(object sender, EventArgs e)
    {
        if (await DisplayAlert(AppResources.Sign_off, AppResources.Sign_off_Confirm, AppResources.Yes, AppResources.No))
        {
            Preferences.Remove("User_Id");

            Preferences.Remove("Session_Cookie");

            await Shell.Current.GoToAsync("//SignIn", true);            
        }
    }

    private void ExitMenuItem_Clicked(object sender, EventArgs e)
    {
        Application.Current.Quit();
    }
}
