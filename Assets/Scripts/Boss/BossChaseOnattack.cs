using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossChaseOnAttack : MonoBehaviour
{
    // Boss 移动速度
    public float moveSpeed = 4f;
    // 追击持续时间（3 秒）
    public float chaseDuration = 3f;
    // 当前仇恨目标
    private Transform currentTarget;
    // 追击计时器
    private float chaseTimer;
    // 初始位置
    private Vector3 originalPosition;

    void Start()
    {
        // 记录 Boss 初始位置
        originalPosition = transform.position;
    }

    void Update()
    {
        // 如果有仇恨目标且计时器未结束
        if (currentTarget != null && chaseTimer > 0)
        {
            chaseTimer -= Time.deltaTime;
            // 计算方向并移动
            Vector3 direction = (currentTarget.position - transform.position).normalized;
            transform.Translate(direction * moveSpeed * Time.deltaTime);
        }
        else if (chaseTimer <= 0 && currentTarget != null)
        {
            // 追击结束，清除目标
            currentTarget = null;
        }
    }

    // 被攻击时调用此方法，传入攻击者
    public void OnTakeDamage(Transform attacker)
    {
        // 更新仇恨目标和计时器
        currentTarget = attacker;
        chaseTimer = chaseDuration;
    }
}
