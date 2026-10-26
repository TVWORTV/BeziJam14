using UnityEditor;
using UnityEngine;

public class TextureImportSettingsProcessor : AssetPostprocessor
{
    private static TextureImportSettings Settings
    {
        get
        {
            return AssetDatabase.LoadAssetAtPath<TextureImportSettings>(
                "Assets/Editor/texture/TextureImportSettings.asset"
            );
        }
    }

    void OnPreprocessTexture()
    {
        if (Settings == null)
        {
            Debug.LogWarning("TextureImportSettings asset not found.");
            return;
        }

        TextureImporter importer = (TextureImporter)assetImporter;

        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.filterMode = FilterMode.Point;
        importer.mipmapEnabled = false;
        importer.isReadable = false;
        importer.spritePixelsPerUnit = Settings.pixelsPerUnit;
    }
}
