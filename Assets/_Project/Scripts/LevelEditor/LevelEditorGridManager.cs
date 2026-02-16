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
        [SerializeField] private LevelEditorToolManager toolManager;

        private LevelEditorCellView[,] cellViews;
        private Cell[,] grid; // Data model
        private Vector2 actualCellSize;

        public int Width => width;
        public int Height => height;
        public bool IsInitialized => cellViews != null && grid != null;

        private void Start()
        {
            InitializeGrid();
        }

        private void Update()
        {
            // Re-initialize if arrays were lost (e.g. after script recompilation in play mode)
            if (!IsInitialized)
            {
                InitializeGrid();
            }
        }

        public void InitializeGrid()
        {
            if (gridContainer == null) return;

            // Remove any old layout components that might interfere
            RemoveComponent<GridLayoutGroup>(gridContainer.gameObject);
            RemoveComponent<VerticalLayoutGroup>(gridContainer.gameObject);
            RemoveComponent<HorizontalLayoutGroup>(gridContainer.gameObject);
            RemoveComponent<ContentSizeFitter>(gridContainer.gameObject);

            // Try to collect existing cells from the hierarchy first
            bool existingCellsFound = TryCollectExistingCells();

            if (!existingCellsFound)
            {
                // No existing cells found — rebuild from scratch
                RebuildGrid();
            }
            else
            {
                // Existing cells found — just ensure data model is in sync
                EnsureDataModelFromCellViews();
            }
        }

        /// <summary>
        /// Attempts to collect LevelEditorCellView references from existing hierarchy children.
        /// Parses X,Y from the cell GameObject name (format: "Cell_{x}_{y}") because the
        /// X/Y properties on LevelEditorCellView are not serialized.
        /// Returns true if a valid grid was found.
        /// </summary>
        private bool TryCollectExistingCells()
        {
            if (gridContainer.childCount == 0) return false;

            var tempCellViews = new LevelEditorCellView[width, height];
            int foundCount = 0;

            // Iterate through rows
            for (int rowIdx = 0; rowIdx < gridContainer.childCount; rowIdx++)
            {
                Transform row = gridContainer.GetChild(rowIdx);
                for (int cellIdx = 0; cellIdx < row.childCount; cellIdx++)
                {
                    var cellView = row.GetChild(cellIdx).GetComponent<LevelEditorCellView>();
                    if (cellView == null) continue;

                    // Parse coordinates from name: "Cell_{x}_{y}"
                    if (!TryParseCellName(cellView.gameObject.name, out int x, out int y))
                        continue;

                    if (x < 0 || x >= width || y < 0 || y >= height)
                        continue;

                    // Re-initialize the cell's runtime X,Y properties
                    cellView.Initialize(x, y);

                    tempCellViews[x, y] = cellView;
                    foundCount++;

                    // Ensure tool manager is wired
                    if (toolManager != null)
                        cellView.SetToolManager(toolManager);
                }
            }

            // Consider it valid if we found at least 80% of expected cells
            int expectedCount = width * height;
            if (foundCount >= expectedCount * 0.8f)
            {
                cellViews = tempCellViews;
                Debug.Log($"[LevelEditorGridManager] Collected {foundCount}/{expectedCount} existing cells from hierarchy.");
                return true;
            }

            return false;
        }

        /// <summary>
        /// Parses "Cell_{x}_{y}" format. Returns false if the name doesn't match.
        /// </summary>
        private static bool TryParseCellName(string name, out int x, out int y)
        {
            x = 0;
            y = 0;
            if (string.IsNullOrEmpty(name) || !name.StartsWith("Cell_"))
                return false;

            // "Cell_3_17" -> split by '_' -> ["Cell", "3", "17"]
            var parts = name.Split('_');
            if (parts.Length != 3)
                return false;

            return int.TryParse(parts[1], out x) && int.TryParse(parts[2], out y);
        }

        /// <summary>
        /// Creates the data model (Cell array) from the existing cellViews.
        /// </summary>
        private void EnsureDataModelFromCellViews()
        {
            grid = new Cell[width, height];
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    grid[x, y] = new Cell(x, y);
                }
            }
        }

        /// <summary>
        /// Destroys all existing children and rebuilds the grid from scratch.
        /// </summary>
        private void RebuildGrid()
        {
            if (cellPrefab == null)
            {
                Debug.LogError("[LevelEditorGridManager] cellPrefab is null, cannot rebuild grid.");
                return;
            }

            // Clear existing children
            var children = new System.Collections.Generic.List<GameObject>();
            foreach (Transform child in gridContainer) children.Add(child.gameObject);
            foreach (var child in children)
            {
                if (Application.isPlaying) Destroy(child);
                else DestroyImmediate(child);
            }

            cellViews = new LevelEditorCellView[width, height];
            grid = new Cell[width, height];

            // Compute actual cell size to fit the container
            actualCellSize = cellSize;
            Vector2 actualSpacing = spacing;

            if (autoFitCells)
            {
                RectTransform containerRect = gridContainer.GetComponent<RectTransform>();
                Canvas.ForceUpdateCanvases();
                float containerW = containerRect.rect.width;
                float containerH = containerRect.rect.height;

                if (containerW > 0 && containerH > 0)
                {
                    float maxCellW = (containerW - (width - 1) * actualSpacing.x) / width;
                    float maxCellH = (containerH - (height - 1) * actualSpacing.y) / height;
                    float fitCell = Mathf.Min(maxCellW, maxCellH);
                    fitCell = Mathf.Max(fitCell, 1f);
                    actualCellSize = new Vector2(fitCell, fitCell);
                }
            }

            float totalGridWidth = width * actualCellSize.x + (width - 1) * actualSpacing.x;
            float totalGridHeight = height * actualCellSize.y + (height - 1) * actualSpacing.y;
            float gridStartY = -totalGridHeight * 0.5f;

            for (int y = 0; y < height; y++)
            {
                GameObject rowGO = new GameObject($"Row_{y}");
                rowGO.transform.SetParent(gridContainer, false);

                RectTransform rowRect = rowGO.AddComponent<RectTransform>();
                rowRect.anchorMin = new Vector2(0.5f, 0.5f);
                rowRect.anchorMax = new Vector2(0.5f, 0.5f);
                rowRect.pivot = new Vector2(0.5f, 0.5f);
                float rowY = gridStartY + y * (actualCellSize.y + actualSpacing.y) + actualCellSize.y * 0.5f;
                rowRect.anchoredPosition = new Vector2(0, rowY);
                rowRect.sizeDelta = new Vector2(totalGridWidth, actualCellSize.y);

                for (int x = 0; x < width; x++)
                {
                    LevelEditorCellView cell = Instantiate(cellPrefab, rowGO.transform);
                    cell.Initialize(x, y);
                    cell.SetColor(0);

                    RectTransform cellRect = cell.GetComponent<RectTransform>();
                    cellRect.anchorMin = new Vector2(0f, 0.5f);
                    cellRect.anchorMax = new Vector2(0f, 0.5f);
                    cellRect.pivot = new Vector2(0f, 0.5f);
                    cellRect.sizeDelta = actualCellSize;
                    float cellX = x * (actualCellSize.x + actualSpacing.x);
                    cellRect.anchoredPosition = new Vector2(cellX, 0);

                    if (toolManager != null)
                        cell.SetToolManager(toolManager);

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
                if (grid != null)
                {
                    grid[x, y].ColorId = colorId;
                    grid[x, y].IsOccupied = colorId > 0;
                }
                if (cellViews != null && cellViews[x, y] != null)
                {
                    cellViews[x, y].SetColor(colorId);
                }
            }
        }

        public Cell GetCell(int x, int y)
        {
            if (grid == null) return null;
            if (x >= 0 && x < width && y >= 0 && y < height)
            {
                return grid[x, y];
            }
            return null;
        }
    }
}
