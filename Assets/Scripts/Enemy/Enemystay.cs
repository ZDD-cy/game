using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyStayInBounds2D : MonoBehaviour
{
    public Transform boundsObject;
    private Collider2D boundsColl;
    private Rigidbody2D rb; // 缓存 Rigidbody2D 组件

    void Start()
    {
        boundsColl = boundsObject.GetComponent<Collider2D>();
        rb = GetComponent<Rigidbody2D>(); // 获取 Rigidbody2D
    }

    void FixedUpdate()
    {
        if (boundsColl == null) return;

        Vector2 currentPos = transform.position;
        float clampedX = Mathf.Clamp(currentPos.x, boundsColl.bounds.min.x, boundsColl.bounds.max.x);
        float clampedY = Mathf.Clamp(currentPos.y, boundsColl.bounds.min.y, boundsColl.bounds.max.y);
        Vector2 clampedPos = new Vector2(clampedX, clampedY);

        // 添加调试日志
        //Debug.Log($"当前位置: {currentPos}, 限制后位置: {clampedPos}, 边界Min: {boundsColl.bounds.min}, 边界Max: {boundsColl.bounds.max}");

        // ... 其余代码 ...
    }
}
