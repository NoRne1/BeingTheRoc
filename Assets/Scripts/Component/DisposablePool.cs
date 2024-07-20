using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisposablePool
{
    private Dictionary<string, System.IDisposable> disposables = new Dictionary<string, System.IDisposable>();

    public void SaveDisposable(string id, System.IDisposable disposable)
    {
        disposables.Add(id, disposable);
    }

    public System.IDisposable GetDisposable(string id)
    {
        if (disposables.ContainsKey(id))
        {
            return disposables[id];
        } else
        {
            return null;
        }
    }

    public void RemoveDisposable(string id)
    {
        disposables.Remove(id);
    }

    public void CleanDisposables()
    {
        foreach(var disposable in disposables.Values)
        {
            disposable.Dispose();
        }
        disposables.Clear();
    }
}
