using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class UpdateLevelEditorUI_v2
{
    public static string Execute()
    {
        Cleanup();
        BuildUI();
        return "Level Editor UI Updated Successfully (v2)";
    }

    private static void Cleanup()
    {
        // 1. Rescue UnitTimeline
        GameObject unitTimeline = GameObject.Find("UnitTimeline");
        if (unitTimeline == null)
        {
            // It might be inside BottomPanel
            GameObject bottomPanel = GameObject.Find("BottomPanel");
            if (bottomPanel != null)
            {
                Transform t = bottomPanel.transform.Find("UnitTimeline");
                if (t != null) unitTimeline = t.gameObject;
            }
        }
        
        if (unitTimeline != null)
        {
            unitTimeline.transform.SetParent(GameObject.Find("Canvas").transform, false);
        }

        // 2. Destroy created panels
        DestroyIfExists("TopBar");
        DestroyIfExists("BottomPanel");
        DestroyIfExists("ToolsHeader");
        DestroyIfExists("ToolsContainer");
        DestroyIfExists("TimelineHeader"); // If created separately
    }

    private static void DestroyIfExists(string name)
    {
        GameObject obj = GameObject.Find(name);
        if (obj != null)
        {
            // If it's a child of something else, find it there too?
            // GameObject.Find searches active objects.
            Object.DestroyImmediate(obj);
        }
        else
        {
            // Search recursively in Canvas just in case
            GameObject canvas = GameObject.Find("Canvas");
            if (canvas != null)
            {
                Transform t = canvas.transform.Find(name);
                if (t != null) Object.DestroyImmediate(t.gameObject);
                
                // Check inside TilePalette for Tools
                Transform tp = canvas.transform.Find("TilePalette");
                if (tp != null)
                {
                    Transform th = tp.Find(name);
                    if (th != null) Object.DestroyImmediate(th.gameObject);
                }
            }
        }
    }

    private static void BuildUI()
    {
        GameObject canvas = GameObject.Find("Canvas");
        if (canvas == null) return;

        // --- 1. Top Bar ---
        GameObject topBar = CreatePanel("TopBar", canvas.transform);
        SetAnchor(topBar.GetComponent<RectTransform>(), AnchorType.TopStretch, 0, 0, 0, 80); // Height 80
        SetColor(topBar, new Color(0.2f, 0.2f, 0.2f));

        // Back Button
        GameObject backBtn = CreateButton("BackButton", topBar.transform, "<");
        SetAnchor(backBtn.GetComponent<RectTransform>(), AnchorType.Left, 10, 0, 60, 60);

        // Title
        GameObject title = CreateText("Title", topBar.transform, "Level Editor:\nCustom 01", 24);
        SetAnchor(title.GetComponent<RectTransform>(), AnchorType.Left, 80, 0, 300, 60);
        title.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Left;

        // Action Buttons Container (Right aligned)
        GameObject actionsContainer = CreatePanel("ActionsContainer", topBar.transform);
        SetAnchor(actionsContainer.GetComponent<RectTransform>(), AnchorType.Right, -10, 0, 250, 60);
        SetColor(actionsContainer, Color.clear);
        HorizontalLayoutGroup hlg = actionsContainer.AddComponent<HorizontalLayoutGroup>();
        hlg.childControlWidth = false;
        hlg.childControlHeight = false;
        hlg.spacing = 10;
        hlg.childAlignment = TextAnchor.MiddleRight;

        CreateActionButton("SaveButton", actionsContainer.transform, "Save", "FloppyDisk");
        CreateActionButton("LoadButton", actionsContainer.transform, "Load", "Folder");
        CreateActionButton("TestPlayButton", actionsContainer.transform, "Test Play", "Play");


        // --- 2. Left Sidebar (Tool Palette) ---
        GameObject tilePalette = GameObject.Find("Canvas/TilePalette");
        if (tilePalette != null)
        {
            SetAnchor(tilePalette.GetComponent<RectTransform>(), AnchorType.LeftStretch, 0, 80, 120, 250); // Below TopBar, Above BottomPanel
            SetColor(tilePalette, new Color(0.25f, 0.25f, 0.25f));

            // Ensure Vertical Layout
            VerticalLayoutGroup vlg = tilePalette.GetComponent<VerticalLayoutGroup>();
            if (vlg == null) vlg = tilePalette.AddComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(10, 10, 10, 10);
            vlg.spacing = 10;
            vlg.childControlHeight = false; // Let children decide height
            vlg.childControlWidth = true;
            vlg.childForceExpandHeight = false;

            // Create Tools Header
            GameObject toolsHeader = CreateText("ToolsHeader", tilePalette.transform, "Tool\nPalette", 18);
            toolsHeader.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
            LayoutElement leHeader = toolsHeader.AddComponent<LayoutElement>();
            leHeader.minHeight = 40;
            leHeader.preferredHeight = 40;
            toolsHeader.transform.SetAsFirstSibling();
            
            // Create Tools Container
            GameObject toolsContainer = CreatePanel("ToolsContainer", tilePalette.transform);
            LayoutElement leContainer = toolsContainer.AddComponent<LayoutElement>();
            leContainer.minHeight = 180; // 3 buttons * 50 + spacing
            leContainer.preferredHeight = 180;
            toolsContainer.transform.SetSiblingIndex(1);
            
            VerticalLayoutGroup toolsVlg = toolsContainer.AddComponent<VerticalLayoutGroup>();
            toolsVlg.childControlHeight = true;
            toolsVlg.childControlWidth = true;
            toolsVlg.spacing = 5;
            toolsVlg.padding = new RectOffset(0, 0, 0, 0);

            CreateToolButton("BrushTool", toolsContainer.transform, "Brush", Color.white);
            CreateToolButton("BucketTool", toolsContainer.transform, "Bucket", new Color(0.9f, 0.9f, 0.9f));
            CreateToolButton("EraserTool", toolsContainer.transform, "Eraser", new Color(1f, 0.8f, 0.8f));

            // Separator
            GameObject separator = CreatePanel("Separator", tilePalette.transform);
            LayoutElement leSep = separator.AddComponent<LayoutElement>();
            leSep.minHeight = 2;
            leSep.preferredHeight = 2;
            SetColor(separator, Color.gray);
            separator.transform.SetSiblingIndex(2);
        }


        // --- 3. Center Area (Level Canvas) ---
        GameObject levelCanvas = GameObject.Find("Canvas/LevelCanvas");
        if (levelCanvas != null)
        {
            SetAnchor(levelCanvas.GetComponent<RectTransform>(), AnchorType.Stretch, 120, 80, 0, 250);
            SetColor(levelCanvas, new Color(0.15f, 0.15f, 0.15f));
        }


        // --- 4. Bottom Area ---
        GameObject bottomPanel = CreatePanel("BottomPanel", canvas.transform);
        SetAnchor(bottomPanel.GetComponent<RectTransform>(), AnchorType.BottomStretch, 0, 0, 0, 250); // Height 250
        SetColor(bottomPanel, new Color(0.2f, 0.2f, 0.2f));

        // Split Bottom Panel into Left (Timeline) and Right (Auto-Balance)
        GameObject timelineArea = CreatePanel("TimelineArea", bottomPanel.transform);
        SetAnchor(timelineArea.GetComponent<RectTransform>(), AnchorType.LeftHalf, 10, 10, 5, 10);
        SetColor(timelineArea, Color.clear);
        
        GameObject autoBalanceArea = CreatePanel("AutoBalanceArea", bottomPanel.transform);
        SetAnchor(autoBalanceArea.GetComponent<RectTransform>(), AnchorType.RightHalf, 5, 10, 10, 10);
        SetColor(autoBalanceArea, new Color(0.25f, 0.25f, 0.25f)); // Darker background for this section

        // --- Timeline Area Content ---
        // Header
        GameObject timelineHeader = CreateText("TimelineHeader", timelineArea.transform, "Unit Stacks Timeline", 20);
        SetAnchor(timelineHeader.GetComponent<RectTransform>(), AnchorType.TopStretch, 0, 0, 0, 30);
        timelineHeader.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Left;

        // Unit Timeline (Existing Object)
        GameObject unitTimeline = GameObject.Find("UnitTimeline");
        if (unitTimeline == null) unitTimeline = GameObject.Find("Canvas/UnitTimeline"); // Just in case

        if (unitTimeline != null)
        {
            unitTimeline.transform.SetParent(timelineArea.transform, false);
            // Reset anchors to fill remaining space
            SetAnchor(unitTimeline.GetComponent<RectTransform>(), AnchorType.Stretch, 0, 35, 0, 0);
            
            // Ensure Horizontal Layout
            HorizontalLayoutGroup timelineHlg = unitTimeline.GetComponent<HorizontalLayoutGroup>();
            if (timelineHlg == null) timelineHlg = unitTimeline.AddComponent<HorizontalLayoutGroup>();
            timelineHlg.childControlWidth = true;
            timelineHlg.childControlHeight = true;
            timelineHlg.childForceExpandWidth = true;
            timelineHlg.childForceExpandHeight = true;
            timelineHlg.spacing = 5;
        }

        // --- Auto-Balance Area Content ---
        // Header
        GameObject abHeader = CreateText("ABHeader", autoBalanceArea.transform, "Auto-Balance Controls", 20);
        SetAnchor(abHeader.GetComponent<RectTransform>(), AnchorType.TopStretch, 0, 5, 0, 30);
        abHeader.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;

        // Generate Button
        GameObject genBtn = CreateButton("GenerateButton", autoBalanceArea.transform, "GENERATE\nSTACKS");
        SetAnchor(genBtn.GetComponent<RectTransform>(), AnchorType.TopCenter, 0, -40, 200, 50);
        genBtn.GetComponent<Image>().color = new Color(0.2f, 0.6f, 1f);
        genBtn.GetComponentInChildren<TextMeshProUGUI>().fontSize = 18;

        // Slider Section
        GameObject sliderContainer = CreatePanel("SliderContainer", autoBalanceArea.transform);
        SetAnchor(sliderContainer.GetComponent<RectTransform>(), AnchorType.TopCenter, 0, -100, 220, 50);
        SetColor(sliderContainer, Color.clear);
        
        GameObject sliderLabel = CreateText("SliderLabel", sliderContainer.transform, "Difficulty / Granularity", 16);
        SetAnchor(sliderLabel.GetComponent<RectTransform>(), AnchorType.TopCenter, 0, 0, 220, 20);
        
        GameObject slider = CreateSlider("DifficultySlider", sliderContainer.transform);
        SetAnchor(slider.GetComponent<RectTransform>(), AnchorType.BottomCenter, 0, 5, 220, 20);

        // Status Panel
        GameObject statusPanel = CreatePanel("StatusPanel", autoBalanceArea.transform);
        SetAnchor(statusPanel.GetComponent<RectTransform>(), AnchorType.BottomStretch, 10, 10, 10, 80); // Height 80
        SetColor(statusPanel, new Color(0.15f, 0.15f, 0.15f));

        GameObject statusText = CreateText("StatusText", statusPanel.transform, "<color=green>Balance OK</color>\nTotal HP: 540\nGrid Blocks: 540", 16);
        statusText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        SetAnchor(statusText.GetComponent<RectTransform>(), AnchorType.Stretch, 5, 5, -5, -5);
    }

    // Helper Methods
    private static GameObject CreatePanel(string name, Transform parent)
    {
        GameObject panel = new GameObject(name);
        panel.transform.SetParent(parent, false);
        panel.AddComponent<RectTransform>();
        panel.AddComponent<CanvasRenderer>();
        panel.AddComponent<Image>();
        return panel;
    }

    private static GameObject CreateButton(string name, Transform parent, string text)
    {
        GameObject btn = new GameObject(name);
        btn.transform.SetParent(parent, false);
        btn.AddComponent<RectTransform>();
        btn.AddComponent<CanvasRenderer>();
        Image img = btn.AddComponent<Image>();
        img.color = Color.white;
        btn.AddComponent<Button>();

        GameObject txt = CreateText("Text", btn.transform, text, 20);
        txt.GetComponent<TextMeshProUGUI>().color = Color.black;
        txt.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        SetAnchor(txt.GetComponent<RectTransform>(), AnchorType.Stretch, 0, 0, 0, 0);

        return btn;
    }

    private static void CreateActionButton(string name, Transform parent, string text, string iconName)
    {
        GameObject btn = CreateButton(name, parent, text);
        LayoutElement le = btn.AddComponent<LayoutElement>();
        le.preferredWidth = 70;
        le.preferredHeight = 60;
        
        // Adjust text size
        TextMeshProUGUI tmp = btn.GetComponentInChildren<TextMeshProUGUI>();
        tmp.fontSize = 14;
        tmp.enableWordWrapping = true;
    }

    private static void CreateToolButton(string name, Transform parent, string text, Color color)
    {
        GameObject btn = CreateButton(name, parent, text);
        btn.GetComponent<Image>().color = color;
        LayoutElement le = btn.AddComponent<LayoutElement>();
        le.preferredHeight = 50; // Fixed height for tool buttons
        le.flexibleHeight = 0;
    }

    private static GameObject CreateText(string name, Transform parent, string content, float fontSize)
    {
        GameObject txtObj = new GameObject(name);
        txtObj.transform.SetParent(parent, false);
        txtObj.AddComponent<RectTransform>();
        txtObj.AddComponent<CanvasRenderer>();
        TextMeshProUGUI tmp = txtObj.AddComponent<TextMeshProUGUI>();
        tmp.text = content;
        tmp.fontSize = fontSize;
        tmp.color = Color.white;
        tmp.alignment = TextAlignmentOptions.Center;
        return txtObj;
    }

    private static GameObject CreateSlider(string name, Transform parent)
    {
        GameObject sliderObj = new GameObject(name);
        sliderObj.transform.SetParent(parent, false);
        RectTransform sliderRect = sliderObj.AddComponent<RectTransform>();
        Slider slider = sliderObj.AddComponent<Slider>();

        // Background
        GameObject bg = CreatePanel("Background", sliderObj.transform);
        SetAnchor(bg.GetComponent<RectTransform>(), AnchorType.Stretch, 0, 5, 0, -5); // Thin bar
        SetColor(bg, Color.gray);

        // Fill Area
        GameObject fillArea = new GameObject("Fill Area");
        fillArea.transform.SetParent(sliderObj.transform, false);
        RectTransform fillAreaRect = fillArea.AddComponent<RectTransform>();
        SetAnchor(fillAreaRect, AnchorType.Stretch, 5, 0, -5, 0);

        // Fill
        GameObject fill = CreatePanel("Fill", fillArea.transform);
        SetAnchor(fill.GetComponent<RectTransform>(), AnchorType.Stretch, 0, 0, 0, 0);
        SetColor(fill, Color.blue);
        slider.fillRect = fill.GetComponent<RectTransform>();

        // Handle Area
        GameObject handleArea = new GameObject("Handle Slide Area");
        handleArea.transform.SetParent(sliderObj.transform, false);
        RectTransform handleAreaRect = handleArea.AddComponent<RectTransform>();
        SetAnchor(handleAreaRect, AnchorType.Stretch, 5, 0, -5, 0);

        // Handle
        GameObject handle = CreatePanel("Handle", handleArea.transform);
        SetAnchor(handle.GetComponent<RectTransform>(), AnchorType.Center, 0, 0, 20, 20);
        SetColor(handle, Color.white);
        slider.handleRect = handle.GetComponent<RectTransform>();

        return sliderObj;
    }

    private static void SetAnchor(RectTransform rect, AnchorType type, float left, float top, float right, float bottom)
    {
        switch (type)
        {
            case AnchorType.TopStretch:
                rect.anchorMin = new Vector2(0, 1);
                rect.anchorMax = new Vector2(1, 1);
                rect.pivot = new Vector2(0.5f, 1);
                rect.offsetMin = new Vector2(left, -bottom); // bottom is height here
                rect.offsetMax = new Vector2(-right, -top);
                rect.sizeDelta = new Vector2(rect.sizeDelta.x, bottom); // Set height
                break;
            case AnchorType.BottomStretch:
                rect.anchorMin = new Vector2(0, 0);
                rect.anchorMax = new Vector2(1, 0);
                rect.pivot = new Vector2(0.5f, 0);
                rect.offsetMin = new Vector2(left, bottom);
                rect.offsetMax = new Vector2(-right, top); // top is height here
                rect.sizeDelta = new Vector2(rect.sizeDelta.x, top); // Set height
                break;
            case AnchorType.LeftStretch:
                rect.anchorMin = new Vector2(0, 0);
                rect.anchorMax = new Vector2(0, 1);
                rect.pivot = new Vector2(0, 0.5f);
                rect.offsetMin = new Vector2(left, bottom);
                rect.offsetMax = new Vector2(right, -top); // right is width here
                rect.sizeDelta = new Vector2(right, rect.sizeDelta.y); // Set width
                break;
            case AnchorType.Stretch:
                rect.anchorMin = new Vector2(0, 0);
                rect.anchorMax = new Vector2(1, 1);
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.offsetMin = new Vector2(left, bottom);
                rect.offsetMax = new Vector2(-right, -top);
                break;
            case AnchorType.Left:
                rect.anchorMin = new Vector2(0, 0.5f);
                rect.anchorMax = new Vector2(0, 0.5f);
                rect.pivot = new Vector2(0, 0.5f);
                rect.anchoredPosition = new Vector2(left, top);
                rect.sizeDelta = new Vector2(right, bottom); // width, height
                break;
            case AnchorType.Right:
                rect.anchorMin = new Vector2(1, 0.5f);
                rect.anchorMax = new Vector2(1, 0.5f);
                rect.pivot = new Vector2(1, 0.5f);
                rect.anchoredPosition = new Vector2(left, top);
                rect.sizeDelta = new Vector2(right, bottom); // width, height
                break;
            case AnchorType.Center:
                rect.anchorMin = new Vector2(0.5f, 0.5f);
                rect.anchorMax = new Vector2(0.5f, 0.5f);
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.anchoredPosition = new Vector2(left, top);
                rect.sizeDelta = new Vector2(right, bottom); // width, height
                break;
            case AnchorType.TopCenter:
                rect.anchorMin = new Vector2(0.5f, 1);
                rect.anchorMax = new Vector2(0.5f, 1);
                rect.pivot = new Vector2(0.5f, 1);
                rect.anchoredPosition = new Vector2(left, top);
                rect.sizeDelta = new Vector2(right, bottom); // width, height
                break;
            case AnchorType.BottomCenter:
                rect.anchorMin = new Vector2(0.5f, 0);
                rect.anchorMax = new Vector2(0.5f, 0);
                rect.pivot = new Vector2(0.5f, 0);
                rect.anchoredPosition = new Vector2(left, top);
                rect.sizeDelta = new Vector2(right, bottom); // width, height
                break;
            case AnchorType.CenterLeft:
                rect.anchorMin = new Vector2(0, 0.5f);
                rect.anchorMax = new Vector2(0, 0.5f);
                rect.pivot = new Vector2(0, 0.5f);
                rect.anchoredPosition = new Vector2(left, top);
                rect.sizeDelta = new Vector2(right, bottom); // width, height
                break;
            case AnchorType.LeftHalf:
                rect.anchorMin = new Vector2(0, 0);
                rect.anchorMax = new Vector2(0.5f, 1);
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.offsetMin = new Vector2(left, bottom);
                rect.offsetMax = new Vector2(-right, -top);
                break;
            case AnchorType.RightHalf:
                rect.anchorMin = new Vector2(0.5f, 0);
                rect.anchorMax = new Vector2(1, 1);
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.offsetMin = new Vector2(left, bottom);
                rect.offsetMax = new Vector2(-right, -top);
                break;
        }
    }

    private static void SetColor(GameObject obj, Color color)
    {
        Image img = obj.GetComponent<Image>();
        if (img != null) img.color = color;
    }

    private enum AnchorType
    {
        TopStretch,
        BottomStretch,
        LeftStretch,
        Stretch,
        Left,
        Right,
        Center,
        TopCenter,
        BottomCenter,
        CenterLeft,
        LeftHalf,
        RightHalf
    }
}
