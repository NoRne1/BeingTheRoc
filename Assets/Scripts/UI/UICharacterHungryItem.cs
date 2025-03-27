using System.Collections.Generic;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
public class UICharacterHungryItem : MonoBehaviour
{
    public TextMeshProUGUI hungryText;
    public Image icon;
    public Image hungryCircleBar;
    public System.IDisposable disposable;

    public void Setup(CharacterModel character)
    {
        icon.overrideSprite = Resloader.LoadSprite(character.Resource, ConstValue.battleItemsPath);
        disposable.IfNotNull(dis => { dis.Dispose(); });
        disposable = NorneStore.Instance.ObservableObject<CharacterModel>(character)
                .AsObservable().TakeUntilDestroy(this).Subscribe(cm =>
                {
                    hungryCircleBar.fillAmount = character.attributes.currentHungry * 1.0f / character.attributes.MaxHungry;
                    hungryText.text = character.attributes.currentHungry + "/" + character.attributes.MaxHungry;
                });
    }

    private void OnMouseEnter() {
        hungryText.gameObject.SetActive(true);
    }

    private void OnMouseExit() {
        hungryText.gameObject.SetActive(false);
    }
}
