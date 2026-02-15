using UnityEngine;
using Gameplay.Grid;
using Gameplay.Mechanics;

namespace Core
{
    public enum GameState
    {
        Initializing,
        Playing,
        Paused,
        GameOver,
        Victory
    }

    /// <summary>
    /// Central game manager. Initializes all systems in the correct order.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [SerializeField] private GridManager gridManager;
        [SerializeField] private ConveyorBelt conveyorBelt;
        [SerializeField] private LandingStrip landingStrip;
        [SerializeField] private BaseManager baseManager;
        [SerializeField] private FlowManager flowManager;
        [SerializeField] private InputManager inputManager;

        public GameState CurrentState { get; private set; }
        public GridManager Grid => gridManager;
        public ConveyorBelt Conveyor => conveyorBelt;
        public LandingStrip Landing => landingStrip;
        public BaseManager Base => baseManager;

        public event System.Action<GameState> OnStateChanged;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            // Auto-find references if not assigned
            if (gridManager == null) gridManager = FindFirstObjectByType<GridManager>();
            if (conveyorBelt == null) conveyorBelt = FindFirstObjectByType<ConveyorBelt>();
            if (landingStrip == null) landingStrip = FindFirstObjectByType<LandingStrip>();
            if (baseManager == null) baseManager = FindFirstObjectByType<BaseManager>();
            if (flowManager == null) flowManager = FindFirstObjectByType<FlowManager>();
            if (inputManager == null) inputManager = FindFirstObjectByType<InputManager>();
        }

        private void Start()
        {
            SetState(GameState.Initializing);
            InitializeSystems();
            SetState(GameState.Playing);
        }

        private void InitializeSystems()
        {
            float cellSize = gridManager != null ? gridManager.CellSize : 0.4f;

            // 1. Grid
            if (gridManager != null)
                gridManager.InitializeGrid();

            // 2. Conveyor (needs grid)
            if (conveyorBelt != null && gridManager != null)
                conveyorBelt.Initialize(gridManager);

            // 3. Landing strip (needs conveyor)
            if (landingStrip != null)
                landingStrip.Initialize(conveyorBelt, cellSize);

            // 4. Base (needs conveyor + landing strip)
            if (baseManager != null)
                baseManager.Initialize(conveyorBelt, landingStrip, cellSize);

            // 5. Flow manager (needs all)
            if (flowManager != null)
                flowManager.Initialize(gridManager, conveyorBelt, landingStrip, baseManager);

            // 6. Input manager (needs base + landing strip)
            if (inputManager != null)
                inputManager.Initialize(baseManager, landingStrip);
        }

        public void SetState(GameState newState)
        {
            CurrentState = newState;
            OnStateChanged?.Invoke(newState);

            switch (newState)
            {
                case GameState.Paused:
                    Time.timeScale = 0;
                    break;
                case GameState.Playing:
                    Time.timeScale = 1;
                    break;
                case GameState.GameOver:
                case GameState.Victory:
                    Time.timeScale = 0;
                    break;
            }
        }

        public void TogglePause()
        {
            if (CurrentState == GameState.Playing)
                SetState(GameState.Paused);
            else if (CurrentState == GameState.Paused)
                SetState(GameState.Playing);
        }
    }
}
