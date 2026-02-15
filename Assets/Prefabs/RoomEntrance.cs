using System.Collections.Generic;
using UnityEngine;

public class RoomEntrance : MonoBehaviour
{
    public List<WallController> walls = new List<WallController>();
    public int RoomIndex;
    public GameObject[] Enemies;
    private bool alreadyTriggered = false;
    public bool IsAllDead;
    public bool IsBoss;
    public bool IsFireBoss;

    private void OnTriggerEnter2D(Collider2D other)
    {
        //Debug.Log($"碰到了: {other.gameObject.name}, Tag: {other.tag}"); // 先看有没有碰到任何东西

        if (alreadyTriggered) return;

        if (other.CompareTag("Player"))
        {
            Debug.Log("触发成功墙出现");
            foreach (var wall in walls)
            {
                if (wall != null)
                {
                    Debug.Log($"正在升起墙: {wall.gameObject.name}");
                    wall.RaiseWall();
                    AudioManager.Instance.PlaySFX("2");
                }
                else
                {
                    Debug.LogError("列表中有一个空的墙引用！");
                }
            }
            alreadyTriggered = true;
            for (int i = 0; i < Enemies.Length; i++)
            {
                GameObject obj = Enemies[i];
                if(!IsBoss)obj.GetComponent<Enemy>().OnEnterRoom(RoomIndex);
                else if (IsFireBoss) obj.GetComponent<BossMeltdownProtocol>().OnEnterRoom();
            }
        }
    }

    // 边框颜色，可在 Inspector 里调整
    public Color gizmoColor = Color.red;
    // 是否只在选中时显示边框
    public bool drawGizmosWhenSelected = true;

    // 当对象被选中时绘制 Gizmos
    private void OnDrawGizmosSelected()
    {
        if (!drawGizmosWhenSelected) return;
        DrawTriggerGizmo();
    }

    // 始终绘制 Gizmos
    private void OnDrawGizmos()
    {
        if (drawGizmosWhenSelected) return;
        DrawTriggerGizmo();
    }

    // 绘制触发区域边框的核心方法
    private void DrawTriggerGizmo()
    {
        Gizmos.color = gizmoColor;

        Collider col = GetComponent<Collider>();
        if (col == null) return;

        // 根据碰撞器类型绘制不同的 Gizmos
        if (col is BoxCollider boxCol)
        {
            Gizmos.DrawWireCube(col.bounds.center, col.bounds.size);
        }
        else if (col is SphereCollider sphereCol)
        {
            Gizmos.DrawWireSphere(col.bounds.center, sphereCol.radius);
        }
        else if (col is CapsuleCollider capsuleCol)
        {
            // 胶囊碰撞器的 Gizmos 绘制稍复杂，这里用一个近似的线框
            Gizmos.DrawWireSphere(col.bounds.center, capsuleCol.radius);
        }
        // 可以继续添加对 MeshCollider 等的支持
    }
    public void CheckAllEnemiesDead()
    {
        IsAllDead = true;
        for (int i = 0; i < Enemies.Length; i++)
        {
            GameObject obj = Enemies[i];
            if (obj.activeInHierarchy)
            {
                IsAllDead = false;
            }
        }

        if (IsAllDead)
        {
            foreach (var wall in walls)
            {
                wall.LowerWall();
                AudioManager.Instance.PlaySFX("2");
            }
        }
    }
    
}


