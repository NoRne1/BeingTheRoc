using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using TMPro;
using DG.Tweening;

public enum CoinType
{
    featherCoin = 0,
    wheatCoin = 1,
}
public class UICoinChangeButton: MonoBehaviour
{
    public CoinType coinType;
    public Transform peakTransform;
    public TextMeshProUGUI changeText;
    public TextMeshProUGUI coinText;
    public float AnimationDuration = 1.0f;
    private int currentCoin = -1;
    private Vector3 changeTextStartPosition;

    // Start is called before the first frame update
    void Start()
    {
        changeTextStartPosition = changeText.transform.position;
        BehaviorSubject<int> coinSubject = null;
        switch(coinType)
        {
            case CoinType.featherCoin:
                coinSubject = GameManager.Instance.featherCoin;
                break;
            case CoinType.wheatCoin:
                coinSubject = GameManager.Instance.wheatCoin;
                break;
            default:
                break;
        }
        coinSubject?.AsObservable().DistinctUntilChanged().TakeUntilDestroy(this).Subscribe(coin =>
        {   
            CoinChanged(coin);
        });
    }

    private void CoinChanged(int coin)
    {
        if (currentCoin == -1)
        {
            coinText.text = coin.ToString();
            currentCoin = coin;
            return;
        }
        AnimateCoinChange(coin - currentCoin);
        currentCoin = coin;
    }

    private void AnimateCoinChange(int amount)
    {
        changeText.transform.position = changeTextStartPosition;
        changeText.gameObject.SetActive(true);
        changeText.DOFade(1, 0); // Ensure the text is fully visible

        //-本身带符号
        changeText.text = (amount > 0 ? "+" : "") + amount.ToString();
        changeText.color = amount > 0 ? Color.green : Color.red;
        
        Vector3 peakPosition = peakTransform.position;
        Vector3 endPosition = coinText.transform.position;

        // Animate the featherCoinText value change
        int startValue = currentCoin;
        int endValue = currentCoin + amount;

        
        
        // Create a sequence for the parabolic motion
        Sequence sequence = DOTween.Sequence();
        sequence.Append(changeText.transform.DOMove(peakPosition, AnimationDuration * 0.25f).SetEase(Ease.OutQuad));
        sequence.Append(changeText.transform.DOMove(endPosition, AnimationDuration * 0.75f).SetEase(Ease.InQuad));
        sequence.Join(changeText.DOFade(0, AnimationDuration * 0.75f).SetEase(Ease.InQuad)); // Fade out while falling

        sequence.OnComplete(() =>
        {
            // Reset position and fade in for the next use
            changeText.transform.position = changeTextStartPosition;
            changeText.DOFade(1, 0);
            changeText.gameObject.SetActive(false);

            DOTween.To(() => startValue, x => startValue = x, endValue, AnimationDuration).OnUpdate(() =>
            {
                coinText.text = startValue.ToString();
            }).OnComplete(() =>
            {
                coinText.text = endValue.ToString();
            });
        });
    }
}

