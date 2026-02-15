using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using Gameplay.Grid;
using Gameplay.Mechanics;
using Gameplay.Units;
using Core;

public class SetupFullScene
{
    public static string Execute()
    {
        // Ensure we're in the Gameplay scene
        var scene = EditorSceneManager.GetActiveScene();

        // ---- Clean up old objects ----
        DestroyIfExists("GridManager");
        DestroyIfExists("GameManager");
        DestroyIfExists("ConveyorBelt");
        DestroyIfExists("LandingStrip");
        DestroyIfExists("TestSceneSetup");
        DestroyIfExists("BaseManager");
        DestroyIfExists("FlowManager");
        DestroyIfExists("InputManager");
        DestroyIfExists("WinLossManager");
        DestroyIfExists("Managers");

        // ---- Ensure prefabs exist ----
        // GridCellView prefab
        string cellPrefabPath = "Assets/_Project/Prefabs/Gameplay/GridCellView.prefab";
        GameObject cellPrefabAsset = AssetDatabase.LoadAssetAtPath<GameObject>(cellPrefabPath);
        if (cellPrefabAsset == null)
        {
            cellPrefabAsset = CreateGridCellViewPrefab(cellPrefabPath);
        }

        // UnitController prefab
        string unitPrefabPath = "Assets/_Project/Prefabs/Gameplay/UnitController.prefab";
        GameObject unitPrefabAsset = AssetDatabase.LoadAssetAtPath<GameObject>(unitPrefabPath);
        if (unitPrefabAsset == null)
        {
            unitPrefabAsset = CreateUnitControllerPrefab(unitPrefabPath);
        }

        // ---- Create scene objects ----

        // 1. GridManager
        GameObject gridGO = new GameObject("GridManager");
        GridManager gridManager = gridGO.AddComponent<GridManager>();
        {
            var so = new SerializedObject(gridManager);
            so.FindProperty("width").intValue = 6;
            so.FindProperty("height").intValue = 6;
            so.FindProperty("cellSize").floatValue = 0.4f;
            so.FindProperty("cellPrefab").objectReferenceValue = cellPrefabAsset.GetComponent<GridCellView>();
            so.ApplyModifiedProperties();
        }

        // 2. ConveyorBelt
        GameObject conveyorGO = new GameObject("ConveyorBelt");
        ConveyorBelt conveyorBelt = conveyorGO.AddComponent<ConveyorBelt>();

        // 3. LandingStrip
        GameObject landingGO = new GameObject("LandingStrip");
        LandingStrip landingStrip = landingGO.AddComponent<LandingStrip>();

        // 4. BaseManager
        GameObject baseGO = new GameObject("BaseManager");
        BaseManager baseManager = baseGO.AddComponent<BaseManager>();
        {
            var so = new SerializedObject(baseManager);
            so.FindProperty("unitPrefab").objectReferenceValue = unitPrefabAsset.GetComponent<UnitController>();
            so.ApplyModifiedProperties();
        }

        // 5. FlowManager
        GameObject flowGO = new GameObject("FlowManager");
        FlowManager flowManager = flowGO.AddComponent<FlowManager>();

        // 6. InputManager
        GameObject inputGO = new GameObject("InputManager");
        InputManager inputManager = inputGO.AddComponent<InputManager>();

        // 7. GameManager (central, references all others)
        GameObject gmGO = new GameObject("GameManager");
        GameManager gameManager = gmGO.AddComponent<GameManager>();
        {
            var so = new SerializedObject(gameManager);
            so.FindProperty("gridManager").objectReferenceValue = gridManager;
            so.FindProperty("conveyorBelt").objectReferenceValue = conveyorBelt;
            so.FindProperty("landingStrip").objectReferenceValue = landingStrip;
            so.FindProperty("baseManager").objectReferenceValue = baseManager;
            so.FindProperty("flowManager").objectReferenceValue = flowManager;
            so.FindProperty("inputManager").objectReferenceValue = inputManager;
            so.ApplyModifiedProperties();
        }

        // 8. TestSceneSetup
        GameObject testGO = new GameObject("TestSceneSetup");
        TestSceneSetup testSetup = testGO.AddComponent<TestSceneSetup>();

        // 9. WinLossManager
        GameObject wlGO = new GameObject("WinLossManager");
        wlGO.AddComponent<WinLossManager>();

        // Mark scene dirty
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);

        return $"Scene setup complete. Created: GridManager, ConveyorBelt, LandingStrip, BaseManager, FlowManager, InputManager, GameManager, TestSceneSetup, WinLossManager. Prefabs verified at {cellPrefabPath} and {unitPrefabPath}.";
    }

    private static void DestroyIfExists(string name)
    {
        var go = GameObject.Find(name);
        if (go != null)
            Object.DestroyImmediate(go);
    }

    private static GameObject CreateGridCellViewPrefab(string path)
    {
        // Load the square sprite
        Sprite squareSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Art/Gameplay/Square.png");

        GameObject cellGO = new GameObject("GridCellView");
        var sr = cellGO.AddComponent<SpriteRenderer>();
        if (squareSprite != null)
            sr.sprite = squareSprite;
        cellGO.AddComponent<GridCellView>();

        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(cellGO, path);
        Object.DestroyImmediate(cellGO);
        return prefab;
    }

    private static GameObject CreateUnitControllerPrefab(string path)
    {
        Sprite squareSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Art/Gameplay/Square.png");

        GameObject unitGO = new GameObject("UnitController");
        var sr = unitGO.AddComponent<SpriteRenderer>();
        if (squareSprite != null)
            sr.sprite = squareSprite;
        sr.sortingOrder = 10;

        // Add BoxCollider2D for click detection
        var col = unitGO.AddComponent<BoxCollider2D>();
        col.size = new Vector2(0.32f, 0.32f);

        unitGO.AddComponent<UnitController>();

        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(unitGO, path);
        Object.DestroyImmediate(unitGO);
        return prefab;
    }
}
