using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class ThawAnchor : MonoBehaviour
{
    [Header("单体锚点配置")]
    public float thawRange = 5f; // 辐射增益范围
    public Color activeColor = Color.cyan; // 激活时颜色
    public Color inActiveColor = Color.gray; // 未激活时颜色
    private SpriteRenderer sr;
    private Collider2D col;
    public bool IsActive { get; private set; } // 外部只读的激活状态

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();
        // 确保碰撞体为触发器，不影响物理
        col.isTrigger = true;
        // 初始化颜色
        sr.color = inActiveColor;
    }

    // 设置锚点激活/未激活
    public void SetAnchorActive(bool active)
    {
        IsActive = active;
        sr.color = active ? activeColor : inActiveColor;
    }

    // 检测目标是否在当前锚点的辐射范围内
    public bool CheckInRange(Collider2D targetCol)
    {
        if (!IsActive) return false;
        // 2D距离检测：目标与锚点的距离 ≤ 辐射范围
        float distance = Vector2.Distance(transform.position, targetCol.transform.position);
        return distance <= thawRange;
    }

    // Gizmos绘制范围，方便调试（Scene视图可见）
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, thawRange);
    }
}
