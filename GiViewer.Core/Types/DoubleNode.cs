using GiViewer.Core.Serialization;

namespace GiViewer.Core.Types;

public sealed class DoubleNode : IScalarNode
{
    private double value = 0;
    internal DoubleNode(double value) => this.value = value;

    public WireType WireType => WireType.Fixed64;
    public ValueKind Kind => ValueKind.Double;
    public void SetValue(double value) => this.value = value;
    public double GetValue() => value;
    public INode Clone() => new DoubleNode(value);

    public int GetSize(int id)
    {
        if (value == 0) return 0;
        return (id == 0 ? 0 : Varint.GetSize(id)) + 8;
    }

    public static DoubleNode Read(ref BufferReader reader) => new DoubleNode(reader.ReadDouble());

    public void Write(ref BufferWriter writer) => writer.WriteDouble(value);
}
