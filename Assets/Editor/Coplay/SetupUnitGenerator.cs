using UnityEngine;
using UnityEditor;
using Coplay.Controllers.Functions;
using LevelEditor;

public class SetupUnitGenerator
{
    public static string Execute()
    {
        // Find the AutoBalanceArea object
        var autoBalanceArea = GameObject.Find("Canvas/BottomPanel/AutoBalanceArea");
        if (autoBalanceArea == null)
        {
            return "Error: Could not find Canvas/BottomPanel/AutoBalanceArea";
        }

        // Add the component if it doesn't exist
        var generator = autoBalanceArea.GetComponent<LevelEditorUnitGenerator>();
        if (generator == null)
        {
            generator = autoBalanceArea.AddComponent<LevelEditorUnitGenerator>();
        }

        // Assign references manually to be safe
        // We need to find objects in the scene
        var gridManager = Object.FindFirstObjectByType<LevelEditorGridManager>();
        var generateButton = GameObject.Find("Canvas/BottomPanel/AutoBalanceArea/GenerateButton")?.GetComponent<UnityEngine.UI.Button>();
        var sliderContainer = GameObject.Find("Canvas/BottomPanel/AutoBalanceArea/SliderContainer");
        var difficultySlider = sliderContainer?.GetComponentInChildren<UnityEngine.UI.Slider>();
        var unitTimeline = GameObject.Find("Canvas/BottomPanel/TimelineArea/UnitTimeline")?.transform;
        
        // Load UnitIcon prefab
        // We need to find the asset path.
        // Assets/_Project/Prefabs/UI/UnitIcon.prefab
        var unitIconPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/UI/UnitIcon.prefab");

        // Use SerializedObject to set private fields
        var so = new SerializedObject(generator);
        so.FindProperty("gridManager").objectReferenceValue = gridManager;
        so.FindProperty("generateButton").objectReferenceValue = generateButton;
        so.FindProperty("difficultySlider").objectReferenceValue = difficultySlider;
        so.FindProperty("unitTimelineContainer").objectReferenceValue = unitTimeline;
        so.FindProperty("unitIconPrefab").objectReferenceValue = unitIconPrefab;
        so.ApplyModifiedProperties();

        // Save the scene
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(autoBalanceArea.scene);
        UnityEditor.SceneManagement.EditorSceneManager.SaveScene(autoBalanceArea.scene);

        return "Successfully added LevelEditorUnitGenerator and assigned references.";
    }
}
