using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections;
using UniRx;
using UniRx.Triggers;

public class EquipmentFusionAnimation : MonoBehaviour
{
    [Header("UI Elements")]
    public Image item1; // 装备 A
    public Image item2; // 装备 B
    public Image item3; // 合成后的装备
    public Image fxEffect; // 爆炸特效
    public RectTransform boundary; // 动画边界

    [Header("Animation Settings")]
    public float moveDurationMin = 1f; // 移动时间范围（最小值）
    public float moveDurationMax = 2f; // 移动时间范围（最大值）
    public float scaleMin = 0.75f; // 缩放范围（最小值）
    public float scaleMax = 1.25f; // 缩放范围（最大值）

    [Header("Positions")]
    private Vector3 positionA; // item1 的目标位置
    private Vector3 positionB; // item2 的目标位置
    private Vector3 positionC; // 合成后的目标位置

    private Sequence fusionSequence;

    private Vector3[] worldCorners = new Vector3[4];
    private float distanceLimit;
    private Coroutine fusionAnimation;

    private void Start()
    {
        boundary.GetWorldCorners(worldCorners);
        distanceLimit = Vector2.Distance(worldCorners[0], worldCorners[2]) / 3;
        positionA = item1.transform.position;
        positionB = item2.transform.position;
        positionC = item3.transform.position;
        // 订阅装备更新逻辑（假设存在订阅机制）
        GameManager.Instance.otherProperty.currentMergeTaskInfo.AsObservable().Subscribe(info =>
        {
            if (info != null)
            {
                item1.overrideSprite = Resloader.LoadSprite(DataManager.Instance.StoreItems[info.equipA].iconResource, ConstValue.equipsPath);
                item2.overrideSprite = Resloader.LoadSprite(DataManager.Instance.StoreItems[info.equipB].iconResource, ConstValue.equipsPath);
                item3.overrideSprite = Resloader.LoadSprite(DataManager.Instance.StoreItems[info.equipResult].iconResource, ConstValue.equipsPath);
            }
        }).AddTo(this);
        this.OnEnableAsObservable()
            .Subscribe(_ => StartFusionAnimation())  // 物体启用时启动动画
            .AddTo(this);

        this.OnDisableAsObservable()
            .Subscribe(_ => StopFusionAnimation())  // 物体禁用时停止动画
            .AddTo(this);
    }

    public void StartFusionAnimation()
    {
        // 启动装备随机动画并存储协程句柄
        if (fusionAnimation == null) // 防止多次启动相同协程
        {
            fusionAnimation = StartCoroutine(FusionAnimationIEnumerator());
        }
    }

    public void StopFusionAnimation()
    {
        // 如果协程已经启动，停止它
        if (fusionAnimation != null)
        {
            StopCoroutine(fusionAnimation);
            fusionAnimation = null; // 协程停止后需要将句柄置为 null
        }

        if (fusionSequence != null && fusionSequence.IsActive())
        {
            fusionSequence.Kill(); // 停止当前动画
            fusionSequence = null;
        }
    }

    IEnumerator FusionAnimationIEnumerator()
    {
        while (true)
        {
            bool flag = false;
            if (fusionSequence != null && fusionSequence.IsActive())
            {
                fusionSequence.Kill(); // 停止当前动画
                fusionSequence = null;
            }

            // 创建新的动画序列
            float animDuration = UnityEngine.Random.Range(moveDurationMin, moveDurationMax);
            fusionSequence = DOTween.Sequence();
            fusionSequence.Append(AnimateItem(item1, animDuration));
            fusionSequence.Join(item1.transform.DOScale(Random.Range(scaleMin, scaleMax), animDuration).SetEase(Ease.InOutQuad));
            fusionSequence.Join(AnimateItem(item2, animDuration));
            fusionSequence.Join(item2.transform.DOScale(Random.Range(scaleMin, scaleMax), animDuration).SetEase(Ease.InOutQuad));
            fusionSequence.OnComplete(()=>{
                flag = true;
            });
            yield return new WaitUntil(()=>flag);
        }
    }

