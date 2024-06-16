using UnityEngine;
using System.Collections;
using static UnityEngine.GraphicsBuffer;
using System.Reflection;

public class BuffManager : MonoSingleton<BuffManager>
{
    // Use this for initialization
    void Start()
	{

	}

	// Update is called once per frame
	void Update()
	{
			
	}

    // for normal
    public void InvokeBuff(BuffModel buff)
    {
        var method = typeof(BuffManager).GetMethod(buff.MethodName);
        object[] parameters = new object[] { buff };
        method?.Invoke(BuffManager.Instance, parameters);
    }

    // for BuffInvokeTime.constant
    public void InvokeBuff(BuffModel buff, bool AddOrRemove)
    {
        var method = typeof(BuffManager).GetMethod(buff.MethodName);
        object[] parameters = new object[] { buff, AddOrRemove };
        method?.Invoke(BuffManager.Instance, parameters);
    }

    private void Nothingness(BuffModel buff)
    {
        buff.num++;
        if (buff.num >= 5)
        {
            BattleManager.Instance.CharacterDie(buff.ownerID);
        }
    }
}

