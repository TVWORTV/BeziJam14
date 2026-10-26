

public class Ability
{
    public AbilityTargetting target; 
    public List<AbilityEffect> effects = new();
}

public enum AbilityTargetting 
{
    Any, 
    AnyAlly,
    AnyEnemy,
    Self 
}