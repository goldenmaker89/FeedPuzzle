using UnityEngine;
using Gameplay.Units;
using Gameplay.Mechanics;
using Gameplay.Grid;
using LevelEditor;
using System.IO;

namespace Core
{
    /// <summary>
    /// Loads a level from JSON (produced by LevelEditor) and populates the Gameplay scene.
    /// Replaces the old random-generation approach.
    /// 
    /// Flow:
    ///   Awake() — loads Level JSON and stores LevelConfig
    ///   GameManager.Start() reads LoadedConfig to get grid dimensions
    ///   Start() (delayed) — populates grid cells, base units, positions camera
    /// </summary>
    public class TestSceneSetup : MonoBehaviour
    {
        [Header("Level to Load")]
        [SerializeField] private int levelNumber = 1;

        private LevelConfig levelConfig;

        /// <summary>
        /// Loaded level configuration. Available after Awake().
        /// GameManager reads this before initialising the grid.
        /// </summary>
        public LevelConfig LoadedConfig => levelConfig;

        // ───────────────────────── lifecycle ─────────────────────────

        private void Awake()
        {
            LoadLevelConfig();
        }

        private void Start()
        {
            // Wait one frame so GameManager.Start() finishes system initialisation first
            Invoke(nameof(Setup), 0.05f);
        }

        // ───────────────────────── JSON loading ─────────────────────

        private void LoadLevelConfig()
        {
            string fileName = $"Level_{levelNumber}.json";
            string path;

#if UNITY_EDITOR
            path = Path.Combine(Application.dataPath, "_Project/Data/Levels", fileName);
#else
            path = Path.Combine(Application.streamingAssetsPath, "Levels", fileName);
#endif

            if (!File.Exists(path))
            {
                Debug.LogError($"[LevelLoader] Level file not found: {path}");
                return;
            }

            string json = File.ReadAllText(path);
            levelConfig = JsonUtility.FromJson<LevelConfig>(json);

            Debug.Log($"[LevelLoader] Loaded config for Level {levelConfig.levelNumber}: " +
                      $"{levelConfig.width}x{levelConfig.height}, " +
                      $"{levelConfig.cells.Count} cells, " +
                      $"{levelConfig.unitStacks.Count} unit stacks");
        }

        // ───────────────────────── scene setup ──────────────────────

        private void Setup()
        {
            var gm = GameManager.Instance;
            if (gm == null)
            {
                Debug.LogError("[LevelLoader] GameManager not found!");
                return;
            }

            var grid = gm.Grid;
            var conveyor = gm.Conveyor;
            var landing = gm.Landing;
            var baseM = gm.Base;

            if (grid == null || conveyor == null || landing == null || baseM == null)
            {
                Debug.LogError("[LevelLoader] Missing manager references!");
                return;
            }

            if (levelConfig == null)
            {
                Debug.LogError("[LevelLoader] No level config loaded — cannot populate scene.");
                return;
            }

            float cellSize = grid.CellSize;
            float gridW = grid.Width * cellSize;
            float gridH = grid.Height * cellSize;
            Vector3 gridOrigin = grid.GetOrigin();
            float gridCenterX = gridOrigin.x + gridW * 0.5f;

            // 1. Populate grid from JSON cells
            PopulateGrid(grid);

            // 2. Position landing strip below grid (below conveyor bottom edge)
            float conveyorOffset = 0.5f; // matches ConveyorBelt.pathOffset
            float landingY = gridOrigin.y - conveyorOffset - cellSize * 2.5f;
            float landingSlotSpacing = cellSize * 1.5f;
            float landingTotalWidth = (landing.Capacity - 1) * landingSlotSpacing;
            float landingStartX = gridCenterX - landingTotalWidth * 0.5f;
            landing.transform.position = new Vector3(landingStartX, landingY, 0);

            // 3. Position base below landing strip
            float baseSlotSpacing = cellSize * 2f;
            float baseTotalWidth = (baseM.SlotCount - 1) * baseSlotSpacing;
            float baseY = landingY - cellSize * 5f;
            float baseStartX = gridCenterX - baseTotalWidth * 0.5f;
            baseM.transform.position = new Vector3(baseStartX, baseY, 0);

            // 4. Populate base from JSON unit stacks
            PopulateBase(baseM);

            // 5. Centre camera to show everything
            CenterCamera(grid, baseM, landing);

            // ── summary log ──
            int totalUnits = 0;
            for (int i = 0; i < baseM.SlotCount; i++)
                totalUnits += baseM.GetSlotUnitCount(i);

            Debug.Log($"=== Level {levelConfig.levelNumber} setup complete ===");
            Debug.Log($"Grid: {grid.Width}x{grid.Height}, Chips: {grid.TotalChips}");
            Debug.Log($"Base: {totalUnits} units across {baseM.SlotCount} slots");
            Debug.Log($"Conveyor capacity: {conveyor.MaxCapacity}");
            Debug.Log($"Landing strip capacity: {landing.Capacity}");
            Debug.Log($"Click on a unit in the BASE to send it to the conveyor!");
        }

        // ───────────────────────── grid population ──────────────────

        private void PopulateGrid(GridManager grid)
        {
            foreach (var cell in levelConfig.cells)
            {
                if (cell.colorId > 0)
                {
                    grid.SetCellColor(cell.x, cell.y, cell.colorId);
                }
            }
        }

        // ───────────────────────── base population ──────────────────

        private void PopulateBase(BaseManager baseM)
        {
            foreach (var stack in levelConfig.unitStacks)
            {
                foreach (var unit in stack.units)
                {
                    // unit.hp maps to the unit's capacity (how many chips it can destroy)
                    baseM.AddUnitToSlot(stack.slotIndex, unit.colorId, unit.hp);
                }
            }
        }

        // ───────────────────────── camera ───────────────────────────

        private void CenterCamera(GridManager grid, BaseManager baseM, LandingStrip landing)
        {
            Camera cam = Camera.main;
            if (cam == null) return;

            Vector3 gridCenter = grid.GetCenter();
            float baseBottomY = baseM.transform.position.y - grid.CellSize * 6f;
            float topY = grid.GetTopRight().y + 1f;

            float centerY = (topY + baseBottomY) * 0.5f;
            cam.transform.position = new Vector3(gridCenter.x, centerY, -10);
            cam.orthographic = true;

            float totalHeight = topY - baseBottomY + 2f;
            float totalWidth = grid.Width * grid.CellSize + 3f;
            float aspect = cam.aspect > 0 ? cam.aspect : 1f;
            cam.orthographicSize = Mathf.Max(totalHeight * 0.55f, totalWidth / (2f * aspect));
        }
    }
}
