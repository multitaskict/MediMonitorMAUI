namespace MediMonitor;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        
    }

    

    private async void MenuItem_Clicked(object sender, EventArgs e)
    {
        await GoToAsync("//SignIn", true);
    }

    private void ExitMenuItem_Clicked(object sender, EventArgs e)
    {
        Application.Current.Quit();
    }
}
