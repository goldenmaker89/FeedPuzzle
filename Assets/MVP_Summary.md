# Core Gameplay MVP Implementation Summary

## Implemented Features

### 1. Grid System
- **Logic**: `GridManager.cs` implements a 2D grid of `Cell` objects.
- **Coordinates**: `GetWorldPosition` maps grid coordinates (x, y) to world space.
- **Raycast**: `GetFirstVisibleCell` implements the "first visible block" logic. It calculates the scan direction based on the unit's position relative to the grid center and iterates through the relevant row or column to find the first occupied cell.

### 2. Unit & Conveyor Movement
- **Path System**: `ConveyorBelt.cs` generates a rectangular path around the grid based on grid dimensions and `pathOffset`.
- **FSM**: `UnitController.cs` implements the state machine:
  - `InQueue`: Initial state.
  - `OnConveyor`: Moving along the path.
  - `Attacking`: Logic handled in `Update` while moving (checks for targets).
  - `Returning`: State after path completion or capacity depletion.
  - `InLandingStrip`: Docked state.
- **Capacity**: Units have `capacity` which decreases upon successful attacks.

### 3. Linked Units
- **Logic**: `LinkedUnitController.cs` acts as a container for two `UnitController`s.
- **Movement**: The container moves on the conveyor, carrying the child units.
- **Landing**: `LandingStrip.cs` checks if there is enough space (2 slots) for the pair. If one unit is dead (capacity 0), it is destroyed, and the survivor docks alone (taking 1 slot). If both are alive, they require 2 slots.

### 4. Win/Loss Condition
- **Traffic Jam**: `ConveyorBelt.cs` checks if a returning unit (or pair) can dock. If the `LandingStrip` is full, it triggers `WinLossManager.OnTrafficJam`.

## How to Verify

1.  **Scene Setup**:
    -   Open `Assets/_Project/Scenes/Gameplay.unity` (or use `TestSceneSetup` in a new scene).
    -   Ensure `GridManager`, `ConveyorBelt`, and `LandingStrip` are present in the scene.
    -   `TestSceneSetup` script (on an object) automatically spawns test units.
    -   **Prefabs**: `UnitController` and `LinkedUnitController` prefabs have been created in `Assets/_Project/Prefabs/Gameplay/` and assigned to `TestSceneSetup`.
    -   **Camera**: `TestSceneSetup` automatically centers the camera on the grid.

2.  **Visual Verification**:
    -   **Movement**: Units should move around the grid.
    -   **Attacking**: Units should change color or disappear (cells) when they "attack" matching colors. (Debug logs show attacks).
    -   **Landing**: Units should stop at the `LandingStrip` positions if they survive.
    -   **Linked Units**: Pairs should move together. If they reach the end, they should dock together if space permits.

3.  **Traffic Jam Test**:
    -   Reduce `LandingStrip` capacity to 1.
    -   Spawn a Linked Pair (requires 2 slots).
    -   Wait for them to return.
    -   Observe "Game Over" log or behavior.

## Next Steps
-   Implement visual feedback for attacks (projectiles/beams).
-   Add UI for Win/Loss screens.
-   Refine "Linked Unit" destruction visuals (currently they just vanish or detach).
