using GiViewer.Core.Serialization;
using System.Text;

namespace GiViewer.Core.Types;

public sealed class StringNode : ILengthNode
{
    private string value = string.Empty;
    internal StringNode(string value) => this.value = value;

    public WireType WireType => WireType.Length;
    public ValueKind Kind => ValueKind.String;

    public void SetValue(string value) => this.value = value;
    public string GetValue() => value;
    public INode Clone() => new StringNode(value);

    public int GetSize(int id)
    {
        if (value.Length == 0) return 0;
        int length = Encoding.UTF8.GetByteCount(value);
        return Varint.GetSize(id) + Varint.GetSize(length) + length;
    }

    public static StringNode Read(ref BufferReader reader, int totalSize) => new StringNode(reader.ReadString(totalSize));

    public void Write(ref BufferWriter writer, int id)
    {
        Varint.Write(ref writer, id);
        writer.WriteString(value);
    }
}
