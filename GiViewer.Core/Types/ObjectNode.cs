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

    /// <summary>
    /// 尝试读取一个对象节点，若失败则返回一个 <see cref="BytesNode"/>
    /// </summary>
    /// <returns>
    /// 成功 - <see cref="ObjectNode"/><br/>
    /// 失败 - <see cref="BytesNode"/>
    /// </returns>
    public static INode Read(ref BufferReader reader, int totalSize)
    {
        var root = new ObjectNode([]);
        int start = reader.Position, end = start + totalSize;

        while (reader.Position < end)
        {
            ProtoTag tag = Varint.Read<int>(ref reader);
            switch (tag.Type)
            {
                case WireType.Varint:
                    root[tag.Id] = IntegerNode.Read(ref reader);
                    continue;
                case WireType.Fixed32:
                    root[tag.Id] = FloatNode.Read(ref reader);
                    continue;
                case WireType.Fixed64:
                    root[tag.Id] = DoubleNode.Read(ref reader);
                    continue;
            }

            // 无效类型
            if (tag.Type != WireType.Length)
            {
                reader.Seek(start, SeekOrigin.Begin);
                return BytesNode.Read(ref reader, totalSize);
            }

            int length = Varint.Read<int>(ref reader);

            if (length == 0)
            {
                root.TryAdd(tag.Id, new ObjectNode([])); // null 节点
                continue;
            }

            // 我们需要判断 WireType.Length 具体是什么类型的数据
            // 无法主动读取 ListNode，我们无法事先得知哪些编号会重复出现，因此这部分由 TryAdd 负责处理
            if (SerializationHelper.IsUtf8String(reader, length))
            {
                root.TryAdd(tag.Id, StringNode.Read(ref reader, length));
            }
            else if (SerializationHelper.IsObject(reader, length))
            {
                root.TryAdd(tag.Id, ObjectNode.Read(ref reader, length));
            }
            else
            {
                root.TryAdd(tag.Id, BytesNode.Read(ref reader, length));
            }
        }
        return root;
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
            if (value is IScalarNode snode)
            {
                Varint.Write(ref writer, key);
                snode.Write(ref writer);
            }
            else if (value is ILengthNode lnode)
            {
                lnode.Write(ref writer, key);
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
