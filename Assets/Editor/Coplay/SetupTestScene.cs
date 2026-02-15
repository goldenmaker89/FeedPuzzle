using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using Gameplay.Grid;
using Gameplay.Mechanics;
using Gameplay.Units;
using Core;
using TMPro;

public class SetupTestScene
{
    public static string Execute()
    {
        // Open the Gameplay scene
        EditorSceneManager.OpenScene("Assets/_Project/Scenes/Gameplay.unity");

        // Create Unit Prefab
        GameObject unitObj = new GameObject("UnitController");
        var spriteRenderer = unitObj.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd"); // Use a default sprite
        var unitController = unitObj.AddComponent<UnitController>();
        
        // Add TextMeshPro for capacity
        GameObject textObj = new GameObject("CapacityText");
        textObj.transform.SetParent(unitObj.transform);
        textObj.transform.localPosition = Vector3.zero;
        var tmp = textObj.AddComponent<TextMeshPro>();
        tmp.fontSize = 4;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.black;
        
        // Assign text to controller
        SerializedObject so = new SerializedObject(unitController);
        so.FindProperty("capacityText").objectReferenceValue = tmp;
        so.ApplyModifiedProperties();

        string prefabPath = "Assets/_Project/Prefabs/Gameplay/UnitController.prefab";
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(unitObj, prefabPath);
        Object.DestroyImmediate(unitObj);

        // Create ConveyorBelt
        GameObject conveyorObj = new GameObject("ConveyorBelt");
        var conveyor = conveyorObj.AddComponent<ConveyorBelt>();
        
        // Create LandingStrip
        GameObject landingObj = new GameObject("LandingStrip");
        var landingStrip = landingObj.AddComponent<LandingStrip>();
        landingObj.transform.position = new Vector3(0, -2, 0); // Below grid

        // Assign references
        SerializedObject conveyorSo = new SerializedObject(conveyor);
        conveyorSo.FindProperty("gridManager").objectReferenceValue = Object.FindFirstObjectByType<GridManager>();
        conveyorSo.FindProperty("landingStrip").objectReferenceValue = landingStrip;
        conveyorSo.ApplyModifiedProperties();

        // Create TestSceneSetup
        GameObject testSetupObj = new GameObject("TestSceneSetup");
        var testSetup = testSetupObj.AddComponent<TestSceneSetup>();
        SerializedObject testSo = new SerializedObject(testSetup);
        testSo.FindProperty("gridManager").objectReferenceValue = Object.FindFirstObjectByType<GridManager>();
        testSo.FindProperty("conveyorBelt").objectReferenceValue = conveyor;
        testSo.FindProperty("landingStrip").objectReferenceValue = landingStrip;
        testSo.FindProperty("unitPrefab").objectReferenceValue = prefab.GetComponent<UnitController>();
        testSo.ApplyModifiedProperties();

        // Save Scene
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());

        return "Test Scene setup complete.";
    }
}
