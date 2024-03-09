using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Unity.VisualScripting;
using UnityEngine;

[Serializable]
public enum TextAnimationType
{
    None = 0,
    Normal = 1,
    Burst = 2
}
public enum TextMoveType
{
    None,
    Up,
    Down,
    Left,
    Right,
    LeftUp,
    LeftDown,
    RightUp,
    RightDown,
    LeftParabola,
    RightParabola
}
public class FightTextManager: MonoBehaviour
{
    private readonly string operatorPlusKeyPostfix = "_operator_plus";
    private readonly string operatorMinusKeyPostfix = "_operator_minus";
    private readonly string numberPrefix = "_NumberImage_";
    private readonly string prefabsPrefix = "Prefabs/FightText/";
    public Dictionary<string, GameObject> numberDic = new Dictionary<string, GameObject>();

    private List<FightTextComponent> handleFightTextGroup = new List<FightTextComponent>();
    private Queue<FightTextInfo> waitFightTextGroup = new Queue<FightTextInfo>();
    private List<FightTextComponent> waitDestoryGroup = new List<FightTextComponent>();

    public Transform FightTextCanvas;
    private Camera fightTextCamera = null;
    public Camera FightTextCamera
    {
        get
        {
            return fightTextCamera == null ? Camera.main : fightTextCamera;
        }
        set
        {
            fightTextCamera = value;
        }
    }

    [Header("渐隐曲线")]
    [SerializeField]
    private AnimationCurve fightTextCure = null;
    [Header("抛物线曲线")]
    [SerializeField]
    private AnimationCurve shootParabolaCure = null;

    [Header("最大等待弹射数量")]
    [SerializeField]
    private int MaxWaitCount = 20;
    [Header("超过此数量弹射加速")]
    [SerializeField]
    private int accelerateThresholdValue = 10;
    [Header("加速弹射速率因子")]
    [SerializeField]
    private float accelerateFactor = 2;
    private bool isAccelerate = false;
    [Header("默认创建周期：秒/一次")]
    [SerializeField]
    private float updateCreatDefualtTime = 0.2f;
    //[SerializeField]
    private float updateCreatTime = 0.2f;
    //[SerializeField]
    private float updateCreatTempTime;
    [Header("从移动到消失的生命周期 单位：秒")]
    [SerializeField]
    private float moveLifeTime = 1.0f;

    [Header("远近缩放因子")]
    [SerializeField]
    private float fightTextScaleFactor = 0.6f;

    [Header("等待指定时间后开始移动")]
    [SerializeField]
    private float delayMoveTime = 0.3f;

    public float DelayMoveTime { get { return delayMoveTime; } set { delayMoveTime = value; } }
    [Range(-4, 4)]
    [Header("初始化位置垂直偏移量")]
    [SerializeField]
    private float initializedVerticalPositionOffset = 0.8f;
    [Range(-4, 4)]
    [Header("初始化位置水平偏移量")]
    [SerializeField]
    private float initializedHorizontalPositionOffset = 0.0f;
    [Header("垂直移动速率")]
    [Range(0, 20)]
    [SerializeField]
    private float verticalMoveSpeed = 10;
    [Header("水平移动速率")]
    [Range(0, 20)]
    [SerializeField]
    private float horizontalMoveSpeed = 10;
    [Header("字体动画类型")]
    public TextAnimationType textAnimationType;
    [Header("字体移动类型")]
    public TextMoveType textMoveType;

