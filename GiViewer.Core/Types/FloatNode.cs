using GiViewer.Core.Serialization;

namespace GiViewer.Core.Types;

public sealed class FloatNode : IScalarNode
{
    private float value = 0;
    internal FloatNode(float value) => this.value = value;

    public WireType WireType => WireType.Fixed32;
    public ValueKind Kind => ValueKind.Float;
    public void SetValue(float value) => this.value = value;
    public float GetValue() => value;
    public INode Clone() => new FloatNode(value);

    public int GetSize(int id)
    {
        if (value == 0) return 0;
        return (id == 0 ? 0 : Varint.GetSize(id)) + 4;
    }

    public static INode Read(ref BufferReader reader) => new FloatNode(reader.ReadFloat());

    public void Write(ref BufferWriter writer) => writer.WriteFloat(value);
}
