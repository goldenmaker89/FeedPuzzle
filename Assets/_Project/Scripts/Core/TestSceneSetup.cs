using UnityEngine;
using Gameplay.Units;
using Gameplay.Mechanics;
using Gameplay.Grid;

namespace Core
{
    /// <summary>
    /// Sets up a test level after GameManager initializes everything.
    /// Populates the grid with chips and the base with destroyers.
    /// Positions Base and LandingStrip below the grid.
    /// </summary>
    public class TestSceneSetup : MonoBehaviour
    {
        [SerializeField] private int gridWidth = 8;
        [SerializeField] private int gridHeight = 8;
        [SerializeField] private int numColors = 4;
        [SerializeField] private float fillPercent = 0.6f;

        private void Start()
        {
            // Wait one frame so GameManager.Start runs first
            Invoke(nameof(Setup), 0.05f);
        }

        private void Setup()
        {
            var gm = GameManager.Instance;
            if (gm == null)
            {
                Debug.LogError("TestSceneSetup: GameManager not found!");
                return;
            }

            var grid = gm.Grid;
            var conveyor = gm.Conveyor;
            var landing = gm.Landing;
            var baseM = gm.Base;

            if (grid == null || conveyor == null || landing == null || baseM == null)
            {
                Debug.LogError("TestSceneSetup: Missing manager references!");
                return;
            }

            float cellSize = grid.CellSize;
            float gridW = grid.Width * cellSize;
            float gridH = grid.Height * cellSize;
            Vector3 gridOrigin = grid.GetOrigin();
            float gridCenterX = gridOrigin.x + gridW * 0.5f;

            // Populate grid with random chips
            int[] colorCounts = PopulateGrid(grid);

            // Position landing strip below grid (below conveyor bottom edge)
            float conveyorOffset = 0.5f; // matches ConveyorBelt pathOffset
            float landingY = gridOrigin.y - conveyorOffset - cellSize * 2.5f;
            float landingSlotSpacing = cellSize * 1.5f;
            float landingTotalWidth = (landing.Capacity - 1) * landingSlotSpacing;
            float landingStartX = gridCenterX - landingTotalWidth * 0.5f;
            landing.transform.position = new Vector3(landingStartX, landingY, 0);

            // Base: below landing strip
            float baseSlotSpacing = cellSize * 2f;
            float baseTotalWidth = (baseM.SlotCount - 1) * baseSlotSpacing;
            float baseY = landingY - cellSize * 5f;
            float baseStartX = gridCenterX - baseTotalWidth * 0.5f;
            baseM.transform.position = new Vector3(baseStartX, baseY, 0);

            // Populate base with units that match the grid colors
            PopulateBase(colorCounts, baseM);

            // Center camera to show everything
            CenterCamera(grid, baseM, landing);

            int totalUnits = 0;
            for (int i = 0; i < baseM.SlotCount; i++)
                totalUnits += baseM.GetSlotUnitCount(i);

            Debug.Log($"=== Test level setup complete ===");
            Debug.Log($"Grid: {grid.Width}x{grid.Height}, Chips: {grid.TotalChips}, Colors: {numColors}");
            Debug.Log($"Base: {totalUnits} units across {baseM.SlotCount} slots");
            Debug.Log($"Conveyor capacity: {conveyor.MaxCapacity}");
            Debug.Log($"Landing strip capacity: {landing.Capacity}");
            Debug.Log($"Click on a unit in the BASE to send it to the conveyor!");
        }

        private int[] PopulateGrid(GridManager grid)
        {
            int[] colorCounts = new int[numColors + 1];

            for (int x = 0; x < grid.Width; x++)
            {
                for (int y = 0; y < grid.Height; y++)
                {
                    if (Random.value < fillPercent)
                    {
                        int colorId = Random.Range(1, numColors + 1);
                        grid.SetCellColor(x, y, colorId);
                        colorCounts[colorId]++;
                    }
                }
            }

            // Log color distribution
            for (int c = 1; c <= numColors; c++)
            {
                Debug.Log($"  Color {c}: {colorCounts[c]} chips");
            }

            return colorCounts;
        }

        private void PopulateBase(int[] colorCounts, BaseManager baseM)
        {
            int slotIndex = 0;
            for (int c = 1; c < colorCounts.Length; c++)
            {
                int remaining = colorCounts[c];
                while (remaining > 0)
                {
                    int cap = Mathf.Min(remaining, Random.Range(2, 5));
                    baseM.AddUnitToSlot(slotIndex % baseM.SlotCount, c, cap);
                    remaining -= cap;
                    slotIndex++;
                }
            }
        }

        private void CenterCamera(GridManager grid, BaseManager baseM, LandingStrip landing)
        {
            Camera cam = Camera.main;
            if (cam == null) return;

            Vector3 gridCenter = grid.GetCenter();
            float baseBottomY = baseM.transform.position.y - grid.CellSize * 6f; // account for stacked units
            float topY = grid.GetTopRight().y + 1f;

            // Center between top of grid and bottom of base
            float centerY = (topY + baseBottomY) * 0.5f;
            cam.transform.position = new Vector3(gridCenter.x, centerY, -10);
            cam.orthographic = true;

            // Size to fit everything with padding
            float totalHeight = topY - baseBottomY + 2f;
            float totalWidth = grid.Width * grid.CellSize + 3f;
            float aspect = cam.aspect > 0 ? cam.aspect : 1f;
            cam.orthographicSize = Mathf.Max(totalHeight * 0.55f, totalWidth / (2f * aspect));
        }
    }
}
