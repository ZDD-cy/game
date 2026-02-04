using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static firetrap;


public class EliteEnemy : MonoBehaviour, IDamageable
{
    private EnemyHitEffect _hitEffect;

    public float maxHealth = 50f;
    public float currentSpeed = 3f;

    [Tooltip("普通敌人掉落物的倍数范围（3-5倍）")]
    public Vector2 dropMultiplierRange = new Vector2(3, 5);

    [Header("狂暴状态配置")]
    public float rageThreshold = 0.3f; // 30%血量以下触发
    public float originalSpeed;
    public float rageSpeedMultiplier = 1.5f; // 攻速移速提升50%
    public bool isUnstoppableWhenEnraged = true; // 狂暴时霸体

    [Header("引用")]
    public EnemyDropSystem dropSystem; // 引用你的掉落系统 

    //内部状态变量
    private float currentHealth;
    private bool isEnraged = false;

    private void Start()
    {
        currentHealth = maxHealth;
    }

    void Update()
    {
        // 检测是否进入狂暴状态
        if (!isEnraged && currentHealth / maxHealth <= rageThreshold)
        {
            EnterRageMode();
        }
    }

    public void TakeDamage(float damage)
    {
        if (isEnraged && isUnstoppableWhenEnraged)
        {
            // 霸体状态下免疫控制，但仍然会掉血
        }
        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        DropLoot();
        Destroy(gameObject);
    }

    private void EnterRageMode()
    {
        isEnraged = true;
        currentSpeed = originalSpeed * rageSpeedMultiplier;

    }

    private void DropLoot()
    {
        // 计算3-5倍的掉落数量
        float dropMultiplier = Random.Range(dropMultiplierRange.x, dropMultiplierRange.y);

        // 掉落硬币（调用你现有的掉落系统）
        int coinCount = Mathf.RoundToInt(dropSystem.coin.minCount * dropMultiplier);
        dropSystem.SpawnDropItem(dropSystem.coin.itemPrefab, coinCount);

        // 掉落珠子（调用你现有的掉落系统）
        if (Random.value <= 0.4f) // 保持和普通敌人一样的珠子掉落概率
        {
            int beadCount = Mathf.RoundToInt(dropSystem.bead.minCount * dropMultiplier);
            dropSystem.SpawnDropItem(dropSystem.bead.itemPrefab, beadCount);
        }
    }

    // 特效播放方法（需要你自己实现）
    private void PlaySpawnEffect() { /* 生成时的特效 */ }
    private void PlayDeathEffect() { /* 死亡时的特效 */ }
    private void PlayRageEffect() { /* 狂暴时的特效 */ }


    // 实现 IDamageable 接口的 ApplyBurn 方法
    public void ApplyBurn(float burnDamage, float burnDuration)
    {
        _hitEffect.ApplyBurn(burnDamage, burnDuration);
    }

    public void ApplySlow(float slowAmount, float slowDuration)
    {
        _hitEffect.ApplySlow(slowAmount, slowDuration);
    }
    
}