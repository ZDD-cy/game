using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDebuffAttack : MonoBehaviour
{
    [Header("攻击设置")]
    public float attackDelay = 2f;      // 第一次攻击延迟（默认2秒）
    public float debuffDamagePerSec = 1f; // Debuff每秒伤害
    public float debuffDuration = 4f;   // Debuff持续时间
    public KeyCode attackKey = KeyCode.J;

    [Header("连线显示")]
    public LineRenderer attackLine;
    public Material dashedLineMaterial;
    public Material solidLineMaterial;

    private PlayerLockEnemy lockEnemyScript;
    private Transform currentTarget;
    private bool isAttacking = false;
    private float attackTimer;
    private float debuffTimer;

    void Start()
    {
        // 获取锁敌脚本引用
        lockEnemyScript = GetComponent<PlayerLockEnemy>();
        if (lockEnemyScript == null)
        {
            Debug.LogError("未找到 PlayerLockEnemy 脚本，请确保已挂载！", this);
            enabled = false;
            return;
        }

        // 初始化连线
        if (attackLine != null)
        {
            attackLine.positionCount = 2;
            attackLine.enabled = false;
        }
        
    }

    void Update()
    {
        // 检测攻击输入
        if (Input.GetKeyDown(attackKey) && !isAttacking)
        {
            currentTarget = lockEnemyScript.currentAttackTarget;
            if (currentTarget != null)
            {
                StartAttackSequence();
            }
            else
            {
                Debug.LogWarning("没有选中任何敌人！");
            }
        }

        // 攻击计时与状态更新
        if (isAttacking)
        {
            UpdateAttackSequence();
            UpdateAttackLine();
        }
    }

    void StartAttackSequence()
    {
        isAttacking = true;
        attackTimer = 0f;
        debuffTimer = 0f;

        // 初始化连线为虚线
        if (attackLine != null)
        {
            attackLine.enabled = true;
            attackLine.material = dashedLineMaterial;
        }
        Debug.Log($"开始对 {currentTarget.name} 施加Debuff攻击...");
    }

    void UpdateAttackSequence()
    {
        // 第一阶段：攻击延迟（2秒）
        if (attackTimer < attackDelay)
        {
            attackTimer += Time.deltaTime;
            if (attackTimer >= attackDelay)
            {
                // 延迟结束，施加Debuff并切换为实线
                ApplyDebuff();
                if (attackLine != null)
                {
                    attackLine.material = solidLineMaterial;
                }
            }
        }
        // 第二阶段：Debuff持续（4秒）
        else if (debuffTimer < debuffDuration)
        {
            debuffTimer += Time.deltaTime;
            // 每秒造成一次伤害
            if (Mathf.Floor(debuffTimer) > Mathf.Floor(debuffTimer - Time.deltaTime))
            {
                if (currentTarget.TryGetComponent(out EnemyTakeDamage enemyHp))
                {
                    enemyHp.TakeDamage(debuffDamagePerSec);
                }
            }
        }
        // 第三阶段：攻击结束
        else
        {
            EndAttack();
        }
    }

    void ApplyDebuff()
    {
        Debug.Log($"对 {currentTarget.name} 施加Debuff，持续 {debuffDuration} 秒");
    }

    void EndAttack()
    {
        isAttacking = false;
        currentTarget = null;
        // 隐藏连线
        if (attackLine != null)
        {
            attackLine.enabled = false;
        }
        Debug.Log("攻击结束，Debuff已移除");
    }

    void UpdateAttackLine()
    {
        if (attackLine != null && currentTarget != null)
        {
            attackLine.SetPosition(0, transform.position);
            attackLine.SetPosition(1, currentTarget.position);
        }
    }

    void OnDrawGizmosSelected()
    {
        // 绘制攻击范围（可选调试用）
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, lockEnemyScript.lockRange);
    }
}
