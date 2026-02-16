using UnityEngine;
using UnityEngine.UI;
using Gameplay.Grid;

namespace LevelEditor
{
    [RequireComponent(typeof(Image))]
    public class LevelEditorCellView : MonoBehaviour
    {
        [SerializeField] private Image image;
        
        public int X { get; private set; }
        public int Y { get; private set; }

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

        public void SetColor(int colorId)
        {
            if (image == null)
                image = GetComponent<Image>();
                
            if (image != null)
            {
                if (colorId == 0)
                {
                    // Use a visible color for empty cells in the editor
                    // Background is 0.15, so let's use 0.25 for empty cells to distinguish from background
                    image.color = new Color(0.25f, 0.25f, 0.25f, 1f);
                }
                else
                {
                    image.color = GridCellView.GetChipColor(colorId);
                }
            }
        }
    }
}
