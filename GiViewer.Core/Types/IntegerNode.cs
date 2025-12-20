using GiViewer.Core.Serialization;

namespace GiViewer.Core.Types;

public sealed class IntegerNode : IScalarNode
{
    private ulong value = 0;
    internal IntegerNode(ulong value) => this.value = value;

    public WireType WireType => WireType.Varint;
    public ValueKind Kind => ValueKind.Integer;

    public void SetValue(long value, bool zigzag = false)
    {
        if (value < 0 && zigzag == false)
            throw new ArgumentOutOfRangeException(nameof(value), $"不接受负值，但得到了 \"{value}\"");
        this.value = zigzag ? Varint.ZigZagEncode(value) : (ulong)value;
    }

    public void SetValue(ulong value) => this.value = value;

    public T GetValue<T>(bool zigzag = false) where T : unmanaged
        => Varint.CastTo<T>(zigzag ? (ulong)Varint.ZigZagDecode(value) : value);

    public INode Clone() => new IntegerNode(value);

    public int GetSize(int id)
    {
        if (value == 0) return 0;
        return (id == 0 ? 0 : Varint.GetSize(id)) + Varint.GetSize(value);
    }

    public void Write(ref BufferWriter writer) => Varint.Write(ref writer, value);
}
