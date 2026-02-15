using UnityEngine;

namespace Gameplay.Grid
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class GridCellView : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer spriteRenderer;

        // Color mapping: index 0 = empty, 1-8 = chip colors
        private static readonly Color[] DefaultColors = new Color[]
        {
            new Color(0.15f, 0.15f, 0.15f, 0.4f), // 0 - empty (dark gray, semi-transparent)
            new Color(0.9f, 0.2f, 0.2f, 1f),     // 1 - red
            new Color(0.2f, 0.4f, 0.9f, 1f),     // 2 - blue
            new Color(0.2f, 0.8f, 0.3f, 1f),     // 3 - green
            new Color(0.95f, 0.85f, 0.1f, 1f),   // 4 - yellow
            new Color(0.8f, 0.3f, 0.8f, 1f),     // 5 - purple
            new Color(1f, 0.5f, 0.1f, 1f),        // 6 - orange
            new Color(0.1f, 0.8f, 0.8f, 1f),     // 7 - cyan
            new Color(0.9f, 0.5f, 0.7f, 1f),     // 8 - pink
        };

        private void Awake()
        {
            if (spriteRenderer == null)
                spriteRenderer = GetComponent<SpriteRenderer>();
        }

        private void OnEnable()
        {
            // Ensure spriteRenderer is assigned even if Awake didn't run yet
            if (spriteRenderer == null)
                spriteRenderer = GetComponent<SpriteRenderer>();
        }

        public void SetColor(int colorId)
        {
            if (spriteRenderer == null)
                spriteRenderer = GetComponent<SpriteRenderer>();

            if (spriteRenderer == null) return;

            if (colorId >= 0 && colorId < DefaultColors.Length)
            {
                spriteRenderer.color = DefaultColors[colorId];
            }
            else
            {
                spriteRenderer.color = DefaultColors[0];
            }
        }

        public static Color GetChipColor(int colorId)
        {
            if (colorId >= 0 && colorId < DefaultColors.Length)
                return DefaultColors[colorId];
            return Color.white;
        }
    }
}
