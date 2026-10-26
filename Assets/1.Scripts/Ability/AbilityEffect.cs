public abstract class AbilityEffect
{
    public EffectTargetting target; 
    public float accuracy_modifier; 
    public float crit_rate_modifier; 
}

public enum EffectTargetting
{
    All, 
    AllEnemies,
    AllAllies, 
    Target,
    Self
}

[Serializable]
public class DamageEffect : AbilityEffect 
{
    public int damage; 
    public int crit_damage_modifier; 
}

[Serializable]
public class HealingEffect : AbilityEffect
{
    public int healing; 
    public int crit_damage_modifier; 
}   

[Serializable]
public class ApplyStatusEffect : AbilityEffect
{
    public StatusEffectConstruct statusEffect;
}

[Serializable]
public class StatusEffectConstruct 
{
    public StatusEffectDefinition effectDefinition; 
    public int durationModifier; 
    public float effectPowerModifier; 
}
