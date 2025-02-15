using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.TextCore.Text;
using UnityEngine.EventSystems;
using UniRx;

public class UIEquipItem : MonoBehaviour
{
    public StoreItemModel storeItem;
    public string ownerID;
    public Vector3 recordPosition;
    public Quaternion recordRotation;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
       
    }

    public void SetAndRecord(Vector3 recordPosition)
    {
        transform.position = recordPosition;
        this.recordPosition = recordPosition;
        this.recordRotation = transform.rotation;
    }

    public void Reset()
    {
        transform.position = recordPosition;
        transform.rotation = recordRotation;
    }
}
