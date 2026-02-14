using GiViewer.App.ViewModels;
using GiViewer.Core;
using System.Windows.Controls;
using Microsoft.Win32;

namespace GiViewer.App.Controls;

public partial class Editor : ContentControl
{
    private readonly EditorViewModel ViewModel = new();
    private NodeItem? RootNode;
    private GiFile? file;

    public Editor()
    {
        InitializeComponent();
        LocalizationProvider.AddListener(OnLanguageChanged);
        DataContext = ViewModel;
        ViewModel.IsEmpty = true;
    }

    private void OnLanguageChanged()
    {
        RootNode?.SetHeader(App.Translate("Editor.RootNode"));
        PropertyTitle.Text = App.Translate("Editor.Property.Title");
        EmptyTip.Text = App.Translate("Editor.Empty");
    }

    public static bool Unsaved { get; set; }
    public string FullPath { get; private set; } = string.Empty;
    public string FileName { get; private set; } = string.Empty;

    public async void OpenFile()
    {
        var picker = new OpenFileDialog();
        picker.Filter = App.Translate("Misc.GiFiles") + "|*.gip;*.gil;*.gia;*.gir";
        picker.CheckPathExists = true;
        if (picker.ShowDialog() != true) return;
        OpenFile(picker.FileName);
    }

    public async void OpenFile(string path)
    {
        try
        {
            CloseFile();
            file = GiFile.ReadFromFile(path);
            RootNode = NodeItem.FromNode(file.RootNode);
            RootNode.SetHeader(App.Translate("Editor.RootNode"));
            View.Items.Add(RootNode);
            ViewModel.IsEmpty = false;
            FullPath = path;
        }
        catch (Exception e)
        {
            var dialog = new ContentDialog { Title = "发生错误", Details = e.Message };
            _ = await dialog.ShowAsync(DialogKind.Ok);
        }
    }

    public void CloseFile()
    {
        if (RootNode is null) return;

        // TODO: 保存文件

        View.Items.Remove(RootNode);
        ViewModel.IsEmpty = true;
        FullPath = string.Empty;
        RootNode = null;
    }
}
