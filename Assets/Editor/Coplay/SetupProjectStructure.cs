using System.IO;
using UnityEditor;
using UnityEngine;

public class SetupProjectStructure
{
    public static string Execute()
    {
        string projectRoot = Application.dataPath + "/_Project";
        string[] folders = new string[]
        {
            "Animations/UI",
            "Animations/Units",
            "Art/UI",
            "Art/Gameplay",
            "Art/VFX",
            "Art/Fonts",
            "Audio/Music",
            "Audio/SFX",
            "Data/Levels",
            "Data/Balance",
            "Data/Settings",
            "Prefabs/Core",
            "Prefabs/UI",
            "Prefabs/Gameplay",
            "Prefabs/VFX",
            "Scenes",
            "Scripts/App",
            "Scripts/Core/FSM",
            "Scripts/Core/ObjectPool",
            "Scripts/Core/Events",
            "Scripts/Gameplay/Grid/Pathfinding",
            "Scripts/Gameplay/Units",
            "Scripts/Gameplay/Mechanics",
            "Scripts/Gameplay/Level",
            "Scripts/Meta/UI/Popups",
            "Scripts/Meta/UI/HUD",
            "Scripts/Meta/Map",
            "Scripts/Meta/Shop",
            "Scripts/Services/Analytics",
            "Scripts/Services/Ads",
            "Scripts/Services/IAP",
            "Scripts/Services/SaveSystem",
            "Scripts/Services/Audio",
            "Scripts/Utils/Extensions",
            "Scripts/Utils/Coroutines",
            "Scripts/Editor/LevelEditor"
        };

        foreach (string folder in folders)
        {
            string path = Path.Combine(projectRoot, folder);
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        AssetDatabase.Refresh();
        return "Project structure created successfully.";
    }
}
