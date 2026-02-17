using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using Gameplay.Grid;
using TMPro;

namespace LevelEditor
{
    public class LevelEditorUnitGenerator : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private LevelEditorGridManager gridManager;
        [SerializeField] private Button generateButton;
        [SerializeField] private Slider difficultySlider;
        [SerializeField] private Transform unitTimelineContainer;
        [SerializeField] private GameObject unitIconPrefab;

        [Header("Settings")]
        [SerializeField] private int maxUnitsPerSlot = 6;
        [SerializeField] private int slotCount = 4;

        private List<UnitStackData> currentUnitStacks = new List<UnitStackData>();

        private void Start()
        {
            // Auto-find references if not assigned
            if (gridManager == null) gridManager = FindFirstObjectByType<LevelEditorGridManager>();
            
            if (generateButton == null)
            {
                var btnObj = GameObject.Find("GenerateButton");
                if (btnObj != null) generateButton = btnObj.GetComponent<Button>();
            }

            if (difficultySlider == null)
            {
                var sliderObj = GameObject.Find("DifficultySlider");
                if (sliderObj != null) difficultySlider = sliderObj.GetComponent<Slider>();
            }

            if (unitTimelineContainer == null)
            {
                var timelineObj = GameObject.Find("UnitTimeline");
                if (timelineObj != null) unitTimelineContainer = timelineObj.transform;
            }

            if (generateButton != null)
            {
                generateButton.onClick.AddListener(GenerateUnitStackWrapper);
            }
        }

        private void GenerateUnitStackWrapper()
        {
            GenerateUnitStack();
        }

        public void GenerateUnitStack()
        {
            if (gridManager == null || unitTimelineContainer == null)
            {
                Debug.LogError("Missing references for Unit Generation");
                return;
            }

            // 1. Scan Grid
            Dictionary<int, int> colorCounts = new Dictionary<int, int>();
            int totalHPAll = 0;
            for (int x = 0; x < gridManager.Width; x++)
            {
                for (int y = 0; y < gridManager.Height; y++)
                {
                    var cell = gridManager.GetCell(x, y);
                    if (cell != null && cell.IsOccupied)
                    {
                        if (!colorCounts.ContainsKey(cell.ColorId))
                            colorCounts[cell.ColorId] = 0;
                        colorCounts[cell.ColorId]++;
                        totalHPAll++;
                    }
                }
            }

            if (totalHPAll == 0) return;

            // 2. Calculate Target Unit Counts
            int maxCapacity = slotCount * maxUnitsPerSlot;
            
            // Determine desired total units based on slider
            // Slider 0 -> Min units (1 per color)
            // Slider 1 -> Max units (maxCapacity)
            float difficulty = difficultySlider != null ? difficultySlider.value : 0.5f;
            int minUnits = colorCounts.Count;
            int targetTotalUnits = Mathf.RoundToInt(Mathf.Lerp(minUnits, maxCapacity, difficulty));
            
            // Clamp to maxCapacity
            if (targetTotalUnits > maxCapacity) targetTotalUnits = maxCapacity;

            // Distribute targetTotalUnits among colors
            Dictionary<int, int> unitsPerColor = new Dictionary<int, int>();
            int assignedUnits = 0;

            // Pass 1: Proportional assignment
            foreach (var kvp in colorCounts)
            {
                int colorId = kvp.Key;
                int hp = kvp.Value;
                
                // Calculate max possible units for this color (to avoid < 10 HP units if possible, but allow small remainders)
                int maxPossible = Mathf.CeilToInt(hp / 10f);
                if (maxPossible < 1) maxPossible = 1;
                
                // Proportional share
                float share = (float)hp / totalHPAll;
                int count = Mathf.RoundToInt(targetTotalUnits * share);
                
                // Clamp
                if (count < 1) count = 1;
                if (count > maxPossible) count = maxPossible;
                
                unitsPerColor[colorId] = count;
                assignedUnits += count;
            }

            // Pass 2: Adjust to match maxCapacity constraint
            // If we assigned more than maxCapacity, reduce from the ones with most units
            while (assignedUnits > maxCapacity)
            {
                int maxC = -1;
                int maxVal = 0;
                foreach(var kvp in unitsPerColor)
                {
                    if (kvp.Value > 1 && kvp.Value > maxVal) // Keep at least 1
                    {
                        maxVal = kvp.Value;
                        maxC = kvp.Key;
                    }
                }
                
                if (maxC != -1)
                {
                    unitsPerColor[maxC]--;
                    assignedUnits--;
                }
                else
                {
                    break; // Cannot reduce further
                }
            }

            // 3. Generate Units
            List<UnitData> allUnits = new List<UnitData>();
            foreach (var kvp in colorCounts)
            {
                int colorId = kvp.Key;
                int hp = kvp.Value;
                
                if (unitsPerColor.ContainsKey(colorId))
                {
                    int count = unitsPerColor[colorId];
                    List<int> distributedHP = DistributeHP(hp, count);
                    foreach(int val in distributedHP)
                    {
                        allUnits.Add(new UnitData { colorId = colorId, hp = val });
                    }
                }
            }

            // 4. Distribute to Slots (Visual)
            // Clear existing
            foreach (Transform child in unitTimelineContainer)
            {
                Destroy(child.gameObject);
            }

            // Create Slots
            List<Transform> slots = new List<Transform>();
            currentUnitStacks.Clear();
            for (int i = 0; i < slotCount; i++)
            {
                GameObject slotObj = new GameObject($"Slot_{i}");
                slotObj.transform.SetParent(unitTimelineContainer, false);
                
                var le = slotObj.AddComponent<LayoutElement>();
                le.flexibleWidth = 1;
                
                var vlg = slotObj.AddComponent<VerticalLayoutGroup>();
                vlg.childControlHeight = false;
                vlg.childControlWidth = true;
                vlg.childForceExpandHeight = false;
                vlg.spacing = 5;
                vlg.padding = new RectOffset(5, 5, 5, 5);
                
                // Optional background
                var img = slotObj.AddComponent<Image>();
                img.color = new Color(0, 0, 0, 0.2f);

                slots.Add(slotObj.transform);
                currentUnitStacks.Add(new UnitStackData { slotIndex = i, units = new List<UnitData>() });
            }

            // Place units
            int currentSlot = 0;
            int[] unitsInSlot = new int[slotCount];

            foreach (var unit in allUnits)
            {
                // Find slot
                int attempts = 0;
                while (unitsInSlot[currentSlot] >= maxUnitsPerSlot && attempts < slotCount)
                {
                    currentSlot = (currentSlot + 1) % slotCount;
                    attempts++;
                }

                if (unitsInSlot[currentSlot] < maxUnitsPerSlot)
                {
                    CreateUnitIcon(unit, slots[currentSlot]);
                    currentUnitStacks[currentSlot].units.Add(unit);
                    unitsInSlot[currentSlot]++;
                    currentSlot = (currentSlot + 1) % slotCount;
                }
                else
                {
                    Debug.LogWarning("All slots are full! Cannot place unit.");
                }
            }
        }

