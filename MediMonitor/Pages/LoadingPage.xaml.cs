using MediMonitor.Resources;

namespace MediMonitor.Pages;

public partial class LoadingPage : ContentPage
{
	public LoadingPage()
	{
		InitializeComponent();
	}

    public void ShowError(Exception ex)
    {
        infoLabel.Text = AppResources.AppLoadError;

        loadError.IsVisible = true;
        IsBusy = false;

        exceptionLabel.Text = ex.Message;

        aiLoader.IsRunning = false;

        retryButton.IsEnabled = true;
    }

    private void retryButton_Clicked(object sender, EventArgs e)
	{
        retryButton.IsEnabled = false;

        App.Relaunch();
	}
}