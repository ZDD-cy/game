using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossRangeChase : MonoBehaviour
{
    // 玩家（敌人）的 Transform 引用
    public Transform target;
    // 追击半径（8 个单位）
    public float chaseRadius = 8f;
    // Boss 移动速度
    public float moveSpeed = 3f;
    // 初始位置
    private Vector3 originalPosition;

    void Start()
    {
        // 记录 Boss 的初始位置
        originalPosition = transform.position;
    }

    void Update()
    {
        if (target != null)
        {
            // 计算 Boss 与目标的距离
            float distanceToTarget = Vector3.Distance(transform.position, target.position);

            if (distanceToTarget <= chaseRadius)
            {
                // 在追击范围内，计算方向并移动
                Vector3 direction = (target.position - transform.position).normalized;
                transform.Translate(direction * moveSpeed * Time.deltaTime);
            }
            else
            {
                // 超出范围，返回初始位置
                transform.position = Vector3.MoveTowards(transform.position, originalPosition, moveSpeed * Time.deltaTime);
            }
        }
    }
}

