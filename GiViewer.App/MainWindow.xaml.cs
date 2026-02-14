using System.Windows;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using GiViewer.App.Controls;

namespace GiViewer.App;

public partial class MainWindow : Window
{
    internal ContentDialog? Dialog { get; private set; }
    private readonly Storyboard dialogFadeInStoryboard = new();
    private readonly Storyboard dialogFadeOutStoryboard = new();

    public MainWindow()
    {
        InitializeComponent();
        var fadeIn = new DoubleAnimation() { From = 0.0, To = 1.0 };
        var fadeOut = new DoubleAnimation() { From = 1.0, To = 0.0 };
        fadeIn.Duration = new Duration(TimeSpan.FromMilliseconds(250));
        fadeOut.Duration = new Duration(TimeSpan.FromMilliseconds(300));
        Storyboard.SetTargetName(fadeIn, SmokeFill.Name);
        Storyboard.SetTargetName(fadeOut, SmokeFill.Name);
        Storyboard.SetTargetProperty(fadeIn, new PropertyPath(Rectangle.OpacityProperty));
        Storyboard.SetTargetProperty(fadeOut, new PropertyPath(Rectangle.OpacityProperty));
        dialogFadeInStoryboard.Children.Add(fadeIn);
        dialogFadeOutStoryboard.Children.Add(fadeOut);
    }

    public async Task OpenDialog(ContentDialog dialog)
    {
        while (Dialog != null) await Task.Delay(250);
        GrayMask.Visibility = Visibility.Visible;
        dialogFadeInStoryboard.Begin(this);
        DialogWrapper.Visibility = Visibility.Visible;
        DialogWrapper.Child = Dialog = dialog;
        await Task.Delay(250);

        dialog.Closing += async (s, e) =>
        {
            dialogFadeOutStoryboard.Begin(this);
            DialogWrapper.Visibility = Visibility.Hidden;
            DialogWrapper.Child = null;
            await Task.Delay(300);
            GrayMask.Visibility = Visibility.Hidden;
            DialogWrapper.MinHeight = 0;
            DialogWrapper.MinWidth = 0;
            this.MinHeight = 240;
            this.MinWidth = 320;
            Dialog = null;
        };
    }

    private void CloseDialog(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        if (Dialog is null) return;
        if (Dialog.IsLightDismiss) Dialog.Close(ContentDialogResult.Close);
    }

    private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        DialogWrapper.MaxHeight = e.NewSize.Height - 60;
        DialogWrapper.MaxWidth = e.NewSize.Width - 80;
    }

    private async void Window_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        if (!Editor.Unsaved) return;
        e.Cancel = true;
        if (Dialog != null) return; // 避免重复调用对话框
        var dialog = new ContentDialog() { Title = "Dialog.Title.Unsave", Details = "Dialog.Details.Unsave" };
        var result = await dialog.ShowAsync(DialogKind.SaveDiscardCancel);
        
        switch (result)
        {
            case ContentDialogResult.Secondary:
                this.Closing -= Window_Closing;
                Close();
                break;

            default:
                break;
        }
    }
}
