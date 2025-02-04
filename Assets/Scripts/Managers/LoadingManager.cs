using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Assets.Scripts.Sound;

public class LoadingManager : MonoSingleton<LoadingManager>
{

    //健康游戏忠告
    public GameObject UITips;
    //加载界面
    public GameObject UILoading;
    //登录界面
    public GameObject UIMain;
    //加载界面中的进度条
    public Slider progressBar;
    //进度条下面的提示文本(第一次加载较慢)
    public TextMeshProUGUI progressText;
    //进度条上面的进度显示(50%)
    public TextMeshProUGUI progressNumber;

    // Use this for initialization
    IEnumerator Start()
    {
        //加载定义文件中的数据
        //DataManager.Instance.Load();

        //先显示忠告界面
        UITips.SetActive(true);
        UILoading.SetActive(false);
        UIMain.SetActive(false);
        //等待两秒显示加载界面
        yield return new WaitForSeconds(1f);
        UILoading.SetActive(true);
        //等待一秒隐藏忠告界面
        yield return new WaitForSeconds(0.5f);
        UITips.SetActive(false);
        //然后加载配置的数据
        yield return DataManager.Instance.LoadData();

        SoundManager.Instance.Init();
        //第一次播放音乐
        SoundManager.Instance.PlayMusic(SoundDefine.Music_Login);
        //在此处初始化用户的自定义音量(不知道为什么,在播放音乐之前)


        //假的进度条模拟器
        for (float i = 0; i < 100;)
        {
            i += Random.Range(0.1f, 1.5f);
            i = Mathf.Min(i, 100);
            //改变进度条的值
            progressBar.value = i;
            //改变进度条上的进度显示文本
            progressNumber.text = ((int)i).ToString() + "%";
            //yield return new WaitForEndOfFrame();
            yield return new WaitForSeconds(0.01f);
        }
        //进度条到100后隐藏加载界面,显示登陆界面
        UILoading.SetActive(false);
        UIMain.SetActive(true);
        yield return null;
    }


    // Update is called once per frame
    void Update()
    {

    }
}
