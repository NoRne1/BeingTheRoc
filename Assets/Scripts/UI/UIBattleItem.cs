using UnityEngine;
using UnityEngine.UI;
using System.Collections;


public class UIBattleItem : MonoBehaviour
{
	public Image itemIcon;
	public BattleItem item;
    public GameObject border;
    public GameObject indicator;

	public bool Selected
	{
		get { return border.activeSelf; }
		set { border.SetActive(value); }
	}

    public bool roundActive
    {
        get { return indicator.activeSelf; }
        set { indicator.SetActive(value); }
    }

    // Use this for initialization
    void Start()
	{
		border.SetActive(false);
        indicator.SetActive(false);
    }

	// Update is called once per frame
	void Update()
	{
			
	}

	public void Setup(BattleItem item)
	{
		itemIcon.overrideSprite = Resloader.LoadSprite(item.Resource);
		this.item = item;
    }
}

