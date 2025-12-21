using GiViewer.Core.Serialization;
using GiViewer.Core.Types;

namespace GiViewer.Core;

public class GiFile
{
    public FileType Type { get; set; }
    public int Version { get; set; } = 1;
    public static int HeadMagicNumber => 0x0326;
    public static int TailMagicNumber => 0x0679;
    public required ObjectNode RootNode { get; set; }

    public static GiFile ReadFromFile(string path)
    {
        ReadOnlySpan<byte> data = File.ReadAllBytes(path);
        if (data.Length < 24) throw new InvalidDataException("文件过小，无法读取数据。");

        var reader = new BufferReader(data);

        // 文件大小
        int fileSize = reader.ReadInt32BE();
        if (fileSize + 4 != data.Length) throw new InvalidDataException("文件大小与头部信息不符。");

        // 版本编号
        int version = reader.ReadInt32BE();

        // 头部魔数
        int headMagic = reader.ReadInt32BE();
        if (headMagic != HeadMagicNumber) throw new InvalidDataException("文件头部魔数不匹配，可能不是有效的 GI 文件。");

        // 文件类型
        FileType type = reader.ReadInt32BE() switch
        {
            1 => FileType.Gip,
            2 => FileType.Gil,
            3 => FileType.Gia,
            4 => FileType.Gir,
            _ => FileType.Unknown
        };

        // 内容长度
        int length = reader.ReadInt32BE();
        if (length + 24 != data.Length) throw new InvalidDataException("内容长度与文件大小不符。");

        // 尾部魔数
        int current = reader.Position;
        reader.Seek(-4, SeekOrigin.End);
        int tailMagic = reader.ReadInt32BE();
        if (tailMagic != TailMagicNumber) throw new InvalidDataException("文件尾部魔数不匹配，可能不是有效的 GI 文件。");

        reader = reader.Slice(current, length);
        var root = ObjectNode.Read(ref reader, length);

        return new GiFile
        {
            Type = type,
            RootNode = root,
            Version = version
        };
    }

    public void WriteToFile(string path)
    {
        int size = RootNode.GetSize(0);
        var writer = new BufferWriter(new byte[size + 24]); // 文件元信息占 24 字节
        writer.WriteInt32BE(size + 20);        // 文件大小
        writer.WriteInt32BE(Version);          // 版本编号
        writer.WriteInt32BE(HeadMagicNumber);  // 头部魔数
        writer.WriteInt32BE((int)Type);        // 文件类型
        writer.WriteInt32BE(size);             // 内容长度
        RootNode.Write(ref writer, 0);         // 内部数据
        writer.WriteInt32BE(TailMagicNumber);  // 尾部魔数
        File.WriteAllBytes(path, writer.Span);
    }
}
