using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class DebugDragPaint
{
    public static string Execute()
    {
        var toolManager = Object.FindFirstObjectByType<LevelEditor.LevelEditorToolManager>();
        var gridManager = Object.FindFirstObjectByType<LevelEditor.LevelEditorGridManager>();
        
        if (toolManager == null) return "ERROR: ToolManager not found";
        if (gridManager == null) return "ERROR: GridManager not found";

        string result = "";
        result += $"ToolManager: Tool={toolManager.CurrentTool}, ColorId={toolManager.SelectedColorId}\n";
        result += $"GridManager: IsInitialized={gridManager.IsInitialized}\n";

        // Simulate a raycast at the center of the grid area
        // First, find the grid container's screen position
        var gridContainerField = typeof(LevelEditor.LevelEditorGridManager).GetField("gridContainer", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var gridContainer = gridContainerField?.GetValue(gridManager) as Transform;
        
        if (gridContainer != null)
        {
            var rect = gridContainer.GetComponent<RectTransform>();
            // For ScreenSpaceOverlay, world position = screen position
            Vector2 centerPos = rect.position;
            result += $"GridContainer center screen pos: {centerPos}\n";

            // Try EventSystem raycast
            if (EventSystem.current != null)
            {
                var pointerData = new PointerEventData(EventSystem.current);
                pointerData.position = centerPos;
                var results = new List<RaycastResult>();
                EventSystem.current.RaycastAll(pointerData, results);
                
                result += $"Raycast at grid center found {results.Count} hits:\n";
                for (int i = 0; i < Mathf.Min(results.Count, 5); i++)
                {
                    var cellView = results[i].gameObject.GetComponent<LevelEditor.LevelEditorCellView>();
                    result += $"  [{i}] {results[i].gameObject.name} - hasCellView={cellView != null}";
                    if (cellView != null)
                        result += $" X={cellView.X}, Y={cellView.Y}";
                    result += "\n";
                }

                // Also try a position that should be on a cell
                var cellViewsField = typeof(LevelEditor.LevelEditorGridManager).GetField("cellViews", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var cellViews = cellViewsField?.GetValue(gridManager) as LevelEditor.LevelEditorCellView[,];
                if (cellViews != null && cellViews[15, 17] != null)
                {
                    var cellRect = cellViews[15, 17].GetComponent<RectTransform>();
                    Vector2 cellScreenPos = cellRect.position;
                    result += $"\nCell(15,17) screen pos: {cellScreenPos}\n";
                    
                    pointerData.position = cellScreenPos;
                    results.Clear();
                    EventSystem.current.RaycastAll(pointerData, results);
                    result += $"Raycast at Cell(15,17) found {results.Count} hits:\n";
                    for (int i = 0; i < Mathf.Min(results.Count, 5); i++)
                    {
                        var cv = results[i].gameObject.GetComponent<LevelEditor.LevelEditorCellView>();
                        result += $"  [{i}] {results[i].gameObject.name} - hasCellView={cv != null}";
                        if (cv != null)
                            result += $" X={cv.X}, Y={cv.Y}";
                        result += "\n";
                    }
                }
            }
            else
            {
                result += "EventSystem.current is NULL!\n";
            }
        }

        return result;
    }
}
