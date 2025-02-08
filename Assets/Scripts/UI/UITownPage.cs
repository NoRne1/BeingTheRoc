using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UniRx;
using DG.Tweening;

public class UITownPage : MonoBehaviour
{
	public List<UITownActionPanel> actionPanels;
	public Image bgMask;
	// Use this for initialization
	void Start()
	{
		GameManager.Instance.timeInterval.AsObservable().Subscribe(timeInterval => {
			Color currentColor = bgMask.color;
			Color toColor = Color.white;
			switch (timeInterval) {
				case TimeInterval.morning:
					toColor = GameUtil.Instance.hexToColor("#ffffff", 0f);
					break;
				case TimeInterval.afternoon:
					toColor = GameUtil.Instance.hexToColor("#C9637A", 90/255.0f);
					break;
				case TimeInterval.night:
					toColor = GameUtil.Instance.hexToColor("#0A0A52", 140/255.0f);
					break;
			}
			DOTween.Sequence()
                .AppendInterval(0.15f) // 延迟 0.15 秒
                .Append(DOTween.To(() => currentColor, x => currentColor = x, toColor, 0.3f)
                    .OnUpdate(() => { bgMask.color = currentColor; })); // 在 Tween 过程中更新文本内容
		}).AddTo(this);
	}

	// Update is called once per frame
	void Update()
	{

	}

	public void SetActionPanels(List<int> actionTypes)
	{
		if (actionPanels.Count == actionTypes.Count)
		{
			for (int i = 0; i < actionPanels.Count; i++)
			{
				actionPanels[i].SetActionType((TownActionType)actionTypes[i]);
			}
		}
		else
		{
			UITip tip = UIManager.Instance.Show<UITip>();
			tip.UpdateTip("town_action_init_error");
		}
	}
}
