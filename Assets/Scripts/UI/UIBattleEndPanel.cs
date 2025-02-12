using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class UIBattleEndPanel : MonoBehaviour
{
    public List<UICharacterAddExpItem> addExpItems;
    public Button closeButton;
    private List<string> idsList;
    
    private Coroutine addExpCoroutine;

    void Start()
    {
        closeButton.onClick.AddListener(OnCloseButtonClicked);
    }
    void OnDisable()
    {
        if (addExpCoroutine != null)
        {
            StopCoroutine(addExpCoroutine);
        }
    }

    public void Setup(List<string> idsList)
    {
        this.idsList = idsList;
        foreach(var index in Enumerable.Range(0, addExpItems.Count))
        {
            if (index < idsList.Count)
            {
                addExpItems[index].Setup(idsList[index]);
                addExpItems[index].gameObject.SetActive(true);
            } else {
                addExpItems[index].gameObject.SetActive(false);
            }
        }
        Invoke("DelayedStartCoroutine", 0.5f);
    }

    void DelayedStartCoroutine()
    {
        addExpCoroutine = StartCoroutine(AddExpInTurn());
    }

    private IEnumerator AddExpInTurn()
    {
        foreach(var index in Enumerable.Range(0, idsList.Count))
        {
            addExpItems[index].AddExp(Random.Range(10,25));
            yield return new WaitForSeconds(1f);
        }
    }

    void OnCloseButtonClicked()
    {
        this.gameObject.SetActive(false);
        BattleManager.Instance.BattleEnd(true);
    }
}
