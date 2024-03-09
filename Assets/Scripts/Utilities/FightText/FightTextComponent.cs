using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FightTextComponent : MonoBehaviour
{

    public Animator animator = null;
    public CanvasGroup canvasGroup = null;
    public RectTransform rectTransform = null;

    public List<RectTransform> childTransformGroup = new List<RectTransform>();

    public List<Vector2> sizeDeltaGroup = new List<Vector2>();
    public Bounds colliderBounds;
    #region From FightTextInfo
    public string content;
    public TextAnimationType animationType;
    public TextMoveType moveType;
    public double delayMoveTime;
    public int size;
    public Transform cacheTranform;
    public double initializedVerticalPositionOffset;
    public double initializedHorizontalPositionOffset;
    #endregion

    public double fadeCurveTime;

    public double xMoveOffeset;
    public double yMoveOffeset;

    public bool isMove = false;
    public void SetInfo(FightTextInfo fightTextInfo)//执行顺序优于Start
    {
        content = fightTextInfo.content;
        animationType = fightTextInfo.animationType;
        moveType = fightTextInfo.moveType;
        delayMoveTime = Time.time + fightTextInfo.delayMoveTime;
        cacheTranform = fightTextInfo.cacheTranform;
        initializedHorizontalPositionOffset = fightTextInfo.initializedHorizontalPositionOffset;
        initializedVerticalPositionOffset = fightTextInfo.initializedVerticalPositionOffset;
    }
    void Start()
    {
        animator = GetComponent<Animator>();
        canvasGroup = GetComponent<CanvasGroup>();
        rectTransform = GetComponent<RectTransform>();
        colliderBounds = cacheTranform.GetComponent<Collider2D>().bounds;
        childTransformGroup = transform.GetComponentsInChildren<RectTransform>().ToList();
        childTransformGroup.Remove(childTransformGroup[0]);

        for (int i = 0; i < childTransformGroup.Count; i++)
        {
            sizeDeltaGroup.Add(childTransformGroup[i].sizeDelta);
        }
        int state = animationType == TextAnimationType.Normal ? 1 : 2;
        animator.SetInteger("state", state);
    }

    public void ChangeScale(double scale)
    {
        for (int i = 0; i < childTransformGroup.Count; i++)
        {
            Vector2 sizeDelta = sizeDeltaGroup[i];
            sizeDelta.x = sizeDelta.x * (float)scale;
            sizeDelta.y = sizeDelta.y * (float)scale;
            childTransformGroup[i].sizeDelta = sizeDelta;
        }
    }

    public void ChangeAlpha(float alpha)
    {
        canvasGroup.alpha = alpha;
    }
}
