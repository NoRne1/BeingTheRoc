using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using TMPro;
using DG.Tweening;

public class UIFeatherButton : MonoBehaviour
{
    public Transform peakTransform;
    public TextMeshProUGUI changeText;
    public TextMeshProUGUI featherCoinText;
    public float AnimationDuration = 1.0f;
    private int currentFeatherCoin = -1;

    // Start is called before the first frame update
    void Start()
    {
        GameManager.Instance.featherCoin.AsObservable().DistinctUntilChanged().TakeUntilDestroy(this).Subscribe(coin =>
        {   
            if (currentFeatherCoin == -1)
            {
                featherCoinText.text = coin.ToString();
                currentFeatherCoin = coin;
                return;
            }
            AnimateFeatherCoinChange(coin - currentFeatherCoin);
            currentFeatherCoin = coin;
        });
    }

    private void AnimateFeatherCoinChange(int amount)
    {
        //-本身带符号
        changeText.text = (amount > 0 ? "+" : "") + amount.ToString();
        changeText.color = amount > 0 ? Color.green : Color.red;
        Vector3 startPosition = changeText.transform.position;
        Vector3 peakPosition = peakTransform.position;
        Vector3 endPosition = featherCoinText.transform.position;

        // Animate the featherCoinText value change
        int startValue = currentFeatherCoin;
        int endValue = currentFeatherCoin + amount;

        changeText.gameObject.SetActive(true);
        changeText.transform.position = startPosition;
        changeText.DOFade(1, 0); // Ensure the text is fully visible
        
        // Create a sequence for the parabolic motion
        Sequence sequence = DOTween.Sequence();
        sequence.Append(changeText.transform.DOMove(peakPosition, AnimationDuration * 0.25f).SetEase(Ease.OutQuad));
        sequence.Append(changeText.transform.DOMove(endPosition, AnimationDuration * 0.75f).SetEase(Ease.InQuad));
        sequence.Join(changeText.DOFade(0, AnimationDuration * 0.75f).SetEase(Ease.InQuad)); // Fade out while falling

        sequence.OnComplete(() =>
        {
            // Reset position and fade in for the next use
            changeText.transform.position = startPosition;
            changeText.DOFade(1, 0);
            changeText.gameObject.SetActive(false);

            DOTween.To(() => startValue, x => startValue = x, endValue, AnimationDuration).OnUpdate(() =>
            {
                featherCoinText.text = startValue.ToString();
            }).OnComplete(() =>
            {
                featherCoinText.text = endValue.ToString();
            });
        });
    }
}
