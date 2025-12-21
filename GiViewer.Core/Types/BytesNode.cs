using GiViewer.Core.Serialization;

namespace GiViewer.Core.Types;

public class BytesNode : ILengthNode
{
    public byte[] Data { get; set; }
    internal BytesNode(byte[] raw) => Data = raw;
    public WireType WireType => WireType.Length;
    public ValueKind Kind => ValueKind.Bytes;

    public INode Clone()
    {
        byte[] target = new byte[Data.Length];
        Buffer.BlockCopy(Data, 0, target, 0, Data.Length);
        return new BytesNode(target);
    }

    public int GetSize(int id)
    {
        if (Data.Length == 0) return 0;
        return Varint.GetSize(id) + Varint.GetSize(Data.Length) + Data.Length;
    }

    public static INode Read(ref BufferReader reader, int totalSize)
    {
        var span = reader.ReadSpan(totalSize);
        return new BytesNode(span.ToArray());
    }

    public void Write(ref BufferWriter writer, int id)
    {
        Varint.Write(ref writer, id);
        Varint.Write(ref writer, Data.Length);
        writer.WriteSpan(Data);
    }
}
