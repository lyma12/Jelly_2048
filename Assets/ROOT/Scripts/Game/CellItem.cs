using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Watermelon.JellyMerge
{
    [System.Serializable]
    public class CellItem
    {
        [SerializeField, ReadOnly]
        private ColorId colorID;
        public ColorId ColorID
        {
            get { return colorID; }
        }

        [SerializeField]
        private CellBehaviour cellBehaviour;
        public CellBehaviour Cell
        {
            get { return cellBehaviour; }
        }
        
        public bool IsEmpty => cellBehaviour == null;
        public Index2 CellIndex {
            get;
            set;
        }

        public CellItem()
        {
            colorID = ColorId.None;
            cellBehaviour = null;
        }

        public void InitColoredItem(CellBehaviour cell)
        {
            cellBehaviour = cell;
            colorID = cell.ColorID;
        }
        public void Clear()
        {
            cellBehaviour = null;
            colorID = ColorId.None;
        }
    }
}