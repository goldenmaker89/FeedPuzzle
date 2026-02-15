using UnityEngine;
using System.Collections.Generic;

namespace Gameplay.Mechanics
{
    /// <summary>
    /// Base class for anything that moves along the conveyor path.
    /// The path is a list of waypoints. Each waypoint corresponds to a cell edge position.
    /// </summary>
    public abstract class ConveyorItem : MonoBehaviour
    {
        protected List<Vector3> path;
        protected float speed;
        protected int pathIndex;
        protected bool isMoving;
        protected Vector3 targetPosition;
        protected bool pathCompleted;

        public bool IsMoving => isMoving;
        public bool PathCompleted => pathCompleted;

        /// <summary>
        /// Override in subclasses to signal that this item should be removed
        /// from the conveyor immediately (e.g. capacity fully depleted).
        /// </summary>
        public virtual bool ShouldBeRemovedImmediately => false;

        public virtual void StartPath(List<Vector3> pathPoints, float speed)
        {
            this.path = new List<Vector3>(pathPoints); // copy to avoid shared reference issues
            this.speed = speed;
            this.pathIndex = 0;
            this.pathCompleted = false;

            if (path != null && path.Count > 0)
            {
                transform.position = path[0];
                if (path.Count > 1)
                {
                    pathIndex = 1;
                    targetPosition = path[1];
                    isMoving = true;
                }
            }
        }

        protected virtual void Update()
        {
            if (isMoving && path != null)
            {
                MoveAlongPath();
            }
        }

        protected virtual void MoveAlongPath()
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);

            if (Vector3.Distance(transform.position, targetPosition) < 0.01f)
            {
                transform.position = targetPosition; // snap exactly

                // Arrived at waypoint - notify subclass
                OnWaypointReached(pathIndex);

                // Check if the item should be removed immediately (e.g. capacity depleted)
                if (ShouldBeRemovedImmediately)
                {
                    isMoving = false;
                    pathCompleted = true;
                    OnPathComplete();
                    return;
                }

                pathIndex++;
                if (pathIndex >= path.Count)
                {
                    // Completed full loop
                    isMoving = false;
                    pathCompleted = true;
                    OnPathComplete();
                }
                else
                {
                    targetPosition = path[pathIndex];
                }
            }
        }

        /// <summary>
        /// Called each time the item reaches a waypoint. Override to perform scanning.
        /// </summary>
        protected virtual void OnWaypointReached(int waypointIndex) { }

        public abstract void OnPathComplete();

        public virtual bool IsValid() => true;
    }
}
