using UnityEngine;

/// <summary>
/// 单例的MonoBehaviour类
/// </summary>
/// <typeparam name="T"></typeparam>
public abstract class MonoSingleton<T> : MonoBehaviour where T : MonoBehaviour
{
    //是否是全局对象
    public bool global = true;
    //泛型实例
    static T instance;
    public static T Instance
    {
        get
        {
            if (instance == null)
            {
                instance = (T)FindObjectOfType<T>();
            }
            return instance;
        }

    }

    void Awake()
    {
        //不销毁对象
        if (global)
        {
            if (instance != null && instance != this.gameObject.GetComponent<T>())
            {
                //已经存在实例且不是当前物体绑定的,就销毁他
                Destroy(this.gameObject);
                return;
            }
            DontDestroyOnLoad(this.gameObject);
            //在初始化脚本时就赋值,避免没有调用instance就创建两个对象
            instance = this.gameObject.GetComponent<T>();
        }
        //初始化方法
        this.OnStart();
    }

    protected virtual void OnStart()
    {

    }
}