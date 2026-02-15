using UnityEngine;
using System.Collections.Generic;
using Gameplay.Units;
using Gameplay.Grid;
using TMPro;

namespace Gameplay.Mechanics
{
    /// <summary>
    /// The Base: 4 slots, each with a queue (stack) of up to 6 units.
    /// Player taps the top (front) unit of a slot to send it to the Conveyor.
    /// Linked units can only be launched when both are at the front of their respective slots.
    /// </summary>
    public class BaseManager : MonoBehaviour
    {
        [SerializeField] private int slotCount = 4;
        [SerializeField] private int maxQueueDepth = 6;
        [SerializeField] private UnitController unitPrefab;
        [SerializeField] private List<UnitController> unitPrefabsByColor;

        [Header("Layout Settings")]
        [Tooltip("Multiplier for horizontal spacing between slots relative to cell size")]
        [SerializeField] private float slotSpacingMultiplier = 2.5f;
        [Tooltip("Multiplier for vertical spacing between units in a stack relative to cell size")]
        [SerializeField] private float unitStackOffsetMultiplier = 1.4f;

        [Header("Unit Settings")]
        [SerializeField] private float unitSize = 0.4f;
        [SerializeField] private bool useGridCellSizeForUnits = false;

        private List<List<UnitController>> slots = new List<List<UnitController>>();
        private ConveyorBelt conveyorBelt;
        private LandingStrip landingStrip;
        private float cellSize = 0.4f;
        private float slotSpacing;
        private float unitStackOffset;

        // Linked pair tracking
        private int nextLinkedGroupId = 0;

        // Visuals
        private TextMeshPro labelText;
        private List<TextMeshPro> slotLabels = new List<TextMeshPro>();

        public int SlotCount => slotCount;

        public event System.Action OnBaseChanged;

        public void Initialize(ConveyorBelt conveyor, LandingStrip strip, float cellSz)
        {
            conveyorBelt = conveyor;
            landingStrip = strip;
            cellSize = cellSz;

            if (useGridCellSizeForUnits)
            {
                unitSize = cellSize;
            }

            // Use unitSize for spacing if we want layout to match units, 
            // or cellSize if we want layout to match grid. 
            // Usually layout should match the objects being laid out (units).
            float layoutBaseSize = useGridCellSizeForUnits ? cellSize : unitSize;

            slotSpacing = layoutBaseSize * slotSpacingMultiplier;
            unitStackOffset = layoutBaseSize * unitStackOffsetMultiplier;

            slots.Clear();
            for (int i = 0; i < slotCount; i++)
            {
                slots.Add(new List<UnitController>());
            }

            CreateVisuals();
        }

        private void CreateVisuals()
        {
            // Create base label
            if (labelText == null)
            {
                GameObject labelObj = new GameObject("BaseLabel");
                labelObj.transform.SetParent(transform);
                labelObj.transform.localPosition = new Vector3((slotCount - 1) * slotSpacing * 0.5f, cellSize * 1.5f, 0);
                labelText = labelObj.AddComponent<TextMeshPro>();
                labelText.text = "BASE";
                labelText.fontSize = 2.5f;
                labelText.alignment = TextAlignmentOptions.Center;
                labelText.color = new Color(0.2f, 0.8f, 0.8f, 1f);
                labelText.sortingOrder = 20;
                var rt = labelText.GetComponent<RectTransform>();
                if (rt != null) rt.sizeDelta = new Vector2(5f, 0.5f);
            }

            // Create slot labels
            foreach (var sl in slotLabels)
            {
                if (sl != null)
                {
                    if (Application.isPlaying) Destroy(sl.gameObject);
                    else DestroyImmediate(sl.gameObject);
                }
            }
            slotLabels.Clear();

            for (int i = 0; i < slotCount; i++)
            {
                GameObject slotLabelObj = new GameObject($"SlotLabel_{i}");
                slotLabelObj.transform.SetParent(transform);
                slotLabelObj.transform.localPosition = new Vector3(i * slotSpacing, cellSize * 0.8f, 0);
                TextMeshPro slotLabel = slotLabelObj.AddComponent<TextMeshPro>();
                slotLabel.text = $"Slot {i + 1}\n(0)";
                slotLabel.fontSize = 1.8f;
                slotLabel.alignment = TextAlignmentOptions.Center;
                slotLabel.color = new Color(0.7f, 0.7f, 0.7f, 0.8f);
                slotLabel.sortingOrder = 20;
                var rt = slotLabel.GetComponent<RectTransform>();
                if (rt != null) rt.sizeDelta = new Vector2(1.5f, 0.6f);
                slotLabels.Add(slotLabel);
            }
        }

