using GiViewer.Core.Serialization;
using System.Collections;

namespace GiViewer.Core.Types;

public class ListNode : ICollectionNode
{
    private int totalSize = 0;
    private readonly List<int> size = [];
    private readonly List<INode> data;
    internal ListNode(List<INode> children, ValueKind kind)
    {
        ChildrenKind = kind;
        data = children;
    }

    public int Count => data.Count;
    public bool Packed => ChildrenKind switch {
        ValueKind.Integer or ValueKind.Float or ValueKind.Double => true,
        _ => false
    };
    
    public INode this[int index]
    {
        get => data[index];
        set => data[index] = value;
    }
    public WireType WireType => WireType.Length;
    public ValueKind Kind => ValueKind.List;
    public ValueKind ChildrenKind { get; }

    public INode Clone()
    {
        var children = new List<INode>(data.Count);
        foreach (var node in data) children.Add(node.Clone());
        return new ListNode(children, ChildrenKind);
    }

    public int GetSize(int id)
    {
        totalSize = 0;

        for (int i = 0; i < data.Count; i++)
        {
            int size = data[i].GetSize(Packed ? 0 : id);
            this.size[i] = size;
            if (size == 0)
            {
                if (!Packed) continue;
                size = 1; // 如果是元组，即便结果是零也要保留
            }
            totalSize += size;
        }

        if (Packed == false || totalSize == 0) return totalSize;
        return Varint.GetSize(id) + Varint.GetSize(totalSize) + totalSize;
    }

    public void Write(ref BufferWriter writer, int id)
    {
        if (totalSize == 0) return;

        if (Packed)
        {
            Varint.Write(ref writer, id);
            Varint.Write(ref writer, totalSize);
            for (int i = 0; i < data.Count; i++)
            {
                var node = (IScalarNode)data[i];
                node.Write(ref writer);
            }
            return;
        }

        for (int i = 0; i < data.Count; i++)
        {
            if (size[i] == 0) continue;
            if (data[i] is ILengthNode node)
                node.Write(ref writer, id);
        }
    }

    public bool TryAdd(INode node)
    {
        if (node.Kind != ChildrenKind) return false;
        data.Add(node);
        return true;
    }

    public bool TryRemove(int index, out INode? node)
    {
        node = null;
        if (index < 0 || index >= data.Count) return false;
        node = data[index];
        data.RemoveAt(index);
        return true;
    }

    public IEnumerator<INode> GetEnumerator() => data.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
