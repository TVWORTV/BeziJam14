


public class AbilityDisplay 
{
    [HideInInspector] public Ability ability_displayed; 
    [HideInInspector] public Character ability_owner; 
    [HideInInspector] public Character target; 
    public void Display()
    {
        /// if this ability
            /// 1. belongs to the AI or 
            /// 2. is not being currently "used" (player is not selecting target for it)
            /// then the ability is displayed as it's base form;
        /// otherwise, 
            /// it is displayed with addition of 
            /// 1. character stats - neceserally; 
            /// 2. and target stats
    }
    public TMP_Text name_txt;

    public TMP_Text value_text;

    public TMP_Text acc_chance;

    public TMP_Text crit_rate_text;
    public TMP_Text crit_value_text;

    public TMP_Text target_text; 

}

public static class AbilityDisplayWriter
{

}