using System.Collections;
using System.Collections.Generic;
using TMPro;
using UniRx;
using UnityEngine;
using DG.Tweening;

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

            // 使用 DOTween 创建延迟调用和 Tween 动画
            DOTween.Sequence()
                .AppendInterval(0.15f) // 延迟 0.15 秒
                .Append(DOTween.To(() => currentTimeLeft, x => currentTimeLeft = x, time, 0.3f)
                    .OnUpdate(() => { timeLeftText.text = ((int)currentTimeLeft).ToString(); }) // 在 Tween 过程中更新文本内容
                    .OnComplete(OnAnimationComplete)); // 动画完成时的回调方法
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
