using UnityEngine;
using UnityEditor;
using System.IO;

public class FixPrefabs
{
    public static string Execute()
    {
        // 1. Create a white square texture
        string texturePath = "Assets/_Project/Art/Gameplay/Square.png";
        string fullPath = Path.Combine(Application.dataPath, "_Project/Art/Gameplay/Square.png");
        
        // Ensure directory exists
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath));

        if (!File.Exists(fullPath))
        {
            Texture2D texture = new Texture2D(32, 32);
            Color[] pixels = new Color[32 * 32];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = Color.white;
            texture.SetPixels(pixels);
            texture.Apply();
            
            byte[] bytes = texture.EncodeToPNG();
            File.WriteAllBytes(fullPath, bytes);
            
            AssetDatabase.Refresh();
        }

        // 2. Configure TextureImporter to be a Sprite
        TextureImporter importer = AssetImporter.GetAtPath(texturePath) as TextureImporter;
        if (importer != null)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.SaveAndReimport();
        }

        // 3. Load the Sprite
        Sprite squareSprite = AssetDatabase.LoadAssetAtPath<Sprite>(texturePath);
        if (squareSprite == null)
        {
            return "Failed to load created sprite.";
        }

        // 4. Assign to Prefabs
        string[] prefabPaths = new string[] 
        {
            "Assets/_Project/Prefabs/Gameplay/UnitController.prefab",
            "Assets/_Project/Prefabs/Gameplay/GridCellView.prefab"
        };

        int count = 0;
        foreach (string path in prefabPaths)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab != null)
            {
                SpriteRenderer sr = prefab.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    sr.sprite = squareSprite;
                    EditorUtility.SetDirty(prefab);
                    count++;
                }
            }
        }
        
        AssetDatabase.SaveAssets();

        return $"Created Square sprite and assigned to {count} prefabs.";
    }
}
