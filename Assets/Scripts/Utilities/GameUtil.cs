using System.Collections;
using UnityEngine;


class GameUtil : Singleton<GameUtil>
{
    public bool InScreen(Vector3 position)
    {
        return Screen.safeArea.Contains(Camera.main.WorldToScreenPoint(position));
    }

    public int CalcDamage(float power, float agile, float attack, float armor)
    {
        if (Random.Range(0f, 100f) < agile)
        {
            return 0;
        } else
        {
            return (int)Mathf.Floor(power * (1f + attack / 100f) * (1 - (armor * 0.05f / (1f + armor * 0.05f))));
        }
    }

    public IEnumerator FadeIn(CanvasGroup canvasGroup, float transitionDuration)
    {
        canvasGroup.alpha = 0;
        canvasGroup.gameObject.SetActive(true);
        float elapsedTime = 0;
        while (elapsedTime < transitionDuration)
        {
            canvasGroup.alpha = Mathf.Lerp(0, 1, (elapsedTime / transitionDuration));
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        canvasGroup.alpha = 1;
    }

    public IEnumerator FadeOut(CanvasGroup canvasGroup, float transitionDuration)
    {
        canvasGroup.gameObject.SetActive(true);
        canvasGroup.alpha = 1;
        float elapsedTime = 0;
        while (elapsedTime < transitionDuration)
        {
            canvasGroup.alpha = Mathf.Lerp(1, 0, (elapsedTime / transitionDuration));
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        canvasGroup.alpha = 0;
        canvasGroup.gameObject.SetActive(false);
    }
}