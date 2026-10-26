



public class AbilityDecider : MonoBehaviour
{

#region Character 

    public void OnCharacterTurnStart(Character character)
    {
        selectedCharacter = character; 
    }

    public void OnCharacterTurnEnd()
    {
        selectedCharacter = null; 
    }

#endregion

#region Ability Selection 

    public void OnAbilitySelected(Ability ability)
    {
        selectedAbility = ability; 
    }
    public void OnAbilityUnselected()
    {
        selectedAbility = null; 
    }

    Ability selectedAbility;

#endregion

    Character selectedCharacter; 
    Character overTarget; 

    public void OnOverTarget(Character target)
    {
        overTarget = target; 
    }

    public void OnMouseRemoveFromTarget(Character target)
    {
        overTarget = null; 
    }

    public Character getOverTarget()
    {
        return overTarget; 
    }

    public void OnClickOverTarget()
    {
        if(overTarget == null || selectedCharacter == null || selectedAbility == null)
        {
            return;    
        }
        /// checks if the target is appropriate - ex. character is Ally and targetting is Ally 
        ExecuteAbility(selectedAbility, selectedCharacter, overTarget); 
    }

    void ExecuteAbility(Ability ab, Character user, Character target)
    {
        /// Executes an effect; 
        /// emits the events and gathers the callbacks; 
        /// Once all callbacks are finished, executes next; 
    }

}

