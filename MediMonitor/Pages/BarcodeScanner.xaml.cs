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

        
    }

    public QrCodeCheck QrCodeCheck { get; set; }

    private async void barcodeView_BarcodesDetected(object sender, BarcodeDetectionEventArgs e)
    {
        var qrCode = e.Results.Where(r => r.Format == BarcodeFormat.QrCode).Select(r => r.Value);

        foreach (var qr in qrCode)
        {
            if (QrCodeCheck.TryParse(qr, out var check))
            {
                QrCodeCheck = check;
            }
        }

        if(QrCodeCheck == null)
        {
            await DisplayAlert("", "Geen geldige QR", AppResources.Cancel);
        }
    }

}