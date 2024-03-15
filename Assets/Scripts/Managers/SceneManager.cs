using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 场景管理
/// </summary>
public class SceneManager : MonoSingleton<SceneManager>
{

    UnityAction<float> onProgress = null;
    public UnityAction onSceneLoadDone = null;

    // Use this for initialization
    protected override void OnStart()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
    //加载场景
    public void LoadScene(string name)
    {
        //启动加载场景的协程
        StartCoroutine(LoadSceneCoroutine(name));
    }
    //加载场景的协程
    IEnumerator LoadSceneCoroutine(string name)
    {
        Debug.LogFormat("LoadScene: {0}", name);
        //异步加载场景
        AsyncOperation async = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(name);
        async.allowSceneActivation = true;
        //给加载完成事件添加函数
        async.completed += LoadSceneCompleted;
        while (!async.isDone)
        {
            if (onProgress != null)
                onProgress(async.progress);
            yield return null;
        }
    }
    //加载完成事件,执行该函数
    private void LoadSceneCompleted(AsyncOperation obj)
    {
        if (onProgress != null)
            onProgress(1f);
        if (this.onSceneLoadDone != null)
        {
            this.onSceneLoadDone();
        }
        Debug.Log("LoadSceneCompleted:" + obj.progress);
    }
}