    /// <summary>
    /// 一次飘字UI父节点
    /// </summary>
    private GameObject fightTextPrefab = null;
    void Start()
    {
        Initialized();
    }
    private void Initialized()
    {
        fightTextCure = new AnimationCurve(new Keyframe[] { new Keyframe(0, 1f), new Keyframe(moveLifeTime, 0f) });
        updateCreatTempTime = updateCreatTime;
        fightTextPrefab = Resources.Load<GameObject>(prefabsPrefix + "FightText_Pure");
        //加法图片
        numberDic.Add(TextAnimationType.Normal.ToString() + operatorPlusKeyPostfix, Resources.Load<GameObject>(prefabsPrefix + TextAnimationType.Normal.ToString() + operatorPlusKeyPostfix));
        numberDic.Add(TextAnimationType.Burst.ToString() + operatorPlusKeyPostfix, Resources.Load<GameObject>(prefabsPrefix + TextAnimationType.Burst.ToString() + operatorPlusKeyPostfix));
        //减法图片
        numberDic.Add(TextAnimationType.Normal.ToString() + operatorMinusKeyPostfix, Resources.Load<GameObject>(prefabsPrefix + TextAnimationType.Normal.ToString() + operatorMinusKeyPostfix));
        numberDic.Add(TextAnimationType.Burst.ToString() + operatorMinusKeyPostfix, Resources.Load<GameObject>(prefabsPrefix + TextAnimationType.Burst.ToString() + operatorMinusKeyPostfix));

        for (int i = 0; i < 10; i++)
        {
            numberDic.Add(TextAnimationType.Normal.ToString() + numberPrefix + i, Resources.Load<GameObject>(prefabsPrefix + TextAnimationType.Normal.ToString() + numberPrefix + i));
        }
        for (int i = 0; i < 10; i++)
        {
            numberDic.Add(TextAnimationType.Burst.ToString() + numberPrefix + i, Resources.Load<GameObject>(prefabsPrefix + TextAnimationType.Burst.ToString() + numberPrefix + i));
        }
    }
    void Update()
    {

        float deltaTime = Time.deltaTime;

        //操作handleFightTextGroup中移动
        for (int i = 0; i < handleFightTextGroup.Count; i++)
        {
            FightTextComponent fightTextComponent = handleFightTextGroup[i];

            Vector3 fightTextCreatPosition = Vector3.zero;
            fightTextCreatPosition = fightTextComponent.colliderBounds.center + (((Vector3.up * fightTextComponent.colliderBounds.size.y) * 0.5f));
            fightTextCreatPosition.x += (float)fightTextComponent.initializedHorizontalPositionOffset;
            fightTextCreatPosition.y += (float)fightTextComponent.initializedVerticalPositionOffset;

            Vector2 anchors = FightTextCamera.WorldToViewportPoint(fightTextCreatPosition);//飘字初始锚点位置
            Vector2 changeAnchoredPosition = new Vector2((float)(anchors.x + fightTextComponent.xMoveOffeset), (float)(anchors.y + fightTextComponent.yMoveOffeset));//飘字这一帧所在位置

            //设定锚点
            fightTextComponent.rectTransform.anchorMax = anchors;
            fightTextComponent.rectTransform.anchorMin = anchors;
            //设置相对坐标
            fightTextComponent.rectTransform.anchoredPosition = changeAnchoredPosition;

            if (fightTextComponent.delayMoveTime <= Time.time)//允许移动操作
            {
                fightTextComponent.isMove = true;
            }

            //处理近大远小
            double objectHigh = ModelInScreenHigh(fightTextComponent.colliderBounds);
            double scale = (objectHigh / 100) * fightTextScaleFactor;

            fightTextComponent.ChangeScale(scale);
            double xMoveOffeset = horizontalMoveSpeed * deltaTime * objectHigh;
            double yMoveOffeset = verticalMoveSpeed * deltaTime * objectHigh;

            if (fightTextComponent.isMove == true)//处理位置信息
            {
                switch (fightTextComponent.moveType)
                {
                    case TextMoveType.None:
                        break;
                    case TextMoveType.Up:
                        {
                            fightTextComponent.yMoveOffeset += yMoveOffeset;
                        }
                        break;
                    case TextMoveType.Down:
                        {
                            fightTextComponent.yMoveOffeset -= yMoveOffeset;
                        }
                        break;
                    case TextMoveType.Left:
                        {
                            fightTextComponent.xMoveOffeset -= xMoveOffeset;
                        }
                        break;
                    case TextMoveType.Right:
                        {
                            fightTextComponent.xMoveOffeset += xMoveOffeset;
                        }
                        break;
                    case TextMoveType.LeftUp:
                        {
                            fightTextComponent.xMoveOffeset -= xMoveOffeset;
                            fightTextComponent.yMoveOffeset += yMoveOffeset;
                        }
                        break;
                    case TextMoveType.LeftDown:
                        {
                            fightTextComponent.xMoveOffeset -= xMoveOffeset;
                            fightTextComponent.yMoveOffeset -= yMoveOffeset;
                        }
                        break;
                    case TextMoveType.RightUp:
                        {
                            fightTextComponent.xMoveOffeset += xMoveOffeset;
                            fightTextComponent.yMoveOffeset += yMoveOffeset;
                        }
                        break;
                    case TextMoveType.RightDown:
                        {
                            fightTextComponent.xMoveOffeset += xMoveOffeset;
                            fightTextComponent.yMoveOffeset -= yMoveOffeset;

                        }
                        break;
                    case TextMoveType.LeftParabola:
                        {
                            float parabola = shootParabolaCure.Evaluate((float)(fightTextComponent.fadeCurveTime / moveLifeTime));
                            fightTextComponent.xMoveOffeset -= xMoveOffeset;
                            fightTextComponent.yMoveOffeset += yMoveOffeset + parabola;
                        }
                        break;
                    case TextMoveType.RightParabola:
                        {
                            float parabola = shootParabolaCure.Evaluate((float)(fightTextComponent.fadeCurveTime / moveLifeTime));
                            fightTextComponent.xMoveOffeset += xMoveOffeset;
                            fightTextComponent.yMoveOffeset += yMoveOffeset + parabola;
                        }
                        break;
                    default:
                        break;
                }
            }

            //处理渐隐
            if (fightTextComponent.isMove == true)
            {
                fightTextComponent.fadeCurveTime += deltaTime;
                float alpha = fightTextCure.Evaluate((float)(fightTextComponent.fadeCurveTime));
                fightTextComponent.ChangeAlpha(alpha);
            }
            else
            {
                fightTextComponent.ChangeAlpha(1);
            }

            //处理删除对应的飘字
            if (fightTextComponent.isMove == true && fightTextComponent.canvasGroup.alpha <= 0)
            {
                waitDestoryGroup.Add(fightTextComponent);
            }
        }

        //是否加速
        isAccelerate = waitFightTextGroup.Count >= accelerateThresholdValue ? true : false;
        if (isAccelerate)
        {
            updateCreatTime = updateCreatTime / accelerateFactor;
        }
        else
        {
            updateCreatTime = updateCreatDefualtTime;
        }

        //创建
        if ((updateCreatTempTime -= deltaTime) <= 0)
        {
            updateCreatTempTime = updateCreatTime;
            if (waitFightTextGroup.Count > 0)
            {
                GameObject tempObj = InstanceFightText(waitFightTextGroup.Dequeue());
                tempObj.transform.SetParent(FightTextCanvas, false);
            }
        }

        //删除已经完全消失飘字
        for (int i = 0; i < waitDestoryGroup.Count; i++)
        {
            handleFightTextGroup.Remove(waitDestoryGroup[i]);
            Destroy(waitDestoryGroup[i].gameObject);
        }
        waitDestoryGroup.Clear();
    }
    public void CreatFightText(string content, TextAnimationType textAnimationType, TextMoveType textMoveType, Transform targetTranform)
    {
        CreatFightText(content, textAnimationType, textMoveType, this.delayMoveTime, this.initializedVerticalPositionOffset, this.initializedHorizontalPositionOffset, targetTranform);
    }
    public void CreatFightText(string content, TextAnimationType textAnimationType, TextMoveType textMoveType, float delayMoveTime, float initializedVerticalPositionOffset, float initializedHorizontalPositionOffset, Transform tempTransform)
    {
        FightTextInfo fightTextInfo = new FightTextInfo();
        fightTextInfo.content = content;
        fightTextInfo.animationType = textAnimationType;
        fightTextInfo.moveType = textMoveType;
        fightTextInfo.delayMoveTime = delayMoveTime;
        fightTextInfo.initializedVerticalPositionOffset = initializedVerticalPositionOffset;
        fightTextInfo.initializedHorizontalPositionOffset = initializedHorizontalPositionOffset;
        fightTextInfo.cacheTranform = tempTransform;

        CreatFightText(fightTextInfo);
    }
    public void CreatFightText(FightTextInfo fightTextInfo)
    {
        if (CheckNumber(fightTextInfo.content))
        {
            if (IsAllowAddFightText())
            {
                waitFightTextGroup.Enqueue(fightTextInfo);
            }
            else
            {
                Debug.LogWarning("数量过多不能添加!!!");
            }
        }
        else
        {
            Debug.LogError("飘字数据不合法");
        }
    }

