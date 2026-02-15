using UnityEngine;
using Gameplay.Mechanics;
using Gameplay.Units;
using System.Collections.Generic;

namespace Meta.UI
{
    public class LandingStripVisualizer : MonoBehaviour
    {
        [SerializeField] private LandingStrip landingStrip;
        [SerializeField] private Transform container;
        [SerializeField] private GameObject unitIconPrefab;
        
        // We need to know what's in the landing strip.
        // LandingStrip stores UnitControllers.
        // We can either poll or add events to LandingStrip.
        // Let's add events to LandingStrip first.
        
        private List<GameObject> spawnedIcons = new List<GameObject>();

        private void Start()
        {
            if (landingStrip == null)
            {
                landingStrip = FindFirstObjectByType<LandingStrip>();
            }

            if (landingStrip != null)
            {
                landingStrip.OnDockedUnitsChanged += UpdateVisuals;
            }
            
            UpdateVisuals();
        }

        private void OnDestroy()
        {
            if (landingStrip != null)
            {
                landingStrip.OnDockedUnitsChanged -= UpdateVisuals;
            }
        }

        private void UpdateVisuals()
        {
            if (landingStrip == null) return;

            var units = landingStrip.GetDockedUnits();

            // Clear old icons
            foreach (var icon in spawnedIcons)
            {
                Destroy(icon);
            }
            spawnedIcons.Clear();

            // Create new icons
            foreach (var unit in units)
            {
                var icon = Instantiate(unitIconPrefab, container);
                
                var image = icon.GetComponent<UnityEngine.UI.Image>();
                if (image != null)
                {
                    image.color = GetColor(unit.ColorId);
                }
                
                var text = icon.GetComponentInChildren<TMPro.TextMeshProUGUI>();
                if (text != null)
                {
                    text.text = unit.Capacity.ToString();
                }
                
                spawnedIcons.Add(icon);
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
