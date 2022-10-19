namespace MediMonitor.Pages;

public partial class BarcodeScanner : ContentPage
{
	public BarcodeScanner()
	{
		InitializeComponent();
	}

	private void barcodeView_BarcodesDetected(object sender, ZXing.Net.Maui.BarcodeDetectionEventArgs e)
	{
		
	}
}