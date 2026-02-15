using UnityEngine;
using System.Collections.Generic;
using Gameplay.Grid;
using Gameplay.Mechanics;

namespace Gameplay.Units
{
    /// <summary>
    /// Container for a linked pair of UnitControllers.
    /// Moves as a single entity on the conveyor. Both children scan/attack independently.
    /// The pair is only destroyed when BOTH units have capacity == 0.
    /// </summary>
    public class LinkedUnitController : ConveyorItem
    {
        [SerializeField] private UnitController unitA;
        [SerializeField] private UnitController unitB;
        [SerializeField] private float spacing = 0.5f;

        public UnitController UnitA => unitA;
        public UnitController UnitB => unitB;

        public bool IsActive => unitA != null || unitB != null;

        /// <summary>
        /// Linked pair should be removed immediately when both units have capacity depleted.
        /// </summary>
        public override bool ShouldBeRemovedImmediately
        {
            get
            {
                bool aDead = unitA == null || unitA.Capacity <= 0;
                bool bDead = unitB == null || unitB.Capacity <= 0;
                return aDead && bDead;
            }
        }

        public void Initialize(UnitController a, UnitController b)
        {
            unitA = a;
            unitB = b;

            // Parent them to this container
            unitA.transform.SetParent(transform);
            unitB.transform.SetParent(transform);

            // Position them relative to container
            unitA.transform.localPosition = new Vector3(-spacing * 0.5f, 0, 0);
            unitB.transform.localPosition = new Vector3(spacing * 0.5f, 0, 0);

            unitA.SetState(UnitState.InQueue);
            unitB.SetState(UnitState.InQueue);
        }

        public void SetConveyorData(GridManager gm, List<ConveyorBelt.WaypointInfo> infos)
        {
            if (unitA != null) unitA.SetConveyorData(gm, infos);
            if (unitB != null) unitB.SetConveyorData(gm, infos);
        }

        public override void StartPath(List<Vector3> pathPoints, float speed)
        {
            base.StartPath(pathPoints, speed);
            if (unitA != null) unitA.SetState(UnitState.OnConveyor);
            if (unitB != null) unitB.SetState(UnitState.OnConveyor);
        }

        protected override void OnWaypointReached(int waypointIndex)
        {
            // Delegate scanning to each child unit
            if (unitA != null && unitA.Capacity > 0)
                unitA.PerformScan(waypointIndex);
            if (unitB != null && unitB.Capacity > 0)
                unitB.PerformScan(waypointIndex);
        }

        /// <summary>
        /// Called when the linked pair finishes a full loop.
        /// Do NOT destroy here - let FlowManager handle the lifecycle.
        /// </summary>
        public override void OnPathComplete()
        {
            if (unitA != null) unitA.SetState(UnitState.Returning);
            if (unitB != null) unitB.SetState(UnitState.Returning);
            isMoving = false;
            // FlowManager will handle destruction or docking via OnItemCompletedLoop
        }

        public override bool IsValid()
        {
            return unitA != null || unitB != null;
        }

        /// <summary>
        /// Total remaining capacity of the pair.
        /// </summary>
        public int TotalCapacity
        {
            get
            {
                int total = 0;
                if (unitA != null) total += unitA.Capacity;
                if (unitB != null) total += unitB.Capacity;
                return total;
            }
        }

        /// <summary>
        /// How many landing strip slots this pair needs (always 2 if returning).
        /// </summary>
        public int RequiredSlots => 2;
    }
}
