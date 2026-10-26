using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using TMPro;

public class TMPFontReplacerWindow : EditorWindow
{
    TMP_FontAsset targetFont;

    [MenuItem("Tools/TMP Font Replacer")]
    static void Open()
    {
        GetWindow<TMPFontReplacerWindow>("TMP Font Replacer");
    }

    void OnGUI()
    {
        GUILayout.Label("Replace TMP Fonts", EditorStyles.boldLabel);

        targetFont = (TMP_FontAsset)EditorGUILayout.ObjectField(
            "New Font Asset",
            targetFont,
            typeof(TMP_FontAsset),
            false);

        GUI.enabled = targetFont != null;

        if (GUILayout.Button("Replace Fonts in Scenes and Prefabs", GUILayout.Height(40)))
        {
            ReplaceFonts();
        }

        GUI.enabled = true;
    }

    void ReplaceFonts()
    {
        int sceneCount = 0;
        int prefabCount = 0;

        // ---------- Loaded scenes ----------
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene scene = SceneManager.GetSceneAt(i);

            if (!scene.isLoaded)
                continue;

            GameObject[] roots = scene.GetRootGameObjects();

            for (int r = 0; r < roots.Length; r++)
            {
                TMP_Text[] texts = roots[r].GetComponentsInChildren<TMP_Text>(true);

                for (int t = 0; t < texts.Length; t++)
                {
                    TMP_Text text = texts[t];

                    if (text.font != targetFont)
                    {
                        Undo.RecordObject(text, "Replace TMP Font");
                        text.font = targetFont;
                        EditorUtility.SetDirty(text);
                        sceneCount++;
                    }
                }
            }

            EditorSceneManager.MarkSceneDirty(scene);
        }

        // ---------- Prefabs in Assets ----------
        string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab");

        for (int i = 0; i < prefabGuids.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(prefabGuids[i]);

            GameObject prefabRoot = PrefabUtility.LoadPrefabContents(path);

            bool changed = false;

            TMP_Text[] texts = prefabRoot.GetComponentsInChildren<TMP_Text>(true);

            for (int t = 0; t < texts.Length; t++)
            {
                TMP_Text text = texts[t];

                if (text.font != targetFont)
                {
                    text.font = targetFont;
                    changed = true;
                    prefabCount++;
                }
            }

            if (changed)
            {
                PrefabUtility.SaveAsPrefabAsset(prefabRoot, path);
            }

            PrefabUtility.UnloadPrefabContents(prefabRoot);
        }

        AssetDatabase.SaveAssets();

        Debug.Log($"TMP Font Replacer finished. Scene texts updated: {sceneCount}, Prefab texts updated: {prefabCount}");

        EditorUtility.DisplayDialog(
            "TMP Font Replacer",
            $"Done!\\n\\nScene texts updated: {sceneCount}\\nPrefab texts updated: {prefabCount}",
            "OK");
    }
}