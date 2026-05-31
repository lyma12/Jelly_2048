using System.Collections;
using System.Collections.Generic;
using ROOT.Scripts.Data;
using UnityEngine;
using Watermelon.JellyMerge;
namespace ROOT.Scripts.Controllers
{
    public class BoardController : MonoBehaviour
    {
        [SerializeField] private Vector2Int gridSize = new Vector2Int(10, 10);
        [SerializeField] private GameObject cellPrefab;
        [SerializeField] private float cellSize = 0.8f;
        [SerializeField] private LevelConfig levelConfig;
        private CellItem[,] cells;
        [SerializeField] private CellsPool cellBehaviourPool; // Pool trong cấu trúc này không lấy lại được mỗi lần destroy sẽ là setactive nó đi
        private List<CellBehaviour> cellBehaviours = new List<CellBehaviour>();
        public Vector2Int LastMove = Vector2Int.zero;
        public void ClearBoard()
        {
            for (var i = 0; i < gridSize.x; i++)
            {
                for (var j = 0; j < gridSize.y; j++)
                {
                    cells[i, j].Clear();
                }
            }
            cellBehaviourPool.ReturnToPoolEverything();
        }
        public void Init()
        {
            cells = new CellItem[gridSize.x, gridSize.y];
            var startPos = new Vector3(-gridSize.x * 0.5f, 0, gridSize.y * 0.5f);
            for (var i = 0; i < gridSize.x; i++)
            {
                for (var j = 0; j < gridSize.y; j++)
                {
                    var newCell = new CellItem();
                    newCell.CellIndex = new Index2(i, j);
                    cells[i, j] = newCell;
                    var visualCell = Instantiate(cellPrefab, transform, true);
                    visualCell.transform.localPosition = startPos + new Vector3(i * cellSize, 0, -j * cellSize);
                }
            }
        }
        public void StartGame()
        {
            ClearBoard();
            SpawnRandomTile();
            SpawnRandomTile();
        }
        private void SpawnRandomTile()
        {
            var (color, position) = GetRandomCellColor();

            var cell = GetCell(position);
            if (cell == null || !cell.IsEmpty)
            {
                position = GetSlotEasy(); // fallback
                cell = GetCell(position);
            }

            SpawnJelly(color, position);
        }
        private void SpawnJelly(ColorId color, Index2 position)
        {
            var cell = GetCell(position);
            if (cell == null)
            {
                Debug.LogError("");
            }
            else
            {
                var worldPosition = GetCellWorldPosition(cell);
                var newCellBehaviour = cellBehaviourPool.GetPooledObject(worldPosition).GetComponent<CellBehaviour>();
                newCellBehaviour.TF.SetParent(transform);
                newCellBehaviour.Init(color, CellBehaviour.GraphicsType.Simple);
            }
        }
        public CellItem GetCell(Index2 index)
        {
            if (index.x < 0 || index.y < 0 || index.x >= gridSize.x || index.y >= gridSize.y)
            {
                return null;
            }
            return cells[index.x, index.y];
        }
        public CellItem GetCellAdjacent(CellItem cell, Index2 dir)
        {
            var index = cell.CellIndex + dir;

            if (index.x < 0 || index.x >= gridSize.x)
                return null;

            if (index.y < 0 || index.y >= gridSize.y)
                return null;

            return cells[index.x, index.y];
        }
        private Index2 GetGridPosition(Vector3 worldPosition)
        {
            var startPos = new Vector3(
                -gridSize.x * 0.5f,
                0,
                gridSize.y * 0.5f
            );

            Vector3 local = transform.InverseTransformPoint(worldPosition) - startPos;

            int x = Mathf.RoundToInt(local.x / cellSize);
            int y = Mathf.RoundToInt(local.z / cellSize);

            return new Index2(x, y);
        }
        public Vector3 GetCellWorldPosition(CellItem cell)
        {
            var startPos = new Vector3(
                -gridSize.x * 0.5f,
                0,
                gridSize.y * 0.5f);

            return transform.TransformPoint(
                startPos +
                new Vector3(
                    cell.CellIndex.x * cellSize,
                    0,
                    cell.CellIndex.y * cellSize));
        }
        public void Move(Index2 dir)
        {
            bool changed = false;

            List<CellItem> ordered = GetTraversalOrder(dir);

            foreach (var cell in ordered)
            {
                if (cell.IsEmpty) continue;

                changed |= MoveCell(cell, dir);
            }

            if (changed)
            {
                LastMove = dir.ToVector2Int();
                StartCoroutine(WaitForChanges());
            }
        }
        private bool waiting = false;
        private IEnumerator WaitForChanges()
        {
            waiting = true;

            yield return new WaitForSeconds(0.1f);

            waiting = false;

            foreach (var tile in cellBehaviours) {
                tile.locked = false;
            }

            if (cellBehaviours.Count < gridSize.x * gridSize.y)
            {
                SpawnRandomTile();
            }

            if (CheckForGameOver()) {
                GameController.GameOver();
            }
        }
        private bool CheckForGameOver()
        {
            if (cellBehaviours.Count < gridSize.x * gridSize.y)
                return false;

            foreach (var tile in cellBehaviours)
            {
                Index2 index = GetGridPosition(tile.TF.position);
                CellItem current = GetCell(index);

                if (current == null) continue;

                CellItem up = GetAdjacentCell(current, Vector2Int.up);
                CellItem down = GetAdjacentCell(current, Vector2Int.down);
                CellItem left = GetAdjacentCell(current, Vector2Int.left);
                CellItem right = GetAdjacentCell(current, Vector2Int.right);

                if (up != null && CanMerge(current, up)) return false;
                if (down != null && CanMerge(current, down)) return false;
                if (left != null && CanMerge(current, left)) return false;
                if (right != null && CanMerge(current, right)) return false;
            }

            return true;
        }
        private CellItem GetAdjacentCell(CellItem cell, Vector2Int dir)
        {
            Index2 index = cell.CellIndex + new Index2(dir.x, dir.y);

            if (!IsInside(index))
                return null;

            return cells[index.x, index.y];
        }
        private bool MoveCell(CellItem cell, Index2 dir)
        {
            Index2 current = cell.CellIndex;
            Index2 next = current + dir;

            CellItem lastValid = null;

            while (IsInside(next))
            {
                CellItem target = cells[next.x, next.y];
                if (!target.IsEmpty)
                {
                    if (CanMerge(cell, target))
                    {
                        Merge(cell, target);
                        return true;
                    }

                    break;
                }

                lastValid = target;
                next += dir;
            }

            if (lastValid != null)
            {
                Swap(cell, lastValid);
                return true;
            }

            return false;
        }
        
