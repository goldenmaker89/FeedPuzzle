using UnityEngine;

namespace Gameplay.Grid
{
    public class Cell
    {
        public int X { get; private set; }
        public int Y { get; private set; }
        public bool IsOccupied { get; set; }
        public int ColorId { get; set; } // 0 = empty, 1-8 = colors

        public Cell(int x, int y)
        {
            X = x;
            Y = y;
            IsOccupied = false;
            ColorId = 0;
        }

        public void Clear()
        {
            ColorId = 0;
            IsOccupied = false;
        }
    }
}
