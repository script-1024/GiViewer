using GiViewer.App.ViewModels;
using GiViewer.Core;
using GiViewer.Core.Types;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace GiViewer.App.Controls;

public partial class NodeItem : TreeViewItem
{
    private readonly NodeItemViewModel ViewModel = new();

    public NodeItem()
    {
        InitializeComponent();
        DataContext = ViewModel;
    }

    public NodeItem(ValueKind kind) : this()
    {
        ValueKind = kind;
    }

    public void SetHeader(string header)
        => ViewModel.Header = header;

    public static T? FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
    {
        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);
            if (child is T t) return t;
            var result = FindVisualChild<T>(child);
            if (result != null) return result;
        }
        return null;
    }

    public ValueKind ValueKind
    {
        get => field;
        set
        {
            ViewModel.SetIconKind(value);
            field = value;
        }
    }

    public static NodeItem FromNode<T>(T node, string? header = null) where T : INode
    {
        var kind = node.Kind;
        var item = new NodeItem(kind);
        var vm = item.ViewModel;

        vm.Header = header;
        switch (node)
        {
            case IntegerNode iNode:
                vm.Extra = new NumberBox
                {
                    Domain = ValueDomain.Integer,
                    Value = iNode.GetValue<int>()
                };
                break;
            case FloatNode fNode:
                vm.Extra = new NumberBox
                {
                    Domain = ValueDomain.Real,
                    Value = fNode.GetValue()
                };
                break;
            case DoubleNode dNode:
                vm.Extra = new NumberBox
                {
                    Domain = ValueDomain.Real,
                    Value = dNode.GetValue()
                };
                break;
            case StringNode sNode:
                vm.Extra = new TextBox
                {
                    Padding = new Thickness(4, 6, 4, 6),
                    Text = sNode.GetValue()
                };
                break;
            case ObjectNode oNode:
                item.IsExpanded = true;
                if (oNode.Count == 0) vm.Extra = "(null)";
                foreach ((int id, INode child) in oNode.ToKeyValuePairs())
                {
                    item.AddChild(FromNode(child, id.ToString()));
                }
                break;
            case ListNode lNode:
                break;
        }

        return item;
    }
}