        private bool IsInside(Index2 i)
        {
            return i.x >= 0 && i.y >= 0 &&
                   i.x < gridSize.x &&
                   i.y < gridSize.y;
        }
        private bool CanMerge(CellItem cell, CellItem target)
        {
            if (cell.IsEmpty || target.IsEmpty) return false;
            if (cell.ColorID == target.ColorID)
            {
                return cell.ColorID != ColorId.Color8;
            }
            return false;
        }
        private void Merge(CellItem a, CellItem b)
        {
            var nextColor = GetNextColor(a.ColorID);
            a.Cell.gameObject.SetActive(false);
            a.Clear();
            b.Cell.Merge(nextColor);
            b.InitColoredItem(b.Cell);
        }
        private ColorId GetNextColor(ColorId currentColor)
        {
            int index = (int)currentColor;

            index = Mathf.Clamp(
                index + 1,
                0,
                System.Enum.GetValues(typeof(ColorId)).Length - 1
            );

            return (ColorId)index;
        }
        private void Swap(CellItem from, CellItem to)
        {
            var cellBehaviour = from.Cell;
            to.InitColoredItem(cellBehaviour);
            from.Clear();
        }
        
        private List<CellItem> GetTraversalOrder(Index2 dir)
        {
            List<CellItem> list = new List<CellItem>();

            bool xAxis = dir.x != 0;
            bool positive = (xAxis ? dir.x : dir.y) > 0;

            for (int i = 0; i < gridSize.x; i++)
            {
                for (int j = 0; j < gridSize.y; j++)
                {
                    int x = xAxis ? (positive ? gridSize.x - 1 - i : i) : i;
                    int y = xAxis ? j : (positive ? gridSize.y - 1 - j : j);

                    list.Add(cells[x, y]);
                }
            }

            return list;
        }
        

