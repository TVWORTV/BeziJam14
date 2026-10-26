using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Character")]
public class CharacterData : ScriptableObject
{
    // public RuntimeAnimationController characterAnimation; 
    // public Sprite characterSprite;

    public List<Ability> availableAbilities = new();

    public StatBlock baseStats = new();
    public StatBlock statsFromUpgrades = new();

    public SkillTree skillTree = new();

    public float GetStat(StatType type)
    {
        return baseStats.Get(type) + statsFromUpgrades.Get(type);
    }

    private void OnValidate()
    {
        skillTree.owner = this;
    }
}