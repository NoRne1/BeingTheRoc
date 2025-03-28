using UnityEngine;
using TMPro;

public class UIExtraHint : MonoBehaviour
{
    public TextMeshProUGUI title;
    public TextMeshProUGUI desc;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Setup(ExtraEntryDesc extraDesc)
    {
        title.text = GameUtil.Instance.GetDirectDisplayString(extraDesc.Title);
        desc.text = GameUtil.Instance.GetDirectDisplayString(extraDesc.Desc);
    }
}
