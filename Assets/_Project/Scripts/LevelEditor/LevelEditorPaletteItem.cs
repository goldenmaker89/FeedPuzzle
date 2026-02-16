using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Gameplay.Grid;

namespace LevelEditor
{
    /// <summary>
    /// Represents a single color swatch in the TilePalette.
    /// Handles click to select color and shows a selection frame (Outline) when active.
    /// </summary>
    [RequireComponent(typeof(Image))]
    public class LevelEditorPaletteItem : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private int colorId;
        [SerializeField] private Image colorImage;

        [Header("Selection Frame")]
        [SerializeField] private Color selectionFrameColor = Color.white;
        [SerializeField] private float selectionFrameWidth = 3f;

        private LevelEditorToolManager toolManager;
        private Outline outline;
        private bool isSelected;

        public int ColorId => colorId;

        private void Awake()
        {
            if (colorImage == null)
                colorImage = GetComponent<Image>();

            // Set the color from the shared color table
            if (colorId > 0)
            {
                colorImage.color = GridCellView.GetChipColor(colorId);
            }

            // Create or get the Outline component for selection frame
            outline = GetComponent<Outline>();
            if (outline == null)
            {
                outline = gameObject.AddComponent<Outline>();
            }
            outline.effectColor = selectionFrameColor;
            outline.effectDistance = new Vector2(selectionFrameWidth, selectionFrameWidth);
            outline.enabled = false;
        }

        public void Initialize(LevelEditorToolManager manager)
        {
            toolManager = manager;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (toolManager != null)
            {
                toolManager.SelectColor(colorId);
            }
        }

        public void SetSelected(bool selected)
        {
            isSelected = selected;
            if (outline != null)
            {
                outline.enabled = selected;
            }
        }
    }
}
