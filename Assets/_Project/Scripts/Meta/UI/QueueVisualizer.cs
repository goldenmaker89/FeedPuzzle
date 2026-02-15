using UnityEngine;
using UnityEngine.UI;
using Gameplay.Mechanics;
using System.Collections.Generic;

namespace Meta.UI
{
    /// <summary>
    /// Visualizes the Base queue status. Shows front units of each slot.
    /// </summary>
    public class QueueVisualizer : MonoBehaviour
    {
        [SerializeField] private BaseManager baseManager;
        [SerializeField] private Transform container;
        [SerializeField] private GameObject unitIconPrefab;
        [SerializeField] private int maxVisibleUnits = 4;

        private List<GameObject> spawnedIcons = new List<GameObject>();

        private void Start()
        {
            if (baseManager == null)
            {
                baseManager = FindFirstObjectByType<BaseManager>();
            }
        }

        private void Update()
        {
            if (baseManager != null && container != null && unitIconPrefab != null)
            {
                UpdateVisuals();
            }
        }

        private void UpdateVisuals()
        {
            // Clear old icons
            foreach (var icon in spawnedIcons)
            {
                if (icon != null) Destroy(icon);
            }
            spawnedIcons.Clear();

            // Show front unit of each slot
            for (int i = 0; i < Mathf.Min(baseManager.SlotCount, maxVisibleUnits); i++)
            {
                var frontUnit = baseManager.GetFrontUnit(i);
                if (frontUnit != null)
                {
                    var icon = Instantiate(unitIconPrefab, container);

                    var image = icon.GetComponent<Image>();
                    if (image != null)
                    {
                        image.color = GetColor(frontUnit.ColorId);
                    }

                    var text = icon.GetComponentInChildren<TMPro.TextMeshProUGUI>();
                    if (text != null)
                    {
                        text.text = $"{frontUnit.Capacity}\n({baseManager.GetSlotUnitCount(i)})";
                    }

                    spawnedIcons.Add(icon);
                }
            }
        }

        private Color GetColor(int colorId)
        {
            switch (colorId)
            {
                case 1: return Color.red;
                case 2: return Color.blue;
                case 3: return Color.green;
                case 4: return Color.yellow;
                default: return Color.white;
            }
        }
    }
}
