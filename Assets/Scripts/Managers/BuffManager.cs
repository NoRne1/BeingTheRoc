using UnityEngine;
using System.Collections;

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

    public void AddBuff(int targetID, string methodName)
    {
        var method = typeof(BuffManager).GetMethod(methodName);
        object[] parameters = new object[] { targetID };
        method?.Invoke(BuffManager.Instance, parameters);
        Debug.Log("buff " + methodName + " has been add");
    }
}

