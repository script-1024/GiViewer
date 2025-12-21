using System.Numerics;

namespace GiViewer.Core.Serialization;

public static class Varint
{
    private static object Cast(Type type, ulong varint)
    {
        type = Nullable.GetUnderlyingType(type) ?? type;
        if (type.IsEnum)
        {
            object fallback = Activator.CreateInstance(type)!;
            object value = Cast(Enum.GetUnderlyingType(type), varint);
            return Enum.IsDefined(type, value) ? Enum.ToObject(type, value) : fallback;
        }
        return type switch
        {
            var t when t == typeof(bool) => varint != 0,
            var t when t == typeof(sbyte) => (sbyte)varint,
            var t when t == typeof(byte) => (byte)varint,
            var t when t == typeof(short) => (short)varint,
            var t when t == typeof(ushort) => (ushort)varint,
            var t when t == typeof(int) => (int)varint,
            var t when t == typeof(uint) => (uint)varint,
            var t when t == typeof(long) => (long)varint,
            var t when t == typeof(ulong) => varint,
            _ => throw new ArgumentException("Unsupported type", nameof(type))
        };
    }

    public static T Cast<T>(ulong varint) where T : unmanaged
        => (T)Cast(typeof(T), varint);

    private static bool TryCast(Type type, ulong varint, out object result)
    {
        type = Nullable.GetUnderlyingType(type) ?? type;
        result = Activator.CreateInstance(type)!; // 默认值
        if (type.IsEnum)
        {
            if (!TryCast(Enum.GetUnderlyingType(type), varint, out object retval)) return false;
            if (!Enum.IsDefined(type, retval)) return false;
            result = Enum.ToObject(type, retval);
            return true;
        }

        object? tmp = type switch
        {
            var t when t == typeof(bool) => varint != 0,
            var t when t == typeof(sbyte) => (sbyte)varint,
            var t when t == typeof(byte) => (byte)varint,
            var t when t == typeof(short) => (short)varint,
            var t when t == typeof(ushort) => (ushort)varint,
            var t when t == typeof(int) => (int)varint,
            var t when t == typeof(uint) => (uint)varint,
            var t when t == typeof(long) => (long)varint,
            var t when t == typeof(ulong) => varint,
            _ => null
        };

        if (tmp is null) return false;
        result = tmp;
        return true;
    }

    public static bool TryCast<T>(ulong varint, out T result) where T : unmanaged
    {
        result = default;
        if (!TryCast(typeof(T), varint, out object retval)) return false;
        result = (T)retval;
        return true;
    }

    private static ulong Read(ref BufferReader reader)
    {
        ulong value = 0;
        for (int i = 0; i < 10; i++)
        {
            byte data = reader.ReadByte();
            value |= (ulong)(data & 0x7F) << (i * 7);
            if ((data & 0x80) == 0) return value;
        }
        throw new InvalidDataException("无效的 Varint 编码");
    }

    public static T Read<T>(ref BufferReader reader) where T : unmanaged
        => Cast<T>(Read(ref reader));

    public static bool TryRead<T>(ref BufferReader reader, out T result) where T : unmanaged
    {
        ulong value = 0;
        result = default;
        for (int i = 0; i < 10; i++)
        {
            byte data = reader.ReadByte();
            value |= (ulong)(data & 0x7F) << (i * 7);
            if ((data & 0x80) == 0)
            {
                if (!TryCast(value, out T retval)) return false;
                result = retval;
                return true;
            }
        }
        return false;
    }

    public static void Write(ref BufferWriter writer, ulong value)
    {
        do
        {
            byte data = (byte)(value & 0x7F);
            value >>= 7;
            if (value != 0) data |= 0x80;
            writer.WriteByte(data);
        }
        while (value != 0);
    }

    public static void Write(ref BufferWriter writer, long value, bool zigzag = false)
    {
        if (value < 0 && zigzag == false)
            throw new ArgumentOutOfRangeException(nameof(value), $"不接受负值，但得到了 \"{value}\"");
        Write(ref writer, zigzag ? ZigZagEncode(value) : (ulong)value);
    }

    public static int GetSize(int value, bool zigzag = false)
    {
        if (value < 0 && zigzag == false)
            throw new ArgumentOutOfRangeException(nameof(value), $"不接受负值，但得到了 \"{value}\"");
        return GetSize(zigzag ? ZigZagEncode(value) : (uint)value);
    }

    public static int GetSize(uint value)
    {
        if (value == 0) return 1;
        int bits = 32 - BitOperations.LeadingZeroCount(value);
        return (bits + 6) / 7;
    }

    public static int GetSize(long value, bool zigzag = false)
    {
        if (value < 0 && zigzag == false)
            throw new ArgumentOutOfRangeException(nameof(value), $"不接受负值，但得到 \"{value}\"");
        return GetSize(zigzag ? ZigZagEncode(value) : (ulong)value);
    }

    public static int GetSize(ulong value)
    {
        if (value == 0) return 1;
        int bits = 64 - BitOperations.LeadingZeroCount(value);
        return (bits + 6) / 7;
    }

    public static uint ZigZagEncode(int value)
        => (uint)((value << 1) ^ (value >> 31));

    public static ulong ZigZagEncode(long value)
        => (ulong)((value << 1) ^ (value >> 63));

    public static int ZigZagDecode(uint value)
        => (int)((value >> 1) ^ (~(value & 1) + 1));

    public static long ZigZagDecode(ulong value)
        => (long)((value >> 1) ^ (~(value & 1) + 1));
}
