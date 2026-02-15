using UnityEngine;
using System.Collections.Generic;

namespace Gameplay.Grid
{
    public class GridManager : MonoBehaviour
    {
        [SerializeField] private int width = 8;
        [SerializeField] private int height = 8;
        [SerializeField] private float cellSize = 0.4f;
        [SerializeField] private GridCellView cellPrefab;

        private Cell[,] grid;
        private GridCellView[,] cellViews;
        private bool initialized;
        private int totalChips;

        public int Width => width;
        public int Height => height;
        public float CellSize => cellSize;
        public int TotalChips => totalChips;

        public event System.Action<int> OnChipDestroyed; // remaining count
        public event System.Action OnAllChipsCleared;

        public void InitializeGrid(int w, int h, float cSize)
        {
            if (initialized) return;
            width = w;
            height = h;
            cellSize = cSize;
            DoInit();
        }

        public void InitializeGrid()
        {
            if (initialized) return;
            DoInit();
        }

        private void DoInit()
        {
            // Clear old visuals if any
            if (cellViews != null)
            {
                for (int x = 0; x < cellViews.GetLength(0); x++)
                    for (int y = 0; y < cellViews.GetLength(1); y++)
                        if (cellViews[x, y] != null)
                            Destroy(cellViews[x, y].gameObject);
            }

            grid = new Cell[width, height];
            cellViews = new GridCellView[width, height];
            totalChips = 0;

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    grid[x, y] = new Cell(x, y);
                    CreateCellVisual(x, y);
                }
            }
            initialized = true;
        }

        private void CreateCellVisual(int x, int y)
        {
            if (cellPrefab == null) return;

            Vector3 worldPos = GetWorldPosition(x, y);
            GridCellView cellView = Instantiate(cellPrefab, worldPos, Quaternion.identity, transform);
            cellView.name = $"Cell_{x}_{y}";
            cellView.transform.localScale = Vector3.one * (cellSize * 0.9f / 0.32f); // scale to fit cell with small gap
            cellViews[x, y] = cellView;
            cellView.SetColor(0);
        }

        /// <summary>
        /// World position of cell center. Grid origin is at transform.position (bottom-left corner of cell 0,0).
        /// </summary>
        public Vector3 GetWorldPosition(int x, int y)
        {
            return transform.position + new Vector3(x * cellSize + cellSize * 0.5f, y * cellSize + cellSize * 0.5f, 0);
        }

        /// <summary>
        /// Bottom-left corner of the grid in world space.
        /// </summary>
        public Vector3 GetOrigin()
        {
            return transform.position;
        }

        /// <summary>
        /// Top-right corner of the grid in world space.
        /// </summary>
        public Vector3 GetTopRight()
        {
            return transform.position + new Vector3(width * cellSize, height * cellSize, 0);
        }

        public Vector3 GetCenter()
        {
            return transform.position + new Vector3(width * cellSize * 0.5f, height * cellSize * 0.5f, 0);
        }

        public Cell GetCell(int x, int y)
        {
            if (x >= 0 && x < width && y >= 0 && y < height)
                return grid[x, y];
            return null;
        }

        public void SetCellColor(int x, int y, int colorId)
        {
            Cell cell = GetCell(x, y);
            if (cell == null) return;

            bool wasOccupied = cell.IsOccupied;
            cell.ColorId = colorId;
            cell.IsOccupied = colorId > 0;

            if (cellViews[x, y] != null)
                cellViews[x, y].SetColor(colorId);

            // Track chip count
            if (!wasOccupied && colorId > 0)
            {
                totalChips++;
            }
            else if (wasOccupied && colorId <= 0)
            {
                totalChips--;
                OnChipDestroyed?.Invoke(totalChips);
                if (totalChips <= 0)
                {
                    OnAllChipsCleared?.Invoke();
                }
            }
        }

        /// <summary>
        /// Scan from an edge inward along a row or column.
        /// edge: 0=bottom, 1=right, 2=top, 3=left
        /// index: the row or column index along that edge
        /// Returns the first occupied cell visible from that edge, or null.
        /// </summary>
        public Cell ScanFromEdge(int edge, int index)
        {
            switch (edge)
            {
                case 0: // bottom edge, scanning upward (column = index)
                    if (index < 0 || index >= width) return null;
                    for (int y = 0; y < height; y++)
                    {
                        Cell c = grid[index, y];
                        if (c.IsOccupied) return c;
                    }
                    break;
                case 1: // right edge, scanning leftward (row = index)
                    if (index < 0 || index >= height) return null;
                    for (int x = width - 1; x >= 0; x--)
                    {
                        Cell c = grid[x, index];
                        if (c.IsOccupied) return c;
                    }
                    break;
                case 2: // top edge, scanning downward (column = index)
                    if (index < 0 || index >= width) return null;
                    for (int y = height - 1; y >= 0; y--)
                    {
                        Cell c = grid[index, y];
                        if (c.IsOccupied) return c;
                    }
                    break;
                case 3: // left edge, scanning rightward (row = index)
                    if (index < 0 || index >= height) return null;
                    for (int x = 0; x < width; x++)
                    {
                        Cell c = grid[x, index];
                        if (c.IsOccupied) return c;
                    }
                    break;
            }
            return null;
        }

        public bool IsCleared()
        {
            return totalChips <= 0;
        }
    }
}
