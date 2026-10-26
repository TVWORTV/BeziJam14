using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

/// <summary>Shared helpers for the Skill Tree Editor: asset paths, creation, type discovery, layout.</summary>
public static class SkillTreeEditorUtility
{
    public const string ResourcesFolder = "Assets/Resources";
    public const string TreeFolder = "Assets/Resources/SkillTrees";

    // ---------------------------------------------------------------- assets

    public static List<SkillTreeAsset> FindAllTrees()
    {
        var result = new List<SkillTreeAsset>();
        if (!AssetDatabase.IsValidFolder(TreeFolder)) return result;

        foreach (string guid in AssetDatabase.FindAssets("t:SkillTreeAsset", new[] { TreeFolder }))
        {
            var asset = AssetDatabase.LoadAssetAtPath<SkillTreeAsset>(AssetDatabase.GUIDToAssetPath(guid));
            if (asset != null) result.Add(asset);
        }

        return result.OrderBy(a => a.name, StringComparer.OrdinalIgnoreCase).ToList();
    }

    public static bool IsValidTreeName(string name, out string error)
    {
        error = null;
        if (string.IsNullOrWhiteSpace(name))
        {
            error = "Enter a name.";
            return false;
        }

        if (name.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
        {
            error = "The name contains characters that are not allowed in file names.";
            return false;
        }

        if (!string.IsNullOrEmpty(AssetDatabase.AssetPathToGUID(PathFor(name.Trim()))))
        {
            error = $"A skill tree called \"{name.Trim()}\" already exists.";
            return false;
        }

        return true;
    }

    /// <summary>Creates Assets/Resources/SkillTrees/{name}.asset seeded with one Initial node.</summary>
    public static SkillTreeAsset CreateTreeAsset(string name)
    {
        EnsureFolders();

        string path = AssetDatabase.GenerateUniqueAssetPath(PathFor(name.Trim()));
        var asset = ScriptableObject.CreateInstance<SkillTreeAsset>();
        asset.AddOption(new Initial(), Vector2.zero);

        AssetDatabase.CreateAsset(asset, path);
        AssetDatabase.SaveAssets();
        return asset;
    }

    private static string PathFor(string name) => $"{TreeFolder}/{name}.asset";

    private static void EnsureFolders()
    {
        if (!AssetDatabase.IsValidFolder(ResourcesFolder)) AssetDatabase.CreateFolder("Assets", "Resources");
        if (!AssetDatabase.IsValidFolder(TreeFolder)) AssetDatabase.CreateFolder(ResourcesFolder, "SkillTrees");
    }

    // ----------------------------------------------------------------- types

    /// <summary>Concrete UpgradeOption types (Initial, Standart and anything added later).</summary>
    public static List<Type> GetNodeTypes() => ConcreteTypes<UpgradeOption>();

    /// <summary>Concrete ApplicationEffect types (GiveStat, GiveAbility and anything added later).</summary>
    public static List<Type> GetEffectTypes() => ConcreteTypes<ApplicationEffect>();

    private static List<Type> ConcreteTypes<T>()
    {
        return TypeCache.GetTypesDerivedFrom<T>()
            .Where(t => !t.IsAbstract && !t.IsGenericType && t.GetConstructor(Type.EmptyTypes) != null)
            .OrderBy(t => t.Name, StringComparer.Ordinal)
            .ToList();
    }

    public static string Nicify(Type type) => ObjectNames.NicifyVariableName(type.Name);

    /// <summary>One-line description of an effect, shown on the node.</summary>
    public static string Describe(ApplicationEffect effect)
    {
        switch (effect)
        {
            case null: return "(empty effect)";
            case GiveStat stat: return $"{(stat.amount >= 0 ? "+" : "")}{stat.amount:0.##} {stat.stat}";
            case GiveAbility _: return "Grants an ability";
            default: return Nicify(effect.GetType());
        }
    }

    // ---------------------------------------------------------------- layout

    /// <summary>
    /// Layered left-to-right layout: column = longest dependency chain from a root,
    /// rows are filled top to bottom within a column.
    /// </summary>
    public static Vector2[] ComputeLayout(IList<UpgradeOption> options)
    {
        const float columnWidth = 300f;
        const float rowHeight = 150f;

        int count = options.Count;
        var depth = new int[count];

        for (int pass = 0; pass < count; pass++)
        {
            bool changed = false;
            for (int i = 0; i < count; i++)
            {
                if (options[i]?.dependant == null) continue;
                foreach (UpgradeOption dependant in options[i].dependant)
                {
                    int j = options.IndexOf(dependant);
                    if (j < 0 || j == i || depth[j] >= depth[i] + 1) continue;
                    depth[j] = depth[i] + 1;
                    changed = true;
                }
            }

            if (!changed) break;
        }

        var rowInColumn = new Dictionary<int, int>();
        var positions = new Vector2[count];
        for (int i = 0; i < count; i++)
        {
            rowInColumn.TryGetValue(depth[i], out int row);
            rowInColumn[depth[i]] = row + 1;
            positions[i] = new Vector2(depth[i] * columnWidth, row * rowHeight);
        }

        return positions;
    }
}
