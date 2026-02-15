using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EnemyBounds : MonoBehaviour
{
    [Header("墙控制器")]
    public WallController wallController;

    [Header("边界显示")]
    public Color gizmoColor = Color.red;
    public bool drawGizmosWhenSelected = true;

    [Header("调试")]
    public bool logListChanges = true;      // 当列表内容变化时输出日志
    public bool logAllDeadDetection = true; // 当检测到全灭时输出日志
    public bool logEveryFrame = false;      // 是否每帧输出（谨慎开启）

    [Header("事件")]
    public UnityEvent onAllEnemiesDefeated;

    private Collider2D _collider;
    private bool allDeadTriggered = false;
    private int lastListCount = -1;          // 记录上一次列表数量，用于检测变化
    private string lastListHash = "";         // 可选的更精细变化检测

    void Awake()
    {
        _collider = GetComponent<Collider2D>();
        if (_collider == null)
            Debug.LogError("EnemyBounds 需要挂载一个 Collider2D 组件！", this);
    }

    void Update()
    {
        if (allDeadTriggered || wallController == null) return;

        // 检测列表是否发生变化（数量变化或内容变化）
        bool listChanged = HasListChanged();

        // 如果需要每帧输出，或者列表发生变化且开启了日志，则输出当前列表快照
        if (logEveryFrame || (listChanged && logListChanges))
        {
            LogCurrentList();
        }

        // 检查是否所有敌人死亡
        bool allDead = true;
        foreach (var enemy in wallController.enemiesInRoom)
        {
            if (enemy != null && !enemy.isDead)
            {
                allDead = false;
                break;
            }
        }

        if (allDead)
        {
            if (logAllDeadDetection)
                Debug.Log("【EnemyBounds】检测到所有敌人死亡！将触发墙壁下降。");

            allDeadTriggered = true;
            onAllEnemiesDefeated.Invoke();
            if (wallController != null)
                wallController.LowerWall();
            enabled = false;
        }
    }

    // 检测列表内容是否发生变化（简单实现：数量变化就算变化）
    private bool HasListChanged()
    {
        if (wallController.enemiesInRoom == null) return false;
        int currentCount = wallController.enemiesInRoom.Count;
        if (currentCount != lastListCount)
        {
            lastListCount = currentCount;
            return true;
        }
        return false;
    }

    // 输出当前列表快照
    private void LogCurrentList()
    {
        if (wallController.enemiesInRoom == null) return;
        string debugInfo = $"【EnemyBounds】敌人列表共有 {wallController.enemiesInRoom.Count} 项：";
        for (int i = 0; i < wallController.enemiesInRoom.Count; i++)
        {
            var enemy = wallController.enemiesInRoom[i];
            if (enemy == null)
                debugInfo += $"\n   [{i}] null";
            else
                debugInfo += $"\n   [{i}] {enemy.name} (isDead={enemy.isDead})";
        }
        Debug.Log(debugInfo);
    }

    // ---------- Gizmos 绘制 ----------
    private void OnDrawGizmosSelected()
    {
        if (!drawGizmosWhenSelected) return;
        DrawTriggerGizmo();
    }

    private void OnDrawGizmos()
    {
        if (drawGizmosWhenSelected) return;
        DrawTriggerGizmo();
    }

    private void DrawTriggerGizmo()
    {
        Gizmos.color = gizmoColor;
        Collider2D col = GetComponent<Collider2D>();
        if (col == null) return;
        Gizmos.DrawWireCube(col.bounds.center, col.bounds.size);
    }
}