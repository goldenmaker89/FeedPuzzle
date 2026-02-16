using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace LevelEditor
{
    public enum EditorTool
    {
        None,
        Brush,
        Bucket,
        Eraser
    }

    /// <summary>
    /// Manages tool selection (Brush, Bucket, Eraser), active color,
    /// and coordinates painting on the grid.
    /// 
    /// Drag painting works by raycasting via EventSystem.RaycastAll every frame
    /// while the pointer is held down. This avoids reliance on OnPointerEnter
    /// which does not fire during drag with InputSystemUIInputModule.
    /// </summary>
    public class LevelEditorToolManager : MonoBehaviour
    {
        [Header("Tool Buttons")]
        [SerializeField] private Button brushButton;
        [SerializeField] private Button bucketButton;
        [SerializeField] private Button eraserButton;

        [Header("Tool Button Highlight")]
        [SerializeField] private Color activeToolColor = new Color(0.3f, 0.8f, 1f, 1f);
        [SerializeField] private Color inactiveToolColor = new Color(1f, 1f, 1f, 1f);

        [Header("Palette")]
        [SerializeField] private LevelEditorPaletteItem[] paletteItems;

        [Header("Grid")]
        [SerializeField] private LevelEditorGridManager gridManager;

        [Header("Default Settings")]
        [SerializeField] private int defaultColorId = 1;

        private EditorTool currentTool = EditorTool.None;
        private int selectedColorId = 1;
        private bool isPainting; // true while mouse/touch is held down on grid
        private List<RaycastResult> raycastResults = new List<RaycastResult>();
        private PointerEventData pointerEventData;

        public EditorTool CurrentTool => currentTool;
        public int SelectedColorId => selectedColorId;
        public bool IsPainting => isPainting;

        public event Action<EditorTool> OnToolChanged;
        public event Action<int> OnColorChanged;

        private void Awake()
        {
            selectedColorId = defaultColorId;
        }

        private void Start()
        {
            // Wire tool buttons
            if (brushButton != null)
                brushButton.onClick.AddListener(OnBrushClicked);
            if (bucketButton != null)
                bucketButton.onClick.AddListener(OnBucketClicked);
            if (eraserButton != null)
                eraserButton.onClick.AddListener(OnEraserClicked);

            // Wire palette items
            if (paletteItems != null)
            {
                for (int i = 0; i < paletteItems.Length; i++)
                {
                    if (paletteItems[i] != null)
                    {
                        paletteItems[i].Initialize(this);
                    }
                }
            }

            // Select brush tool by default
            SelectTool(EditorTool.Brush);
        }

        private void OnBrushClicked()
        {
            SelectTool(EditorTool.Brush);
        }

        private void OnBucketClicked()
        {
            SelectTool(EditorTool.Bucket);
        }

        private void OnEraserClicked()
        {
            SelectTool(EditorTool.Eraser);
        }

        public void SelectTool(EditorTool tool)
        {
            currentTool = tool;
            UpdateToolButtonVisuals();
            OnToolChanged?.Invoke(currentTool);

            if (tool == EditorTool.Brush || tool == EditorTool.Bucket)
            {
                UpdatePaletteSelection();
            }
        }

        public void SelectColor(int colorId)
        {
            selectedColorId = colorId;
            UpdatePaletteSelection();
            OnColorChanged?.Invoke(selectedColorId);

            if (currentTool == EditorTool.None || currentTool == EditorTool.Eraser)
            {
                SelectTool(EditorTool.Brush);
            }
        }

        /// <summary>
        /// Called by LevelEditorCellView when pointer goes down on a cell.
        /// </summary>
        public void OnCellPointerDown(LevelEditorCellView cellView)
        {
            if (currentTool == EditorTool.None) return;

            isPainting = true;
            ApplyToolToCell(cellView);
        }

        /// <summary>
        /// Called by LevelEditorCellView when pointer enters a cell (for drag painting).
        /// Kept as a fallback but the main drag painting is done in Update via raycast.
        /// </summary>
        public void OnCellPointerEnter(LevelEditorCellView cellView)
        {
            if (!isPainting) return;
            if (currentTool == EditorTool.None) return;

            ApplyToolToCell(cellView);
        }

        /// <summary>
        /// Called when pointer is released anywhere.
        /// </summary>
        public void OnPointerUp()
        {
            isPainting = false;
        }

        /// <summary>
        /// Returns true if the primary pointer (mouse left button or first touch) is currently pressed.
        /// Uses the new Input System.
        /// </summary>
        private bool IsPointerPressed()
        {
            var mouse = Mouse.current;
            if (mouse != null && mouse.leftButton.isPressed)
                return true;

            var touchscreen = Touchscreen.current;
            if (touchscreen != null && touchscreen.primaryTouch.press.isPressed)
                return true;

            return false;
        }

        /// <summary>
        /// Returns the current pointer position in screen coordinates.
        /// Uses the new Input System.
        /// </summary>
        private Vector2 GetPointerPosition()
        {
            var mouse = Mouse.current;
            if (mouse != null)
                return mouse.position.ReadValue();

            var touchscreen = Touchscreen.current;
            if (touchscreen != null && touchscreen.primaryTouch.press.isPressed)
                return touchscreen.primaryTouch.position.ReadValue();

            return Vector2.zero;
        }

        private void Update()
        {
            bool pointerPressed = IsPointerPressed();

            // Stop painting when pointer is released
            if (isPainting && !pointerPressed)
            {
                isPainting = false;
            }

            // During painting, raycast every frame to find cell under pointer
            if (isPainting && currentTool != EditorTool.None)
            {
                PaintUnderPointerViaRaycast();
            }

            // Start painting if pointer is pressed over a grid cell (backup for missed OnPointerDown)
            if (!isPainting && pointerPressed && currentTool != EditorTool.None)
            {
                var cellView = RaycastForCell(GetPointerPosition());
                if (cellView != null)
                {
                    isPainting = true;
                    ApplyToolToCell(cellView);
                }
            }
        }

        /// <summary>
        /// Uses EventSystem.RaycastAll to find the cell under the current pointer position
        /// and applies the current tool to it.
        /// </summary>
        private void PaintUnderPointerViaRaycast()
        {
            var cellView = RaycastForCell(GetPointerPosition());
            if (cellView != null)
            {
                ApplyToolToCell(cellView);
            }
        }

        /// <summary>
        /// Performs an EventSystem raycast at the given screen position and returns
        /// the LevelEditorCellView under it, if any.
        /// </summary>
        private LevelEditorCellView RaycastForCell(Vector2 screenPosition)
        {
            if (EventSystem.current == null) return null;

            if (pointerEventData == null)
                pointerEventData = new PointerEventData(EventSystem.current);

            pointerEventData.position = screenPosition;
            raycastResults.Clear();
            EventSystem.current.RaycastAll(pointerEventData, raycastResults);

            for (int i = 0; i < raycastResults.Count; i++)
            {
                var cellView = raycastResults[i].gameObject.GetComponent<LevelEditorCellView>();
                if (cellView != null)
                {
                    return cellView;
                }
            }

            return null;
        }

        private void ApplyToolToCell(LevelEditorCellView cellView)
        {
            if (gridManager == null) return;

            switch (currentTool)
            {
                case EditorTool.Brush:
                    var cell = gridManager.GetCell(cellView.X, cellView.Y);
                    if (cell != null && !cell.IsOccupied)
                    {
                        gridManager.SetCellColor(cellView.X, cellView.Y, selectedColorId);
                    }
                    break;

                case EditorTool.Eraser:
                    gridManager.SetCellColor(cellView.X, cellView.Y, 0);
                    break;

                case EditorTool.Bucket:
                    break;
            }
        }

        private void UpdateToolButtonVisuals()
        {
            SetToolButtonColor(brushButton, currentTool == EditorTool.Brush);
            SetToolButtonColor(bucketButton, currentTool == EditorTool.Bucket);
            SetToolButtonColor(eraserButton, currentTool == EditorTool.Eraser);
        }

        private void SetToolButtonColor(Button button, bool isActive)
        {
            if (button == null) return;
            var image = button.GetComponent<Image>();
            if (image != null)
            {
                image.color = isActive ? activeToolColor : inactiveToolColor;
            }
        }

        private void UpdatePaletteSelection()
        {
            if (paletteItems == null) return;
            for (int i = 0; i < paletteItems.Length; i++)
            {
                if (paletteItems[i] != null)
                {
                    paletteItems[i].SetSelected(paletteItems[i].ColorId == selectedColorId);
                }
            }
        }
    }
}
