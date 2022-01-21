using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#region namespace BattleCity start
namespace BattleCity
{
public struct Cursor : System.IEquatable<Cursor>
{
    public int i;
    public int j;

    public Cursor(int i, int j) {
        this.i = i;
        this.j = j;
    }

    public void Set(int i, int j) {
        this.i = i;
        this.j = j;
    }

    public override bool Equals(object obj) {
        return obj is Cursor cursor && Equals(cursor);
    }

    public bool Equals(Cursor other) {
        return i == other.i &&
                j == other.j;
    }

    public override int GetHashCode() {
        int hashCode = -118560031;
        hashCode = hashCode * -1521134295 + i.GetHashCode();
        hashCode = hashCode * -1521134295 + j.GetHashCode();
        return hashCode;
    }

    public override string ToString() {
        return string.Format("({0}, {1})", i, j);
    }

    public static bool operator==(Cursor left, Cursor right) {
        return left.Equals(right);
    }

    public static bool operator!=(Cursor left, Cursor right) {
        return !(left == right);
    }
}

public class MapGenerator : GenericSingleton<MapGenerator>
{
    // 地图生成元素：0 无  1 基地  2 砖  3 草从  4 公路  5 钢铁  6 水
    public enum MapElement {
        NULL,
        BASE,
        BRICK,
        GRASS,
        ROAD,
        STEEL,
        WATER
    };

    public GameObject elementRoot;
    public GameObject[] pfElements;
    public GameObject pathSign;
    private const int row = 14;
    private const int col = 19;
    private readonly Vector2 posZero = new Vector2(-(float)col / 2f + 0.5f, (float)row / 2f - 0.5f);
    private int[,] mapDatas;

    protected override void Awake() {
        base.Awake();
    }

    public Vector2 CursorToPostion(Cursor cursor) {
        return new Vector2(posZero.x + cursor.j, posZero.y - cursor.i);
    }

    public Cursor PositionToCursor(Vector2 position) {
        position.x -= posZero.x - 0.5f;
        position.y -= posZero.y + 0.5f;
        return new Cursor((int)-position.y, (int)position.x);
    }

    public void GenerateMap() {
        GenerateMapData();
        InstantiateMap();
    }

    private void GenerateMapData() {
        mapDatas = new int[row, col];
        for (int i = 0; i < mapDatas.GetLength(0); i++)
            for (int j = 0; j < mapDatas.GetLength(1); mapDatas[i, j++] = -1);

        Cursor cursor = new Cursor();
        // Player respawn point
        cursor.i = row - 1;
        cursor.j = col / 2 - 2;
        mapDatas[cursor.i, cursor.j] = (int)MapElement.NULL;

        // Enemy respawn point
        cursor.i = 0;
        cursor.j = 0;
        mapDatas[cursor.i, cursor.j] = (int)MapElement.NULL;
        mapDatas[cursor.i, cursor.j += col / 2] = (int)MapElement.NULL;
        mapDatas[cursor.i, cursor.j += col / 2] = (int)MapElement.NULL;

        // Base and its defenses
        cursor.i = row - 1;
        cursor.j = col / 2;
        mapDatas[cursor.i, cursor.j] = (int)MapElement.BASE;
        mapDatas[cursor.i, --cursor.j] = (int)MapElement.BRICK;
        mapDatas[--cursor.i, cursor.j] = (int)MapElement.BRICK;
        mapDatas[cursor.i, ++cursor.j] = (int)MapElement.BRICK;
        mapDatas[cursor.i, ++cursor.j] = (int)MapElement.BRICK;
        mapDatas[++cursor.i, cursor.j] = (int)MapElement.BRICK;

        for (cursor.i = 0; cursor.i < mapDatas.GetLength(0); cursor.i++) {
            for (cursor.j = 0; cursor.j < mapDatas.GetLength(1); cursor.j++) {
                if (mapDatas[cursor.i, cursor.j] == -1) {
                    int sample = Random.Range(0, 100);
                    int elementValue = (int)MapElement.NULL;
                    if (sample < 40)
                        elementValue = (int)MapElement.NULL;
                    else if (sample < 70)
                        elementValue = (int)MapElement.BRICK;
                    else if (sample < 80)
                        elementValue = (int)MapElement.GRASS;
                    else if (sample < 88)
                        elementValue = (int)MapElement.ROAD;
                    else if (sample < 95)
                        elementValue = (int)MapElement.STEEL;
                    else if (sample < 100)
                        elementValue = (int)MapElement.WATER;

                    mapDatas[cursor.i, cursor.j] = elementValue;
                }
            }
        }
    }

    private void InstantiateMap() {
        Cursor cursor = new Cursor();
        for (cursor.i = 0; cursor.i < mapDatas.GetLength(0); cursor.i++) {
            for (cursor.j = 0; cursor.j < mapDatas.GetLength(1); cursor.j++) {
                int data = mapDatas[cursor.i, cursor.j];
                if (data != -1 && data != (int)MapElement.NULL) {
                    GameObject elementInstance = Instantiate(pfElements[data]);
                    elementInstance.transform.position = CursorToPostion(cursor);
                    elementInstance.transform.SetParent(elementRoot.transform, false);
                }
            }
        }
    }

