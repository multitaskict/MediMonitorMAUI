using MediMonitor.Resources;
using MediMonitor.Service.Models;
using MediMonitor.Service.Web;

using ZXing.Net.Maui;

namespace MediMonitor.Pages;

public partial class BarcodeScanner : ContentPage
{
    public BarcodeScanner()
    {
        InitializeComponent();

        barcodeView.Options = new BarcodeReaderOptions
        {
            AutoRotate = true,
            Formats = BarcodeFormat.QrCode,
            Multiple = false,
            TryHarder = true
        };
    }

    public QrCodeCheck QrCodeCheck { get; set; }

    public string QrCode { get; set; }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();

        barcodeView.IsDetecting = false;
        barcodeView.IsTorchOn = false;

        //Unload the camera.
        barcodeView.Handler.DisconnectHandler();
    }

    private void barcodeView_BarcodesDetected(object sender, BarcodeDetectionEventArgs e)
    {
        try
        {
            var qrCode = e.Results.Where(r => r.Format == BarcodeFormat.QrCode).Select(r => r.Value).ToArray();
            var invalidList = new List<string>();

            foreach (var qr in qrCode)
            {
                var tQr = qr;
                if (tQr.Equals("demo", StringComparison.CurrentCultureIgnoreCase))
                {
                    tQr = "T00000000-M0000-C000";
                }       

                if (QrCodeCheck.TryParse(tQr, out var check))
                {
                    QrCodeCheck = check;
                    QrCode = tQr;

                    MainThread.BeginInvokeOnMainThread(() => Navigation.PopModalAsync());
                    return;
                 }
                else
                {
                    invalidList.Add(qr);
                }
            }

            if (QrCodeCheck == null)
            {
                DisplayMessageOnMainThread(AppResources.Sign_In_Error, AppResources.Unsupported_QR + Environment.NewLine + string.Join(Environment.NewLine, invalidList), AppResources.Cancel);
            }
        }
        catch(Exception ex)
        {
            DisplayMessageOnMainThread(AppResources.Sign_In_Error, ex.Message, AppResources.Cancel);
        }
    }

    private void DisplayMessageOnMainThread(string title, string message, string cancel)
    {
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            await DisplayAlert(title, message, cancel);
        });
    }

    async void cancelButton_Clicked(System.Object sender, System.EventArgs e)
    {
        await Navigation.PopModalAsync(true);
    }
    

    void flashButton_Clicked(System.Object sender, System.EventArgs e)
    {
        barcodeView.IsTorchOn = !barcodeView.IsTorchOn;

        if(Application.Current.Resources.TryGetValue(barcodeView.IsTorchOn ? "ActiveButtonStyle" : "DefaultButtonStyle", out var styleObject) &&  styleObject is Style style)
        {
            flashButton.Style = style;
        }
    }
}