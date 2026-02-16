using UnityEngine;
using UnityEngine.UI;
using Gameplay.Grid;

namespace LevelEditor
{
    /// <summary>
    /// Manages the level editor grid. Cells are grouped into row sub-containers
    /// to avoid Unity 6 ATGTextJobSystem crash when selecting objects in large
    /// flat hierarchies (1000+ siblings under one parent).
    /// 
    /// Hierarchy structure:
    ///   GridContainer (no layout group — positions are set manually)
    ///     Row_0  -> Cell_0_0, Cell_1_0, ... Cell_29_0
    ///     Row_1  -> Cell_0_1, Cell_1_1, ... Cell_29_1
    ///     ...
    ///     Row_34 -> Cell_0_34, Cell_1_34, ... Cell_29_34
    /// </summary>
    public class LevelEditorGridManager : MonoBehaviour
    {
        [SerializeField] private int width = 30;
        [SerializeField] private int height = 35;
        [SerializeField] private LevelEditorCellView cellPrefab;
        [SerializeField] private Transform gridContainer;
        [SerializeField] private Vector2 cellSize = new Vector2(11, 11);
        [SerializeField] private Vector2 spacing = new Vector2(1, 1);
        [SerializeField] private bool autoFitCells = true;

        private LevelEditorCellView[,] cellViews;
        private Cell[,] grid; // Data model

        public int Width => width;
        public int Height => height;

        private void Start()
        {
            InitializeGrid();
        }

        public void InitializeGrid()
        {
            if (gridContainer == null) return;
            if (cellPrefab == null) return;

            // Clear existing children (row containers or loose cells)
            var children = new System.Collections.Generic.List<GameObject>();
            foreach (Transform child in gridContainer) children.Add(child.gameObject);
            foreach (var child in children)
            {
                if (Application.isPlaying) Destroy(child);
                else DestroyImmediate(child);
            }

            // Remove any old layout components that might interfere
            RemoveComponent<GridLayoutGroup>(gridContainer.gameObject);
            RemoveComponent<VerticalLayoutGroup>(gridContainer.gameObject);
            RemoveComponent<HorizontalLayoutGroup>(gridContainer.gameObject);
            RemoveComponent<ContentSizeFitter>(gridContainer.gameObject);

            cellViews = new LevelEditorCellView[width, height];
            grid = new Cell[width, height];

            // Compute actual cell size to fit the container
            Vector2 actualCellSize = cellSize;
            Vector2 actualSpacing = spacing;

            if (autoFitCells)
            {
                RectTransform containerRect = gridContainer.GetComponent<RectTransform>();
                // Force canvas update to get accurate rect
                Canvas.ForceUpdateCanvases();
                float containerW = containerRect.rect.width;
                float containerH = containerRect.rect.height;

                if (containerW > 0 && containerH > 0)
                {
                    // Calculate the max cell size that fits both dimensions
                    // totalWidth = width * cellW + (width-1) * spacingX
                    // totalHeight = height * cellH + (height-1) * spacingY
                    // For square cells with uniform spacing ratio:
                    float maxCellW = (containerW - (width - 1) * actualSpacing.x) / width;
                    float maxCellH = (containerH - (height - 1) * actualSpacing.y) / height;
                    float fitCell = Mathf.Min(maxCellW, maxCellH);
                    fitCell = Mathf.Max(fitCell, 1f); // minimum 1px
                    actualCellSize = new Vector2(fitCell, fitCell);
                }
            }

            // Calculate total grid size with actual cell size
            float totalGridWidth = width * actualCellSize.x + (width - 1) * actualSpacing.x;
            float totalGridHeight = height * actualCellSize.y + (height - 1) * actualSpacing.y;

            // Create rows centered in the container
            float gridStartY = -totalGridHeight * 0.5f;

            for (int y = 0; y < height; y++)
            {
                GameObject rowGO = new GameObject($"Row_{y}");
                rowGO.transform.SetParent(gridContainer, false);

                RectTransform rowRect = rowGO.AddComponent<RectTransform>();
                // Anchor to center of container
                rowRect.anchorMin = new Vector2(0.5f, 0.5f);
                rowRect.anchorMax = new Vector2(0.5f, 0.5f);
                rowRect.pivot = new Vector2(0.5f, 0.5f);
                // Position: centered horizontally, stacked vertically
                float rowY = gridStartY + y * (actualCellSize.y + actualSpacing.y) + actualCellSize.y * 0.5f;
                rowRect.anchoredPosition = new Vector2(0, rowY);
                rowRect.sizeDelta = new Vector2(totalGridWidth, actualCellSize.y);

                for (int x = 0; x < width; x++)
                {
                    LevelEditorCellView cell = Instantiate(cellPrefab, rowGO.transform);
                    cell.Initialize(x, y);
                    cell.SetColor(0); // Empty

                    // Position cell within the row
                    RectTransform cellRect = cell.GetComponent<RectTransform>();
                    cellRect.anchorMin = new Vector2(0f, 0.5f);
                    cellRect.anchorMax = new Vector2(0f, 0.5f);
                    cellRect.pivot = new Vector2(0f, 0.5f);
                    cellRect.sizeDelta = actualCellSize;
                    float cellX = x * (actualCellSize.x + actualSpacing.x);
                    cellRect.anchoredPosition = new Vector2(cellX, 0);

                    cellViews[x, y] = cell;
                    grid[x, y] = new Cell(x, y);
                }
            }
        }

        private void RemoveComponent<T>(GameObject go) where T : Component
        {
            T comp = go.GetComponent<T>();
            if (comp != null)
            {
                if (Application.isPlaying) Destroy(comp);
                else DestroyImmediate(comp);
            }
        }

        public void SetCellColor(int x, int y, int colorId)
        {
            if (x >= 0 && x < width && y >= 0 && y < height)
            {
                grid[x, y].ColorId = colorId;
                grid[x, y].IsOccupied = colorId > 0;
                cellViews[x, y].SetColor(colorId);
            }
        }

        public Cell GetCell(int x, int y)
        {
            if (x >= 0 && x < width && y >= 0 && y < height)
            {
                return grid[x, y];
            }
            return null;
        }
    }
}
