using System.Collections;
using System.Collections.Generic;
using TMPro;
using UniRx;
using UnityEngine;

public class UITimeLeft : MonoBehaviour
{
    public TextMeshProUGUI timeLeftText;
    public Animator animator;
    private int currentTimeLeft;
    // Start is called before the first frame update
    void Start()
    {
        currentTimeLeft = GameManager.Instance.timeLeft.Value;
        timeLeftText.text = currentTimeLeft.ToString();
        GameManager.Instance.timeLeft.AsObservable().Skip(1).DistinctUntilChanged().TakeUntilDestroy(this).Subscribe(time =>
        {
            if(currentTimeLeft > time)
            {
                animator.SetTrigger("time_minus");
            } else
            {
                animator.SetTrigger("time_plus");
            }
            LeanTween.delayedCall(0.15f, () =>
            {
                // 创建文本内容变化的Tween动画
                LeanTween.value(gameObject, currentTimeLeft, time, 0.3f)
                .setOnUpdate((float value) => { timeLeftText.text = ((int)value).ToString(); }) // 在Tween过程中更新文本内容
                .setOnComplete(OnAnimationComplete); // 动画完成时的回调方法
                currentTimeLeft = time;
            });
        });
    }

    // Update is called once per frame
    void Update()
    {

    }

    // 动画完成后的回调方法
    void OnAnimationComplete()
    {
        Debug.Log("Text animation complete!");
    }
}
