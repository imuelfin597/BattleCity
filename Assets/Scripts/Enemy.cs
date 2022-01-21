using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#region namespace BattleCity start
namespace BattleCity
{
public class Enemy : Tank
{
    public enum State {
        IDLE,
        ROAMING,
        CHASING,
        CHASING_BASE,
        ATTACKING,
    };

    // 标位中心位置偏差阈值
    private const float posDeviationThreshold = 0.12f;
    private State state = State.IDLE;
    // 记录当前状态剩余时间
    private float timer = -1;
    // 记录原地卡死剩余时间
    private float stuckTimer = -1;
    private Cursor cur = new Cursor(-1, -1);
    private Cursor goal = new Cursor(-1, -1);
    private Cursor stepGoal = new Cursor(-1, -1);
    private List<Cursor> path = new List<Cursor>();
    private float distanceToPlayer = -1f;  // distance to the chasing player

    protected override void Start() {
        base.Start();

        stepGoal = MapGenerator.Instance.PositionToCursor(transform.localPosition);

        // path每一秒更新
        StartCoroutine(
            GameManager.Instance.NewClosureWithYieldInstruction(
                () => path = MapGenerator.Instance.AStarSearch(cur, goal),
                new WaitForSeconds(1),
                0
            )
        );
    }

    protected override void Update() {
        base.Update();
        timer -= Time.deltaTime;
        stuckTimer -=Time.deltaTime;

        Cursor newCursor = MapGenerator.Instance.PositionToCursor(transform.localPosition);
        if (newCursor != cur) {
            cur = newCursor;
            stuckTimer = 5f;
        } else if (stuckTimer <= 0) {
            while (true) {
                stepGoal.Set(cur.i + Random.Range(-2, 3), cur.j + Random.Range(-2, 3));
                if (MapGenerator.Instance.IsReachable(stepGoal)) {
                    stuckTimer = 5f;
                    break;
                }
            }
        }

        Player playerInstance = GameManager.Instance.GetPlayerInstance();
        distanceToPlayer =
            playerInstance != null ?
            Vector2.Distance(transform.position, playerInstance.transform.position) : -1f;

        UpdateState();
    }

    private void UpdateState() {
        if (state == State.IDLE) {
            if (timer <= 0) {
                state = State.ROAMING;
                return;
            }
            NoticeTarget();
        } else if (state == State.ROAMING) {
            if (!HasGoal()) {
                while (true) {
                    goal.Set(cur.i + Random.Range(-5, 6), cur.j + Random.Range(-5, 6));
                    if (!MapGenerator.Instance.IsReachable(goal)) continue;
                    path = MapGenerator.Instance.AStarSearch(cur, goal);
                    if (path.Count > 0) break;
                }
            }
            MoveToGoal();
            if (IsArrivedAt(goal)) {
                state = State.IDLE;
                ResetGoal();
                timer = Random.Range(0f, 3f);
            }

            NoticeTarget();
        } else if (state == State.CHASING) {
            Player playerInstance = GameManager.Instance.GetPlayerInstance();
            float stopChaseRange = 8f;
            if (playerInstance == null || distanceToPlayer > stopChaseRange) {
                state = State.ROAMING;
                ResetGoal();
                return;
            }
            if (playerInstance == null) return;

            float attackRange = 5f;
            if (distanceToPlayer < attackRange) {
                if (Fire()) {
                    state = State.ATTACKING;
                    timer = 0.5f;
                    return;
                }
            }
            goal = MapGenerator.Instance.PositionToCursor(playerInstance.transform.localPosition);
            MoveToGoal();
        } else if (state == State.CHASING_BASE) {
            goal = MapGenerator.Instance.GetBaseCursor();
            MoveToGoal();
            NoticeTarget();

            float attackRange = 5f;
            if (Vector2.Distance(MapGenerator.Instance.GetBasePosition(), transform.localPosition) < attackRange) {
                if (Fire()) {
                    state = State.ATTACKING;
                    timer = 0.5f;
                    return;
                }
            }
        } else if (state == State.ATTACKING) {
            Stop();
            if (timer <= 0) {
                if (!NoticeTarget()) {
                    state = State.ROAMING;
                }
                return;
            }
        }
    }

    private void ResetGoal() {
        goal.Set(-1, -1);
    }

    private bool HasGoal() {
        return !(goal.i == -1 && goal.j == -1);
    }

    private void MoveToGoal() {
        bool isArrived = IsArrivedAt(stepGoal);
        Vector2 vec = MapGenerator.Instance.CursorToPostion(stepGoal) - (Vector2)transform.localPosition;
        if (isArrived) {
            vec = Vector2.zero;
        } else if (Mathf.Abs(vec.x) > Mathf.Abs(vec.y)) {
            if (Mathf.Abs(vec.y) < posDeviationThreshold) {
                vec.Set(Mathf.Sign(vec.x), 0);
            } else {
                vec.Set(0, Mathf.Sign(vec.y));
            }
        } else {
            if (Mathf.Abs(vec.x) < posDeviationThreshold) {
                vec.Set(0, Mathf.Sign(vec.y));
            } else {
                vec.Set(Mathf.Sign(vec.x), 0);
            }
        }
        Move(vec.x, vec.y);
        if (MapGenerator.Instance.GetMapData(stepGoal) == (int)MapGenerator.MapElement.BRICK) {
            Fire();
        }

        if (isArrived) {
            // 必须要到达步进目标后才算一个完整步进，才能更改步进目标
            int index = path.IndexOf(cur);
            if (index != -1 && index + 1 < path.Count) {
                // 只要path中包含当前标位且有下一个标位
                stepGoal = path[index + 1];
            }
        }
    }

    private bool IsArrivedAt(Cursor cursor) {
        float distance = Vector2.Distance(transform.localPosition, MapGenerator.Instance.CursorToPostion(cursor));
        return distance < posDeviationThreshold;
    }

    private bool NoticeTarget() {
        Player playerInstance = GameManager.Instance.GetPlayerInstance();
        float noticeRange = 7f;
        if (playerInstance && distanceToPlayer != -1 && distanceToPlayer < noticeRange ) {
            state = State.CHASING;
            return true;
        } else if (
            Vector2.Distance(
                MapGenerator.Instance.GetBasePosition(),
                transform.localPosition
            ) < noticeRange
        ) {
            state = State.CHASING_BASE;
            return true;
        }
        return false;
    }

    public bool Fire() {
        return base.Fire(false);
    }

    protected override void Die() {
        base.Die();
        GameManager.Instance.EnemyDied();
    }
}
}
#endregion
