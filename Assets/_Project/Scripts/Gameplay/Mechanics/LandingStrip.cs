using UnityEngine;
using System.Collections.Generic;
using Gameplay.Units;
using TMPro;

namespace Gameplay.Mechanics
{
    /// <summary>
    /// Landing Strip: 5 slots buffer zone.
    /// Units that finish a conveyor loop with remaining capacity dock here.
    /// Player can click a docked unit to re-launch it onto the conveyor.
    /// Linked pairs take 2 slots.
    /// </summary>
    public class LandingStrip : MonoBehaviour
    {
        [SerializeField] private int capacity = 5;
        
        [Header("Visual Settings")]
        [SerializeField] private float slotSpacingMultiplier = 1.5f;
        [SerializeField] private Sprite slotSprite;
        [SerializeField] private Color emptySlotColor = new Color(0.3f, 0.3f, 0.3f, 0.3f);
        [SerializeField] private Color occupiedSlotColor = new Color(0.5f, 0.5f, 0.2f, 0.3f);
        [SerializeField] private Vector3 labelOffset = new Vector3(0, 0.5f, 0);
        [SerializeField] private float labelFontSize = 2.5f;

        private float slotSpacing = 0.6f;
        private List<UnitController> dockedUnits = new List<UnitController>();
        private ConveyorBelt conveyorBelt;
        private float cellSize = 0.4f;

        // Visuals
        private List<SpriteRenderer> slotVisuals = new List<SpriteRenderer>();
        private TextMeshPro labelText;

        public int Capacity => capacity;
        public int OccupiedSlots => dockedUnits.Count;
        public int FreeSlots => capacity - dockedUnits.Count;

        public event System.Action OnDockedUnitsChanged;

        public void Initialize(ConveyorBelt conveyor, float cellSz)
        {
            conveyorBelt = conveyor;
            cellSize = cellSz;
            slotSpacing = cellSize * slotSpacingMultiplier;
            CreateVisuals();
        }

        private void CreateVisuals()
        {
            // Clear old visuals
            foreach (var sv in slotVisuals)
            {
                if (sv != null) Destroy(sv.gameObject);
            }
            slotVisuals.Clear();

            // Create slot indicator sprites
            for (int i = 0; i < capacity; i++)
            {
                GameObject slotObj = new GameObject($"LandingSlot_{i}");
                slotObj.transform.SetParent(transform);
                slotObj.transform.localPosition = new Vector3(i * slotSpacing, 0, 0);
                
                // Scale based on sprite size (assuming 32px base if null, or actual size)
                float spriteBaseSize = slotSprite ? slotSprite.bounds.size.x : 1f;
                // If sprite is null, Unity uses a 1x1 unit square (usually 100ppu => 0.01 world units? No, default sprite is 1x1 world unit usually if created via CreatePrimitive, but here we add SpriteRenderer)
                // Actually if sprite is null, nothing renders. We need a default sprite or the user must assign one.
                // For now, let's assume user assigns one or we use a default white texture created at runtime if needed.
                
                float scale = (cellSize * 0.9f) / (spriteBaseSize > 0 ? spriteBaseSize : 1f);
                if (slotSprite == null) scale = cellSize * 0.9f; // Fallback scaling

                slotObj.transform.localScale = Vector3.one * scale;

                SpriteRenderer sr = slotObj.AddComponent<SpriteRenderer>();
                sr.sprite = slotSprite;
                sr.color = emptySlotColor;
                sr.sortingOrder = 1;
                slotVisuals.Add(sr);
            }

            // Create label
            if (labelText == null)
            {
                GameObject labelObj = new GameObject("LandingLabel");
                labelObj.transform.SetParent(transform);
                // Center label above the strip
                float centerX = (capacity - 1) * slotSpacing * 0.5f;
                labelObj.transform.localPosition = new Vector3(centerX, 0, 0) + labelOffset;
                
                labelText = labelObj.AddComponent<TextMeshPro>();
                labelText.text = $"LANDING STRIP (0/{capacity})";
                labelText.fontSize = labelFontSize;
                labelText.alignment = TextAlignmentOptions.Center;
                labelText.color = new Color(0.8f, 0.8f, 0.2f, 1f);
                labelText.sortingOrder = 20;
                var rt = labelText.GetComponent<RectTransform>();
                if (rt != null) rt.sizeDelta = new Vector2(10f, 1f);
            }
            else
            {
                 // Update position if already exists
                float centerX = (capacity - 1) * slotSpacing * 0.5f;
                labelText.transform.localPosition = new Vector3(centerX, 0, 0) + labelOffset;
                labelText.fontSize = labelFontSize;
            }
        }

        public bool HasSpace(int requiredSlots = 1)
        {
            return dockedUnits.Count + requiredSlots <= capacity;
        }

