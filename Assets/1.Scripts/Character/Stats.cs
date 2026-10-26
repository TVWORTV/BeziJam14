
using System;
using System.Collections.Generic;
using UnityEngine;

public enum StatType
{
    Health,
    Accuracy,
    CritRate,
    Speed
}

[Serializable]
public struct StatEntry
{
    public StatType type;
    public float value;
}

[Serializable]
public class StatBlock
{
    [SerializeField] private List<StatEntry> entries = new();

    public float Get(StatType type)
    {
        foreach (StatEntry entry in entries)
        {
            if (entry.type == type) return entry.value;
        }
        return 0f;
    }

    public void Add(StatType type, float amount)
    {
        for (int i = 0; i < entries.Count; i++)
        {
            if (entries[i].type != type) continue;
            StatEntry entry = entries[i];
            entry.value += amount;
            entries[i] = entry;
            return;
        }
        entries.Add(new StatEntry { type = type, value = amount });
    }
}