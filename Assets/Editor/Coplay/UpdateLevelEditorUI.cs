using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

public class UpdateLevelEditorUI
{
    public static string Execute()
    {
        GameObject canvas = GameObject.Find("Canvas");
        if (canvas == null) return "Canvas not found!";

        // --- 1. Top Bar ---
        GameObject topBar = CreatePanel("TopBar", canvas.transform);
        SetAnchor(topBar.GetComponent<RectTransform>(), AnchorType.TopStretch, 0, 0, 0, 100);
        SetColor(topBar, new Color(0.2f, 0.2f, 0.2f));

        // Back Button
        GameObject backBtn = CreateButton("BackButton", topBar.transform, "<");
        SetAnchor(backBtn.GetComponent<RectTransform>(), AnchorType.Left, 10, 0, 80, 80);

        // Title
        GameObject title = CreateText("Title", topBar.transform, "Level Editor: Custom 01", 36);
        SetAnchor(title.GetComponent<RectTransform>(), AnchorType.Left, 100, 0, 400, 50);

        // Action Buttons
        GameObject saveBtn = CreateButton("SaveButton", topBar.transform, "Save");
        SetAnchor(saveBtn.GetComponent<RectTransform>(), AnchorType.Right, -200, 0, 80, 80);
        
        GameObject loadBtn = CreateButton("LoadButton", topBar.transform, "Load");
        SetAnchor(loadBtn.GetComponent<RectTransform>(), AnchorType.Right, -110, 0, 80, 80);

        GameObject playBtn = CreateButton("TestPlayButton", topBar.transform, "Play");
        SetAnchor(playBtn.GetComponent<RectTransform>(), AnchorType.Right, -20, 0, 80, 80);


        // --- 2. Left Sidebar (Tool Palette) ---
        GameObject tilePalette = GameObject.Find("Canvas/TilePalette");
        if (tilePalette != null)
        {
            SetAnchor(tilePalette.GetComponent<RectTransform>(), AnchorType.LeftStretch, 0, 100, 150, 300);
            SetColor(tilePalette, new Color(0.25f, 0.25f, 0.25f));

            // Create Tools Header
            GameObject toolsHeader = CreateText("ToolsHeader", tilePalette.transform, "Tool Palette", 24);
            toolsHeader.transform.SetAsFirstSibling();
            
            // Create Tools Container
            GameObject toolsContainer = CreatePanel("ToolsContainer", tilePalette.transform);
            LayoutElement le = toolsContainer.AddComponent<LayoutElement>();
            le.minHeight = 250;
            toolsContainer.transform.SetSiblingIndex(1);
            VerticalLayoutGroup vlg = toolsContainer.AddComponent<VerticalLayoutGroup>();
            vlg.childControlHeight = false;
            vlg.childControlWidth = false;
            vlg.spacing = 10;
            vlg.padding = new RectOffset(10, 10, 10, 10);

            CreateButton("BrushTool", toolsContainer.transform, "Brush");
            CreateButton("BucketTool", toolsContainer.transform, "Bucket");
            CreateButton("EraserTool", toolsContainer.transform, "Eraser");
        }


        // --- 3. Center Area (Level Canvas) ---
        GameObject levelCanvas = GameObject.Find("Canvas/LevelCanvas");
        if (levelCanvas != null)
        {
            SetAnchor(levelCanvas.GetComponent<RectTransform>(), AnchorType.Stretch, 150, 100, 0, 300);
            SetColor(levelCanvas, new Color(0.15f, 0.15f, 0.15f));
        }


        // --- 4. Bottom Area ---
        GameObject bottomPanel = CreatePanel("BottomPanel", canvas.transform);
        SetAnchor(bottomPanel.GetComponent<RectTransform>(), AnchorType.BottomStretch, 0, 0, 0, 300);
        SetColor(bottomPanel, new Color(0.2f, 0.2f, 0.2f));

        // Unit Timeline
        GameObject unitTimeline = GameObject.Find("Canvas/UnitTimeline");
        if (unitTimeline != null)
        {
            unitTimeline.transform.SetParent(bottomPanel.transform);
            SetAnchor(unitTimeline.GetComponent<RectTransform>(), AnchorType.LeftHalf, 10, 10, -10, -10);
            
            // Add Header
            GameObject timelineHeader = CreateText("TimelineHeader", unitTimeline.transform, "Unit Stacks Timeline", 24);
            timelineHeader.transform.SetAsFirstSibling();
        }

        // Auto-Balance Controls
        GameObject autoBalancePanel = CreatePanel("AutoBalancePanel", bottomPanel.transform);
        SetAnchor(autoBalancePanel.GetComponent<RectTransform>(), AnchorType.RightHalf, 10, 10, -10, -10);
        SetColor(autoBalancePanel, new Color(0.25f, 0.25f, 0.25f));

        GameObject abHeader = CreateText("ABHeader", autoBalancePanel.transform, "Auto-Balance Controls", 24);
        SetAnchor(abHeader.GetComponent<RectTransform>(), AnchorType.TopCenter, 0, -10, 300, 30);

        GameObject genBtn = CreateButton("GenerateButton", autoBalancePanel.transform, "GENERATE STACKS");
        SetAnchor(genBtn.GetComponent<RectTransform>(), AnchorType.TopCenter, 0, -50, 250, 60);
        genBtn.GetComponent<Image>().color = new Color(0.2f, 0.6f, 1f);

        // Slider
        GameObject slider = CreateSlider("DifficultySlider", autoBalancePanel.transform);
        SetAnchor(slider.GetComponent<RectTransform>(), AnchorType.Center, 0, 20, 250, 20);
        CreateText("SliderLabel", slider.transform, "Difficulty / Granularity", 18).GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 20);

        // Status Panel
        GameObject statusPanel = CreatePanel("StatusPanel", autoBalancePanel.transform);
        SetAnchor(statusPanel.GetComponent<RectTransform>(), AnchorType.BottomCenter, 0, 10, 250, 100);
        SetColor(statusPanel, new Color(0.15f, 0.15f, 0.15f));

        GameObject statusText = CreateText("StatusText", statusPanel.transform, "Balance OK\nTotal HP: 540\nGrid Blocks: 540", 18);
        statusText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        SetAnchor(statusText.GetComponent<RectTransform>(), AnchorType.Stretch, 10, 10, -10, -10);

        return "Level Editor UI Updated Successfully";
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
