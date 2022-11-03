namespace MediMonitor;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
    }    

    private async void MenuItem_Clicked(object sender, EventArgs e)
    {
        Preferences.Remove("User_Id");
        Preferences.Remove("Session_Cookie");

        await GoToAsync("//SignIn", true);
    }

    private void ExitMenuItem_Clicked(object sender, EventArgs e)
    {
        Application.Current.Quit();
    }
}
