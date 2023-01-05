using MediMonitor.Resources;

using System.Globalization;

namespace MediMonitor.Helpers;

public static class LanguageSettingHelper
{
    private const string SystemCulture = "systemCulture";

    private const string Culture = "culture";

    public static void SetLanguage(string cultureName = "")
    {
        //Check if the cultureName parameter is set.
        if(!string.IsNullOrWhiteSpace(cultureName))
        {
            //It's set, so save the setting
            Preferences.Set(Culture, cultureName);
        }
        else
        {
            //It's not set, get the value from the Settings.
            cultureName = Preferences.Get(Culture, string.Empty);
        }

        if (!string.IsNullOrWhiteSpace(cultureName))
        {
            var currentCulture = AppResources.Culture ?? CultureInfo.CurrentUICulture;
            var cultureInfo = CultureInfo.GetCultureInfo(cultureName);

            //en-US / en-GB start with "en",  nl-NL, nl-BE start with "nl"
            if (!currentCulture.Name.StartsWith(cultureInfo.Name))
            {
                CultureInfo.CurrentUICulture = cultureInfo;
                CultureInfo.CurrentCulture = cultureInfo;
                AppResources.Culture = cultureInfo;
            }

            //App language is set to OS-language, so remove the setting
            if (GetSystemCultureName().StartsWith(cultureInfo.Name))
            {
                Preferences.Remove(Culture);
            }
        }
    }

    /// <summary>                  
    /// Save the System language as a setting.
    /// </summary>
    public static void SetSystemLanguage()
    {
        var systemCulture = Preferences.Get(SystemCulture, string.Empty);
        if (string.IsNullOrWhiteSpace(systemCulture) || systemCulture != CultureInfo.CurrentUICulture.Name)
        {
            Preferences.Set(SystemCulture, CultureInfo.CurrentUICulture.Name);
            Preferences.Remove(Culture);
        }
    }

    public static string GetSystemCultureName()
    {
        return Preferences.Get(SystemCulture, CultureInfo.CurrentUICulture.Name);
    }

    public static void SetLanguage(CultureInfo culture)
    {
        SetLanguage(culture.Name);
    }

}
