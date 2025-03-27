using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class UISelectBattleItemsPanel : MonoBehaviour
{
    public Button closeButton;
    public MultiSelectToggleGroup toggleGroup;
    public List<UICharacterItem> items;
    public Button confirmButton;

    private List<int> path;

    private int maxSelections = 3;

    // Start is called before the first frame update
    void Start()
    {
        closeButton.onClick.AddListener(OnCloseButtonClicked);
        confirmButton.onClick.AddListener(OnConfirmButtonClicked);
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
    }

    void OnCloseButtonClicked()
    {
        this.gameObject.SetActive(false);
    }

    void OnConfirmButtonClicked()
    {
        var activeToggles = toggleGroup.GetActiveToggles().ToList();
        if (activeToggles.Count >= 1 && activeToggles.Count <= maxSelections)
        {
            StartCoroutine(MapManager.Instance.MovePlayerPosAnimIEnumerator(path, () =>
            {
                GameManager.Instance.SwitchPage(PageType.battle, () =>
                {
                    BattleManager.Instance.StartBattle(toggleGroup.GetActiveToggles().ToList()
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
