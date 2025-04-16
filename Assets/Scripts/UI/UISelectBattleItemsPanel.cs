using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UISelectBattleItemsPanel : MonoBehaviour
{
    public Button closeButton;
    public TextMeshProUGUI wheatTitleText;
    public TextMeshProUGUI characterTitleText;
    public Slider wheatNumSlider;
    public TextMeshProUGUI wheatNumText;
    public MultiSelectToggleGroup toggleGroup;
    public List<UICharacterItem> items;
    public Button confirmButton;

    private List<int> path;

    private int maxSelections = 3;

    // Start is called before the first frame update
    void Start()
    {
        wheatTitleText.text = GameUtil.Instance.GetDisplayString("battle_prepare_wheat_title");
        characterTitleText.text = GameUtil.Instance.GetDisplayString("battle_prepare_character_title");
        closeButton.onClick.AddListener(OnCloseButtonClicked);
        confirmButton.onClick.AddListener(OnConfirmButtonClicked);
        wheatNumSlider.onValueChanged.AddListener((value) =>
        {
            wheatNumText.text = string.Format("{0}", (int)value);
        });
        wheatNumSlider.value = 0;
    }

    public void Setup(List<string> idsList, List<int> path)
    {
        this.path = path;
        foreach (var index in Enumerable.Range(0, items.Count))
        {
            if (index < idsList.Count)
            {
                items[index].Setup(idsList[index]);
                items[index].gameObject.SetActive(true);
            }
            else
            {
                items[index].gameObject.SetActive(false);
            }
        }

        wheatNumSlider.maxValue = Math.Min(GameManager.Instance.wheatCoin.Value, GlobalAccess.battleGranaryOpacity);
        wheatNumSlider.value = 0;
    }

    void OnCloseButtonClicked()
    {
        this.gameObject.SetActive(false);
    }

    void OnConfirmButtonClicked()
    {
        var activeToggles = toggleGroup.GetActiveToggles().ToList();
        if ((int)wheatNumSlider.value <= 0)
        {
            var tip = UIManager.Instance.Show<UITip>();
            tip.UpdateTip("start_battle_wheat_num_invalid");
            return;
        }
        if ((int)wheatNumSlider.value > GameManager.Instance.wheatCoin.Value)
        {
            var tip = UIManager.Instance.Show<UITip>();
            tip.UpdateGeneralTip("start_battle_wheat_num_invalid");
            return;
        }
        if (activeToggles.Count >= 1 && activeToggles.Count <= maxSelections)
        {
            StartCoroutine(MapManager.Instance.MovePlayerPosAnimIEnumerator(path, () =>
            {
                GameManager.Instance.SwitchPage(PageType.battle, () =>
                {
                    GameManager.Instance.WheatCoinChanged(-(int)wheatNumSlider.value);
                    BattleManager.Instance.StartBattle((int)wheatNumSlider.value, toggleGroup.GetActiveToggles().ToList()
                        .Select(toggle => toggle.GetComponent<UICharacterItem>().id).ToList()
                        , MapManager.Instance.CurrentTownNode.model.battleInfo);
                     this.gameObject.SetActive(false);
                });
            }));
        }
        else
        {
            var tip = UIManager.Instance.Show<UITip>();
            tip.UpdateTip("start_battle_char_num_invalid");
        }
    }
}
