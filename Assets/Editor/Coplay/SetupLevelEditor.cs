using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.UI;
using TMPro;
using Coplay.Controllers.Functions;

public class SetupLevelEditor
{
    public static string Execute()
    {
        // Create a new scene
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        
        // Create Main Camera
        var camera = new GameObject("Main Camera");
        var camComp = camera.AddComponent<Camera>();
        camComp.clearFlags = CameraClearFlags.SolidColor;
        camComp.backgroundColor = new Color(0.19f, 0.19f, 0.19f); // Dark gray background
        camera.tag = "MainCamera";
        camera.transform.position = new Vector3(0, 0, -10);

        // Create EventSystem
        var eventSystem = new GameObject("EventSystem");
        eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
        eventSystem.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();

        // Create Canvas
        var canvasGO = new GameObject("Canvas");
        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasGO.AddComponent<CanvasScaler>();
        canvasGO.AddComponent<GraphicRaycaster>();

        // Create Tile Palette (Left Panel)
        var tilePalette = CreatePanel(canvasGO.transform, "TilePalette", new Color(0.2f, 0.2f, 0.2f, 1f));
        var tilePaletteRect = tilePalette.GetComponent<RectTransform>();
        tilePaletteRect.anchorMin = new Vector2(0, 0.2f); // Left side, leaving space for bottom panel
        tilePaletteRect.anchorMax = new Vector2(0.2f, 1); // 20% width
        tilePaletteRect.offsetMin = Vector2.zero;
        tilePaletteRect.offsetMax = Vector2.zero;
        
        // Add Layout to Tile Palette
        var paletteLayout = tilePalette.AddComponent<VerticalLayoutGroup>();
        paletteLayout.childControlHeight = false;
        paletteLayout.childControlWidth = true;
        paletteLayout.spacing = 10;
        paletteLayout.padding = new RectOffset(10, 10, 10, 10);
        paletteLayout.childAlignment = TextAnchor.UpperCenter;
        
        CreateLabel(tilePalette.transform, "Tile Palette", false); // Label part of layout
        
        // Add placeholder buttons for Tile Palette
        Color[] paletteColors = { Color.red, Color.green, Color.blue, Color.yellow, Color.magenta, Color.cyan };
        foreach (var color in paletteColors)
        {
            CreatePaletteItem(tilePalette.transform, color);
        }

        // Create Unit Timeline (Bottom Panel)
        var unitTimeline = CreatePanel(canvasGO.transform, "UnitTimeline", new Color(0.25f, 0.25f, 0.25f, 1f));
        var unitTimelineRect = unitTimeline.GetComponent<RectTransform>();
        unitTimelineRect.anchorMin = new Vector2(0, 0); // Bottom
        unitTimelineRect.anchorMax = new Vector2(1, 0.2f); // 20% height
        unitTimelineRect.offsetMin = Vector2.zero;
        unitTimelineRect.offsetMax = Vector2.zero;
        
        // Add Layout to Unit Timeline
        var timelineLayout = unitTimeline.AddComponent<HorizontalLayoutGroup>();
        timelineLayout.childControlHeight = true;
        timelineLayout.childControlWidth = true;
        timelineLayout.spacing = 20;
        timelineLayout.padding = new RectOffset(20, 20, 40, 20); // Top padding for label
        timelineLayout.childAlignment = TextAnchor.MiddleCenter;

        CreateLabel(unitTimeline.transform, "Unit Timeline (4 Queues)", true);

        // Add 4 Queue placeholders
        for (int i = 1; i <= 4; i++)
        {
            CreateQueuePlaceholder(unitTimeline.transform, $"Queue {i}");
        }

        // Create Level Canvas (Center Panel)
        var levelCanvas = CreatePanel(canvasGO.transform, "LevelCanvas", new Color(0.15f, 0.15f, 0.15f, 1f));
        var levelCanvasRect = levelCanvas.GetComponent<RectTransform>();
        levelCanvasRect.anchorMin = new Vector2(0.2f, 0.2f); // Fill remaining space
        levelCanvasRect.anchorMax = new Vector2(1, 1);
        levelCanvasRect.offsetMin = Vector2.zero;
        levelCanvasRect.offsetMax = Vector2.zero;
        CreateLabel(levelCanvas.transform, "Level Canvas", true);

        // Save the scene
        string scenePath = "Assets/_Project/Scenes/LevelEditor.unity";
        EditorSceneManager.SaveScene(scene, scenePath);

        return $"Created LevelEditor scene at {scenePath} with UI structure: Tile Palette, Level Canvas, Unit Timeline.";
    }

    private static GameObject CreatePanel(Transform parent, string name, Color color)
    {
        var panel = new GameObject(name);
        panel.transform.SetParent(parent, false);
        var image = panel.AddComponent<Image>();
        image.color = color;
        return panel;
    }

    private static void CreateLabel(Transform parent, string text, bool ignoreLayout = true)
    {
        var labelGO = new GameObject("Label");
        labelGO.transform.SetParent(parent, false);
        var tmp = labelGO.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontSize = 24;
        tmp.color = Color.white;
        
        if (ignoreLayout)
        {
            var rect = labelGO.GetComponent<RectTransform>();
            var layoutElement = labelGO.AddComponent<LayoutElement>();
            layoutElement.ignoreLayout = true;

            rect.anchorMin = new Vector2(0, 1);
            rect.anchorMax = new Vector2(1, 1);
            rect.pivot = new Vector2(0.5f, 1);
            rect.sizeDelta = new Vector2(0, 50);
            rect.anchoredPosition = new Vector2(0, -10);
        }
        else
        {
            var layoutElement = labelGO.AddComponent<LayoutElement>();
            layoutElement.minHeight = 40;
            layoutElement.preferredHeight = 40;
        }
    }

    private static void CreatePaletteItem(Transform parent, Color color)
    {
        var item = new GameObject("PaletteItem");
        item.transform.SetParent(parent, false);
        var img = item.AddComponent<Image>();
        img.color = color;
        
        var layout = item.AddComponent<LayoutElement>();
        layout.preferredHeight = 50;
        layout.flexibleWidth = 1;
    }

    private static void CreateQueuePlaceholder(Transform parent, string name)
    {
        var queue = new GameObject(name);
        queue.transform.SetParent(parent, false);
        var img = queue.AddComponent<Image>();
        img.color = new Color(0.3f, 0.3f, 0.3f, 1f);
        
        var outline = queue.AddComponent<Outline>();
        outline.effectColor = Color.black;
        outline.effectDistance = new Vector2(2, -2);

        var labelGO = new GameObject("Label");
        labelGO.transform.SetParent(queue.transform, false);
        var tmp = labelGO.AddComponent<TextMeshProUGUI>();
        tmp.text = name;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontSize = 18;
        tmp.color = Color.gray;
        
        var rect = labelGO.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }
}
