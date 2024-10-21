using System.Collections;
using System.Collections.Generic;
using TMPro;
using UniRx;
using UnityEngine;
using DG.Tweening;

public class UITimeLeft : MonoBehaviour
{
    public TextMeshProUGUI timeLeftText;
    // public Animator animator;
    private int currentTimeLeft;
    // Start is called before the first frame update
    void OnEnable() 
    {
        timeLeftText.text = (GameManager.Instance.timeLeft.Value / 3).ToString();
    }

    void Start()
    {
        originalPosition = transform.position;
        currentTimeLeft = GameManager.Instance.timeLeft.Value;
        timeLeftText.text = (currentTimeLeft / 3).ToString();
        GameManager.Instance.timeLeft.AsObservable().Skip(1).DistinctUntilChanged().TakeUntilDestroy(this).Subscribe(time =>
        {
            RotateClock(time - currentTimeLeft);
            // if(currentTimeLeft > time)
            // {
            //     animator.SetTrigger("time_minus");
            // } else
            // {
            //     animator.SetTrigger("time_plus");
            // }

            // 使用 DOTween 创建延迟调用和 Tween 动画
            DOTween.Sequence()
                .AppendInterval(0.15f) // 延迟 0.15 秒
                .Append(DOTween.To(() => currentTimeLeft, x => currentTimeLeft = x, time, 0.3f)
                    .OnUpdate(() => { timeLeftText.text = ((int)currentTimeLeft / 3).ToString(); }) // 在 Tween 过程中更新文本内容
                    .OnComplete(OnAnimationComplete)); // 动画完成时的回调方法
        });
    }

    // Update is called once per frame
    void Update()
    {

    }

    public Transform hourHand;  // 时针的Transform
    private Vector3 originalScale = new Vector3(1, 1, 1);  // 保存原始缩放
    private Vector3 originalPosition;  // 保存原始缩放

    // pastTime = 1 表示顺时针旋转 120 度，pastTime = -1 表示逆时针旋转 120 度
    public void RotateClock(int pastTime)
    {
        // 目标角度，顺时针 120 度或逆时针 120 度
        float targetAngle = pastTime * 120f;
        
        // 动画顺序：放大 -> 旋转 -> 缩回原来大小
        Sequence clockSequence = DOTween.Sequence();
        // 放大到1.2倍
        clockSequence.Append(gameObject.transform.DOScale(originalScale * 1.8f, 0.15f));
        clockSequence.Join(gameObject.transform.DOMove(GameUtil.Instance.GetMovedWorldPosition(originalPosition, new Vector3(0, -60, 0)), 0.15f));
        // 旋转目标角度
        clockSequence.Append(hourHand.DORotate(new Vector3(0, 0, hourHand.eulerAngles.z + targetAngle), 0.3f, RotateMode.FastBeyond360));

        // 缩回原来的大小
        clockSequence.Append(gameObject.transform.DOScale(originalScale, 0.15f));
        clockSequence.Join(gameObject.transform.DOMove(originalPosition, 0.15f));

        // 启动动画序列
        clockSequence.Play();
    }

    // 动画完成后的回调方法
    void OnAnimationComplete()
    {
        Debug.Log("Text animation complete!");
    }
}
