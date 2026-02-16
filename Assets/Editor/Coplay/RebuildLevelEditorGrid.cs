using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using LevelEditor;

public class RebuildLevelEditorGrid
{
    public static string Execute()
    {
        // Find GridContainer
        GameObject gridContainer = null;
        GameObject canvas = GameObject.Find("Canvas");
        if (canvas != null)
        {
            Transform levelCanvas = canvas.transform.Find("LevelCanvas");
            if (levelCanvas != null)
            {
                Transform gc = levelCanvas.Find("GridContainer");
                if (gc != null) gridContainer = gc.gameObject;
            }
        }

        if (gridContainer == null)
        {
            return "Error: GridContainer not found.";
        }

        // Count current children before rebuild
        int oldChildCount = gridContainer.transform.childCount;

        // Get the grid manager
        LevelEditorGridManager gridManager = gridContainer.GetComponent<LevelEditorGridManager>();
        if (gridManager == null)
        {
            return "Error: LevelEditorGridManager not found on GridContainer.";
        }

        // Remove any old layout components
        var oldGridLayout = gridContainer.GetComponent<GridLayoutGroup>();
        if (oldGridLayout != null) Object.DestroyImmediate(oldGridLayout);
        
        var oldVertLayout = gridContainer.GetComponent<VerticalLayoutGroup>();
        if (oldVertLayout != null) Object.DestroyImmediate(oldVertLayout);
        
        var oldFitter = gridContainer.GetComponent<ContentSizeFitter>();
        if (oldFitter != null) Object.DestroyImmediate(oldFitter);

        // Update serialized properties
        SerializedObject so = new SerializedObject(gridManager);
        var cellSizeProp = so.FindProperty("cellSize");
        if (cellSizeProp != null) cellSizeProp.vector2Value = new Vector2(11, 11);
        var spacingProp = so.FindProperty("spacing");
        if (spacingProp != null) spacingProp.vector2Value = new Vector2(1, 1);
        so.ApplyModifiedProperties();

        // Ensure the GridContainer RectTransform is set to stretch-fill with padding
        RectTransform gridRect = gridContainer.GetComponent<RectTransform>();
        gridRect.anchorMin = Vector2.zero;
        gridRect.anchorMax = Vector2.one;
        gridRect.offsetMin = new Vector2(10, 10);
        gridRect.offsetMax = new Vector2(-10, -10);

        // Force re-initialization with the new row-based hierarchy
        gridManager.InitializeGrid();

        // Count new children after rebuild
        int newChildCount = gridContainer.transform.childCount;

        // Count total cells
        int totalCells = 0;
        int maxSiblingsPerRow = 0;
        foreach (Transform row in gridContainer.transform)
        {
            totalCells += row.childCount;
            if (row.childCount > maxSiblingsPerRow)
                maxSiblingsPerRow = row.childCount;
        }

        // Mark scene dirty and save
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
        UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();

        return $"Grid rebuilt successfully!\n" +
               $"Before: {oldChildCount} direct children under GridContainer\n" +
               $"After: {newChildCount} row containers under GridContainer\n" +
               $"Total cells: {totalCells}\n" +
               $"Max siblings per row: {maxSiblingsPerRow}\n" +
               $"Scene saved.";
    }
}