    private GameObject InstanceFightText(FightTextInfo fightTextInfo)
    {
        GameObject fightText = Instantiate(fightTextPrefab);
        //先拼装字体，顺序颠倒会造成组件无法找到对应物体
        BuildNumber(fightTextInfo, fightText.transform);

        FightTextComponent tempFightTextComponent = fightText.GetComponent<FightTextComponent>();
        tempFightTextComponent.SetInfo(fightTextInfo);
        handleFightTextGroup.Add(tempFightTextComponent);
        return fightText;
    }

    private void BuildNumber(FightTextInfo fightTextInfo, Transform parent)
    {
        string tempNumber = fightTextInfo.content;
        char numberOperator = tempNumber[0];
        string animationType = "";
        string plusOrMinus = "";

        //出现字体时对应的动画类型
        animationType = fightTextInfo.animationType == TextAnimationType.None ? TextAnimationType.Normal.ToString() : fightTextInfo.animationType.ToString();

        #region 运算符
        GameObject operatorObj = null;
        plusOrMinus = numberOperator == '+' ? operatorPlusKeyPostfix : operatorMinusKeyPostfix;

        operatorObj = Instantiate(numberDic[animationType + plusOrMinus]);
        operatorObj.transform.SetParent(parent, false);
        #endregion

        #region 数字
        GameObject numberObj = null;
        for (int i = 1; i < tempNumber.Length; i++)
        {
            numberObj = Instantiate(numberDic[animationType + numberPrefix + tempNumber[i]]);
            numberObj.transform.SetParent(parent, false);
        }
        #endregion
    }

    private double ModelInScreenHigh(Bounds bounds)
    {
        Vector3 playerCenter = bounds.center;
        Vector3 halfYVector3 = (Vector3.up * bounds.size.y) * 0.5f;
        float topValue = FightTextCamera.WorldToScreenPoint(playerCenter + halfYVector3).y;
        float bottomValue = FightTextCamera.WorldToScreenPoint(playerCenter - halfYVector3).y;
        return topValue - bottomValue;
    }

    private bool IsAllowAddFightText()
    {
        return waitFightTextGroup.Count < MaxWaitCount;
    }
    /// <summary>
    /// 正则检查字符是否合法
    /// </summary>
    /// <param name="content">飘字内容</param>
    /// <returns></returns>
    private bool CheckNumber(string content)
    {
        string pattern = @"^(\+|\-)\d*$";
        bool IsLegal = Regex.IsMatch(content, pattern);
        return IsLegal;
    }
}

