using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;

public class SpringMechanism : MonoBehaviour
{
    public LineRenderer lineRenderer;
    public Transform springBase; // 弹簧起点
    public Button poleButton; //弹簧杆按钮
    public Transform ball; // 小球
    public float maxStretch = 3f; // 最大拉伸长度
    public float springForceMultiplier = 10f; // 弹簧力倍数

    private Vector3 initialBallPosition;
    private Vector3 dragStartPosition;
    private bool poleButtonClicked;

    void Start()
    {
        initialBallPosition = ball.position;
        lineRenderer.positionCount = 2;
        lineRenderer.SetPosition(0, springBase.position);
        lineRenderer.SetPosition(1, ball.position);
        // 鼠标输入来模拟拉动弹簧
        poleButton.OnClickAsObservable().Subscribe(_=>{
            poleButtonClicked = true;
            dragStartPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            dragStartPosition.z = 0f;
        }).AddTo(this);
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (GameUtil.Instance.IsPointInsideGameObject(poleButton.gameObject, Input.mousePosition))
            {
                poleButtonClicked = true;
                dragStartPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                dragStartPosition.z = 0f;
            }
        }

        if (poleButtonClicked && Input.GetMouseButton(0))
        {
            Vector3 currentDragPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            currentDragPosition.z = 0f;

            Vector3 direction = currentDragPosition - dragStartPosition;
            float distance = Mathf.Min(direction.magnitude, maxStretch); // 最大拉伸限制

            // 更新弹簧线和力
            lineRenderer.SetPosition(1, springBase.position + direction.normalized * distance);
        }

        if (poleButtonClicked && Input.GetMouseButton(0))
        {
            Vector3 currentDragPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            currentDragPosition.z = 0f;

            Vector3 direction = currentDragPosition - dragStartPosition;
            float distance = Mathf.Min(direction.magnitude, maxStretch); // 最大拉伸限制

            // 更新弹簧线和力
            lineRenderer.SetPosition(1, springBase.position + direction.normalized * distance);
        }

        if (poleButtonClicked && Input.GetMouseButtonUp(0))
        {
            poleButtonClicked = false;
            // 发射小球
            Vector3 releaseDirection = (lineRenderer.GetPosition(1) - springBase.position).normalized;
            float releaseForce = (lineRenderer.GetPosition(1) - springBase.position).magnitude * springForceMultiplier;

            // 向小球施加力
            Rigidbody2D rb = ball.GetComponent<Rigidbody2D>();
            rb.AddForce(releaseDirection * releaseForce, ForceMode2D.Impulse);

            // 重置弹簧
            lineRenderer.SetPosition(1, springBase.position);
        }
    }
}
