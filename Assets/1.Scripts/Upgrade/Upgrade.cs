using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SkillTree
{
    public CharacterData owner;
    [SerializeReference] public List<UpgradeOption> options = new();

    public void Initialize()
    {
        foreach (UpgradeOption option in options)
        {
            if (option is Initial && !option.completed)
            {
                option.Complete(owner);
            }
        }
    }

    public bool CanComplete(UpgradeOption option)
    {
        if (option.completed) return false;
        if (option is Initial) return true;

        foreach (UpgradeOption other in options)
        {
            if (other.completed && other.dependant.Contains(option)) return true;
        }
        return false;
    }

    public bool Complete(UpgradeOption option)
    {
        if (!CanComplete(option)) return false;
        option.Complete(owner);
        return true;
    }

    public bool Revert(UpgradeOption option)
    {
        if (!option.completed) return false;
        return option.Revert(owner);
    }
}

[Serializable]
public abstract class UpgradeOption
{
    [SerializeReference] public List<UpgradeOption> dependant = new();
    [SerializeReference] public ApplicationStrategy application = new();
    public bool completed;

    public virtual void Complete(CharacterData owner)
    {
        completed = true;
        application.OnApply(owner);
    }

    public abstract bool Revert(CharacterData owner);
}

[Serializable]
public class Initial : UpgradeOption
{
    public override bool Revert(CharacterData owner)
    {
        return false;
    }
}

[Serializable]
public class Standart : UpgradeOption
{
    public override bool Revert(CharacterData owner)
    {
        foreach (UpgradeOption option in dependant)
        {
            if (option.completed) option.Revert(owner);
        }

        application.OnRevert(owner);
        completed = false;
        return true;
    }
}

[Serializable]
public class ApplicationStrategy
{
    [SerializeReference] public List<ApplicationEffect> effects = new();

    public virtual void OnApply(CharacterData owner)
    {
        foreach (ApplicationEffect effect in effects)
        {
            effect.OnApply(owner);
        }
    }

    public virtual void OnRevert(CharacterData owner)
    {
        for (int i = effects.Count - 1; i >= 0; i--)
        {
            effects[i].OnRevert(owner);
        }
    }
}

[Serializable]
public abstract class ApplicationEffect
{
    public abstract void OnApply(CharacterData owner);
    public abstract void OnRevert(CharacterData owner);
}

[Serializable]
public class GiveStat : ApplicationEffect
{
    public StatType stat;
    public float amount;

    public override void OnApply(CharacterData owner)
    {
        owner.statsFromUpgrades.Add(stat, amount);
    }

    public override void OnRevert(CharacterData owner)
    {
        owner.statsFromUpgrades.Add(stat, -amount);
    }
}

[Serializable]
public class GiveAbility : ApplicationEffect
{
    public Ability ability;

    public override void OnApply(CharacterData owner)
    {
        if (!owner.availableAbilities.Contains(ability))
        {
            owner.availableAbilities.Add(ability);
        }
    }

    public override void OnRevert(CharacterData owner)
    {
        owner.availableAbilities.Remove(ability);
    }
}