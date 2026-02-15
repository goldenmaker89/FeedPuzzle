using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using Gameplay.Units;
using Gameplay.Mechanics;

namespace Core
{
    /// <summary>
    /// Handles player input using the new Input System.
    /// Clicking on units in Base or LandingStrip.
    /// Raycasts into the 2D world to find clickable units.
    /// </summary>
    public class InputManager : MonoBehaviour
    {
        public static InputManager Instance { get; private set; }

        [SerializeField] private BaseManager baseManager;
        [SerializeField] private LandingStrip landingStrip;
        [SerializeField] private Camera mainCamera;

        private Mouse mouse;
        private Touchscreen touchscreen;

        public event System.Action OnTap;

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
        }

        public void Initialize(BaseManager bm, LandingStrip ls)
        {
            baseManager = bm;
            landingStrip = ls;
            if (mainCamera == null)
                mainCamera = Camera.main;
        }

        private void Update()
        {
            if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameState.Playing)
                return;

            // Mouse input
            if (mouse == null)
                mouse = Mouse.current;

            if (mouse != null && mouse.leftButton.wasPressedThisFrame)
            {
                if (IsPointerOverUI()) return;
                Vector2 screenPos = mouse.position.ReadValue();
                HandleClick(screenPos);
                return;
            }

            // Touch input
            if (touchscreen == null)
                touchscreen = Touchscreen.current;

            if (touchscreen != null && touchscreen.primaryTouch.press.wasPressedThisFrame)
            {
                if (IsPointerOverUI()) return;
                Vector2 screenPos = touchscreen.primaryTouch.position.ReadValue();
                HandleClick(screenPos);
            }
        }

        private void HandleClick(Vector2 screenPos)
        {
            if (mainCamera == null) return;

            Vector3 worldPos = mainCamera.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, 0));
            worldPos.z = 0;

            // Raycast to find a UnitController via collider
            RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector2.zero);
            if (hit.collider != null)
            {
                UnitController unit = hit.collider.GetComponent<UnitController>();
                if (unit != null)
                {
                    TryInteractWithUnit(unit);
                    return;
                }
            }

            // Fallback: check proximity to base slot front units
            float clickRadius = 0.3f;
            if (baseManager != null)
            {
                float bestDist = float.MaxValue;
                int bestSlot = -1;

                for (int i = 0; i < baseManager.SlotCount; i++)
                {
                    UnitController frontUnit = baseManager.GetFrontUnit(i);
                    if (frontUnit != null)
                    {
                        float dist = Vector2.Distance(worldPos, frontUnit.transform.position);
                        if (dist < clickRadius && dist < bestDist)
                        {
                            bestDist = dist;
                            bestSlot = i;
                        }
                    }
                }

                if (bestSlot >= 0)
                {
                    baseManager.TryLaunchFromSlot(bestSlot);
                    return;
                }
            }

            // Check proximity to landing strip units
            if (landingStrip != null)
            {
                var docked = landingStrip.GetDockedUnits();
                float bestDist = float.MaxValue;
                UnitController bestUnit = null;

                for (int i = 0; i < docked.Count; i++)
                {
                    if (docked[i] != null)
                    {
                        float dist = Vector2.Distance(worldPos, docked[i].transform.position);
                        if (dist < clickRadius && dist < bestDist)
                        {
                            bestDist = dist;
                            bestUnit = docked[i];
                        }
                    }
                }

                if (bestUnit != null)
                {
                    landingStrip.TryRelaunchByUnit(bestUnit);
                    return;
                }
            }

            OnTap?.Invoke();
        }

        private void TryInteractWithUnit(UnitController unit)
        {
            if (unit.State == UnitState.InQueue && baseManager != null)
            {
                baseManager.TryLaunchByUnit(unit);
            }
            else if (unit.State == UnitState.InLandingStrip && landingStrip != null)
            {
                landingStrip.TryRelaunchByUnit(unit);
            }
        }

        private bool IsPointerOverUI()
        {
            if (EventSystem.current == null)
                return false;
            return EventSystem.current.IsPointerOverGameObject();
        }
    }
}
