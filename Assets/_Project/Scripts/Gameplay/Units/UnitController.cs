using UnityEngine;
using System.Collections.Generic;
using Gameplay.Grid;
using Gameplay.Mechanics;
using TMPro;

namespace Gameplay.Units
{
    public enum UnitState
    {
        InQueue,      // Sitting in Base slot queue
        OnConveyor,   // Moving along conveyor
        Returning,    // Finished loop, waiting to dock
        InLandingStrip // Parked on landing strip
    }

    [RequireComponent(typeof(SpriteRenderer))]
    public class UnitController : ConveyorItem
    {
        [SerializeField] private int maxCapacity = 5;
        [SerializeField] private int colorId = 1;
        [SerializeField] private TextMeshPro capacityText;

        [Header("Visual Settings")]
        [SerializeField] private Sprite unitSprite;
        [SerializeField] private float baseSpriteSize = 0.32f; // Used for scaling calculation
        [SerializeField] private List<Color> unitColors = new List<Color>
        {
            Color.white,                           // 0
            new Color(0.9f, 0.2f, 0.2f, 1f),     // 1 - red
            new Color(0.2f, 0.4f, 0.9f, 1f),     // 2 - blue
            new Color(0.2f, 0.8f, 0.3f, 1f),     // 3 - green
            new Color(0.95f, 0.85f, 0.1f, 1f),   // 4 - yellow
            new Color(0.8f, 0.3f, 0.8f, 1f),     // 5 - purple
            new Color(1f, 0.5f, 0.1f, 1f),        // 6 - orange
            new Color(0.1f, 0.8f, 0.8f, 1f),     // 7 - cyan
            new Color(0.9f, 0.5f, 0.7f, 1f),     // 8 - pink
        };

        private int currentCapacity;
        private GridManager gridManager;
        private List<ConveyorBelt.WaypointInfo> waypointInfos;
        private SpriteRenderer spriteRenderer;
        private BoxCollider2D boxCollider;

        public int Capacity => currentCapacity;
        public int MaxCap => maxCapacity;
        public int ColorId => colorId;
        public UnitState State { get; private set; }

        /// <summary>
        /// Unit should be removed from conveyor immediately when capacity is fully depleted.
        /// </summary>
        public override bool ShouldBeRemovedImmediately => currentCapacity <= 0;

        // Linked unit support
        public int LinkedId { get; set; } = -1; // -1 = not linked

        public void Initialize(int colorId, int capacity)
        {
            this.colorId = colorId;
            this.maxCapacity = capacity;
            this.currentCapacity = capacity;
            State = UnitState.InQueue;
            EnsureVisuals();
            UpdateVisuals();
        }

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            boxCollider = GetComponent<BoxCollider2D>();
        }

        private void EnsureVisuals()
        {
            if (spriteRenderer == null)
                spriteRenderer = GetComponent<SpriteRenderer>();

            if (spriteRenderer != null)
            {
                spriteRenderer.sortingOrder = 10;
                if (unitSprite != null)
                {
                    spriteRenderer.sprite = unitSprite;
                }
            }

            if (boxCollider == null)
                boxCollider = GetComponent<BoxCollider2D>();

            // Try to find existing capacity text (from prefab)
            if (capacityText == null)
            {
                capacityText = GetComponentInChildren<TextMeshPro>();
            }

            // Create capacity text if missing
            if (capacityText == null)
            {
                GameObject textObj = new GameObject("CapacityLabel");
                textObj.transform.SetParent(transform);
                textObj.transform.localPosition = Vector3.zero;
                textObj.transform.localScale = Vector3.one;
                capacityText = textObj.AddComponent<TextMeshPro>();
                capacityText.alignment = TextAlignmentOptions.Center;
                capacityText.fontSize = 3f;
                capacityText.color = Color.white;
                capacityText.sortingOrder = 15;

                // Set rect transform size
                var rt = capacityText.GetComponent<RectTransform>();
                if (rt != null)
                {
                    rt.sizeDelta = new Vector2(1f, 0.5f);
                }
            }
        }

