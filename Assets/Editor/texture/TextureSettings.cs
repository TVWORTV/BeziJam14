using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(
    fileName = "TextureImportSettings",
    menuName = "Import Settings/Texture Import Settings"
)]
public class TextureImportSettings : ScriptableObject
{
    public int pixelsPerUnit = 16;

#if UNITY_EDITOR
    private void OnValidate()
    {
        // Delay is important to avoid import loops
        EditorApplication.delayCall += ReimportTextures;
    }

    private void ReimportTextures()
    {
        if (this == null) return;

        string[] textureGuids = AssetDatabase.FindAssets(
        "t:Texture2D",
        new[] { "Assets" }   // 👈 THIS is the key
        );


        foreach (string guid in textureGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);

            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
        }

        Debug.Log("Reimported textures due to TextureImportSettings change.");
    }
#endif
}