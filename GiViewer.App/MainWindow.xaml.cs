using System.Windows;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using GiViewer.App.Controls;

namespace GiViewer.App;

public partial class MainWindow : Window
{
    public bool Unsaved { get; set; }
    internal ContentDialog? Dialog { get; private set; }
    private readonly Storyboard dialogFadeInStoryboard = new();
    private readonly Storyboard dialogFadeOutStoryboard = new();

    public MainWindow()
    {
        InitializeComponent();
        DataContext = this;

        var fadeIn = new DoubleAnimation() { From = 0.0, To = 1.0 };
        var fadeOut = new DoubleAnimation() { From = 1.0, To = 0.0 };
        fadeIn.Duration = new Duration(TimeSpan.FromMilliseconds(250));
        fadeOut.Duration = new Duration(TimeSpan.FromMilliseconds(300));
        Storyboard.SetTargetName(fadeIn, GrayMaskRect.Name);
        Storyboard.SetTargetName(fadeOut, GrayMaskRect.Name);
        Storyboard.SetTargetProperty(fadeIn, new PropertyPath(Rectangle.OpacityProperty));
        Storyboard.SetTargetProperty(fadeOut, new PropertyPath(Rectangle.OpacityProperty));
        dialogFadeInStoryboard.Children.Add(fadeIn);
        dialogFadeOutStoryboard.Children.Add(fadeOut);
    }

    public async Task OpenDialog(ContentDialog dialog)
    {
        while (Dialog != null) await Task.Delay(250);

        this.MinHeight = 280;
        this.MinWidth = 400;
        GrayMask.Visibility = Visibility.Visible;
        dialogFadeInStoryboard.Begin(this);
        ContentBorder.Visibility = Visibility.Visible;
        DialogWrapper.Content = Dialog = dialog;
        await Task.Delay(250);

        dialog.Closing += async (s, e) =>
        {
            dialogFadeOutStoryboard.Begin(this);
            ContentBorder.Visibility = Visibility.Hidden;
            DialogWrapper.Content = null;
            await Task.Delay(300);
            GrayMask.Visibility = Visibility.Hidden;
            ContentBorder.MinHeight = 0;
            ContentBorder.MinWidth = 0;
            this.MinHeight = 240;
            this.MinWidth = 320;
            Dialog = null;
        };
    }

    private void CloseDialog(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        if (Dialog is null) return;
        if (Dialog.IsLightDismiss) Dialog.Close(Controls.ContentDialogResult.Close);
    }

    private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        ContentBorder.MaxHeight = e.NewSize.Height - 60;
        ContentBorder.MaxWidth = e.NewSize.Width - 80;
    }

    private async void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
        if (!Unsaved) return;
        e.Cancel = true;
        if (Dialog != null) return; // 避免重复调用对话框
        var dialog = new ContentDialog() { Title = "Dialog.Title.Unsave", Details = "Dialog.Details.Unsave" };
        var result = await dialog.ShowAsync(DialogKind.SaveDiscardCancel);
        
        switch (result)
        {
            case ContentDialogResult.Secondary:
                Unsaved = false;
                Close();
                break;

            default:
                break;
        }
    }
}
