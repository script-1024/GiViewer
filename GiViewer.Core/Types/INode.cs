using GiViewer.Core.Serialization;

namespace GiViewer.Core.Types;

/// <summary>
/// 表示一个 Proto 节点的公共接口
/// </summary>
public interface INode
{
    /// <summary>
    /// 取得线路类型
    /// </summary>
    public WireType WireType { get; }

    /// <summary>
    /// 取得数据类型
    /// </summary>
    public ValueKind Kind { get; }

    /// <summary>
    /// 对节点深层拷贝
    /// </summary>
    /// <returns>与原节点类型相同的新实例</returns>
    public INode Clone();

    /// <summary>
    /// 取得所需的缓冲区大小
    /// </summary>
    public int GetSize(int id);
}

/// <summary>
/// 表示一个标量 Proto 节点
/// </summary>
public interface IScalarNode : INode
{
    /// <summary>
    /// 从缓冲区读取一个节点
    /// </summary>
    /// <param name="reader"></param>
    /// <returns></returns>
    public abstract static INode Read(ref BufferReader reader);

    /// <summary>
    /// 将节点写入缓冲区中
    /// </summary>
    public void Write(ref BufferWriter writer);
}

/// <summary>
/// 表示一个具有长度信息的 Proto 节点
/// </summary>
public interface ILengthNode : INode
{
    /// <summary>
    /// 从缓冲区读取一个指定长度的节点
    /// </summary>
    public abstract static INode Read(ref BufferReader reader, int totalSize);

    /// <summary>
    /// 将节点写入缓冲区中
    /// </summary>
    /// <param name="id">
    /// 集合本身的编号。<br/>
    /// 对于 <see cref="ObjectNode"/>，若此值为 0，会将自身视作根节点，从而避免输出最开始的长度字段；<br/>
    /// 对于 <see cref="ListNode"/>，它需要根据此值来为 repeated 字段重复输出其编号。
    /// </param>
    public void Write(ref BufferWriter writer, int id);
}

/// <summary>
/// 表示一个节点集合
/// </summary>
public interface ICollectionNode : ILengthNode, IEnumerable<INode>
{
    /// <summary>
    /// 取得集合内的元素数量
    /// </summary>
    public int Count { get; }

    /// <summary>
    /// 取得指定索引或编号的子节点
    /// </summary>
    public INode this[int index] { get; }
}