        /// <summary>
        /// Try to dock a conveyor item that finished its loop.
        /// Returns true if docked successfully.
        /// </summary>
        public bool TryDock(ConveyorItem item)
        {
            if (item is UnitController unit)
            {
                if (unit.Capacity <= 0)
                {
                    // Should have been destroyed by FlowManager, but safety check
                    Destroy(unit.gameObject);
                    return true;
                }

                if (HasSpace(1))
                {
                    DockUnit(unit);
                    return true;
                }
                return false;
            }
            else if (item is LinkedUnitController linked)
            {
                bool aDead = linked.UnitA == null || linked.UnitA.Capacity <= 0;
                bool bDead = linked.UnitB == null || linked.UnitB.Capacity <= 0;

                if (aDead && bDead)
                {
                    // Both spent - should have been handled by FlowManager
                    if (linked.UnitA != null) Destroy(linked.UnitA.gameObject);
                    if (linked.UnitB != null) Destroy(linked.UnitB.gameObject);
                    Destroy(linked.gameObject);
                    return true;
                }

                // Linked pairs always take 2 slots per GDD
                if (HasSpace(2))
                {
                    // Dock both (even the one with 0 capacity - it stays with its partner)
                    if (linked.UnitA != null)
                    {
                        linked.UnitA.transform.SetParent(null);
                        DockUnit(linked.UnitA);
                    }
                    if (linked.UnitB != null)
                    {
                        linked.UnitB.transform.SetParent(null);
                        DockUnit(linked.UnitB);
                    }
                    Destroy(linked.gameObject);
                    return true;
                }
                return false;
            }
            return false;
        }

        private void DockUnit(UnitController unit)
        {
            dockedUnits.Add(unit);
            unit.SetState(UnitState.InLandingStrip);
            unit.transform.SetParent(transform);
            UpdateSlotPositions();
            UpdateLabel();
            OnDockedUnitsChanged?.Invoke();
            Debug.Log($"[LandingStrip] Docked unit C{unit.ColorId} ({unit.Capacity}/{unit.MaxCap}). Slots: {dockedUnits.Count}/{capacity}");
        }

        /// <summary>
        /// Try to relaunch a docked unit back onto the conveyor.
        /// For linked units, both partners must be relaunched together.
        /// </summary>
        public bool TryRelaunch(UnitController unit)
        {
            if (!dockedUnits.Contains(unit)) return false;
            if (conveyorBelt == null || !conveyorBelt.CanAddUnit()) return false;

            // Check if linked
            if (unit.LinkedId >= 0)
            {
                return TryRelaunchLinkedPair(unit);
            }

            // Single unit relaunch
            if (unit.Capacity <= 0) return false; // can't relaunch empty unit alone

            dockedUnits.Remove(unit);
            unit.transform.SetParent(null);
            conveyorBelt.AddUnit(unit);

            UpdateSlotPositions();
            UpdateLabel();
            OnDockedUnitsChanged?.Invoke();
            Debug.Log($"[LandingStrip] Relaunched unit C{unit.ColorId}. Slots: {dockedUnits.Count}/{capacity}");
            return true;
        }

        private bool TryRelaunchLinkedPair(UnitController unit)
        {
            // Find partner on landing strip
            UnitController partner = null;
            foreach (var u in dockedUnits)
            {
                if (u != unit && u.LinkedId == unit.LinkedId)
                {
                    partner = u;
                    break;
                }
            }

            if (partner == null) return false; // partner not on strip
            if (!conveyorBelt.CanAddUnit()) return false;

            // Remove both
            dockedUnits.Remove(unit);
            dockedUnits.Remove(partner);

            // Create linked container
            GameObject containerObj = new GameObject("LinkedPair");
            LinkedUnitController linked = containerObj.AddComponent<LinkedUnitController>();

            unit.transform.SetParent(null);
            partner.transform.SetParent(null);
            linked.Initialize(unit, partner);

            conveyorBelt.AddUnit(linked);

            UpdateSlotPositions();
            UpdateLabel();
            OnDockedUnitsChanged?.Invoke();
            return true;
        }

        public void RemoveUnit(UnitController unit)
        {
            if (dockedUnits.Contains(unit))
            {
                dockedUnits.Remove(unit);
                UpdateSlotPositions();
                UpdateLabel();
                OnDockedUnitsChanged?.Invoke();
            }
        }

        private void UpdateSlotPositions()
        {
            for (int i = 0; i < dockedUnits.Count; i++)
            {
                if (dockedUnits[i] == null) continue;
                dockedUnits[i].transform.localPosition = new Vector3(i * slotSpacing, 0, 0);
            }

            // Update slot visual colors
            for (int i = 0; i < slotVisuals.Count; i++)
            {
                if (slotVisuals[i] == null) continue;
                if (i < dockedUnits.Count)
                    slotVisuals[i].color = occupiedSlotColor;
                else
                    slotVisuals[i].color = emptySlotColor;
            }
        }

        private void UpdateLabel()
        {
            if (labelText != null)
            {
                labelText.text = $"LANDING STRIP ({dockedUnits.Count}/{capacity})";
            }
        }

        public List<UnitController> GetDockedUnits()
        {
            return dockedUnits;
        }

        /// <summary>
        /// Try to relaunch by clicking on a unit.
        /// </summary>
        public bool TryRelaunchByUnit(UnitController unit)
        {
            return TryRelaunch(unit);
        }
    }
}
