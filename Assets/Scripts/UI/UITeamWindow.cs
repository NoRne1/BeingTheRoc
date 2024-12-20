using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using UniRx;

public class UITeamWindow : UIWindow
{
    public List<Button> characterButtons;
    public BehaviorSubject<string> currentCharacterID;
    private IDisposable disposable;
    public UITeamInfoPage infoPage;
    public UITeamBagPage bagPage;
    public Button bagButton;

    private bool normalOrBattleInit = true;
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void NormalInit()
    {
        normalOrBattleInit = true;
        currentCharacterID = new BehaviorSubject<string>(GameManager.Instance.characterRelaysDic.GetValueByIndex(0).Value.uuid);
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
            if (i < GameManager.Instance.characterRelaysDic.Count)
            {
                string characterID = GameManager.Instance.characterRelaysDic.GetValueByIndex(i).Value.uuid;
                characterButtons[i].OnClickAsObservable().TakeUntilDestroy(this).Subscribe(_ =>
                {
                    currentCharacterID.OnNext(characterID);
                });
                characterButtons[i].transform.GetChild(0).GetComponent<Image>().overrideSprite =
                    Resloader.LoadSprite(GameManager.Instance.characterRelaysDic[characterID].Value.Resource, ConstValue.battleItemsPath);
                characterButtons[i].gameObject.SetActive(true);
            } else
            {
                characterButtons[i].gameObject.SetActive(false);
            }
        }
        bagButton.gameObject.SetActive(true);
        SwitchPage(false);
    }

    public void BattleInit()
    {
        normalOrBattleInit = false;
        var battleItems = BattleManager.Instance.battleItemManager.battleItemIDs.Select(uuid => GlobalAccess.GetBattleItem(uuid))
            .Where(item => item.type == BattleItemType.player).ToList();
        currentCharacterID = new BehaviorSubject<string>(battleItems[0].uuid);
        currentCharacterID.AsObservable().DistinctUntilChanged().TakeUntilDestroy(this).Subscribe(cid =>
        {
            if (disposable != null) { disposable.Dispose(); }
            disposable = NorneStore.Instance.ObservableObject<BattleItem>(new BattleItem(cid)).TakeUntilDestroy(this).Subscribe(battleItem =>
            {
                infoPage.UpdateBattleItem(battleItem);
                bagPage.UpdateBattleItem(battleItem);
            });
        });
        for (int i = 0; i < characterButtons.Count; i++)
        {
            if (i < GameManager.Instance.characterRelaysDic.Count)
            {
                string characterID = battleItems[i].uuid;
                characterButtons[i].OnClickAsObservable().TakeUntilDestroy(this).Subscribe(_ =>
                {
                    currentCharacterID.OnNext(characterID);
                });
                characterButtons[i].transform.GetChild(0).GetComponent<Image>().overrideSprite =
                    Resloader.LoadSprite(battleItems[i].Resource, ConstValue.battleItemsPath);
                characterButtons[i].gameObject.SetActive(true);
            }
            else
            {
                characterButtons[i].gameObject.SetActive(false);
            }
        }
        bagButton.gameObject.SetActive(false);
        SwitchPage(true);
    }


    public void SwitchPage(bool infoOrBag)
    {
        infoPage.gameObject.SetActive(infoOrBag);
        bagPage.gameObject.SetActive(!infoOrBag);
    }

    public void LevelUp()
    {
        if (normalOrBattleInit)
        {
            var character = NorneStore.Instance.ObservableObject<CharacterModel>(new CharacterModel(currentCharacterID.Value)).Value;
            if (character.attributes.remainExp > GlobalAccess.levelUpExp &&
                character.attributes.level < GlobalAccess.maxLevel)
            {
                //可以升级
                UISkillSelect skillSelect = UIManager.Instance.Show<UISkillSelect>();
                skillSelect.Setup(character);
                skillSelect.selectedAction = (skill) =>
                {
                    character.LevelUp();
                    if(character.attributes.level == 1)
                    {
                        character.Skill1 = skill.ID;
                    } else if (character.attributes.level == 2)
                    {
                        character.Skill2 = skill.ID;
                    }
                    else if (character.attributes.level == 3)
                    {
                        character.Skill3 = skill.ID;
                    }
                    NorneStore.Instance.Update<CharacterModel>(character, true);
                };
            }
        }
        else
        {
            var tip = UIManager.Instance.Show<UITip>();
            tip.UpdateTip("在战斗中渡劫升级乃是兵家大忌");
        }
    }

    void OnDestroy()
    {
        // 确保子对象订阅被取消
        infoPage.disposable.IfNotNull(dis => { dis.Dispose(); });
    }
}
