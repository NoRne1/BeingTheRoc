using UniRx;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public class UICloud : MonoBehaviour
{
    public Image cloudImage;
    public int spriteID;
    // 控制云朵随机移动的最大值
    public float moveDistance = 30f;
    // 控制云朵随机旋转的最大角度
    public float maxRotation = 30f;
    // 控制时间间隔（秒）
    public float timeInterval = 1f;

    public Subject<int> cloudDestroySubject;

    private void Start()
    {
        // 定时每隔 timeInterval 调用 MoveAndRotate 方法
        InvokeRepeating("MoveAndRotate", 0f, timeInterval);
    }

    private void Update() {
        if (!GameUtil.Instance.InScreen(transform))
        {
            cloudDestroySubject.OnNext(spriteID);
            Destroy(this.gameObject);
        }
    }

    public void Setup(int spriteID, Sprite sprite, Subject<int> subject)
    {
        this.spriteID = spriteID;
        cloudImage.overrideSprite = sprite;
        cloudDestroySubject = subject;
    }

    private void MoveAndRotate()
    {
        // 随机移动云朵
        Vector3 resultPos = GameUtil.Instance.GetMovedWorldPosition(transform.position, new Vector3(
            Random.Range(0, moveDistance),
            Random.Range(-moveDistance, moveDistance),
            0)); // 假设云朵在2D空间，Z轴不变
        transform.position = resultPos;

        // 随机旋转云朵
        float randomRotation = Random.Range(-maxRotation, maxRotation);
        transform.Rotate(0, 0, randomRotation);
    }

    // 可选：停止云朵的移动
    private void OnDisable()
    {
        CancelInvoke("MoveAndRotate");
    }
}