using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class UpdateLevelEditorUI_v10
{
    public static string Execute()
    {
        Cleanup();
        BuildUI();
        return "Level Editor UI Updated Successfully (v10)";
    }

    private static void Cleanup()
    {
        // 1. Rescue UnitTimeline
        GameObject unitTimeline = GameObject.Find("UnitTimeline");
        if (unitTimeline == null)
        {
            // It might be inside BottomPanel or TimelineArea
            GameObject bottomPanel = GameObject.Find("BottomPanel");
            if (bottomPanel != null)
            {
                Transform t = bottomPanel.transform.Find("TimelineArea/UnitTimeline");
                if (t == null) t = bottomPanel.transform.Find("UnitTimeline");
                if (t != null) unitTimeline = t.gameObject;
            }
        }
        
        if (unitTimeline != null)
        {
            unitTimeline.transform.SetParent(GameObject.Find("Canvas").transform, false);
            // Reset layout group if needed
            HorizontalLayoutGroup hlg = unitTimeline.GetComponent<HorizontalLayoutGroup>();
            if (hlg != null) Object.DestroyImmediate(hlg);
        }

        // 2. Destroy created panels
        DestroyIfExists("TopBar");
        DestroyIfExists("BottomPanel");
        
        // 3. Clean up TilePalette additions
        GameObject tilePalette = GameObject.Find("Canvas/TilePalette");
        if (tilePalette != null)
        {
            DestroyChildIfExists(tilePalette.transform, "ToolsHeader");
            DestroyChildIfExists(tilePalette.transform, "ToolsContainer");
            DestroyChildIfExists(tilePalette.transform, "Separator");
        }
    }

    private static void DestroyIfExists(string name)
    {
        GameObject obj = GameObject.Find(name);
        if (obj != null) Object.DestroyImmediate(obj);
        else
        {
            GameObject canvas = GameObject.Find("Canvas");
            if (canvas != null)
            {
                Transform t = canvas.transform.Find(name);
                if (t != null) Object.DestroyImmediate(t.gameObject);
            }
        }
    }

    private static void DestroyChildIfExists(Transform parent, string name)
    {
        Transform t = parent.Find(name);
        if (t != null) Object.DestroyImmediate(t.gameObject);
    }

    private static void BuildUI()
    {
        GameObject canvas = GameObject.Find("Canvas");
        if (canvas == null) return;

        // --- 1. Top Bar ---
        GameObject topBar = CreatePanel("TopBar", canvas.transform);
        SetAnchor(topBar.GetComponent<RectTransform>(), AnchorType.TopStretch, 0, 0, 0, 60); // Height 60
        SetColor(topBar, new Color(0.2f, 0.2f, 0.2f));

        // Back Button
        GameObject backBtn = CreateButton("BackButton", topBar.transform, "<");
        SetAnchor(backBtn.GetComponent<RectTransform>(), AnchorType.Left, 10, 0, 40, 40);

        // Title
        GameObject title = CreateText("Title", topBar.transform, "Level Editor: Custom 01", 20);
        // Left 60, Right 250 (to avoid buttons)
        SetAnchor(title.GetComponent<RectTransform>(), AnchorType.Stretch, 60, 0, 250, 0);
        title.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Left;

        // Action Buttons Container (Right aligned)
        GameObject actionsContainer = CreatePanel("ActionsContainer", topBar.transform);
        SetAnchor(actionsContainer.GetComponent<RectTransform>(), AnchorType.Right, -10, 0, 250, 60);
        SetColor(actionsContainer, Color.clear);
        HorizontalLayoutGroup hlg = actionsContainer.AddComponent<HorizontalLayoutGroup>();
        hlg.childControlWidth = false;
        hlg.childControlHeight = false;
        hlg.childForceExpandWidth = false;
        hlg.childForceExpandHeight = false;
        hlg.spacing = 10;
        hlg.childAlignment = TextAnchor.MiddleRight;
        hlg.padding = new RectOffset(0, 10, 0, 0);

        CreateActionButton("SaveButton", actionsContainer.transform, "Save", new Color(0.3f, 0.6f, 0.9f));
        CreateActionButton("LoadButton", actionsContainer.transform, "Load", new Color(0.3f, 0.6f, 0.9f));
        CreateActionButton("TestPlayButton", actionsContainer.transform, "Play", new Color(0.2f, 0.8f, 0.4f));


        // --- 2. Left Sidebar (Tool Palette + Tile Palette) ---
        GameObject tilePalette = GameObject.Find("Canvas/TilePalette");
        if (tilePalette != null)
        {
            // Full height on the left
            SetAnchor(tilePalette.GetComponent<RectTransform>(), AnchorType.LeftStretch, 0, 60, 120, 0); // Top 60, Bottom 0, Width 120
            SetColor(tilePalette, new Color(0.25f, 0.25f, 0.25f));

            // Ensure Vertical Layout
            VerticalLayoutGroup vlg = tilePalette.GetComponent<VerticalLayoutGroup>();
            if (vlg == null) vlg = tilePalette.AddComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(5, 5, 10, 10);
            vlg.spacing = 5;
            vlg.childControlHeight = true; // Enable this to respect LayoutElements
            vlg.childControlWidth = true;
            vlg.childForceExpandHeight = false;
            vlg.childAlignment = TextAnchor.UpperCenter;

            // Create Tools Header
            GameObject toolsHeader = CreateText("ToolsHeader", tilePalette.transform, "Tools", 16);
            toolsHeader.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
            LayoutElement leHeader = toolsHeader.AddComponent<LayoutElement>();
            leHeader.minHeight = 30;
            leHeader.preferredHeight = 30;
            toolsHeader.transform.SetAsFirstSibling();
            
            // Create Tools Container
            GameObject toolsContainer = CreatePanel("ToolsContainer", tilePalette.transform);
            LayoutElement leContainer = toolsContainer.AddComponent<LayoutElement>();
            leContainer.minHeight = 160; // 3 buttons * 50 + spacing
            leContainer.preferredHeight = 160;
            toolsContainer.transform.SetSiblingIndex(1);
            SetColor(toolsContainer, Color.clear);
            
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

            // Ensure existing Label ("Tile Palette") is correctly placed
            Transform label = tilePalette.transform.Find("Label");
            if (label != null)
            {
                label.SetSiblingIndex(3);
                LayoutElement leLabel = label.GetComponent<LayoutElement>();
                if (leLabel == null) leLabel = label.gameObject.AddComponent<LayoutElement>();
                leLabel.minHeight = 30;
                leLabel.preferredHeight = 30;
                
                TextMeshProUGUI tmp = label.GetComponent<TextMeshProUGUI>();
                if (tmp != null) tmp.fontSize = 16;
            }
        }


        // --- 3. Bottom Area ---
        GameObject bottomPanel = CreatePanel("BottomPanel", canvas.transform);
        // Bottom Stretch, but start after sidebar (Left 120)
        SetAnchor(bottomPanel.GetComponent<RectTransform>(), AnchorType.BottomStretch, 120, 0, 0, 280); // Height 280
        SetColor(bottomPanel, new Color(0.2f, 0.2f, 0.2f));

        // Split Bottom Panel into Left (Timeline) and Right (Auto-Balance)
        GameObject timelineArea = CreatePanel("TimelineArea", bottomPanel.transform);
        SetAnchor(timelineArea.GetComponent<RectTransform>(), AnchorType.LeftHalf, 0, 0, 0, 0); // Fill left half
        SetColor(timelineArea, Color.clear);
        
        // Use Vertical Layout Group for TimelineArea
        VerticalLayoutGroup timelineVlg = timelineArea.AddComponent<VerticalLayoutGroup>();
        timelineVlg.padding = new RectOffset(10, 10, 10, 10);
        timelineVlg.spacing = 10; // Increased spacing
        timelineVlg.childControlHeight = true; // Enable this to respect LayoutElements
        timelineVlg.childControlWidth = true;
        timelineVlg.childForceExpandHeight = false;
        timelineVlg.childAlignment = TextAnchor.UpperCenter;

        GameObject autoBalanceArea = CreatePanel("AutoBalanceArea", bottomPanel.transform);
        SetAnchor(autoBalanceArea.GetComponent<RectTransform>(), AnchorType.RightHalf, 0, 0, 0, 0); // Fill right half
        SetColor(autoBalanceArea, new Color(0.25f, 0.25f, 0.25f)); // Darker background

        // --- Timeline Area Content ---
        // Header
        GameObject timelineHeader = CreateText("TimelineHeader", timelineArea.transform, "Unit Stacks Timeline", 18);
        timelineHeader.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Left;
        LayoutElement leTimelineHeader = timelineHeader.AddComponent<LayoutElement>();
        leTimelineHeader.minHeight = 30;
        leTimelineHeader.preferredHeight = 30;

        // Unit Timeline (Existing Object)
        GameObject unitTimeline = GameObject.Find("UnitTimeline");
        if (unitTimeline == null) unitTimeline = GameObject.Find("Canvas/UnitTimeline"); // Just in case

        if (unitTimeline != null)
        {
            unitTimeline.transform.SetParent(timelineArea.transform, false);
            
            // Reset RectTransform to ensure it plays nice with Layout Group
            RectTransform rt = unitTimeline.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 0);
            rt.anchorMax = new Vector2(1, 1);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            // Ensure Horizontal Layout
            HorizontalLayoutGroup timelineHlg = unitTimeline.GetComponent<HorizontalLayoutGroup>();
            if (timelineHlg == null) timelineHlg = unitTimeline.AddComponent<HorizontalLayoutGroup>();
            timelineHlg.childControlWidth = true;
            timelineHlg.childControlHeight = true;
            timelineHlg.childForceExpandWidth = true;
            timelineHlg.childForceExpandHeight = true;
            timelineHlg.spacing = 5;
            timelineHlg.childAlignment = TextAnchor.MiddleCenter; // Center children vertically

            // Layout Element for UnitTimeline to fill remaining space
            LayoutElement leTimeline = unitTimeline.GetComponent<LayoutElement>();
            if (leTimeline == null) leTimeline = unitTimeline.AddComponent<LayoutElement>();
            leTimeline.flexibleHeight = 1;
            leTimeline.minHeight = 100;

            // Disable the old label inside UnitTimeline if it exists
            Transform oldLabel = unitTimeline.transform.Find("Label");
            if (oldLabel != null) oldLabel.gameObject.SetActive(false);
        }

        // --- Auto-Balance Area Content ---
        // Use Vertical Layout Group for AutoBalanceArea
        VerticalLayoutGroup abVlg = autoBalanceArea.AddComponent<VerticalLayoutGroup>();
        abVlg.padding = new RectOffset(10, 10, 10, 10);
        abVlg.spacing = 10;
        abVlg.childControlHeight = true; // Enable this to respect LayoutElements
        abVlg.childControlWidth = true;
        abVlg.childForceExpandHeight = false;
        abVlg.childAlignment = TextAnchor.UpperCenter;

        // Header
        GameObject abHeader = CreateText("ABHeader", autoBalanceArea.transform, "Auto-Balance Controls", 18);
        abHeader.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        LayoutElement leAbHeader = abHeader.AddComponent<LayoutElement>();
        leAbHeader.minHeight = 30;
        leAbHeader.preferredHeight = 30;

        // Generate Button
        GameObject genBtn = CreateButton("GenerateButton", autoBalanceArea.transform, "GENERATE\nSTACKS");
        genBtn.GetComponent<Image>().color = new Color(0.2f, 0.6f, 1f);
        genBtn.GetComponentInChildren<TextMeshProUGUI>().fontSize = 14;
        LayoutElement leBtn = genBtn.AddComponent<LayoutElement>();
        leBtn.minHeight = 40;
        leBtn.preferredHeight = 40;

        // Slider Section
        GameObject sliderContainer = CreatePanel("SliderContainer", autoBalanceArea.transform);
        SetColor(sliderContainer, Color.clear);
        LayoutElement leSlider = sliderContainer.AddComponent<LayoutElement>();
        leSlider.minHeight = 50;
        leSlider.preferredHeight = 50;
        
        GameObject sliderLabel = CreateText("SliderLabel", sliderContainer.transform, "Difficulty / Granularity", 14);
        SetAnchor(sliderLabel.GetComponent<RectTransform>(), AnchorType.TopCenter, 0, 0, 200, 20);
        
        GameObject slider = CreateSlider("DifficultySlider", sliderContainer.transform);
        SetAnchor(slider.GetComponent<RectTransform>(), AnchorType.BottomCenter, 0, 5, 200, 20);

        // Status Panel
        GameObject statusPanel = CreatePanel("StatusPanel", autoBalanceArea.transform);
        SetColor(statusPanel, new Color(0.15f, 0.15f, 0.15f));
        LayoutElement leStatus = statusPanel.AddComponent<LayoutElement>();
        leStatus.minHeight = 60;
        leStatus.preferredHeight = 60;
        leStatus.flexibleHeight = 1; // Fill remaining space if any

        GameObject statusText = CreateText("StatusText", statusPanel.transform, "<color=green>Balance OK</color>\nTotal HP: 540\nGrid Blocks: 540", 14);
        statusText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        SetAnchor(statusText.GetComponent<RectTransform>(), AnchorType.Stretch, 5, 5, -5, -5);


        // --- 4. Center Area (Level Canvas) ---
        GameObject levelCanvas = GameObject.Find("Canvas/LevelCanvas");
        if (levelCanvas != null)
        {
            // Stretch to fill remaining space: Left 120, Top 60, Bottom 280, Right 0
            SetAnchor(levelCanvas.GetComponent<RectTransform>(), AnchorType.Stretch, 120, 60, 0, 280);
            SetColor(levelCanvas, new Color(0.15f, 0.15f, 0.15f));
        }
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

    private static void CreateActionButton(string name, Transform parent, string text, Color color)
    {
        GameObject btn = CreateButton(name, parent, text);
        btn.GetComponent<Image>().color = color;
        
        // Set explicit size
        RectTransform rt = btn.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(60, 40);
        
        LayoutElement le = btn.AddComponent<LayoutElement>();
        le.preferredWidth = 60;
        le.preferredHeight = 40;
        le.minWidth = 60;
        le.minHeight = 40;
        
        // Adjust text size
        TextMeshProUGUI tmp = btn.GetComponentInChildren<TextMeshProUGUI>();
        tmp.fontSize = 12;
        tmp.enableWordWrapping = false;
    }

    private static void CreateToolButton(string name, Transform parent, string text, Color color)
    {
        GameObject btn = CreateButton(name, parent, text);
        btn.GetComponent<Image>().color = color;
        LayoutElement le = btn.AddComponent<LayoutElement>();
        le.preferredHeight = 40; // Fixed height for tool buttons
        le.flexibleHeight = 0;
        
        TextMeshProUGUI tmp = btn.GetComponentInChildren<TextMeshProUGUI>();
        tmp.fontSize = 12;
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
        SetAnchor(handle.GetComponent<RectTransform>(), AnchorType.Center, 0, 0, 15, 15);
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
                rect.pivot = new Vector2(0.5f, 0.5f);
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
