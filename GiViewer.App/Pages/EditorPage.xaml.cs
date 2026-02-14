using GiViewer.App.Controls;
using System.Windows;
using System.Windows.Controls;

namespace GiViewer.App.Pages;

public partial class EditorPage : Page
{
    public EditorPage()
    {
        InitializeComponent();
        LocalizationProvider.AddListener(OnLanguageChanged);
    }

    private void OnLanguageChanged()
    {
        foreach (var i in Menu.Items)
        {
            if (i is not MenuItem item) continue;
            item.Header = App.Translate(item.Tag.ToString());
            foreach (var j in item.Items)
            {
                if (j is not MenuItem subitem) continue;
                subitem.Header = App.Translate(subitem.Tag.ToString());
            }
        }
    }

    private void MenuItem_Click(object sender, RoutedEventArgs e)
    {
        string tag = ((MenuItem)sender).Tag.ToString() ?? string.Empty;
        var testDialog = new ContentDialog {
            Title = "Title",
            Details = "Description",
            IsLightDismiss = true
        };

        switch (tag)
        {
            case "Menu.File.Open":
                Editor.OpenFile();
                break;

            case "Menu.File.Close":
                Editor.CloseFile();
                break;

            case "Menu.File.Exit":
                App.Window.Close();
                break;
                
            default:
                break;
        }
    }
}
