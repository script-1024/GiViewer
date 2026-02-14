namespace GiViewer.App;

public sealed class Finalizer(Action callback) : IDisposable
{
    private readonly Action finalize = callback;
    private bool disposed = false;

    public void Dispose()
    {
        if (disposed) return;
        finalize();
        disposed = true;
    }
}
