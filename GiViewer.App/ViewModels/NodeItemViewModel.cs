using GiViewer.Core;
using System.ComponentModel;
using System.Windows.Media;

namespace GiViewer.App.ViewModels;
public class NodeItemViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    private void SetProperty<T>(ref T property, T value)
    {
        property = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(null));
    }

    public string? Header
    {
        get => field;
        set => SetProperty(ref field, value);
    }

    public Geometry? IconData
    {
        get => field;
        set => SetProperty(ref field, value);
    }

    public Brush? IconBrush
    {
        get => field;
        set => SetProperty(ref field, value);
    }

    public object? Extra
    {
        get => field;
        set => SetProperty(ref field, value);
    }

    public void SetIconKind(ValueKind kind)
        => (IconData, IconBrush) = IconHelper.GetValues(kind);
}

internal static class IconHelper
{
    public static (Geometry, Brush) GetValues(ValueKind kind)
    {
        return kind switch
        {
            ValueKind.Integer => (IntegerGeometry, IntegerBrush),
            ValueKind.Float => (FloatGeometry, FloatBrush),
            ValueKind.Double => (DoubleGeometry, DoubleBrush),
            ValueKind.String => (StringGeometry, StringBrush),
            ValueKind.Object => (ObjectGeometry, ObjectBrush),
            ValueKind.Bytes => (BytesGeometry, BytesBrush),
            ValueKind.List => (ListGeometry, ListBrush),
            _ => throw new ArgumentOutOfRangeException(nameof(kind))
        };
    }

    private static readonly Geometry IntegerGeometry = Geometry.Parse("M15 6H33v6H27V36h6v6H15V36h6V12H15Z");
    private static readonly Brush IntegerBrush = new SolidColorBrush(Color.FromRgb(62, 85, 173));

    private static readonly Geometry FloatGeometry = Geometry.Parse("M12 6H36v6H18v9H30v6H18V42H12Z");
    private static readonly Brush FloatBrush = new SolidColorBrush(Color.FromRgb(204, 158, 41));

    private static readonly Geometry DoubleGeometry = Geometry.Parse("M12 6h12q12 0 12 12v12q0 12-12 12H12Zm6 30h6q6 0 6-6V18q0-6-6-6H18Z");
    private static readonly Brush DoubleBrush = new SolidColorBrush(Color.FromRgb(66, 110, 46));

    private static readonly Geometry StringGeometry = Geometry.Parse("M13 28q-9 0-9-9t9-9 9 9-9 21L9 37q6-8 4-9m22 0q-9 0-9-9t9-9 9 9-9 21l-4-3q6-8 4-9Z");
    private static readonly Brush StringBrush = new SolidColorBrush(Color.FromRgb(22, 22, 22));

    private static readonly Geometry ObjectGeometry = Geometry.Parse("M21 6H18Q9 6 9 15v6H6v6h3v6q0 9 9 9h3V36H18q-3 0-3-3V15q0-3 3-3h3Zm6 0h3q9 0 9 9v6h3v6H39v6q0 9-9 9H27V36h3q3 0 3-3V15q0-3-3-3H27Z");
    private static readonly Brush ObjectBrush = new SolidColorBrush(Color.FromRgb(102, 102, 102));

    private static readonly Geometry BytesGeometry = Geometry.Parse("M24 18a3 3 90 00-3 3v6a3 3 90 003 3 3 3 90 003-3V21a3 3 90 00-3-3m0-3a6 6 90 016 6v6a6 6 90 01-6 6 6 6 90 01-6-6V21a6 6 90 016-6m6-3h6V36H30v6H42V6H30ZM18 12H12V36h6v6H6V6H18Z");
    private static readonly Brush BytesBrush = new SolidColorBrush(Color.FromRgb(173, 29, 29));

    private static readonly Geometry ListGeometry = Geometry.Parse("M30 12h6V36H30v6H42V6H30ZM18 12H12V36h6v6H6V6H18Z");
    private static readonly Brush ListBrush = new SolidColorBrush(Color.FromRgb(224, 97, 1));
}
