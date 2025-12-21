namespace GiViewer.Core.Serialization;

internal readonly struct ProtoTag
{
    public int Id { get; init; }
    public WireType Type { get; init; }

    public static implicit operator ProtoTag(int varint)
    {
        int id = varint >> 3;
        WireType type = (WireType)(varint & 0b111);
        return new ProtoTag { Id = id, Type = type };
    }
}
