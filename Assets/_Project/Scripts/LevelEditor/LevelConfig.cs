using System;
using System.Collections.Generic;
using UnityEngine;

namespace LevelEditor
{
    [Serializable]
    public class LevelConfig
    {
        public int levelNumber;
        public int width;
        public int height;
        public List<CellData> cells = new List<CellData>();
    }

    [Serializable]
    public class CellData
    {
        public int x;
        public int y;
        public int colorId;
        public bool isOccupied;
    }
}
