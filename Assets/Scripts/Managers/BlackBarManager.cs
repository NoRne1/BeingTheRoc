using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class BlackBarManager : MonoSingleton<BlackBarManager>
{
    public GameObject blackBarPrefab; // 小黑条预制件
    public Transform blackBarParent; // 小黑条的父对象
    public Transform startPos; // 入场起始位置
    public Transform endPos; // 入场结束位置
    public Transform exitPos; // 离场位置

    private Queue<string> messageQueue = new Queue<string>(); // 缓存区
    private bool isDisplaying = false;
    private GameObject currentBlackBar; // 当前显示的小黑条

    void Update()
    {
        if (!isDisplaying && messageQueue.Count > 0)
        {
            StartCoroutine(DisplayNextMessage());
        }
    }

    // 外部调用的方法，用于添加新消息到队列中
    public void AddMessage(string message)
    {
        messageQueue.Enqueue(GameUtil.Instance.GetDirectDisplayString(message));
    }

    private IEnumerator DisplayNextMessage()
    {
        isDisplaying = true;
        float enterDuration = 0.5f;
        float displayDuration = 1f;
        float exitDuration = 0.5f;
        if (messageQueue.Count > 2)
        {
            // 如果队列中有超过2条消息，加快显示速度
            enterDuration /= 2f;
            displayDuration /= 2f;
            exitDuration /= 2f; 
        }
        if (messageQueue.Count > 4)
        {
            // 如果队列中有超过4条消息，加快显示速度
            enterDuration /= 2f;
            displayDuration /= 2f;
            exitDuration /= 2f;
        }


        string message = messageQueue.Dequeue();
        GameObject newBlackBar = Instantiate(blackBarPrefab, blackBarParent);
        TextMeshProUGUI messageText = newBlackBar.GetComponentInChildren<TextMeshProUGUI>();
        CanvasGroup canvasGroup = newBlackBar.GetComponent<CanvasGroup>();

        if (canvasGroup == null)
        {
            canvasGroup = newBlackBar.AddComponent<CanvasGroup>();
        }

        messageText.text = message;

        newBlackBar.transform.position = startPos.position;
        canvasGroup.alpha = 0;

        Sequence sequence = DOTween.Sequence();

        // 入场动画
        sequence.Append(newBlackBar.transform.DOMove(endPos.position, enterDuration));
        sequence.Join(canvasGroup.DOFade(1, enterDuration));

        if (currentBlackBar != null)
        {
            CanvasGroup currentCanvasGroup = currentBlackBar.GetComponent<CanvasGroup>();

            if (currentCanvasGroup == null)
            {
                currentCanvasGroup = currentBlackBar.AddComponent<CanvasGroup>();
            }

            // 离场动画
            sequence.Insert(0, currentBlackBar.transform.DOMove(exitPos.position, exitDuration));
            sequence.Insert(0, currentCanvasGroup.DOFade(0, exitDuration))
                    .OnComplete(() => Destroy(currentBlackBar));
        }

        sequence.AppendInterval(displayDuration);

        // 继续离场动画
        sequence.Append(newBlackBar.transform.DOMove(exitPos.position, exitDuration));
        sequence.Join(canvasGroup.DOFade(0, exitDuration))
                .OnComplete(() => Destroy(newBlackBar));

        currentBlackBar = newBlackBar;

        yield return sequence.WaitForCompletion();

        isDisplaying = false;
    }
}
