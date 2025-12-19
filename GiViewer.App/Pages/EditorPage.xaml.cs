using System.Windows;
using System.Windows.Controls;
using GiViewer.App.Controls;

namespace GiViewer.App.Pages;

public partial class EditorPage : Page
{
    public EditorPage()
    {
        InitializeComponent();
    }

    private void MenuItem_Click(object sender, RoutedEventArgs e)
    {
        string tag = ((MenuItem)sender).Tag.ToString() ?? string.Empty;
        switch (tag)
        {
            case "File.Exit":
                App.Window.Close();
                break;

            default:
                break;
        }
    }
}
