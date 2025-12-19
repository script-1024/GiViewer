using System.ComponentModel;

namespace GiViewer.App;

public class LocalizationProvider : INotifyPropertyChanged
{
    public static LocalizationProvider Instance { get; } = new();

    public string this[string key] =>
        Resources.Languages.Strings.ResourceManager.GetString(key) ?? key;

    public event PropertyChangedEventHandler? PropertyChanged;

    public void Refresh()
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(null));
    }
}
