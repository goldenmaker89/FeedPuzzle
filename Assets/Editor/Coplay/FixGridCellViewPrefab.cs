using UnityEngine;
using UnityEditor;
using Gameplay.Grid;

public class FixGridCellViewPrefab
{
    public static string Execute()
    {
        string result = "";

        // Fix GridCellView prefab - assign spriteRenderer
        string prefabPath = "Assets/_Project/Prefabs/Gameplay/GridCellView.prefab";
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if (prefab != null)
        {
            // Open prefab for editing
            string assetPath = AssetDatabase.GetAssetPath(prefab);
            GameObject prefabRoot = PrefabUtility.LoadPrefabContents(assetPath);

            GridCellView cellView = prefabRoot.GetComponent<GridCellView>();
            SpriteRenderer sr = prefabRoot.GetComponent<SpriteRenderer>();

            if (cellView != null && sr != null)
            {
                // Use SerializedObject to set the private field
                var so = new SerializedObject(cellView);
                var prop = so.FindProperty("spriteRenderer");
                if (prop != null)
                {
                    prop.objectReferenceValue = sr;
                    so.ApplyModifiedProperties();
                    result += "Fixed GridCellView spriteRenderer reference.\n";
                }
                else
                {
                    result += "Could not find spriteRenderer property.\n";
                }
            }
            else
            {
                result += $"GridCellView: {cellView != null}, SpriteRenderer: {sr != null}\n";
            }

            PrefabUtility.SaveAsPrefabAsset(prefabRoot, assetPath);
            PrefabUtility.UnloadPrefabContents(prefabRoot);
        }
        else
        {
            result += "GridCellView prefab not found.\n";
        }

        // Fix UnitController prefab - ensure BoxCollider2D is present
        string unitPrefabPath = "Assets/_Project/Prefabs/Gameplay/UnitController.prefab";
        GameObject unitPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(unitPrefabPath);
        if (unitPrefab != null)
        {
            string assetPath = AssetDatabase.GetAssetPath(unitPrefab);
            GameObject prefabRoot = PrefabUtility.LoadPrefabContents(assetPath);

            BoxCollider2D col = prefabRoot.GetComponent<BoxCollider2D>();
            if (col == null)
            {
                col = prefabRoot.AddComponent<BoxCollider2D>();
                result += "Added BoxCollider2D to UnitController prefab.\n";
            }

            // Set collider size to match sprite
            SpriteRenderer sr = prefabRoot.GetComponent<SpriteRenderer>();
            if (sr != null && sr.sprite != null)
            {
                col.size = sr.sprite.bounds.size;
                result += $"Set BoxCollider2D size to {col.size}.\n";
            }

            PrefabUtility.SaveAsPrefabAsset(prefabRoot, assetPath);
            PrefabUtility.UnloadPrefabContents(prefabRoot);
        }
        else
        {
            result += "UnitController prefab not found.\n";
        }

        AssetDatabase.Refresh();
        return result;
    }
}
