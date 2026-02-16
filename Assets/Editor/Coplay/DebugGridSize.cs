using UnityEngine;
using UnityEditor;

public class DebugGridSize
{
    public static string Execute()
    {
        // Get Canvas size
        var canvas = GameObject.Find("Canvas");
        var canvasRect = canvas.GetComponent<RectTransform>();
        
        // Get LevelCanvas
        var levelCanvas = canvas.transform.Find("LevelCanvas");
        var lcRect = levelCanvas.GetComponent<RectTransform>();
        
        // Get GridContainer
        var gridContainer = levelCanvas.Find("GridContainer");
        var gcRect = gridContainer.GetComponent<RectTransform>();
        
        // Force layout rebuild to get accurate sizes
        Canvas.ForceUpdateCanvases();
        
        return $"Canvas: size={canvasRect.rect.size}\n" +
               $"LevelCanvas: size={lcRect.rect.size}, anchoredPos={lcRect.anchoredPosition}, sizeDelta={lcRect.sizeDelta}, anchorMin={lcRect.anchorMin}, anchorMax={lcRect.anchorMax}\n" +
               $"GridContainer: size={gcRect.rect.size}, anchoredPos={gcRect.anchoredPosition}, sizeDelta={gcRect.sizeDelta}, anchorMin={gcRect.anchorMin}, anchorMax={gcRect.anchorMax}\n" +
               $"Grid total: width={30*11+29*1}={359}, height={35*11+34*1}={419}";
    }
}
