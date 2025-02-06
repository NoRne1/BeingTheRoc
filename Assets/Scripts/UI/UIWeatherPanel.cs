using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class UIWeatherPanel : MonoBehaviour
{
	public Image icon;
	public TextMeshProUGUI title;
	public TextMeshProUGUI desc;

	public WeatherDefine define;

	public void Setup(WeatherDefine define)
	{
		this.define = define;
		icon.overrideSprite = Resloader.LoadSprite(define.Resource, ConstValue.weatherPath);
		title.text = GameUtil.Instance.GetDirectDisplayString(define.title);
		desc.text = GameUtil.Instance.GetDirectDisplayString(define.desc);
	}
}

