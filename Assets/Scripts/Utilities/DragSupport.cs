using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DragSupport : MonoBehaviour
{
    //偏移值
    Vector3 m_Offset;

    void Start()
    {
        
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0) && !UIManager.Instance.HasActiveUIWindow())
        {
            StartCoroutine(OnMouseDown());
        }
    }

    private IEnumerator OnMouseDown()
    {
        m_Offset = Camera.main.WorldToScreenPoint(transform.position) - new Vector3
        (0, Input.mousePosition.y, 0);
        //Debug.Log("m_Offset: " + m_Offset);
        while (Input.GetMouseButton(0))
        {
            Vector3 drag_pos = new Vector3(0, Input.mousePosition.y, 0) + m_Offset;
            drag_pos.y = Mathf.Max(120, Mathf.Min(960, drag_pos.y));
            //Debug.Log("drag pos: " + drag_pos);
            transform.position = Camera.main.ScreenToWorldPoint(drag_pos);
            //等待固定更新
            yield return new WaitForFixedUpdate();
        }
    }
}
