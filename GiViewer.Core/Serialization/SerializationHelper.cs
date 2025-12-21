namespace GiViewer.Core.Serialization;

public static class SerializationHelper
{
    public static bool IsUtf8String(BufferReader reader, int length)
    {
        int end = reader.Position + length;
        while (reader.Position < end)
        {
            byte b = reader.ReadByte();
            int needed = 0, codePoint = 0;

            // 1-byte
            if ((b & 0x80) == 0)
            {
                // 我们假设千星沙箱不允许奇匠输入控制字符
                if (b < 0x20 && b != '\n' && b != '\t' && b != '\r') return false;
                continue;
            }

            // 2-byte
            if ((b & 0xE0) == 0xC0)
            {
                needed = 1;
                codePoint = b & 0x1F;

                // 不接受过长字符串，首字节 0xC0、0xC1 始终无效
                if (codePoint == 0x00) return false;
            }
            // 3-byte
            else if ((b & 0xF0) == 0xE0)
            {
                needed = 2;
                codePoint = b & 0x0F;
            }
            else return false; // Unity 不支持代理对，因此 0xF0 ~ 0xFF 首字节无效

            // 若长度信息不匹配
            if (reader.Position + needed >= end) return false;

            // 累积后续字节
            for (int i = 0; i < needed; i++)
            {
                byte c = reader.ReadByte();
                if ((c & 0xC0) != 0x80) return false; // 后续字节必须以 0b10xxxxxx 开头
                codePoint = (codePoint << 6) | (c & 0x3F);
            }

            // 检查是否存在无效代码点
            if (needed == 1 && codePoint < 0x80) return false;
            if (needed == 2 && codePoint < 0x800) return false;

            // 不接受代理对 U+D800 ~ U+DFFF
            if (codePoint >= 0xD800 && codePoint <= 0xDFFF) return false;
        }
        return true; // 通过检查
    }

    public static bool IsObject(BufferReader reader, int length)
    {
        int end = reader.Position + length;
        while (reader.Position < end)
        {
            if (!Varint.TryRead(ref reader, out int tagValue)) return false;
            ProtoTag tag = tagValue;
            switch (tag.Type)
            {
                case WireType.Varint:
                    if (!Varint.TryRead(ref reader, out ulong _)) return false;
                    continue;
                case WireType.Fixed32:
                    if (!reader.Available(4)) return false;
                    reader.Seek(4, SeekOrigin.Current);
                    continue;
                case WireType.Fixed64:
                    if (!reader.Available(8)) return false;
                    reader.Seek(8, SeekOrigin.Current);
                    continue;
                case WireType.Length:
                    if (!Varint.TryRead(ref reader, out int size)) return false;
                    reader.Seek(size, SeekOrigin.Current);
                    continue;
                default:
                    return false;
            }
        }
        return true;
    }
}