        /// <summary>
        /// Add a unit config to a specific slot queue. Creates the visual unit.
        /// </summary>
        public UnitController AddUnitToSlot(int slotIndex, int colorId, int capacity)
        {
            if (slotIndex < 0 || slotIndex >= slotCount) return null;
            if (slots[slotIndex].Count >= maxQueueDepth) return null;

            UnitController prefabToUse = unitPrefab;
            if (unitPrefabsByColor != null && colorId >= 0 && colorId < unitPrefabsByColor.Count && unitPrefabsByColor[colorId] != null)
            {
                prefabToUse = unitPrefabsByColor[colorId];
            }

            UnitController unit = Instantiate(prefabToUse, transform);
            unit.Initialize(colorId, capacity);
            unit.SetScale(unitSize);
            unit.gameObject.name = $"Unit_S{slotIndex}_C{colorId}_Cap{capacity}";

            slots[slotIndex].Add(unit);
            UpdateSlotVisuals();
            OnBaseChanged?.Invoke();
            return unit;
        }

        /// <summary>
        /// Create a linked pair across two slots.
        /// </summary>
        public void CreateLinkedPair(int slotA, int colorA, int capA, int slotB, int colorB, int capB)
        {
            var unitA = AddUnitToSlot(slotA, colorA, capA);
            var unitB = AddUnitToSlot(slotB, colorB, capB);
            if (unitA != null && unitB != null)
            {
                int groupId = nextLinkedGroupId++;
                unitA.LinkedId = groupId;
                unitB.LinkedId = groupId;
            }
        }

        /// <summary>
        /// Try to launch the front unit of a slot onto the conveyor.
        /// </summary>
        public bool TryLaunchFromSlot(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= slotCount) return false;
            if (slots[slotIndex].Count == 0) return false;

            UnitController frontUnit = slots[slotIndex][0];
            if (frontUnit == null) return false;

            // Check if this unit is linked
            if (frontUnit.LinkedId >= 0)
            {
                return TryLaunchLinkedPair(frontUnit);
            }

            // Single unit launch
            if (conveyorBelt == null || !conveyorBelt.CanAddUnit()) return false;

            slots[slotIndex].RemoveAt(0);
            frontUnit.transform.SetParent(null);
            conveyorBelt.AddUnit(frontUnit);

            UpdateSlotVisuals();
            OnBaseChanged?.Invoke();
            Debug.Log($"[Base] Launched unit C{frontUnit.ColorId} from slot {slotIndex}.");
            return true;
        }

        private bool TryLaunchLinkedPair(UnitController unit)
        {
            UnitController partner = FindLinkedPartner(unit);
            if (partner == null)
            {
                Debug.LogWarning("[Base] Linked partner not found in base.");
                return false;
            }

            int slotA = FindSlotIndex(unit);
            int slotB = FindSlotIndex(partner);
            if (slotA < 0 || slotB < 0) return false;

            // Both must be at front of their slots
            if (slots[slotA][0] != unit || slots[slotB][0] != partner)
            {
                Debug.LogWarning("[Base] Linked pair: both must be at front of their slots.");
                return false;
            }

            if (conveyorBelt == null || !conveyorBelt.CanAddUnit()) return false;

            slots[slotA].RemoveAt(0);
            slots[slotB].RemoveAt(0);

            GameObject containerObj = new GameObject("LinkedPair");
            LinkedUnitController linked = containerObj.AddComponent<LinkedUnitController>();

            unit.transform.SetParent(null);
            partner.transform.SetParent(null);
            linked.Initialize(unit, partner);
            
            // Set spacing based on unit size
            float spacing = useGridCellSizeForUnits ? cellSize : unitSize;
            linked.SetSpacing(spacing * 1.1f);

            conveyorBelt.AddUnit(linked);

            UpdateSlotVisuals();
            OnBaseChanged?.Invoke();
            Debug.Log($"[Base] Launched linked pair from slots {slotA} and {slotB}.");
            return true;
        }

        private UnitController FindLinkedPartner(UnitController unit)
        {
            if (unit.LinkedId < 0) return null;

            for (int s = 0; s < slots.Count; s++)
            {
                for (int i = 0; i < slots[s].Count; i++)
                {
                    var other = slots[s][i];
                    if (other != unit && other.LinkedId == unit.LinkedId)
                        return other;
                }
            }

            // Also check landing strip for partner
            return null;
        }

