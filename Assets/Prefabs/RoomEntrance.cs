using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomEntrance : MonoBehaviour
{
    public WallController wall;

    private bool alreadyTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (alreadyTriggered) return;

        if (other.CompareTag("Player"))
        {
            wall.RaiseWall();
            alreadyTriggered = true;
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
}


