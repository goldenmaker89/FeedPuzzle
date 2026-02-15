using UnityEngine;
using UnityEditor;
using Coplay.Controllers.Functions;
using Core;
using Gameplay.Mechanics;
using Meta.UI;
using UnityEngine.UI;

public class SetupSceneManagers
{
    public static string Execute()
    {
        // 1. Create Managers GameObject
        var managersGO = GameObject.Find("Managers");
        if (managersGO == null)
        {
            managersGO = new GameObject("Managers");
        }

        // 2. Add InputManager
        if (managersGO.GetComponent<InputManager>() == null)
            managersGO.AddComponent<InputManager>();

        // 3. Add FlowManager
        var flowManager = managersGO.GetComponent<FlowManager>();
        if (flowManager == null)
            flowManager = managersGO.AddComponent<FlowManager>();

        // Assign ConveyorBelt
        var conveyorBelt = GameObject.FindFirstObjectByType<ConveyorBelt>();
        if (conveyorBelt != null)
        {
            var so = new SerializedObject(flowManager);
            so.FindProperty("conveyorBelt").objectReferenceValue = conveyorBelt;
            
            // Assign UnitPrefab
            var unitPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/Gameplay/UnitController.prefab");
            if (unitPrefab != null)
            {
                so.FindProperty("unitPrefab").objectReferenceValue = unitPrefab.GetComponent<Gameplay.Units.UnitController>();
            }
            else
            {
                Debug.LogError("UnitController prefab not found!");
            }
            so.ApplyModifiedProperties();
        }

        // 4. Add UIManager
        var uiManager = managersGO.GetComponent<UIManager>();
        if (uiManager == null)
            uiManager = managersGO.AddComponent<UIManager>();

        // Assign UI references
        var canvas = GameObject.Find("GameplayCanvas");
        if (canvas != null)
        {
            var hud = canvas.transform.Find("HUD")?.gameObject;
            var pauseMenu = canvas.transform.Find("PauseMenu")?.gameObject;
            var gameOverScreen = canvas.transform.Find("GameOverScreen")?.gameObject;
            var victoryScreen = canvas.transform.Find("VictoryScreen")?.gameObject;

            var so = new SerializedObject(uiManager);
            so.FindProperty("hud").objectReferenceValue = hud;
            so.FindProperty("pauseMenu").objectReferenceValue = pauseMenu;
            so.FindProperty("gameOverScreen").objectReferenceValue = gameOverScreen;
            so.FindProperty("victoryScreen").objectReferenceValue = victoryScreen;

            // Assign Buttons
            if (hud != null)
            {
                var pauseBtn = hud.transform.Find("PauseButton")?.GetComponent<Button>();
                so.FindProperty("pauseButton").objectReferenceValue = pauseBtn;
            }
            if (pauseMenu != null)
            {
                var resumeBtn = pauseMenu.transform.Find("ResumeButton")?.GetComponent<Button>();
                so.FindProperty("resumeButton").objectReferenceValue = resumeBtn;
                var restartBtn = pauseMenu.transform.Find("RestartButton")?.GetComponent<Button>();
                so.FindProperty("restartButton").objectReferenceValue = restartBtn;
            }
            if (gameOverScreen != null)
            {
                var restartBtn = gameOverScreen.transform.Find("RestartButton")?.GetComponent<Button>();
                so.FindProperty("gameOverRestartButton").objectReferenceValue = restartBtn;
            }
            if (victoryScreen != null)
            {
                var restartBtn = victoryScreen.transform.Find("RestartButton")?.GetComponent<Button>();
                so.FindProperty("victoryRestartButton").objectReferenceValue = restartBtn;
            }
            so.ApplyModifiedProperties();
            
            // Disable screens initially (except HUD)
            if (pauseMenu != null) pauseMenu.SetActive(false);
            if (gameOverScreen != null) gameOverScreen.SetActive(false);
            if (victoryScreen != null) victoryScreen.SetActive(false);
        }

        // 5. Create UnitIconPrefab
        var iconGO = new GameObject("UnitIcon");
        var iconRect = iconGO.AddComponent<RectTransform>();
        iconRect.sizeDelta = new Vector2(50, 50);
        var iconImage = iconGO.AddComponent<Image>();
        iconImage.color = Color.white;
        
        var textGO = new GameObject("Text");
        textGO.transform.SetParent(iconGO.transform);
        var textRect = textGO.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;
        var tmp = textGO.AddComponent<TMPro.TextMeshProUGUI>();
        tmp.text = "1";
        tmp.alignment = TMPro.TextAlignmentOptions.Center;
        tmp.color = Color.black;
        tmp.fontSize = 24;

        // Save as prefab
        if (!AssetDatabase.IsValidFolder("Assets/_Project/Prefabs/UI"))
        {
            if (!AssetDatabase.IsValidFolder("Assets/_Project/Prefabs"))
            {
                AssetDatabase.CreateFolder("Assets/_Project", "Prefabs");
            }
            AssetDatabase.CreateFolder("Assets/_Project/Prefabs", "UI");
        }
        var iconPrefab = PrefabUtility.SaveAsPrefabAsset(iconGO, "Assets/_Project/Prefabs/UI/UnitIcon.prefab");
        GameObject.DestroyImmediate(iconGO);

        // 6. Add QueueVisualizer
        var queueVisualizer = managersGO.GetComponent<QueueVisualizer>();
        if (queueVisualizer == null)
            queueVisualizer = managersGO.AddComponent<QueueVisualizer>();
            
        if (canvas != null)
        {
            var queueArea = canvas.transform.Find("HUD/QueueArea");
            var so = new SerializedObject(queueVisualizer);
            so.FindProperty("flowManager").objectReferenceValue = flowManager;
            so.FindProperty("container").objectReferenceValue = queueArea;
            so.FindProperty("unitIconPrefab").objectReferenceValue = iconPrefab;
            so.ApplyModifiedProperties();
        }

        // 7. Add LandingStripVisualizer
        var landingVisualizer = managersGO.GetComponent<LandingStripVisualizer>();
        if (landingVisualizer == null)
            landingVisualizer = managersGO.AddComponent<LandingStripVisualizer>();

        var landingStrip = GameObject.FindFirstObjectByType<LandingStrip>();
        if (canvas != null && landingStrip != null)
        {
            var landingArea = canvas.transform.Find("HUD/LandingArea");
            var so = new SerializedObject(landingVisualizer);
            so.FindProperty("landingStrip").objectReferenceValue = landingStrip;
            so.FindProperty("container").objectReferenceValue = landingArea;
            so.FindProperty("unitIconPrefab").objectReferenceValue = iconPrefab;
            so.ApplyModifiedProperties();
        }

        return "Managers setup complete.";
    }
}
