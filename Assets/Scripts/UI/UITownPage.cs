using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class UITownPage : MonoBehaviour
{
	public List<UITownActionPanel> actionPanels;
	// Use this for initialization
	void Start()
	{

	}

	// Update is called once per frame
	void Update()
	{

	}

	public void SetActionPanels(List<int> actionTypes)
	{
		if (actionPanels.Count == actionPanels.Count)
		{
			for (int i = 0; i < actionPanels.Count; i++)
			{
				actionPanels[i].SetActionType((TownActionType)actionTypes[i]);
			}
		}
		else
		{
			UITip tip = UIManager.Instance.Show<UITip>();
			tip.UpdateTip(DataManager.Instance.Language["town_action_init_error"]);
		}
	}
}
