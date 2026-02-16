using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using LevelEditor;

public class SetupBrushTool
{
    public static string Execute()
    {
        var results = new System.Text.StringBuilder();

        // Find Canvas
        var canvas = GameObject.Find("Canvas");
        if (canvas == null) return "ERROR: Canvas not found";

        // Find TilePalette
        var tilePalette = canvas.transform.Find("TilePalette");
        if (tilePalette == null) return "ERROR: TilePalette not found";

        // Find tool buttons
        var toolsContainer = tilePalette.Find("ToolsContainer");
        var brushButton = toolsContainer?.Find("BrushTool")?.GetComponent<Button>();
        var bucketButton = toolsContainer?.Find("BucketTool")?.GetComponent<Button>();
        var eraserButton = toolsContainer?.Find("EraserTool")?.GetComponent<Button>();

        results.AppendLine($"BrushButton: {brushButton != null}");
        results.AppendLine($"BucketButton: {bucketButton != null}");
        results.AppendLine($"EraserButton: {eraserButton != null}");

        // Find GridContainer and its LevelEditorGridManager
        var levelCanvas = canvas.transform.Find("LevelCanvas");
        var gridContainer = levelCanvas?.Find("GridContainer");
        var gridManager = gridContainer?.GetComponent<LevelEditorGridManager>();
        results.AppendLine($"GridManager: {gridManager != null}");

        // Add LevelEditorToolManager to Canvas (or TilePalette)
        // We'll put it on Canvas so it's easy to find
        var toolManager = canvas.GetComponent<LevelEditorToolManager>();
        if (toolManager == null)
        {
            toolManager = Undo.AddComponent<LevelEditorToolManager>(canvas);
            results.AppendLine("Added LevelEditorToolManager to Canvas");
        }
        else
        {
            results.AppendLine("LevelEditorToolManager already exists on Canvas");
        }

        // Set serialized fields via SerializedObject
        var so = new SerializedObject(toolManager);

        so.FindProperty("brushButton").objectReferenceValue = brushButton;
        so.FindProperty("bucketButton").objectReferenceValue = bucketButton;
        so.FindProperty("eraserButton").objectReferenceValue = eraserButton;
        so.FindProperty("gridManager").objectReferenceValue = gridManager;
        so.FindProperty("defaultColorId").intValue = 1;

        // Find palette items (PaletteItem_0 through PaletteItem_8)
        // PaletteItem_0 is colorId 0 (empty), PaletteItem_1 is colorId 1 (red), etc.
        // We need to add LevelEditorPaletteItem to each and set colorId
        var paletteItemsList = new System.Collections.Generic.List<LevelEditorPaletteItem>();

        for (int i = 0; i <= 8; i++)
        {
            var paletteItemTransform = tilePalette.Find($"PaletteItem_{i}");
            if (paletteItemTransform == null)
            {
                results.AppendLine($"PaletteItem_{i}: NOT FOUND");
                continue;
            }

            var paletteItem = paletteItemTransform.GetComponent<LevelEditorPaletteItem>();
            if (paletteItem == null)
            {
                paletteItem = Undo.AddComponent<LevelEditorPaletteItem>(paletteItemTransform.gameObject);
                results.AppendLine($"Added LevelEditorPaletteItem to PaletteItem_{i}");
            }

            // Set colorId via SerializedObject
            var paletteSO = new SerializedObject(paletteItem);
            paletteSO.FindProperty("colorId").intValue = i;
            
            // Set colorImage reference
            var colorImage = paletteItemTransform.GetComponent<Image>();
            paletteSO.FindProperty("colorImage").objectReferenceValue = colorImage;
            
            paletteSO.ApplyModifiedProperties();

            paletteItemsList.Add(paletteItem);
        }

        // Set paletteItems array on tool manager
        var paletteItemsProp = so.FindProperty("paletteItems");
        paletteItemsProp.arraySize = paletteItemsList.Count;
        for (int i = 0; i < paletteItemsList.Count; i++)
        {
            paletteItemsProp.GetArrayElementAtIndex(i).objectReferenceValue = paletteItemsList[i];
        }

        so.ApplyModifiedProperties();

        // Also set the toolManager reference on the GridManager
        if (gridManager != null)
        {
            var gridSO = new SerializedObject(gridManager);
            gridSO.FindProperty("toolManager").objectReferenceValue = toolManager;
            gridSO.ApplyModifiedProperties();
            results.AppendLine("Set toolManager on GridManager");
        }

        // Mark scene dirty
        EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();

        results.AppendLine("\nSetup complete!");
        return results.ToString();
    }
}
