using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using LevelEditor;
using Gameplay.Grid;

public class SetupLevelEditorGrid
{
    public static string Execute()
    {
        // Find LevelCanvas
        GameObject levelCanvas = GameObject.Find("LevelCanvas");
        if (levelCanvas == null)
        {
            // Try to find it under Canvas
            GameObject canvas = GameObject.Find("Canvas");
            if (canvas != null)
            {
                Transform t = canvas.transform.Find("LevelCanvas");
                if (t != null) levelCanvas = t.gameObject;
            }
        }

        if (levelCanvas == null)
        {
            return "Error: LevelCanvas not found in the scene.";
        }

        // Create GridContainer
        Transform gridContainerTransform = levelCanvas.transform.Find("GridContainer");
        GameObject gridContainer;
        if (gridContainerTransform == null)
        {
            gridContainer = new GameObject("GridContainer");
            gridContainer.transform.SetParent(levelCanvas.transform, false);
        }
        else
        {
            gridContainer = gridContainerTransform.gameObject;
        }

        // Setup RectTransform for GridContainer
        RectTransform gridRect = gridContainer.GetComponent<RectTransform>();
        if (gridRect == null) gridRect = gridContainer.AddComponent<RectTransform>();

        // Stretch to fill parent with some padding
        gridRect.anchorMin = Vector2.zero;
        gridRect.anchorMax = Vector2.one;
        gridRect.offsetMin = new Vector2(10, 10);
        gridRect.offsetMax = new Vector2(-10, -10);

        // Remove old GridLayoutGroup if present (we now use VerticalLayoutGroup + row HorizontalLayoutGroups)
        GridLayoutGroup oldGridLayout = gridContainer.GetComponent<GridLayoutGroup>();
        if (oldGridLayout != null)
        {
            Object.DestroyImmediate(oldGridLayout);
        }

        // Create Cell Prefab
        string prefabPath = "Assets/_Project/Prefabs/UI/LevelEditorCell.prefab";
        GameObject cellPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

        if (cellPrefab == null)
        {
            GameObject tempCell = new GameObject("LevelEditorCell");
            Image img = tempCell.AddComponent<Image>();
            img.color = Color.white; // Default
            tempCell.AddComponent<LevelEditorCellView>();

            // Ensure directory exists
            if (!System.IO.Directory.Exists("Assets/_Project/Prefabs/UI"))
            {
                System.IO.Directory.CreateDirectory("Assets/_Project/Prefabs/UI");
            }

            cellPrefab = PrefabUtility.SaveAsPrefabAsset(tempCell, prefabPath);
            GameObject.DestroyImmediate(tempCell);
        }

        // Add LevelEditorGridManager to GridContainer
        LevelEditorGridManager gridManager = gridContainer.GetComponent<LevelEditorGridManager>();
        if (gridManager == null) gridManager = gridContainer.AddComponent<LevelEditorGridManager>();

        // Configure GridManager via SerializedObject
        SerializedObject so = new SerializedObject(gridManager);
        so.FindProperty("width").intValue = 30;
        so.FindProperty("height").intValue = 35;
        so.FindProperty("cellPrefab").objectReferenceValue = cellPrefab.GetComponent<LevelEditorCellView>();
        so.FindProperty("gridContainer").objectReferenceValue = gridContainer.transform;
        so.FindProperty("cellSize").vector2Value = new Vector2(11, 11);
        so.FindProperty("spacing").vector2Value = new Vector2(1, 1);
        so.ApplyModifiedProperties();

        // Force initialization - this will create the row-based hierarchy
        gridManager.InitializeGrid();

        // Mark scene dirty
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());

        return "LevelEditor Grid setup complete with row-based hierarchy (35 rows x 30 cells each). " +
               "This avoids the Unity 6 ATGTextJobSystem crash by limiting siblings per parent to 35 (rows) and 30 (cells per row).";
    }
}
