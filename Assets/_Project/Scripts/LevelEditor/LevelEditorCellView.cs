using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Gameplay.Grid;

namespace LevelEditor
{
    [RequireComponent(typeof(Image))]
    public class LevelEditorCellView : MonoBehaviour, IPointerDownHandler, IPointerEnterHandler
    {
        [SerializeField] private Image image;

        public int X { get; private set; }
        public int Y { get; private set; }

        private LevelEditorToolManager toolManager;

        private void Awake()
        {
            if (image == null)
                image = GetComponent<Image>();
        }

        public void Initialize(int x, int y)
        {
            X = x;
            Y = y;
            name = $"Cell_{x}_{y}";
        }

        /// <summary>
        /// Called after grid creation to wire up the tool manager reference.
        /// </summary>
        public void SetToolManager(LevelEditorToolManager manager)
        {
            toolManager = manager;
        }

        public void SetColor(int colorId)
        {
            if (image == null)
                image = GetComponent<Image>();

            if (image != null)
            {
                if (colorId == 0)
                {
                    image.color = new Color(0.25f, 0.25f, 0.25f, 1f);
                }
                else
                {
                    image.color = GridCellView.GetChipColor(colorId);
                }
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            // Only allow painting with left mouse button
            if (eventData.button != PointerEventData.InputButton.Left) return;

            if (toolManager != null)
            {
                toolManager.OnCellPointerDown(this);
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            // Drag painting is primarily handled by ToolManager.Update() via raycast.
            // This is kept as a fallback for cases where OnPointerEnter does fire.
            if (toolManager != null)
            {
                toolManager.OnCellPointerEnter(this);
            }
        }
    }
}
