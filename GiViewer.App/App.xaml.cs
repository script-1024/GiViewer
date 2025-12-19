using System.Windows;

namespace GiViewer.App;

public partial class App : Application
{
    public static MainWindow Window => (MainWindow)Current.MainWindow;

    public static string Translate(string? key, string fallback = "")
        => LocalizationProvider.Instance[key ?? fallback];
}
