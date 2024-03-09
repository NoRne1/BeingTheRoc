using UnityEngine;


class GameUtil : Singleton<GameUtil>
{
    public bool InScreen(Vector3 position)
    {
        return Screen.safeArea.Contains(Camera.main.WorldToScreenPoint(position));
    }

    public int CalcDamage(float power, float agile, float attack, float armor)
    {
        if (Random.Range(0f, 100f) < agile)
        {
            return 0;
        } else
        {
            return (int)Mathf.Floor(power * (1f + attack / 100f) * (1 - (armor * 0.05f / (1f + armor * 0.05f))));
        }
    }
}