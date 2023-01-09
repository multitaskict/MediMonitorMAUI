using ZXing.Net.Maui;
using ZXing.Net.Maui.Controls;

namespace MediMonitor;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();

        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");

                fonts.AddFont("FA6Brands-Regular-400.otf", "FontAwesomeBrands");
                fonts.AddFont("FA6Duotone-Solid-900.otf", "FontAwesomeDuotone");
                fonts.AddFont("FA6Pro-Light-300.otf", "FontAwesomeLight");
                fonts.AddFont("FA6Pro-Regular-400.otf", "FontAwesomeRegular");
                fonts.AddFont("FA6Pro-Solid-900.otf", "FontAwesomeSolid");
                fonts.AddFont("FA6Pro-Thin-100.otf", "FontAwesomeThin");
            })
            .ConfigureEssentials(essentials =>
            {
                essentials.UseVersionTracking();
            })
            .UseBarcodeReader();

        return builder.Build();
    }
}
