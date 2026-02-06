using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
[RequireComponent(typeof(Collider2D))]
public class TriggerGizmo : MonoBehaviour
{
    public Color gizmoColor = Color.blue;
    public FrostBoss frostBoss;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("player") && !frostBoss.isFightActive) ;
        {
            frostBoss.isFightActive = true;
            Debug.Log("玩家进入房间，BOSS战斗激活！");
        }
    }
    void OnDrawGizmos()
    {
        Gizmos.color = gizmoColor;
        Collider2D col = GetComponent<Collider2D>();
        if (col is BoxCollider2D box)
        {
            Gizmos.DrawWireCube(transform.position + (Vector3)box.offset, box.size);
        }
        else if (col is CircleCollider2D circle)
        {
            // 第一步：计算球体中心点
            Vector3 sphereCenter = transform.position + new Vector3(circle.offset.x, circle.offset.y, 0);
            // 第二步：画Gizmo
            Gizmos.DrawWireSphere(sphereCenter, circle.radius);

        }
    }
}
