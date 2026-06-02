using System.Collections;
using System.Collections.Generic;
using ROOT.Scripts.Data;
using UnityEngine;
using Watermelon;
using Watermelon.JellyMerge;
namespace ROOT.Scripts.Controllers
{
    public class BoardController : MonoBehaviour
    {
        [Header("References")]
        public MeshFilter borderMeshFilter;
        
        [SerializeField] private Vector2Int gridSize = new Vector2Int(10, 10);
        [SerializeField] private float cellSize = 0.8f;
        [SerializeField] private LevelConfig levelConfig;
        private CellItem[,] cells;
        [SerializeField] private CellsPool cellBehaviourPool; // Pool trong cấu trúc này không lấy lại được mỗi lần destroy sẽ là setactive nó đi
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
            for (var i = 0; i < gridSize.x; i++)
            {
                for (var j = 0; j < gridSize.y; j++)
                {
                    var newCell = new CellItem();
                    newCell.CellIndex = new Index2(i, j);
                    cells[i, j] = newCell;
                }
            }
            var boardWorldSize = new Vector2((gridSize.x + 1) * cellSize, (gridSize.y + 1) * cellSize);
            CameraController.Init(transform.position, boardWorldSize);

            GenerateBorder(CameraController.FrustrumSize);
        }
        public void StartGame()
        {
            ClearBoard();
            SpawnRandomTile();
            SpawnRandomTile();
        }
        private void SpawnRandomTile()
        {
            if (GetOccupiedCount() >= gridSize.x * gridSize.y) return;

            var (color, position) = GetRandomCellColor();

            var cell = GetCell(position);
            if (cell == null || !cell.IsEmpty)
            {
                var empty = GetEmptySlots();
                if (empty.Count == 0) return;
                position = GetSlotEasy();
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
                newCellBehaviour.Init(color);
                newCellBehaviour.PlaySpawnAnimation();
                cell.InitColoredItem(newCellBehaviour);
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
            var origin = GridOrigin();
            Vector3 local = transform.InverseTransformPoint(worldPosition) - origin;
            int x = Mathf.RoundToInt(local.x / cellSize);
            int y = Mathf.RoundToInt(local.z / cellSize);
            return new Index2(x, y);
        }
        public Vector3 GetCellWorldPosition(CellItem cell)
        {
            return transform.TransformPoint(GridOrigin() + new Vector3(
                cell.CellIndex.x * cellSize,
                0,
                cell.CellIndex.y * cellSize));
        }
        // (0,0) = bottom-left, y tăng dần lên trên (+Z)
        private Vector3 GridOrigin() => new Vector3(
            -(gridSize.x - 1) * cellSize * 0.5f,
            0,
            -(gridSize.y - 1) * cellSize * 0.5f);
        public void Move(Index2 dir)
        {
            if (waiting) return;

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

            yield return new WaitForSeconds(CellBehaviour.AnimationTime + 0.05f);

            waiting = false;

            // unlock tất cả sau animation để turn tiếp theo có thể merge bình thường
            for (int x = 0; x < gridSize.x; x++)
                for (int y = 0; y < gridSize.y; y++)
                    if (!cells[x, y].IsEmpty && cells[x, y].Cell != null)
                        cells[x, y].Cell.locked = false;

            if (GetOccupiedCount() < gridSize.x * gridSize.y)
                SpawnRandomTile();

            if (CheckForGameOver())
                GameController.GameOver();
        }
        private int GetOccupiedCount()
        {
            int count = 0;
            for (int x = 0; x < gridSize.x; x++)
                for (int y = 0; y < gridSize.y; y++)
                    if (!cells[x, y].IsEmpty) count++;
            return count;
        }
        private bool CheckForGameOver()
        {
            if (GetOccupiedCount() < gridSize.x * gridSize.y)
                return false;

            for (int x = 0; x < gridSize.x; x++)
            {
                for (int y = 0; y < gridSize.y; y++)
                {
                    CellItem current = cells[x, y];
                    if (current.IsEmpty) continue;

                    if (CanMerge(current, GetAdjacentCell(current, Vector2Int.up)))    return false;
                    if (CanMerge(current, GetAdjacentCell(current, Vector2Int.down)))  return false;
                    if (CanMerge(current, GetAdjacentCell(current, Vector2Int.left)))  return false;
                    if (CanMerge(current, GetAdjacentCell(current, Vector2Int.right))) return false;
                }
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
            if (cell == null || target == null) return false;
            if (cell.IsEmpty || target.IsEmpty) return false;
            if (target.Cell != null && target.Cell.locked) return false;
            return cell.ColorID == target.ColorID && cell.ColorID != ColorId.Color8;
        }
        private void Merge(CellItem a, CellItem b)
        {
            var nextColor = GetNextColor(a.ColorID);
            var aCell    = a.Cell;
            var bCell    = b.Cell;
            var target   = GetCellWorldPosition(b);

            // a trượt vào b; khi đến nơi mới đổi màu + sync CellItem + bounce + cộng điểm
            aCell.Move(target, true, () =>
            {
                bCell.Merge(nextColor);
                b.InitColoredItem(bCell);
                bCell.PlayBounce();
                GameController.AddScore(1 << (int)nextColor); // Color2→4, Color3→8 ... Color8→256
            });

            a.Clear();
            b.Cell.locked = true;
        }
        private static readonly int MaxColorIndex =
            System.Enum.GetValues(typeof(ColorId)).Length - 1;

        private ColorId GetNextColor(ColorId currentColor)
        {
            return (ColorId)Mathf.Clamp((int)currentColor + 1, 0, MaxColorIndex);
        }
        private void Swap(CellItem from, CellItem to)
        {
            var cellBehaviour = from.Cell;
            // DOMove đến world position đúng của ô đích, không phụ thuộc cellSize
            var target = GetCellWorldPosition(to);
            cellBehaviour.Move(target, false);
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
        #region Init

        private void GenerateBorder(Vector2 environmentSize)
        {
            float iL = -gridSize.x * cellSize * 0.5f;
            float iR =  gridSize.x * cellSize * 0.5f;
            float iB = -gridSize.y * cellSize * 0.5f;
            float iT =  gridSize.y * cellSize * 0.5f;

            float oL = -environmentSize.x * 0.5f;
            float oR =  environmentSize.x * 0.5f;
            float oB = -environmentSize.y * 0.5f;
            float oT =  environmentSize.y * 0.5f;

            List<Vector3> verts = new List<Vector3>();
            List<int> tris = new List<int>();

            // Flat cap — 4 rows × 4 cols
            // Row 0: z = oT
            verts.Add(new Vector3(oL, 1f, oT)); // 0
            verts.Add(new Vector3(iL, 1f, oT)); // 1
            verts.Add(new Vector3(iR, 1f, oT)); // 2
            verts.Add(new Vector3(oR, 1f, oT)); // 3
            // Row 1: z = iT
            verts.Add(new Vector3(oL, 1f, iT)); // 4
            verts.Add(new Vector3(iL, 1f, iT)); // 5
            verts.Add(new Vector3(iR, 1f, iT)); // 6
            verts.Add(new Vector3(oR, 1f, iT)); // 7
            // Row 2: z = iB
            verts.Add(new Vector3(oL, 1f, iB)); // 8
            verts.Add(new Vector3(iL, 1f, iB)); // 9
            verts.Add(new Vector3(iR, 1f, iB)); // 10
            verts.Add(new Vector3(oR, 1f, iB)); // 11
            // Row 3: z = oB
            verts.Add(new Vector3(oL, 1f, oB)); // 12
            verts.Add(new Vector3(iL, 1f, oB)); // 13
            verts.Add(new Vector3(iR, 1f, oB)); // 14
            verts.Add(new Vector3(oR, 1f, oB)); // 15

            // Top strip (rows 0-1)
            tris.AddRange(new int[] { 0, 5, 4,  0, 1, 5,  1, 2, 6,  1, 6, 5,  2, 3, 7,  2, 7, 6 });
            // Left strip (rows 1-2, cols 0-1)
            tris.AddRange(new int[] { 4, 9, 8,  4, 5, 9 });
            // Right strip (rows 1-2, cols 2-3)
            tris.AddRange(new int[] { 6, 7, 11,  6, 11, 10 });
            // Bottom strip (rows 2-3)
            tris.AddRange(new int[] { 8, 9, 13,  8, 13, 12,  9, 10, 14,  9, 14, 13,  10, 15, 14,  10, 11, 15 });

            // Vertical walls
            verts.Add(new Vector3(iL, 1f, iT)); // 16
            verts.Add(new Vector3(iR, 1f, iT)); // 17
            verts.Add(new Vector3(iL, 0f, iT)); // 18
            verts.Add(new Vector3(iR, 0f, iT)); // 19
            verts.Add(new Vector3(iL, 1f, iB)); // 20
            verts.Add(new Vector3(iR, 1f, iB)); // 21
            verts.Add(new Vector3(iL, 0f, iB)); // 22
            verts.Add(new Vector3(iR, 0f, iB)); // 23

            tris.AddRange(new int[] { 20, 16, 18,  20, 18, 22 }); // left wall  (x = iL)
            tris.AddRange(new int[] { 16, 17, 19,  16, 19, 18 }); // back wall  (z = iT)
            tris.AddRange(new int[] { 17, 21, 19,  19, 21, 23 }); // right wall (x = iR)
            tris.AddRange(new int[] { 21, 20, 22,  21, 22, 23 }); // front wall (z = iB)

            Mesh mesh = new Mesh();
            mesh.vertices = verts.ToArray();
            mesh.triangles = tris.ToArray();
            mesh.RecalculateNormals();

            borderMeshFilter.mesh = mesh;
        }

        #endregion

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
            if (emptySlots.Count == 0)
            {
                Debug.LogError("[BoardController] GetSlotEasy: no empty slots!");
                return Index2.zero;
            }
            emptySlots.Sort((a, b) =>
                CountOccupiedNeighbours(b)
                    .CompareTo(CountOccupiedNeighbours(a)));
            return emptySlots[0];
        }
        private Index2 GetSlotHard()
        {
            List<Index2> emptySlots = GetEmptySlots();
            if (emptySlots.Count == 0)
            {
                Debug.LogError("[BoardController] GetSlotHard: no empty slots!");
                return Index2.zero;
            }
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
            // Tìm config cao nhất có levelScore <= score
            // List phải được sort tăng dần theo levelScore
            var result = levelConfig.LevelConfigDatas[0];
            for (var i = 0; i < levelConfig.LevelConfigDatas.Count; i++)
            {
                if (levelConfig.LevelConfigDatas[i].levelScore <= score)
                    result = levelConfig.LevelConfigDatas[i];
                else
                    break;
            }
            return result;
        }

        #endregion
    }
}