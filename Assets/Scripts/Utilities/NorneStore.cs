using System;
using System.Collections.Generic;
using UniRx;
using System.Linq;
using Newtonsoft.Json;

public interface IStorable
{
    string StorableCategory { get; }
    string Identifier { get; }
}

public class SafeDictionaryCache<Key, Value> where Key : IEquatable<Key>
{
    private readonly Dictionary<Key, Value> _dic = new Dictionary<Key, Value>();
    private readonly object _lock = new object();

    public System.Collections.Generic.Dictionary<Key, Value>.KeyCollection Keys
    {
        get { return _dic.Keys; }
    }

    public Value this[Key key]
    {
        get
        {
            lock (_lock)
            {
                return _dic[key];
            }
        }
        set
        {
            lock (_lock)
            {
                _dic[key] = value;
            }
        }
    }

    public void Remove(Key key)
    {
        if (_dic.ContainsKey(key))
        {
            lock (_lock)
            {
                _dic.Remove(key);
            }
        }
    }

    public void RemoveAll()
    {
        lock (_lock)
        {
            _dic.Clear();
        }
    }
}

public class NorneStore
{
    private static NorneStore _instance;
    public static NorneStore Instance => _instance ?? (_instance = new NorneStore());

    private SafeDictionaryCache<string, object> _datas = new SafeDictionaryCache<string, object>();
    private SafeDictionaryCache<string, DateTime> _exts = new SafeDictionaryCache<string, DateTime>();

    public void Purge()
    {
        _datas.RemoveAll();
        _exts.RemoveAll();
    }

    public IObservable<T> ObservableObject<T>(T storable) where T : IStorable
    {
        if (string.IsNullOrEmpty(storable.Identifier))
        {
            return new NorneRelay<T>(storable);
        }

        return ObservableObject<T>(storable.StorableCategory, storable.Identifier, storable);
    }

    public IObservable<T> ObservableObject<T>(string category, string identifier, T defaultValue) where T : IStorable
    {
        var key = $"{category}-{identifier}";
        if (_datas[key] is NorneAcceptableRelay<T> relay)
        {
            return relay;
        }
        else
        {
            var newRelay = new NorneAcceptableRelay<T>(defaultValue);
            _datas[key] = newRelay;
            return newRelay;
        }
    }

    public void Remove<T>(T storable) where T : IStorable
    {
        var key = $"{storable.StorableCategory}-{storable.Identifier}";
        _datas.Remove(key);
    }

    public void RemoveAll()
    {
        _datas.RemoveAll();
        _exts.RemoveAll();
    }
    public void Update<T>(T storable, bool isFull = false) where T : IStorable, new()
    {
        if (string.IsNullOrEmpty(storable.Identifier))
        {
            return;
        }

        var key = $"{storable.StorableCategory}-{storable.Identifier}";

        if(isFull)
        {
            _exts[key] = DateTime.Now;
        }
        NorneAcceptableRelay<T> relay;
        if (_datas.Keys.ToList().Contains(key) && _datas[key] is NorneAcceptableRelay<T>)
        {
            relay = (NorneAcceptableRelay<T>)_datas[key];
        } else
        {
            relay = (NorneAcceptableRelay<T>)new NorneAcceptableRelay<T>(storable);
            _datas[key] = relay;
            return;
        }

        if (isFull)
        {
            relay.Accept(storable);
        }
        else
        {
            Dictionary<string, object> oldDic = relay.Value.ToDictionary();
            Dictionary<string, object> newDic = storable.ToDictionary();
            if (oldDic != null && newDic != null)
            {
                oldDic.RecursiveMerge(newDic);
            }
            else
            {
                return;
            }

            try
            {
                T newStorable = GameUtil.Instance.CreateObjectFromDictionary<T>(oldDic);
                relay.Accept(newStorable);
            }
            catch
            {
                // 处理序列化/反序列化错误
            }
        }
    }

    public bool IsObjectFullUpdatedRecently(IStorable storable)
    {
        if (string.IsNullOrEmpty(storable.Identifier))
        {
            return false;
        }

        var key = $"{storable.StorableCategory}-{storable.Identifier}";
        if (_exts[key] != default)
        {
            return (DateTime.Now - _exts[key]).TotalSeconds < 300;
        }
        return false;
    }
}

public class NorneRelay<T> : IObservable<T>
{
    private BehaviorSubject<T> _subject;

    public T Value => _subject.Value;

    public NorneRelay(T value)
    {
        _subject = new BehaviorSubject<T>(value);
    }

    public void Accept(T value)
    {
        _subject.OnNext(value);
    }

    public IObservable<T> AsObservable()
    {
        return _subject.AsObservable();
    }

    public IDisposable Subscribe(IObserver<T> observer)
    {
        return _subject.AsObservable().Subscribe(observer);
    }
}

public class NorneAcceptableRelay<T> : NorneRelay<T>
{
    public NorneAcceptableRelay(T value) : base(value)
    {
    }

    public new void Accept(T value)
    {
        base.Accept(value);
    }

    public NorneRelay<T> AsNorneRelay()
    {
        return this;
    }
}