        public void SetConveyorData(GridManager gm, List<ConveyorBelt.WaypointInfo> infos)
        {
            gridManager = gm;
            waypointInfos = infos;
        }

        public void SetState(UnitState newState)
        {
            State = newState;
        }

        public override void StartPath(List<Vector3> pathPoints, float speed)
        {
            base.StartPath(pathPoints, speed);
            SetState(UnitState.OnConveyor);
        }

        protected override void OnWaypointReached(int waypointIndex)
        {
            if (gridManager == null || waypointInfos == null) return;
            if (currentCapacity <= 0) return;
            if (waypointIndex < 0 || waypointIndex >= waypointInfos.Count) return;

            var info = waypointInfos[waypointIndex];
            if (info.edge < 0) return; // corner waypoint, no scanning

            // Scan from this edge
            Cell target = gridManager.ScanFromEdge(info.edge, info.index);
            if (target != null && target.IsOccupied && target.ColorId == this.colorId)
            {
                // Destroy the chip
                gridManager.SetCellColor(target.X, target.Y, 0);
                currentCapacity--;
                UpdateVisuals();
                Debug.Log($"[Unit C{colorId}] Destroyed chip at ({target.X},{target.Y}). Remaining capacity: {currentCapacity}/{maxCapacity}");
            }
        }

        /// <summary>
        /// Called when the unit finishes a full loop around the conveyor.
        /// Do NOT destroy here - let FlowManager handle the lifecycle.
        /// </summary>
        public override void OnPathComplete()
        {
            SetState(UnitState.Returning);
            isMoving = false;
            // FlowManager will handle destruction or docking via OnItemCompletedLoop
        }

        public void DecreaseCapacity()
        {
            if (currentCapacity > 0)
            {
                currentCapacity--;
                UpdateVisuals();
            }
        }

        private void UpdateVisuals()
        {
            if (spriteRenderer != null)
            {
                if (colorId >= 0 && colorId < unitColors.Count)
                    spriteRenderer.color = unitColors[colorId];
                else
                    spriteRenderer.color = Color.white;
            }

            if (capacityText != null)
            {
                capacityText.text = $"{currentCapacity}/{maxCapacity}";
            }

            // Update name for debugging
            gameObject.name = $"Unit_C{colorId}_{currentCapacity}/{maxCapacity}";
        }

        /// <summary>
        /// Public scan method called by LinkedUnitController at each waypoint.
        /// </summary>
        public void PerformScan(int waypointIndex)
        {
            if (gridManager == null || waypointInfos == null) return;
            if (currentCapacity <= 0) return;
            if (waypointIndex < 0 || waypointIndex >= waypointInfos.Count) return;

            var info = waypointInfos[waypointIndex];
            if (info.edge < 0) return;

            Cell target = gridManager.ScanFromEdge(info.edge, info.index);
            if (target != null && target.IsOccupied && target.ColorId == this.colorId)
            {
                gridManager.SetCellColor(target.X, target.Y, 0);
                currentCapacity--;
                UpdateVisuals();
                Debug.Log($"[LinkedUnit C{colorId}] Destroyed chip at ({target.X},{target.Y}). Remaining capacity: {currentCapacity}/{maxCapacity}");
            }
        }

        /// <summary>
        /// Scale the unit to match cell size. Also scales collider.
        /// </summary>
        public void SetScale(float cellSize)
        {
            float spriteSize = baseSpriteSize;
            if (unitSprite != null)
            {
                // Use actual sprite bounds if available, but keep baseSpriteSize as a reference/override
                // Or just rely on baseSpriteSize as the "expected" size of the sprite in units
            }
            
            float desiredSize = cellSize * 0.8f;
            float scale = desiredSize / spriteSize;
            transform.localScale = Vector3.one * scale;
        }

        public Color GetUnitColor(int colorId)
        {
            if (colorId >= 0 && colorId < unitColors.Count)
                return unitColors[colorId];
            return Color.white;
        }
    }
}
