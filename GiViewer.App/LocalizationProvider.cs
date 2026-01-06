using System.ComponentModel;
using System.Globalization;

namespace GiViewer.App;

public class LocalizationProvider : INotifyPropertyChanged
{
    public delegate void LanguageChangedEventHandler();
    public event LanguageChangedEventHandler? LanguageChanged;
    public event PropertyChangedEventHandler? PropertyChanged;
    public CultureInfo Language
    {
        get => field;
        set
        {
            field = value;
            LanguageChanged?.Invoke();
        }
    } = CultureInfo.CurrentCulture;

    public static LocalizationProvider Instance { get; } = new();

    public string this[string key]
        => Resources.Languages.Strings.ResourceManager.GetString(key, Language) ?? key;
}
