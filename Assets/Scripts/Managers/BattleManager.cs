using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using System.Linq;
using UnityEngine.TextCore.Text;

public enum RoundTime
{
    begin = 0,
    acting = 1,
    end = 2,
}

public class BattleManager : MonoSingleton<BattleManager>
{
    public UIMoveBar moveBar;
    public BehaviorSubject<int> timePass = new BehaviorSubject<int>(-1);
    public System.IDisposable? timePassDispose;
    public List<NorneRelay<CharacterModel>> characters = new List<NorneRelay<CharacterModel>>();
    public BehaviorSubject<RoundTime> roundTime = new BehaviorSubject<RoundTime>(RoundTime.end);
    public System.IDisposable? roundRelayDispose;

    // Use this for initialization
    void Start()
    {
        moveBar.Init();
    }

    // Update is called once per frame
    void Update()
    {

    }

    ~BattleManager()
    {
        timePassDispose?.Dispose();
        roundRelayDispose?.Dispose();
        timePassDispose = null;
        roundRelayDispose = null;
    }

    public IEnumerator StartBattle(List<int> characterIDs)
    {
        timePassDispose?.Dispose();
        roundRelayDispose?.Dispose();
        characters.Clear();

        timePassDispose = timePass.AsObservable().TakeUntilDestroy(this).Subscribe(time => {
            CalcCharacterAndShow(time);
        });
        for (int i = 0; i < characterIDs.Count; i++)
        {
            characters.Add(
                NorneStore.Instance.ObservableObject<CharacterModel>(new CharacterModel(characterIDs[i]))
            );
        }
        //手动执行一次排序和显示(-999手是特殊指令值)
        timePass.OnNext(-999);
        yield return null;

        //正式开始回合流程
        roundRelayDispose = roundTime.AsObservable().TakeUntilDestroy(this).Subscribe(time => {
            StartCoroutine(ProcessRound(time));
        });
    }

    public IEnumerator ProcessRound(RoundTime time)
    {
        switch(time)
        {
            case RoundTime.begin:
                Debug.Log("round begin");
                yield return new WaitForSeconds(1f);
                roundTime.OnNext(RoundTime.acting);
                break;
            case RoundTime.acting:
                Debug.Log("round acting");
                break;
            case RoundTime.end:
                Debug.Log("round end");
                int temp = Mathf.CeilToInt(GlobalAccess.roundDistance / characters[0].value.Speed);
                characters[0].value.remainActingTime = temp + characters[1].value.remainActingTime; // 因为后续还会timePass一次
                timePass.OnNext(characters[1].value.remainActingTime);
                yield return new WaitForSeconds(1f);
                roundTime.OnNext(RoundTime.begin);
                break;
        }
        yield return null;
    }
    public void CalcCharacterAndShow(int time)
    {
        if(time >= 0)
        {
            foreach (NorneRelay<CharacterModel> character in characters)
            {
                character.value.remainActingTime -= time;
            }
            ResortCharacter();
            moveBar.Show(characters.Select(c => (BattleItem)c.value).ToList());
        } else if (time == -999)
        {
            foreach (NorneRelay<CharacterModel> character in characters)
            {
                character.value.remainActingTime = Mathf.CeilToInt(GlobalAccess.roundDistance / character.value.Speed);
            }
            ResortCharacter();
            moveBar.Show(characters.Select(c => (BattleItem)c.value).ToList());
        }
    }

    public void ResortCharacter()
    {
        characters.Sort((a, b) => a.value.remainActingTime.CompareTo(b.value.remainActingTime));
    }

    public void RoundEnd()
    {
        roundTime.OnNext(RoundTime.end);
    }
}
