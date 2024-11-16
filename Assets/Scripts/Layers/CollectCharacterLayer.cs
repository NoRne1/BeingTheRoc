using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UniRx;
using System.Linq;

public class CollectCharacterLayer : MonoBehaviour
{
    public CanvasGroup selfMask;
    public Transform collectButtons;
    public List<UICollectButton> collectCharacterButtons;

    public Subject<CollectCharacterInfo> collectButtonSubject = new Subject<CollectCharacterInfo>();
    // Start is called before the first frame update
    private void Start() 
    {
        foreach(var index in Enumerable.Range(0, collectCharacterButtons.Count))
        {
            collectCharacterButtons[index].Setup(index);
            collectCharacterButtons[index].GetComponent<Button>().OnClickAsObservable().Subscribe(_ => {
                collectButtonSubject.OnNext(collectCharacterButtons[index].info);
            }).AddTo(this);
        }
    }

    public IEnumerator Show()
    {
        StartCoroutine(GameUtil.Instance.FadeIn(selfMask, 0.3f));
        collectButtons.localScale = new Vector3(1, 0, 1);
        // 使用 DOTween 动画 y 轴从  到 1，持续时间为 1 秒
        collectButtons.DOScaleY(1, 0.5f).SetEase(Ease.OutBounce);
        yield return new WaitForSeconds(0.5f);
    }
    
    public IEnumerator Close()
    {
        StartCoroutine(GameUtil.Instance.FadeOut(selfMask, 0.15f));
        yield return new WaitForSeconds(0.15f);
    }
}
