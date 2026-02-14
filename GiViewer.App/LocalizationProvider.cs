using System.Globalization;

namespace GiViewer.App;

public class LocalizationProvider
{
    // 使用弱引用避免内存泄漏
    private readonly WeakEvent LanguageChanged = new();

    public CultureInfo Language
    {
        get => field;
        set
        {
            field = value;
            LanguageChanged.Raise();
        }
    } = CultureInfo.CurrentCulture;

    public static LocalizationProvider Current { get; } = new();

    public string this[string key]
        => Resources.Languages.Strings.ResourceManager.GetString(key, Language) ?? key;

    public static void AddListener(Action handler)
    {
        Current.LanguageChanged.AddListener(handler);
        handler.Invoke();
    }
}
