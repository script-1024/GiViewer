using System.Collections;
using GiViewer.Core.Serialization;

namespace GiViewer.Core.Types;

public sealed class ObjectNode : ICollectionNode, IEnumerable<INode>
{
    private int totalSize = 0;
    private readonly Dictionary<int, int> size = [];
    private readonly Dictionary<int, INode> data;
    internal ObjectNode(Dictionary<int, INode> children) => data = children;

    public INode this[int index]
    {
        get => data[index];
        set => data[index] = value;
    }

    public int Count => data.Count;
    public WireType WireType => WireType.Length;
    public ValueKind Kind => ValueKind.Object;

    public INode Clone()
    {
        var children = new Dictionary<int, INode>(data.Count);
        foreach (var (key, value) in data)
            children.Add(key, value.Clone());
        return new ObjectNode(children);
    }

    public int GetSize(int id)
    {
        totalSize = 0;

        foreach (var (key, value) in data)
        {
            int size = value.GetSize(key);
            this.size[key] = size;
            if (size == 0) continue;
            totalSize += size;
        }

        if (id == 0) return totalSize == 0 ? 0 : Varint.GetSize(totalSize) + totalSize;

        // 允许空对象（特例）
        return Varint.GetSize(id) + Varint.GetSize(totalSize) + totalSize;
    }

    public void Write(ref BufferWriter writer, int id)
    {
        if (id > 0)
        {
            Varint.Write(ref writer, id);
            Varint.Write(ref writer, totalSize);
        }

        foreach (var (key, value) in data)
        {
            if (size[key] == 0) continue;
            if (value is IScalarNode scalar)
            {
                Varint.Write(ref writer, key);
                scalar.Write(ref writer);
            }
            else if (value is ICollectionNode collection)
            {
                collection.Write(ref writer, key);
            }
        }
    }

    public bool TryAdd(int index, INode node)
    {
        if (data.TryAdd(index, node)) return true;
        INode current = data[index];
        if (current is ListNode listNode) return listNode.TryAdd(node);
        if (current.Kind != node.Kind) return false;
        var list = new List<INode> { current, node };
        data[index] = new ListNode(list, node.Kind);
        return true;
    }

    public bool TryRemove(int index, out INode? node) => data.Remove(index, out node);

    public IEnumerator<INode> GetEnumerator() => data.Values.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
