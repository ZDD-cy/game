using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAttackDOT : MonoBehaviour
{
    private float damagePerSecond;
    private float duration;
    private float timer;
    private bool isDoingT;
    private Player targetPlayer; // 要攻击的玩家对象
    public Transform target;      // 从 EnemyAutoTarget 来的目标
    private EnemyAutoTarget autoTarget;

    private void Start()
    {
        autoTarget = GetComponent<EnemyAutoTarget>();
    }

    private void Update()
    {
        // 只有锁定了目标，才允许攻击
        if (autoTarget != null && autoTarget.currentTarget != null)
        // 如果正在DOT效果中，就持续扣血
        if (isDoingT && targetPlayer != null)
        {
            if (timer < duration)
            {
                timer += Time.deltaTime;
                targetPlayer.hp -= damagePerSecond * Time.deltaTime;
                // Debug.Log($"对玩家造成 {damagePerSecond * Time.deltaTime} 点伤害");
            }
            else
            {
                isDoingT = false;
                // Debug.Log("DOT效果结束");
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 检测到玩家进入攻击范围
        if (other.CompareTag("player"))
        {
            // 找到玩家脚本并赋值
            targetPlayer = other.GetComponent<Player>();
            if (targetPlayer != null)
            {
                // 启动DOT，例如：每秒2点伤害，持续5秒
                ApplyDOT(2f, 5f);
                Debug.Log("玩家进入攻击范围，DOT启动");
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        // 玩家离开攻击范围，停止DOT
        if (other.CompareTag("player"))
        {
            isDoingT = false;
            targetPlayer = null;
            Debug.Log("玩家离开攻击范围，DOT停止");
        }
    }

    public void ApplyDOT(float damage, float time)
    {
        damagePerSecond = damage;
        duration = time;
        timer = 0;
        isDoingT = true;
    }
}
