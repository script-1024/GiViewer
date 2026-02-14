namespace GiViewer.App;

public class WeakEvent
{
    private readonly List<WeakReference<Action>> Handlers = [];

    public void AddListener(Action handler)
        => Handlers.Add(new WeakReference<Action>(handler));

    public IDisposable AddDisposableListener(Action handler)
    {
        var weakRef = new WeakReference<Action>(handler);
        Handlers.Add(weakRef);
        return new Finalizer(() => Handlers.Remove(weakRef));
    }

    public void Raise()
    {
        foreach (var weakRef in Handlers)
        {
            if (weakRef.TryGetTarget(out var handler)) handler.Invoke();
            else Handlers.Remove(weakRef);
        }
    }
}

public class WeakEvent<TEventArgs> where TEventArgs : EventArgs
{
    private readonly List<WeakReference<Action<object, TEventArgs>>> Handlers = [];

    public void AddListener(Action<object, TEventArgs> handler)
        => Handlers.Add(new WeakReference<Action<object, TEventArgs>>(handler));

    public IDisposable AddDisposableListener(Action<object, TEventArgs> handler)
    {
        var weakRef = new WeakReference<Action<object, TEventArgs>>(handler);
        Handlers.Add(weakRef);
        return new Finalizer(() => Handlers.Remove(weakRef));
    }

    public void Raise(object sender, TEventArgs args)
    {
        foreach (var weakRef in Handlers)
        {
            if (weakRef.TryGetTarget(out var handler)) handler.Invoke(sender, args);
            else Handlers.Remove(weakRef);
        }
    }
}
