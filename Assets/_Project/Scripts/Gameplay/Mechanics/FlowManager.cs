using UnityEngine;
using Gameplay.Units;
using Gameplay.Grid;
using Core;

namespace Gameplay.Mechanics
{
    /// <summary>
    /// Orchestrates the full game flow:
    /// Base → Conveyor → LandingStrip cycle.
    /// Handles item completion events and win/loss checks.
    /// </summary>
    public class FlowManager : MonoBehaviour
    {
        [SerializeField] private GridManager gridManager;
        [SerializeField] private ConveyorBelt conveyorBelt;
        [SerializeField] private LandingStrip landingStrip;
        [SerializeField] private BaseManager baseManager;

        private bool gameEnded;
        private bool gameStarted; // prevents premature win check before level is populated

        public void Initialize(GridManager gm, ConveyorBelt cb, LandingStrip ls, BaseManager bm)
        {
            gridManager = gm;
            conveyorBelt = cb;
            landingStrip = ls;
            baseManager = bm;

            // Subscribe to conveyor completion
            conveyorBelt.OnItemCompletedLoop += HandleItemCompletedLoop;

            // Subscribe to grid cleared
            gridManager.OnAllChipsCleared += HandleAllChipsCleared;

            // Delay game start check to allow TestSceneSetup to populate the grid
            Invoke(nameof(EnableGameStarted), 0.2f);
        }

        private void OnDestroy()
        {
            if (conveyorBelt != null)
                conveyorBelt.OnItemCompletedLoop -= HandleItemCompletedLoop;
            if (gridManager != null)
                gridManager.OnAllChipsCleared -= HandleAllChipsCleared;
        }

        private void HandleItemCompletedLoop(ConveyorItem item)
        {
            if (gameEnded) return;
            if (item == null) return;

            // Determine if the item should be destroyed (capacity fully spent)
            bool shouldDestroy = false;
            if (item is UnitController unit)
            {
                shouldDestroy = unit.Capacity <= 0;
                Debug.Log($"[Flow] Unit C{unit.ColorId} completed loop. Capacity: {unit.Capacity}/{unit.MaxCap}. Destroy: {shouldDestroy}");
            }
            else if (item is LinkedUnitController linked)
            {
                bool aDead = linked.UnitA == null || linked.UnitA.Capacity <= 0;
                bool bDead = linked.UnitB == null || linked.UnitB.Capacity <= 0;
                shouldDestroy = aDead && bDead;
                Debug.Log($"[Flow] LinkedPair completed loop. A dead: {aDead}, B dead: {bDead}. Destroy: {shouldDestroy}");
            }

            if (shouldDestroy)
            {
                // Item is spent, destroy it
                if (item is LinkedUnitController lnk)
                {
                    if (lnk.UnitA != null) Destroy(lnk.UnitA.gameObject);
                    if (lnk.UnitB != null) Destroy(lnk.UnitB.gameObject);
                }
                Destroy(item.gameObject);
                Debug.Log("[Flow] Unit destroyed (capacity depleted).");

                // Check win after destruction
                CheckWinCondition();
                return;
            }

            // Try to dock on landing strip
            if (landingStrip.TryDock(item))
            {
                Debug.Log($"[Flow] Unit docked on landing strip. Slots used: {landingStrip.OccupiedSlots}/{landingStrip.Capacity}");
                // Successfully docked
                CheckWinCondition();
            }
            else
            {
                // Traffic jam! Game over
                Debug.LogError($"[Flow] TRAFFIC JAM! Landing strip full ({landingStrip.OccupiedSlots}/{landingStrip.Capacity}). Cannot dock.");
                TriggerGameOver();
            }
        }

        private void EnableGameStarted()
        {
            gameStarted = true;
            Debug.Log("[Flow] Game flow enabled. Win/loss checks active.");
        }

        private void HandleAllChipsCleared()
        {
            if (gameEnded || !gameStarted) return;
            TriggerVictory();
        }

        private void CheckWinCondition()
        {
            if (gameEnded || !gameStarted) return;
            if (gridManager.IsCleared())
            {
                TriggerVictory();
            }
        }

        private void Update()
        {
            if (gameEnded || !gameStarted) return;

            // Additional check: if grid is cleared and no units on conveyor, victory
            if (gridManager != null && gridManager.IsCleared() && conveyorBelt != null && conveyorBelt.CurrentCount == 0)
            {
                TriggerVictory();
            }
        }

        private void TriggerGameOver()
        {
            if (gameEnded) return;
            gameEnded = true;
            Debug.LogError("=== GAME OVER: Traffic Jam! Landing strip is full. ===");
            if (GameManager.Instance != null)
                GameManager.Instance.SetState(GameState.GameOver);
        }

        private void TriggerVictory()
        {
            if (gameEnded) return;
            gameEnded = true;
            Debug.Log("=== VICTORY: All chips cleared! ===");
            if (GameManager.Instance != null)
                GameManager.Instance.SetState(GameState.Victory);
        }
    }
}
