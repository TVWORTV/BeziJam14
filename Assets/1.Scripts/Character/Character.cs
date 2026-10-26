

public class Character : MonoBehaviour
{
    public CharacterData data; 
    public List<StatusEffect> statusEffects = new(); 
    public int initiative; 

    public float GetStat(StatType type)
    {
        return data.GetStat(type);
    }
}

