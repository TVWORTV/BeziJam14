using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Asset wrapper around a <see cref="SkillTree"/>. Trees live as assets in
/// Assets/Resources/SkillTrees and are authored with the Skill Tree Editor window
/// (Tools > DD System > Skill Tree Editor).
///
/// Load at runtime with Resources.Load&lt;SkillTreeAsset&gt;("SkillTrees/AssetName").
/// Note: UpgradeOption.completed is runtime state stored on the options themselves, so
/// Instantiate() the asset per character instead of mutating the shared asset.
/// </summary>
[CreateAssetMenu(fileName = "NewSkillTree", menuName = "DD System/Skill Tree")]
public class SkillTreeAsset : ScriptableObject
{
    public SkillTree tree = new();

    // Editor layout only. Parallel to tree.options: same index = same node.
    [SerializeField, HideInInspector] private List<Vector2> nodePositions = new();

    public int NodePositionCount => nodePositions.Count;

    public Vector2 GetNodePosition(int index)
    {
        return index >= 0 && index < nodePositions.Count ? nodePositions[index] : Vector2.zero;
    }

    public void SetNodePosition(int index, Vector2 position)
    {
        if (index < 0) return;
        while (nodePositions.Count <= index) nodePositions.Add(Vector2.zero);
        nodePositions[index] = position;
    }

    public void SetAllNodePositions(IList<Vector2> positions)
    {
        nodePositions = new List<Vector2>(positions);
    }

    /// <summary>Appends an option and its editor position. Returns the new index.</summary>
    public int AddOption(UpgradeOption option, Vector2 position)
    {
        tree.options.Add(option);
        TrimPositions();
        int index = tree.options.Count - 1;
        SetNodePosition(index, position);
        return index;
    }

    /// <summary>Removes an option and every dependency pointing at it.</summary>
    public void RemoveOptionAt(int index)
    {
        if (index < 0 || index >= tree.options.Count) return;

        UpgradeOption removed = tree.options[index];
        foreach (UpgradeOption other in tree.options)
        {
            if (other != null && other.dependant != null) other.dependant.Remove(removed);
        }

        tree.options.RemoveAt(index);
        if (index < nodePositions.Count) nodePositions.RemoveAt(index);
        TrimPositions();
    }

    private void TrimPositions()
    {
        int extra = nodePositions.Count - tree.options.Count;
        if (extra > 0) nodePositions.RemoveRange(tree.options.Count, extra);
    }
}
