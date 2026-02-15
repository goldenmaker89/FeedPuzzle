using UnityEngine;
using Gameplay.Units;

namespace Gameplay.Mechanics
{
    /// <summary>
    /// Helper for creating linked unit pairs in the Base.
    /// </summary>
    public class LinkedUnitManager : MonoBehaviour
    {
        [SerializeField] private BaseManager baseManager;

        public void Initialize(BaseManager bm)
        {
            baseManager = bm;
        }

        /// <summary>
        /// Create a linked pair in the base across two slots.
        /// </summary>
        public void CreateLinkedPair(int slotA, int colorA, int capA, int slotB, int colorB, int capB)
        {
            if (baseManager == null) return;
            baseManager.CreateLinkedPair(slotA, colorA, capA, slotB, colorB, capB);
        }
    }
}
