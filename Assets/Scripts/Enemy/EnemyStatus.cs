using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyStatus : MonoBehaviour
{
    [Header("基础属性")]
    public int hp = 20; // 敌人血量，可按需改
    [Header("Debuff默认配置")]
    public int debuffPerSecDamage = 1; // 每秒Debuff伤害（默认1点）
    public float debuffDuration = 4f; // Debuff持续时间（默认4s）

    private int currentDebuffLayer; // 当前Debuff层数
    private float debuffTimer; // Debuff计时（每层独立，取最大值）

    void Update()
    {
        // Debuff持续计时，大于0则持续掉血
        if (debuffTimer > 0)
        {
            debuffTimer -= Time.deltaTime;
            TakeDamage(debuffPerSecDamage * Time.deltaTime); // 每秒掉血，浮点型避免帧跳
        }
        else
        {
            currentDebuffLayer = 0; // 计时结束，层数清零
        }
    }

    // 接收伤害（整数/浮点型兼容）
    public void TakeDamage(float damage)
    {
        hp -= Mathf.RoundToInt(damage);
        if (hp <= 0)
        {
            Die();
        }
    }

    // 添加Debuff（叠加层数，重置计时）
    public void AddDebuff()
    {
        currentDebuffLayer++;
        debuffTimer = debuffDuration; // 每次叠加重置持续时间（也可改为叠加时长，按需注释）
        // 若要叠加时长，替换上面一行：debuffTimer += debuffDuration;
    }

    // 敌人死亡逻辑（清空目标、销毁/回收）
    private void Die()
    {
        // 通知玩家脚本清空目标（避免空引用）
        FindObjectOfType<PlayerTargetAttack>().ClearTarget();
        // 敌人死亡：销毁/回收，按需选择
        // Destroy(gameObject); 
        gameObject.SetActive(false);
    }

    // 获取当前是否有Debuff
    public bool HasDebuff()
    {
        return currentDebuffLayer > 0 && debuffTimer > 0;
    }
}
