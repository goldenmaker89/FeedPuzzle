using UnityEngine;
using UnityEditor;
using Gameplay.Units;
using Gameplay.Grid;

public class FixUnitPrefab
{
    public static string Execute()
    {
        // Load the square sprite
        Sprite squareSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Art/Gameplay/Square.png");

        // Recreate UnitController prefab without TMP child
        string unitPrefabPath = "Assets/_Project/Prefabs/Gameplay/UnitController.prefab";
        
        GameObject unitGO = new GameObject("UnitController");
        var sr = unitGO.AddComponent<SpriteRenderer>();
        if (squareSprite != null)
            sr.sprite = squareSprite;
        sr.sortingOrder = 10;

        // Add BoxCollider2D for click detection
        var col = unitGO.AddComponent<BoxCollider2D>();
        col.size = new Vector2(0.32f, 0.32f);

        unitGO.AddComponent<UnitController>();

        // Save prefab (overwrite)
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(unitGO, unitPrefabPath);
        Object.DestroyImmediate(unitGO);

        // Also recreate GridCellView prefab to ensure it's clean
        string cellPrefabPath = "Assets/_Project/Prefabs/Gameplay/GridCellView.prefab";
        GameObject cellGO = new GameObject("GridCellView");
        var cellSr = cellGO.AddComponent<SpriteRenderer>();
        if (squareSprite != null)
            cellSr.sprite = squareSprite;
        cellGO.AddComponent<GridCellView>();

        GameObject cellPrefab = PrefabUtility.SaveAsPrefabAsset(cellGO, cellPrefabPath);
        Object.DestroyImmediate(cellGO);

        // Reassign to GridManager in scene
        GridManager gridManager = Object.FindFirstObjectByType<GridManager>();
        if (gridManager != null)
        {
            SerializedObject so = new SerializedObject(gridManager);
            so.FindProperty("cellPrefab").objectReferenceValue = cellPrefab.GetComponent<GridCellView>();
            so.ApplyModifiedProperties();
        }

        // Reassign to BaseManager in scene
        Gameplay.Mechanics.BaseManager baseManager = Object.FindFirstObjectByType<Gameplay.Mechanics.BaseManager>();
        if (baseManager != null)
        {
            SerializedObject so = new SerializedObject(baseManager);
            so.FindProperty("unitPrefab").objectReferenceValue = prefab.GetComponent<UnitController>();
            so.ApplyModifiedProperties();
        }

        // Save scene
        UnityEditor.SceneManagement.EditorSceneManager.SaveScene(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());

        return $"Recreated prefabs. UnitController: no TMP child, has BoxCollider2D. GridCellView: clean. Assigned to scene managers.";
    }
}
