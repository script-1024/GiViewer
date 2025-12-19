using System.Windows;
using System.Windows.Controls;

namespace GiViewer.App.Controls;

public partial class ContentDialog : UserControl
{
    public ContentDialog()
    {
        InitializeComponent();
        DataContext = this;
    }

    public bool IsLightDismiss { get; set; }
    public string? Title { get; set; }
    public object? Details { get; set; }

    public EventHandler<DialogClosingEventArgs>? Closing;
    private TaskCompletionSource<ContentDialogResult>? tcs;
    private Button? closeButton, primaryButton, secondaryButton;

    public async Task<ContentDialogResult> ShowAsync(DialogKind kind = DialogKind.Ok)
    {
        SetDialogKind(kind);
        tcs = new TaskCompletionSource<ContentDialogResult>();
        TxtTitle.Text = App.Translate(Title, "Dialog.Title.Missing");
        if (Details != null)
        {
            if (Details is string str)
            {
                Details = new TextBlock()
                {
                    Text = App.Translate(str)
                };
            }
            Presenter.Content = Details;
        }

        await App.Window.OpenDialog(this);

        var result = await tcs.Task;
        Closing?.Invoke(this, new(result));

        primaryButton?.ClearValue(Button.StyleProperty);
        closeButton = primaryButton = secondaryButton = null;
        Presenter.Content = null;
        return result;
    }

    public void Close(ContentDialogResult result)
    {
        tcs?.TrySetResult(result);
    }

    private void SetDialogKind(DialogKind kind)
    {
        Button1.Content = Button2.Content = Button3.Content = string.Empty;

        switch (kind)
        {
            case DialogKind.Ok:
                Button1.Content = App.Translate("Dialog.Button.Ok");
                primaryButton = Button1;
                break;

            case DialogKind.OkCancel:
                Button1.Content = App.Translate("Dialog.Button.Cancel");
                Button2.Content = App.Translate("Dialog.Button.Ok");
                closeButton = Button1;
                primaryButton = Button2;
                break;

            case DialogKind.AbortRetryIgnore:
                Button1.Content = App.Translate("Dialog.Button.Ignore");
                Button2.Content = App.Translate("Dialog.Button.Retry");
                Button3.Content = App.Translate("Dialog.Button.Abort");
                closeButton = Button1;
                secondaryButton = Button2;
                primaryButton = Button3;
                break;

            case DialogKind.YesNoCancel:
                Button1.Content = App.Translate("Dialog.Button.Cancel");
                Button2.Content = App.Translate("Dialog.Button.No");
                Button3.Content = App.Translate("Dialog.Button.Yes");
                closeButton = Button1;
                secondaryButton = Button2;
                primaryButton = Button3;
                break;

            case DialogKind.YesNo:
                Button1.Content = App.Translate("Dialog.Button.No");
                Button2.Content = App.Translate("Dialog.Button.Yes");
                closeButton = Button1;
                primaryButton = Button2;
                break;

            case DialogKind.RetryCancel:
                Button1.Content = App.Translate("Dialog.Button.Cancel");
                Button2.Content = App.Translate("Dialog.Button.Retry");
                closeButton = Button1;
                primaryButton = Button2;
                break;

            case DialogKind.CancelTryContinue:
                Button1.Content = App.Translate("Dialog.Button.Continue");
                Button2.Content = App.Translate("Dialog.Button.Try");
                Button3.Content = App.Translate("Dialog.Button.Cancel");
                primaryButton = Button1;
                secondaryButton = Button2;
                closeButton = Button3;
                break;

            case DialogKind.SaveDiscardCancel:
                Button1.Content = App.Translate("Dialog.Button.Cancel");
                Button2.Content = App.Translate("Dialog.Button.Discard");
                Button3.Content = App.Translate("Dialog.Button.Save");
                closeButton = Button1;
                secondaryButton = Button2;
                primaryButton = Button3;
                break;

            default:
                throw new ArgumentException("Invalid dialog kind", nameof(kind));
        }

        Button1.Visibility = string.IsNullOrEmpty(Button1.Content?.ToString()) ?
            Visibility.Hidden : Visibility.Visible;
        Button2.Visibility = string.IsNullOrEmpty(Button2.Content?.ToString()) ?
            Visibility.Hidden : Visibility.Visible;
        Button3.Visibility = string.IsNullOrEmpty(Button3.Content?.ToString()) ?
            Visibility.Hidden : Visibility.Visible;

        closeButton?.Click += (s, e) => tcs?.TrySetResult(ContentDialogResult.Close);
        primaryButton?.Click += (s, e) => tcs?.TrySetResult(ContentDialogResult.Primary);
        secondaryButton?.Click += (s, e) => tcs?.TrySetResult(ContentDialogResult.Secondary);

        primaryButton?.SetResourceReference(Button.StyleProperty, "AccentButtonStyle");
    }
}

public class DialogClosingEventArgs(ContentDialogResult result) : EventArgs
{
    public ContentDialogResult Result => result;
}

public enum ContentDialogResult
{
    Close = 0,
    Primary = 1,
    Secondary = 2
}

public enum DialogKind
{
    Ok = 0,
    OkCancel = 1,
    AbortRetryIgnore = 2,
    YesNoCancel = 3,
    YesNo = 4,
    RetryCancel = 5,
    CancelTryContinue = 6,
    SaveDiscardCancel = 7
}