    Tween AnimateItem(Image item, float animDuration)
    {
        // 左下角和右上角的世界坐标
        Vector2 bottomLeft = worldCorners[0];
        Vector2 topRight = worldCorners[2];

        // 随机生成起点（物体当前位置作为起点）
        Vector2 startPoint = item.transform.position;

        // 随机生成终点，确保终点和起点的距离大于 distanceLimit
        Vector2 endPoint;
        do
        {
            endPoint = new Vector2(
                UnityEngine.Random.Range(bottomLeft.x, topRight.x),
                UnityEngine.Random.Range(bottomLeft.y, topRight.y)
            );
        } while (Vector2.Distance(startPoint, endPoint) < distanceLimit);

        // 随机生成控制点
        Vector2 controlPoint = new Vector2(
            UnityEngine.Random.Range(bottomLeft.x, topRight.x),
            UnityEngine.Random.Range(bottomLeft.y, topRight.y)
        );

        // 生成路径点（离散化贝塞尔曲线）
        int pathResolution = 30;
        Vector3[] path = new Vector3[pathResolution];
        for (int i = 0; i < pathResolution; i++)
        {
            float t = i / (float)(pathResolution - 1);
            path[i] = CalculateBezierPoint(t, startPoint, controlPoint, endPoint);
        }

        // 创建移动动画
        return item.transform.DOPath(path, animDuration, PathType.CatmullRom).SetEase(Ease.InOutSine);
    }


    // 计算贝塞尔曲线点
    Vector3 CalculateBezierPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2)
    {
        float u = 1 - t;
        float tt = t * t;
        float uu = u * u;

        return (uu * p0) + (2 * u * t * p1) + (tt * p2);
    }

    // 合成完成后触发的动画逻辑
    public void StartFusionCompleteAnimation(System.Action onFusionComplete)
    {
        StopFusionAnimation();

        // 创建合成完成动画序列
        Sequence fusionCompleteSequence = DOTween.Sequence();

        float recoverTime = 1.0f; // 合成动画中移动时间
        // 1. item1 和 item2 移动到各自目标位置并恢复大小
        fusionCompleteSequence.Append(item1.rectTransform.DOMove(positionA, recoverTime).SetEase(Ease.OutQuad));
        fusionCompleteSequence.Join(item2.rectTransform.DOMove(positionB, recoverTime).SetEase(Ease.OutQuad));
        fusionCompleteSequence.Join(item1.rectTransform.DOScale(Vector3.one, recoverTime).SetEase(Ease.OutBack));
        fusionCompleteSequence.Join(item2.rectTransform.DOScale(Vector3.one, recoverTime).SetEase(Ease.OutBack));

        // 2. item1 和 item2 移动到合成中心位置
        fusionCompleteSequence.Append(item1.rectTransform.DOMove(positionC, 0.5f).SetEase(Ease.InOutQuad));
        fusionCompleteSequence.Join(item2.rectTransform.DOMove(positionC, 0.5f).SetEase(Ease.InOutQuad));

        // 3. 淡出 item1 和 item2
        fusionCompleteSequence.Append(item1.DOFade(0f, 0.2f));
        fusionCompleteSequence.Join(item2.DOFade(0f, 0.2f));

        // 4. 显示爆炸特效
        fusionCompleteSequence.AppendCallback(() => {
            //显示但color.alpha==0
            fxEffect.gameObject.SetActive(true);
            fxEffect.DOFade(0f, 0f);
            fxEffect.transform.DOScale(1f, 0f);
        });
        fusionCompleteSequence.Append(fxEffect.DOFade(1f, 0.3f).SetEase(Ease.OutQuad));
        fusionCompleteSequence.Join(fxEffect.transform.DOScale(1.5f, 0.3f));
        
        // 5. 显示合成后的装备
        fusionCompleteSequence.AppendCallback(() =>
        {
            item3.gameObject.SetActive(true);
            fxEffect.DOFade(0f, 0.3f).OnComplete(() =>
            {
                onFusionComplete?.Invoke(); // 执行回调
                item3.gameObject.SetActive(false);
            });
        });

        fusionCompleteSequence.Play();
    }
}