        public List<UnitStackData> GetUnitStacks()
        {
            return currentUnitStacks;
        }

        public void LoadUnitStacks(List<UnitStackData> stacks)
        {
            if (unitTimelineContainer == null) return;

            // Clear existing
            foreach (Transform child in unitTimelineContainer)
            {
                Destroy(child.gameObject);
            }

            currentUnitStacks = stacks;
            if (currentUnitStacks == null) currentUnitStacks = new List<UnitStackData>();

            // Create Slots
            List<Transform> slots = new List<Transform>();
            for (int i = 0; i < slotCount; i++)
            {
                GameObject slotObj = new GameObject($"Slot_{i}");
                slotObj.transform.SetParent(unitTimelineContainer, false);
                
                var le = slotObj.AddComponent<LayoutElement>();
                le.flexibleWidth = 1;
                
                var vlg = slotObj.AddComponent<VerticalLayoutGroup>();
                vlg.childControlHeight = false;
                vlg.childControlWidth = true;
                vlg.childForceExpandHeight = false;
                vlg.spacing = 5;
                vlg.padding = new RectOffset(5, 5, 5, 5);
                
                // Optional background
                var img = slotObj.AddComponent<Image>();
                img.color = new Color(0, 0, 0, 0.2f);

                slots.Add(slotObj.transform);
            }

            // Populate slots from data
            foreach (var stack in currentUnitStacks)
            {
                if (stack.slotIndex >= 0 && stack.slotIndex < slots.Count)
                {
                    foreach (var unit in stack.units)
                    {
                        CreateUnitIcon(unit, slots[stack.slotIndex]);
                    }
                }
            }
        }

        private List<int> DistributeHP(int totalHP, int count)
        {
            List<int> results = new List<int>();
            if (count <= 0) return results;

            // Initialize with 0
            for(int i=0; i<count; i++) results.Add(0);

            // Base 10s
            int base10s = (totalHP / count / 10) * 10;
            for(int i=0; i<count; i++) results[i] = base10s;
            
            int remaining = totalHP - (base10s * count);
            
            // Distribute remaining 10s
            int remainingTens = remaining / 10;
            for(int i=0; i<remainingTens; i++)
            {
                results[i % count] += 10;
            }
            
            // Distribute remaining ones to the last unit
            int remainingOnes = remaining % 10;
            if (remainingOnes > 0)
            {
                results[count - 1] += remainingOnes;
            }
            
            return results;
        }

        private void CreateUnitIcon(UnitData unit, Transform parent)
        {
            if (unitIconPrefab == null) return;

            GameObject iconObj = Instantiate(unitIconPrefab, parent);
            
            // Set Color
            var img = iconObj.GetComponent<Image>();
            if (img != null)
            {
                img.color = GridCellView.GetChipColor(unit.colorId);
            }

            // Set Text (HP)
            var textComp = iconObj.GetComponentInChildren<TextMeshProUGUI>();
            if (textComp == null)
            {
                GameObject textObj = new GameObject("HP_Text");
                textObj.transform.SetParent(iconObj.transform, false);
                textComp = textObj.AddComponent<TextMeshProUGUI>();
                textComp.alignment = TextAlignmentOptions.Center;
                textComp.fontSize = 14;
                textComp.color = Color.black;
                
                var rt = textObj.GetComponent<RectTransform>();
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.one;
                rt.sizeDelta = Vector2.zero;
            }
            
            textComp.text = unit.hp.ToString();
        }
    }
}
