using UnityEngine;
using UnityEditor;
using Gameplay.Units;
using Gameplay.Grid;
using Core;

public class CreateGamePrefabs
{
    public static string Execute()
    {
        // Create UnitController Prefab
        GameObject unitGO = new GameObject("UnitController");
        unitGO.AddComponent<SpriteRenderer>();
        unitGO.AddComponent<UnitController>();
        
        // Add TextMeshPro for capacity if needed (optional)
        GameObject textGO = new GameObject("CapacityText");
        textGO.transform.SetParent(unitGO.transform);
        textGO.AddComponent<TMPro.TextMeshPro>();
        
        string unitPrefabPath = "Assets/_Project/Prefabs/Gameplay/UnitController.prefab";
        GameObject unitPrefab = PrefabUtility.SaveAsPrefabAsset(unitGO, unitPrefabPath);
        GameObject.DestroyImmediate(unitGO);

        // Create LinkedUnitController Prefab
        GameObject linkedGO = new GameObject("LinkedUnitController");
        linkedGO.AddComponent<LinkedUnitController>();
        
        string linkedPrefabPath = "Assets/_Project/Prefabs/Gameplay/LinkedUnitController.prefab";
        GameObject linkedPrefab = PrefabUtility.SaveAsPrefabAsset(linkedGO, linkedPrefabPath);
        GameObject.DestroyImmediate(linkedGO);

        // Create GridCellView Prefab
        GameObject cellGO = new GameObject("GridCellView");
        cellGO.AddComponent<SpriteRenderer>();
        GridCellView cellView = cellGO.AddComponent<GridCellView>();
        
        // Set up colors for GridCellView (using reflection or serialized object)
        SerializedObject cellSO = new SerializedObject(cellView);
        SerializedProperty colorsProp = cellSO.FindProperty("colors");
        colorsProp.arraySize = 5;
        colorsProp.GetArrayElementAtIndex(0).colorValue = Color.white; // Empty
        colorsProp.GetArrayElementAtIndex(1).colorValue = Color.red;
        colorsProp.GetArrayElementAtIndex(2).colorValue = Color.blue;
        colorsProp.GetArrayElementAtIndex(3).colorValue = Color.green;
        colorsProp.GetArrayElementAtIndex(4).colorValue = Color.yellow;
        cellSO.ApplyModifiedProperties();
        
        string cellPrefabPath = "Assets/_Project/Prefabs/Gameplay/GridCellView.prefab";
        GameObject cellPrefab = PrefabUtility.SaveAsPrefabAsset(cellGO, cellPrefabPath);
        GameObject.DestroyImmediate(cellGO);

        // Assign to TestSceneSetup in scene
        TestSceneSetup setup = Object.FindFirstObjectByType<TestSceneSetup>();
        if (setup != null)
        {
            SerializedObject so = new SerializedObject(setup);
            so.FindProperty("unitPrefab").objectReferenceValue = unitPrefab.GetComponent<UnitController>();
            so.FindProperty("linkedUnitPrefab").objectReferenceValue = linkedPrefab.GetComponent<LinkedUnitController>();
            so.ApplyModifiedProperties();
        }
        
        // Assign to GridManager in scene
        Gameplay.Grid.GridManager gridManager = Object.FindFirstObjectByType<Gameplay.Grid.GridManager>();
        if (gridManager != null)
        {
            SerializedObject so = new SerializedObject(gridManager);
            so.FindProperty("cellPrefab").objectReferenceValue = cellPrefab.GetComponent<GridCellView>();
            so.ApplyModifiedProperties();
        }

        return $"Created prefabs at {unitPrefabPath}, {linkedPrefabPath}, {cellPrefabPath}, and assigned to TestSceneSetup and GridManager.";
    }
}
