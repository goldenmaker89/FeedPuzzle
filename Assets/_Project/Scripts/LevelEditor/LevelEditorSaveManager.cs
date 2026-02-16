using UnityEngine;
using UnityEngine.UI;
using System.IO;
using Gameplay.Grid;

namespace LevelEditor
{
    public class LevelEditorSaveManager : MonoBehaviour
    {
        [SerializeField] private LevelEditorGridManager gridManager;
        [SerializeField] private Button saveButton;
        [SerializeField] private int currentLevelNumber = 1;

        private void Start()
        {
            if (saveButton != null)
            {
                saveButton.onClick.AddListener(SaveLevel);
            }
        }

        public void SaveLevel()
        {
            if (gridManager == null)
            {
                Debug.LogError("[LevelEditorSaveManager] GridManager is not assigned!");
                return;
            }

            LevelConfig config = new LevelConfig();
            config.levelNumber = currentLevelNumber;
            config.width = gridManager.Width;
            config.height = gridManager.Height;

            for (int x = 0; x < gridManager.Width; x++)
            {
                for (int y = 0; y < gridManager.Height; y++)
                {
                    Cell cell = gridManager.GetCell(x, y);
                    if (cell != null)
                    {
                        CellData cellData = new CellData();
                        cellData.x = x;
                        cellData.y = y;
                        cellData.colorId = cell.ColorId;
                        cellData.isOccupied = cell.IsOccupied;
                        config.cells.Add(cellData);
                    }
                }
            }

            string json = JsonUtility.ToJson(config, true);
            string fileName = $"Level_{currentLevelNumber}.json";
            string path;

            #if UNITY_EDITOR
            path = Path.Combine(Application.dataPath, "_Project/Data/Levels", fileName);
            #else
            path = Path.Combine(Application.persistentDataPath, fileName);
            #endif

            // Ensure directory exists
            string directory = Path.GetDirectoryName(path);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllText(path, json);
            Debug.Log($"[LevelEditorSaveManager] Level saved to {path}");
            
            #if UNITY_EDITOR
            UnityEditor.AssetDatabase.Refresh();
            #endif
        }
    }
}
