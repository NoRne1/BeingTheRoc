using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;

public class UITeamWindowCharacterButton : MonoBehaviour
{
    public string characterID;
    public BehaviorSubject<string> currentCharacterID;

    public void SetSubject(BehaviorSubject<string> subject)
    {
        currentCharacterID = subject;
    }

    public void OnClickAction()
    {
        if (characterID != null && currentCharacterID != null)
        {
            currentCharacterID.OnNext(characterID);
        }
    }
}
