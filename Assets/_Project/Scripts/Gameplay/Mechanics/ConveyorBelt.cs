using UnityEngine;
using System.Collections.Generic;
using Gameplay.Units;
using Gameplay.Grid;

namespace Gameplay.Mechanics
{
    /// <summary>
    /// The conveyor belt surrounds the grid. Units move along it, scanning inward at each cell.
    /// The path has one waypoint per cell edge, going clockwise:
    ///   Bottom edge (left→right), Right edge (bottom→top), Top edge (right→left), Left edge (top→bottom).
    /// Each waypoint stores which edge and cell index it corresponds to, so units can scan.
    /// </summary>
    [ExecuteAlways]
    public class ConveyorBelt : MonoBehaviour
    {
        [SerializeField] private GridManager gridManager;
        [SerializeField] private float speed = 3.0f;
        [SerializeField] private int maxCapacity = 5;
        [SerializeField] private float pathOffset = 0.5f; // Distance from grid edge
        
        [Header("Visual Settings")]
        [SerializeField] private Material conveyorMaterial;
        [SerializeField] private float conveyorWidth = 0.2f;
        [SerializeField] private float textureTiling = 1.0f;

        private List<Vector3> pathPoints = new List<Vector3>();
        private List<WaypointInfo> waypointInfos = new List<WaypointInfo>(); // parallel to pathPoints
        private List<ConveyorItem> itemsOnBelt = new List<ConveyorItem>();
        private bool initialized;
        private Vector3 lastGridPos;
        private int lastGridWidth;
        private int lastGridHeight;
        private float lastGridCellSize;

        // Visual
        private LineRenderer lineRenderer;

        public int MaxCapacity => maxCapacity;
        public int CurrentCount => itemsOnBelt.Count;
        public List<Vector3> PathPoints => pathPoints;
        public List<WaypointInfo> WaypointInfos => waypointInfos;
        public float Speed => speed;

        public event System.Action<ConveyorItem> OnItemCompletedLoop; // item finished a full loop
        public event System.Action OnCountChanged;

        public struct WaypointInfo
        {
            public int edge;  // 0=bottom, 1=right, 2=top, 3=left, -1=corner
            public int index; // cell index along that edge
        }

        public void Initialize(GridManager gm)
        {
            gridManager = gm;
            GeneratePath();
            CreateVisual();
            initialized = true;
        }

        private void GeneratePath()
        {
            pathPoints.Clear();
            waypointInfos.Clear();

            if (gridManager == null) return;

            Vector3 origin = gridManager.GetOrigin();
            float cSize = gridManager.CellSize;
            int w = gridManager.Width;
            int h = gridManager.Height;

            // Bottom edge: left to right (y = origin.y - offset)
            float bottomY = origin.y - pathOffset;
            float topY = origin.y + h * cSize + pathOffset;
            float leftX = origin.x - pathOffset;
            float rightX = origin.x + w * cSize + pathOffset;

            // Start at bottom-left corner
            AddPoint(new Vector3(leftX, bottomY, 0), -1, -1);

            // Bottom edge waypoints (one per column, centered)
            for (int x = 0; x < w; x++)
            {
                float px = origin.x + x * cSize + cSize * 0.5f;
                AddPoint(new Vector3(px, bottomY, 0), 0, x);
            }

            // Bottom-right corner
            AddPoint(new Vector3(rightX, bottomY, 0), -1, -1);

            // Right edge waypoints (one per row, bottom to top)
            for (int y = 0; y < h; y++)
            {
                float py = origin.y + y * cSize + cSize * 0.5f;
                AddPoint(new Vector3(rightX, py, 0), 1, y);
            }

            // Top-right corner
            AddPoint(new Vector3(rightX, topY, 0), -1, -1);

            // Top edge waypoints (right to left)
            for (int x = w - 1; x >= 0; x--)
            {
                float px = origin.x + x * cSize + cSize * 0.5f;
                AddPoint(new Vector3(px, topY, 0), 2, x);
            }

            // Top-left corner
            AddPoint(new Vector3(leftX, topY, 0), -1, -1);

            // Left edge waypoints (top to bottom)
            for (int y = h - 1; y >= 0; y--)
            {
                float py = origin.y + y * cSize + cSize * 0.5f;
                AddPoint(new Vector3(leftX, py, 0), 3, y);
            }

            // Close loop back to start
            AddPoint(new Vector3(leftX, bottomY, 0), -1, -1);
        }

