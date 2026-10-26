public abstract class StatusEffectDefinition
{

}

#region Individual Effects 

public class Parry : StatusEffectDefinition
{
    // when attacked, parry with an ability 
    public Ability executeOnParry;     
}

public class FollowUp : StatusEffectDefinition
{
    // when a character specified or all allies if bool, execute ability on same target; 
    public Character follows; 
    public bool followsAllAllies; 
    public Ability executeOnFollow;
}

public class Strength : StatusEffectDefinition
{
    // adds this value to attacks 
    public int value; 
}

public class Weakness : StatusEffectDefinition
{
    // removes this value from attacks 
    public int value; 
}

public class Stun : StatusEffectDefinition
{

}
public class Charmed : StatusEffectDefinition
{

}

public class Armor : StatusEffectDefinition
{
    // removes this amount to damage taken; ex. takes 4 damage from a 6 damage attack if value = 2  
    public int value; 
}

public class Fragile : StatusEffectDefinition
{
    // adds this amount to damage taken; ex. takes 6 damage from a 4 damage attack if value = 2  
    public int value; 
}

public class Fleeting : StatusEffectDefinition
{
    // adds this to speed 
    public int value; 
}

public class Slow : StatusEffectDefinition 
{
    // removes this much speed 
    public int value; 
}

#endregion
