using UnityEngine;

public class SkillManager : MonoSingleton<SkillManager>
{
    // Start is called before the first frame update 
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void InvokeSkill(string targetID, string methodName)
    {
        var method = typeof(SkillManager).GetMethod(methodName);
        object[] parameters = new object[] { targetID };
        method?.Invoke(SkillManager.Instance, parameters);
        Debug.Log("skill " + methodName + " has been invoked");
    }
}