        private void AddPoint(Vector3 pos, int edge, int index)
        {
            pathPoints.Add(pos);
            waypointInfos.Add(new WaypointInfo { edge = edge, index = index });
        }

        private void CreateVisual()
        {
            if (pathPoints.Count < 2) return;

            lineRenderer = gameObject.GetComponent<LineRenderer>();
            if (lineRenderer == null)
                lineRenderer = gameObject.AddComponent<LineRenderer>();

            lineRenderer.positionCount = pathPoints.Count;
            lineRenderer.SetPositions(pathPoints.ToArray());
            lineRenderer.startWidth = conveyorWidth;
            lineRenderer.endWidth = conveyorWidth;
            lineRenderer.loop = false;
            lineRenderer.useWorldSpace = true;
            lineRenderer.sortingOrder = -1;
            lineRenderer.textureMode = LineTextureMode.Tile;

            if (conveyorMaterial != null)
            {
                lineRenderer.material = conveyorMaterial;
                lineRenderer.material.mainTextureScale = new Vector2(textureTiling * pathPoints.Count, 1);
            }
            else
            {
                // Create a simple material if none assigned
                lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
                lineRenderer.startColor = new Color(0.3f, 0.8f, 1f, 0.5f);
                lineRenderer.endColor = new Color(0.3f, 0.8f, 1f, 0.5f);
            }
        }

        public void UpdateVisualSettings(float width, Material mat, float tiling)
        {
            conveyorWidth = width;
            conveyorMaterial = mat;
            textureTiling = tiling;
            CreateVisual();
        }

        public bool CanAddUnit()
        {
            return itemsOnBelt.Count < maxCapacity;
        }

        public void IncreaseCapacity(int amount = 1)
        {
            maxCapacity += amount;
            OnCountChanged?.Invoke();
        }

        public void AddUnit(ConveyorItem item)
        {
            if (!initialized) return;
            if (!CanAddUnit()) return;

            itemsOnBelt.Add(item);

            // Give the unit references it needs
            if (item is UnitController unit)
            {
                unit.SetConveyorData(gridManager, waypointInfos);
            }
            else if (item is LinkedUnitController linked)
            {
                linked.SetConveyorData(gridManager, waypointInfos);
            }

            item.StartPath(pathPoints, speed);
            OnCountChanged?.Invoke();

            Debug.Log($"[Conveyor] Unit added. Belt count: {itemsOnBelt.Count}/{maxCapacity}");
        }

        private void Update()
        {
            if (!Application.isPlaying)
            {
                if (gridManager != null)
                {
                    bool changed = false;
                    if (gridManager.transform.position != lastGridPos) changed = true;
                    if (gridManager.Width != lastGridWidth) changed = true;
                    if (gridManager.Height != lastGridHeight) changed = true;
                    if (Mathf.Abs(gridManager.CellSize - lastGridCellSize) > 0.001f) changed = true;

                    if (changed)
                    {
                        Initialize(gridManager);
                        lastGridPos = gridManager.transform.position;
                        lastGridWidth = gridManager.Width;
                        lastGridHeight = gridManager.Height;
                        lastGridCellSize = gridManager.CellSize;
                    }
                }
                return;
            }

            // Clean up null references and completed items
            for (int i = itemsOnBelt.Count - 1; i >= 0; i--)
            {
                ConveyorItem item = itemsOnBelt[i];

                // Null check (destroyed externally)
                if (item == null)
                {
                    itemsOnBelt.RemoveAt(i);
                    OnCountChanged?.Invoke();
                    continue;
                }

                if (item.PathCompleted)
                {
                    itemsOnBelt.RemoveAt(i);
                    OnCountChanged?.Invoke();
                    OnItemCompletedLoop?.Invoke(item);
                }
            }
        }

        /// <summary>
        /// Draw the conveyor path as gizmos for debugging.
        /// </summary>
        private void OnDrawGizmos()
        {
            if (pathPoints == null || pathPoints.Count < 2) return;
            Gizmos.color = Color.cyan;
            for (int i = 0; i < pathPoints.Count - 1; i++)
            {
                Gizmos.DrawLine(pathPoints[i], pathPoints[i + 1]);
            }
        }

        private void OnValidate()
        {
            if (!Application.isPlaying)
            {
                if (gridManager == null)
                    gridManager = FindObjectOfType<GridManager>();
                
                if (gridManager != null)
                {
                    Initialize(gridManager);
                }
            }
        }
    }
}
