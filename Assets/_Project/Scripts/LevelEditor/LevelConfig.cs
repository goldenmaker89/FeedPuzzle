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
        public List<UnitStackData> unitStacks = new List<UnitStackData>();
    }

    [Serializable]
    public class CellData
    {
        public int x;
        public int y;
        public int colorId;
        public bool isOccupied;
    }

    [Serializable]
    public class UnitStackData
    {
        public int slotIndex;
        public List<UnitData> units = new List<UnitData>();
    }

    [Serializable]
    public class UnitData
    {
        public int colorId;
        public int hp;
    }
}
