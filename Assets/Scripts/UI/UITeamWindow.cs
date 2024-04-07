using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using UnityEngine.TextCore.Text;

public class UITeamWindow : UIWindow
{
    public List<Button> characterButtons;
    public BehaviorSubject<int> currentCharacterID;
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
        currentCharacterID = new BehaviorSubject<int>(GameManager.Instance.characterIDs[0]);
        currentCharacterID.AsObservable().DistinctUntilChanged().TakeUntilDestroy(this).Subscribe(cid =>
        {
            if(disposable != null) { disposable.Dispose(); }
            disposable = NorneStore.Instance.ObservableObject<CharacterModel>(new CharacterModel(cid)).TakeUntilDestroy(this).Subscribe(character =>
            {
                print(DataManager.Instance.Characters[cid]);
                infoPage.UpdateCharacter(character);
                bagPage.UpdateCharacter(character);
            });
        });
        for(int i = 0; i < characterButtons.Count; i++)
        {
            if (i < GameManager.Instance.characterIDs.Count)
            {
                int characterID = GameManager.Instance.characterIDs[i];
                characterButtons[i].OnClickAsObservable().Subscribe(_ =>
                {
                    currentCharacterID.OnNext(characterID);
                });
                characterButtons[i].transform.GetChild(0).GetComponent<Image>().overrideSprite =
                    Resloader.LoadSprite(DataManager.Instance.Characters[characterID].Resource);
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
