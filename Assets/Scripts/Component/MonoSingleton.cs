using UnityEngine;

/// <summary>
/// 单例的MonoBehaviour类
/// </summary>
/// <typeparam name="T"></typeparam>
public abstract class MonoSingleton<T> : MonoBehaviour where T : MonoBehaviour
{
    public bool global = true;
    private static T instance;
    private static bool isInitialized = false;
    private static readonly object lockObject = new object();

    public static T Instance
    {
        get
        {
            if (!isInitialized)
            {
                Debug.LogError($"Instance of {typeof(T)} is not initialized yet. Make sure it's accessed after Awake.");
            }
            return instance;
        }
    }

    protected virtual void Awake()
    {
        lock (lockObject)
        {
            if (instance == null)
            {
                instance = this as T;
                isInitialized = true;

                if (global)
                {
                    DontDestroyOnLoad(this.gameObject);
                }
            }
            else if (instance != this)
            {
                Destroy(gameObject);
                return;
            }

            OnStart();
        }
    }

    protected virtual void OnStart()
    {
        // 子类可以重写此方法进行初始化
    }
}