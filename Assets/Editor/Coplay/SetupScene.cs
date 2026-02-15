using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using Gameplay.Grid;
using Core;

public class SetupScene
{
    public static string Execute()
    {
        // Create a new scene
        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        
        // Create GridCellView Prefab
        GameObject cellObj = new GameObject("GridCellView");
        var spriteRenderer = cellObj.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd"); // Use a default sprite
        var cellView = cellObj.AddComponent<GridCellView>();
        
        // Set up colors for GridCellView (using reflection or serialized object since fields are private)
        SerializedObject so = new SerializedObject(cellView);
        SerializedProperty colorsProp = so.FindProperty("colors");
        colorsProp.arraySize = 5;
        colorsProp.GetArrayElementAtIndex(0).colorValue = Color.clear; // Empty
        colorsProp.GetArrayElementAtIndex(1).colorValue = Color.red;
        colorsProp.GetArrayElementAtIndex(2).colorValue = Color.blue;
        colorsProp.GetArrayElementAtIndex(3).colorValue = Color.green;
        colorsProp.GetArrayElementAtIndex(4).colorValue = Color.yellow;
        so.ApplyModifiedProperties();

        string prefabPath = "Assets/_Project/Prefabs/Gameplay/GridCellView.prefab";
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(cellObj, prefabPath);
        Object.DestroyImmediate(cellObj);

        // Create GridManager
        GameObject gridManagerObj = new GameObject("GridManager");
        var gridManager = gridManagerObj.AddComponent<GridManager>();
        
        // Assign prefab to GridManager
        SerializedObject gmSo = new SerializedObject(gridManager);
        gmSo.FindProperty("cellPrefab").objectReferenceValue = prefab.GetComponent<GridCellView>();
        gmSo.FindProperty("width").intValue = 10;
        gmSo.FindProperty("height").intValue = 10;
        gmSo.FindProperty("cellSize").floatValue = 1.1f; // Add some spacing
        gmSo.ApplyModifiedProperties();

        // Create GameManager
        GameObject gameManagerObj = new GameObject("GameManager");
        var gameManager = gameManagerObj.AddComponent<GameManager>();
        
        // Assign GridManager to GameManager
        SerializedObject gameManagerSo = new SerializedObject(gameManager);
        gameManagerSo.FindProperty("gridManager").objectReferenceValue = gridManager;
        gameManagerSo.ApplyModifiedProperties();

        // Save Scene
        EditorSceneManager.SaveScene(scene, "Assets/_Project/Scenes/Gameplay.unity");

        return "Scene setup complete.";
    }
}