    public Vector2 GetPlayerRespawnPosition() {
        return CursorToPostion(new Cursor(row - 1, col / 2 - 2));
    }

    public Vector2 GetEnemyRespawnPosition() {
        int i = 0;
        int j = (col / 2) * Random.Range(0, 3);
        return CursorToPostion(new Cursor(i, j));
    }

    public Cursor GetBaseCursor() {
        return new Cursor(row - 1, col / 2);
    }

    public Vector2 GetBasePosition() {
        return CursorToPostion(GetBaseCursor());
    }

    // @note    对两个游标使用A*路径搜索
    // @param   start 开始游标  goal 目标游标
    // @return  路径序列，无连通路径时为空
    public List<Cursor> AStarSearch(Cursor start, Cursor goal) {
        if (!(IsReachable(start) && IsReachable(goal))) return new List<Cursor>();
        // 用来预估代价，进行启发式搜索
        int Heuristic(ref Cursor a, ref Cursor b) {
            return Mathf.Abs(a.i - b.i) + Mathf.Abs(a.j - b.j);
        }

        // 已探索区域的边界，值为FCost
        Dictionary<Cursor, int> frontier = new Dictionary<Cursor, int>();
        frontier[start] = Heuristic(ref start, ref goal);

        // 从起点到某一点的最低成本路径的值，即GCost
        Dictionary<Cursor, int> gCosts = new Dictionary<Cursor, int>();
        gCosts[start] = 0;

        // 保存每个点对上一个点的指向
        Dictionary<Cursor, Cursor> cameFrom = new Dictionary<Cursor, Cursor>();

        bool isFounded = false;
        while (frontier.Count != 0) {
            // 获取边界中最低FCost的点
            Cursor cur = new Cursor(-1, -1);
            foreach (KeyValuePair<Cursor, int> kvp in frontier) {
                if (cur.i == -1 && cur.j == -1)
                    cur = kvp.Key;
                if (kvp.Value > frontier[cur])
                    continue;
                if (kvp.Value < frontier[cur] || gCosts[kvp.Key] > gCosts[cur])
                    // 取FCost较小的点，两者相等时取GCost较大的，这样可以优先探索较全的路径
                    cur = kvp.Key;
            }
            frontier.Remove(cur);

            if (cur == goal) {
                isFounded = true;
                break;
            }

            for (int m = 0; m < 4; m++) {
                Cursor neighbor = new Cursor();
                if (m == 0)
                    neighbor.Set(cur.i, cur.j - 1);
                else if (m == 1)
                    neighbor.Set(cur.i - 1, cur.j);
                else if (m == 2)
                    neighbor.Set(cur.i, cur.j + 1);
                else if (m == 3)
                    neighbor.Set(cur.i + 1, cur.j);

                if (!IsReachable(neighbor))
                    continue;

                int newGCost = gCosts[cur] + 1;  // 此处1为各个游标间的距离
                if (!gCosts.ContainsKey(neighbor) || newGCost < gCosts[neighbor]) {
                    gCosts[neighbor] = newGCost;
                    frontier[neighbor] = newGCost + Heuristic(ref neighbor, ref goal);
                    cameFrom[neighbor] = cur;
                }
            }
        }

        // 重构路径
        List<Cursor> path = new List<Cursor>();
        if (isFounded) {
            for (Cursor cur = goal; cameFrom.ContainsKey(cur); cur = cameFrom[cur]) {
                path.Add(cur);
            }
            path.Add(start);
            path.Reverse();
        }

        return path;
    }

    public bool IsInBound(Cursor cursor) {
        if (cursor.i < 0 || cursor.i > row - 1 || cursor.j < 0 || cursor.j > col - 1)
            return false;
        return true;
    }

    public bool IsReachable(Cursor cursor) {
        if (!IsInBound(cursor))
            return false;
        int data = mapDatas[cursor.i, cursor.j];
        if (data == (int)MapElement.STEEL || data == (int)MapElement.WATER)
            return false;
        return true;
    }

    public int GetMapData(Cursor cursor) {
        // if (!IsInBound(cursor)) {
        //     Debug.LogError("Indexing map data container using a out-of-bound cursor: " + cursor.ToString());
        // }
        return mapDatas[cursor.i, cursor.j];
    }

    public void SetMapData(Cursor cursor, MapElement element) {
        // if (!IsInBound(cursor)) {
        //     Debug.LogError("Indexing map data container using a out-of-bound cursor: " + cursor.ToString());
        // }
        mapDatas[cursor.i, cursor.j] = (int)element;
    }

    // @note    使用路径序列在场景中生成路径标识（Debug性质）
    // @param   path 路径序列
    public void ShowPath(List<Cursor> path) {
        foreach(Cursor cursor in path) {
            GameObject sign = Instantiate(pathSign, elementRoot.transform);
            sign.transform.localPosition = CursorToPostion(cursor);
            sign.GetComponent<Animator>().enabled = false;
        }
    }
}
}
#endregion
