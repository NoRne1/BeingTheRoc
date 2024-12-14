using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class SpringMechanism : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public Button poleButton; // 弹簧杆按钮
    public RectTransform springRectTransform; // 弹簧杆的 RectTransform
    public float minStretch = 0.5f; // 最小拉伸长度
    public float returnSpeed = 5f; // 弹簧恢复的速度
    public Transform springJointPos;
    public SpringJoint2D springJoint; // SpringJoint2D 用于物理弹簧模拟
    public Rigidbody2D ballRigidbody;
    public Transform ball;
    public Transform launchArea;
    private Vector3 dragStartPosition; // 鼠标按下时的屏幕坐标
    private bool poleButtonClicked;
    private float initialSpringHeight;
    private Vector3 springJointInitialPos;

    void Start()
    {
        initialSpringHeight = springRectTransform.sizeDelta.y; // 获取弹簧杆的初始高度
        springJointInitialPos = springJointPos.position;
        ballRigidbody.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        // 确保一开始不受弹簧力影响
        springJoint.enabled = false;
    }

    // 按下弹簧杆按钮时触发
    public void OnPointerDown(PointerEventData eventData)
    {
        poleButtonClicked = true;
        dragStartPosition = Input.mousePosition; // 获取鼠标按下时的屏幕坐标
    }

    // 松开弹簧杆按钮时触发
    public void OnPointerUp(PointerEventData eventData)
    {
        poleButtonClicked = false;

        // 弹簧恢复到原始长度
        StartCoroutine(ReturnSpringToBase());
        // 虚拟弹簧恢复到原始位置
        StartCoroutine(ReturnSpringJointToBase());
    }

    void Update()
    {
        // 如果正在拉伸并且鼠标按下
        if (poleButtonClicked && Input.GetMouseButton(0))
        {
            Vector3 currentDragPosition = Input.mousePosition; // 获取当前的鼠标屏幕坐标

            // 计算鼠标移动的距离（屏幕坐标差值）
            float dragDistance = currentDragPosition.y - dragStartPosition.y;
            
            // 限制最大和最小拉伸值
            float newHeight = Mathf.Min(initialSpringHeight, Mathf.Max(initialSpringHeight + dragDistance, initialSpringHeight * minStretch)); // 这里通过比例因子调整
            float dragPercent = (initialSpringHeight - newHeight) / initialSpringHeight;

            // 更新弹簧杆的高度
            springRectTransform.sizeDelta = new Vector2(springRectTransform.sizeDelta.x, newHeight);

            // 动态调整 springJointPos 的位置
            springJointPos.position = GameUtil.Instance.GetMovedWorldPosition(springJointInitialPos, new Vector3(0, dragPercent * 300, 0));
        }
    }

    // 弹簧恢复到初始位置的协程
    private IEnumerator ReturnSpringToBase()
    {
        // 等待弹簧杆恢复到原始高度
        float elapsedTime = 0f;
        while (elapsedTime < 1f)
        {
            elapsedTime += Time.deltaTime * returnSpeed;
            springRectTransform.sizeDelta = new Vector2(springRectTransform.sizeDelta.x, Mathf.Lerp(springRectTransform.sizeDelta.y, initialSpringHeight, elapsedTime));
            yield return null;
        }

        // 确保恢复到精确的初始状态
        springRectTransform.sizeDelta = new Vector2(springRectTransform.sizeDelta.x, initialSpringHeight);
    }

    private IEnumerator ReturnSpringJointToBase()
    {
        if (GameUtil.Instance.IsPointInsideGameObject(launchArea.gameObject, Camera.main.WorldToScreenPoint(ball.position)))
        {
            springJoint.enabled = true;
            bool springRebounded = false;
            while (!springRebounded)
            {
                // 判断是否恢复到初始高度
                if (Vector3.Distance(ball.position, springJointPos.position) <= springJoint.distance)
                {
                    springRebounded = true;
                }
                yield return null;
            }
            springJoint.enabled = false;
        }
        springJointPos.position = springJointInitialPos;
    }
}