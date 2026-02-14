using System.ComponentModel;

namespace GiViewer.App.ViewModels;
public class EditorViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    private void SetProperty<T>(ref T property, T value)
    {
        property = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(null));
    }

    public bool IsEmpty
    {
        get => field;
        set => SetProperty(ref field, value);
    }
}