        private int FindSlotIndex(UnitController unit)
        {
            for (int s = 0; s < slots.Count; s++)
            {
                if (slots[s].Contains(unit)) return s;
            }
            return -1;
        }

        public bool IsAtFront(UnitController unit)
        {
            int slot = FindSlotIndex(unit);
            if (slot < 0) return false;
            return slots[slot].Count > 0 && slots[slot][0] == unit;
        }

        public bool TryLaunchByUnit(UnitController unit)
        {
            int slot = FindSlotIndex(unit);
            if (slot < 0) return false;
            if (slots[slot][0] != unit) return false;
            return TryLaunchFromSlot(slot);
        }

        public UnitController GetFrontUnit(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= slotCount) return null;
            if (slots[slotIndex].Count == 0) return null;
            return slots[slotIndex][0];
        }

        public int GetSlotUnitCount(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= slotCount) return 0;
            return slots[slotIndex].Count;
        }

        public bool IsEmpty()
        {
            for (int i = 0; i < slots.Count; i++)
                if (slots[i].Count > 0) return false;
            return true;
        }

        private void UpdateSlotVisuals()
        {
            for (int s = 0; s < slots.Count; s++)
            {
                for (int i = 0; i < slots[s].Count; i++)
                {
                    var unit = slots[s][i];
                    if (unit == null) continue;

                    float x = s * slotSpacing;
                    float y = -i * unitStackOffset;
                    unit.transform.localPosition = new Vector3(x, y, 0);

                    // Dim units that are not at front
                    var sr = unit.GetComponent<SpriteRenderer>();
                    if (sr != null)
                    {
                        Color c = sr.color;
                        c.a = (i == 0) ? 1f : 0.5f;
                        sr.color = c;
                    }
                }

                // Update slot label
                if (s < slotLabels.Count && slotLabels[s] != null)
                {
                    slotLabels[s].text = $"Slot {s + 1}\n({slots[s].Count})";
                }
            }
        }

        public Vector3 GetSlotWorldPosition(int slotIndex)
        {
            return transform.position + new Vector3(slotIndex * slotSpacing, 0, 0);
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, 0.2f);

            if (slotCount <= 0) return;

            // Try to get cell size from GridManager if not set
            float effectiveCellSize = cellSize > 0 ? cellSize : 0.4f;
            var gm = FindObjectOfType<GridManager>();
            if (gm != null) effectiveCellSize = gm.CellSize;

            float effectiveSlotSpacing = effectiveCellSize * slotSpacingMultiplier;
            float effectiveStackOffset = effectiveCellSize * unitStackOffsetMultiplier;

            for (int i = 0; i < slotCount; i++)
            {
                Vector3 slotPos = transform.position + new Vector3(i * effectiveSlotSpacing, 0, 0);
                
                // Draw slot base
                Gizmos.color = new Color(1, 1, 0, 0.5f);
                Gizmos.DrawWireCube(slotPos, new Vector3(effectiveCellSize, effectiveCellSize, 0.1f));

                // Draw stack height
                Gizmos.color = new Color(1, 1, 0, 0.2f);
                Vector3 topOfStack = slotPos + new Vector3(0, -maxQueueDepth * effectiveStackOffset, 0);
                Gizmos.DrawLine(slotPos, topOfStack);
                Gizmos.DrawWireCube(topOfStack, new Vector3(effectiveCellSize * 0.8f, effectiveCellSize * 0.8f, 0.1f));
            }
        }

        [ContextMenu("Generate Preview")]
        private void GeneratePreview()
        {
            // Try to get cell size from GridManager
            var gm = FindObjectOfType<GridManager>();
            if (gm != null) cellSize = gm.CellSize;
            else if (cellSize <= 0) cellSize = 0.4f;

            if (useGridCellSizeForUnits)
            {
                unitSize = cellSize;
            }

            float layoutBaseSize = useGridCellSizeForUnits ? cellSize : unitSize;
            slotSpacing = layoutBaseSize * slotSpacingMultiplier;
            unitStackOffset = layoutBaseSize * unitStackOffsetMultiplier;

            // Clear existing
            ClearPreview();

            CreateVisuals();
        }

        [ContextMenu("Clear Preview")]
        private void ClearPreview()
        {
            var children = new List<GameObject>();
            foreach (Transform child in transform) children.Add(child.gameObject);
            
            foreach (var child in children)
            {
                if (child.name.StartsWith("BaseLabel") || child.name.StartsWith("SlotLabel"))
                {
                    DestroyImmediate(child);
                }
            }
            
            labelText = null;
            slotLabels.Clear();
        }
#endif
    }
}
