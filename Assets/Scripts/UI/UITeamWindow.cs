using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using UnityEngine.TextCore.Text;
using System.Xml;

public class UITeamWindow : UIWindow
{
    public List<Button> characterButtons;
    public BehaviorSubject<string> currentCharacterID;
    private IDisposable disposable;
    public UITeamInfoPage infoPage;
    public UITeamBagPage bagPage;
    // Start is called before the first frame update
    void Start()
    {
        Init();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void Init()
    {
        currentCharacterID = new BehaviorSubject<string>(GameManager.Instance.characterRelays.GetValueByIndex(0).Value.uuid);
        currentCharacterID.AsObservable().DistinctUntilChanged().TakeUntilDestroy(this).Subscribe(cid =>
        {
            if(disposable != null) { disposable.Dispose(); }
            disposable = NorneStore.Instance.ObservableObject<CharacterModel>(new CharacterModel(cid)).TakeUntilDestroy(this).Subscribe(character =>
            {
                infoPage.UpdateCharacter(character);
                bagPage.UpdateCharacter(character);
            });
        });
        for(int i = 0; i < characterButtons.Count; i++)
        {
            if (i < GameManager.Instance.characterRelays.Count)
            {
                string characterID = GameManager.Instance.characterRelays.GetValueByIndex(i).Value.uuid;
                characterButtons[i].OnClickAsObservable().TakeUntilDestroy(this).Subscribe(_ =>
                {
                    currentCharacterID.OnNext(characterID);
                });
                characterButtons[i].transform.GetChild(0).GetComponent<Image>().overrideSprite =
                    Resloader.LoadSprite(GameManager.Instance.characterRelays[characterID].Value.Resource);
                characterButtons[i].gameObject.SetActive(true);
            } else
            {
                characterButtons[i].gameObject.SetActive(false);
            }
        }
    }


    public void switchPage(bool infoOrBag)
    {
        infoPage.gameObject.SetActive(infoOrBag);
        bagPage.gameObject.SetActive(!infoOrBag);
    }
}