        #region Logicc

        private int CountOccupiedNeighbours(Index2 index)
        {
            var count = 0;

            Index2[] dirs =
            {
                Index2.left,
                Index2.right,
                Index2.up,
                Index2.down
            };

            foreach (var dir in dirs)
            {
                var neighbour = index + dir;

                if (neighbour.x < 0 || neighbour.x >= gridSize.x)
                    continue;

                if (neighbour.y < 0 || neighbour.y >= gridSize.y)
                    continue;

                if (!cells[neighbour.x, neighbour.y].IsEmpty)
                    count++;
            }

            return count;
        }
        private Index2 GetSlotEasy()
        {
            List<Index2> emptySlots = GetEmptySlots();

            emptySlots.Sort((a, b) =>
                CountOccupiedNeighbours(b)
                    .CompareTo(CountOccupiedNeighbours(a)));

            return emptySlots[0];
        }
        private Index2 GetSlotHard()
        {
            List<Index2> emptySlots = GetEmptySlots();

            emptySlots.Sort((a, b) =>
                CountOccupiedNeighbours(a)
                    .CompareTo(CountOccupiedNeighbours(b)));

            return emptySlots[0];
        }
        private List<Index2> GetEmptySlots()
        {
            List<Index2> result = new List<Index2>();

            for (var x = 0; x < gridSize.x; x++)
            {
                for (var y = 0; y < gridSize.y; y++)
                {
                    if (cells[x, y].IsEmpty)
                    {
                        result.Add(new Index2(x, y));
                    }
                }
            }

            return result;
        }
        private (ColorId, Index2) GetRandomCellColor()
        {
            var config =
                GetLevelConfigByScore(GameController.CurrentScore);

            var slot =
                GetSlotEmpty(config.spawnRule);

            var color =
                GetColorSpawn(config.colorWeight);

            return (color, slot);
        }
        private Index2 GetSlotEmpty(SpawnDifficulty spawnRule)
        {
            switch (spawnRule)
            {
                case SpawnDifficulty.Easy:
                case SpawnDifficulty.Normal:
                    return GetSlotEasy();
                case SpawnDifficulty.Hard:
                    return GetSlotHard();
                default:
                    return GetSlotEasy();

            }
        }
        private ColorId GetColorSpawn(List<ColorSpawnData> rules)
        {
            int totalWeight = 0;

            foreach (var rule in rules)
                totalWeight += rule.Weight;

            int random =
                UnityEngine.Random.Range(0, totalWeight);

            int current = 0;

            foreach (var rule in rules)
            {
                current += rule.Weight;

                if (random < current)
                    return rule.ColorId;
            }

            return rules[0].ColorId;
        }
        private LevelConfigData GetLevelConfigByScore(int score)
        {
            var result = levelConfig.LevelConfigDatas[0];
            for (var i = 0; i < levelConfig.LevelConfigDatas.Count; i++)
            {
                if (levelConfig.LevelConfigDatas[i].levelScore >= score)
                {
                    result = levelConfig.LevelConfigDatas[i];
                    break;
                }
            }
            return result;
        }

        #endregion
    }
}