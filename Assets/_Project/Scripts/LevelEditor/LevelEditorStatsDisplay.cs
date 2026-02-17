using UnityEngine;
using TMPro;
using Gameplay.Grid;

namespace LevelEditor
{
    public class LevelEditorStatsDisplay : MonoBehaviour
    {
        [SerializeField] private LevelEditorGridManager gridManager;
        [SerializeField] private TextMeshProUGUI statusText;

        private void Start()
        {
            if (gridManager != null)
            {
                gridManager.OnGridChanged += UpdateStats;
                UpdateStats();
            }
        }

        private void OnDestroy()
        {
            if (gridManager != null)
            {
                gridManager.OnGridChanged -= UpdateStats;
            }
        }

        private void UpdateStats()
        {
            if (gridManager == null || statusText == null) return;

            int occupiedCount = 0;
            for (int x = 0; x < gridManager.Width; x++)
            {
                for (int y = 0; y < gridManager.Height; y++)
                {
                    var cell = gridManager.GetCell(x, y);
                    if (cell != null && cell.IsOccupied)
                    {
                        occupiedCount++;
                    }
                }
            }

            statusText.text = $"Total HP: {occupiedCount}";
        }
    }
}
