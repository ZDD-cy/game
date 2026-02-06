using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IceTrap : MonoBehaviour
{
    [Header("陷阱设置")]
    [Tooltip("减速比例（0~1，0.5表示减速50%）")]
    public float slowDownRate = 0.5f; // 默认减速50%

    [Tooltip("减速与伤害持续时间（秒）")]
    public float effectDuration = 3f;

    [Tooltip("陷阱冷却时间（秒，0表示一次性陷阱）")]
    public float trapCooldown = 5f;

    [Tooltip("每秒造成的伤害值")]
    public float damagePerSecond = 1f;

    private bool _isTrapActive = true;

    void Start()
    {
        _isTrapActive = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("陷阱触发检测：进入 OnTriggerEnter2D 方法");
        if (other.CompareTag("player") && _isTrapActive)
        {
            Debug.Log("陷阱触发：检测到 Player 标签，且陷阱处于激活状态");
            Player player = other.GetComponent<Player>();
            if (player != null && !player.isFrozen)
            {
                Debug.Log("陷阱触发：成功获取 Player 组件，开始施加冰冻效果");
                // 标记玩家状态
                player.isInIceTrap = true;
                player.isFrozen = true;
                player.ApplySlow(slowDownRate,effectDuration);
               
                // 启动持续伤害协程
                StartCoroutine(PlayerTakeDamage(player));
                if (player.currentHealth > 0)
                {
                    player.isInIceTrap = false;
                    player.isFrozen = false;
                }

            }
            else
            {
                if (player == null)
                {
                    Debug.LogWarning("陷阱触发：未找到 Player 组件！");
                }
                else if (player.isFrozen)
                {
                    Debug.Log("陷阱触发：玩家已处于冰冻状态，跳过");
                }
            }
        }
    }
    

    IEnumerator PlayerTakeDamage(Player player)
    {
        float elapsedTime = 0f;

        // 持续整个效果时间，并且只在玩家存活时执行
        while (elapsedTime < effectDuration && player.currentHealth > 0)
        {
            // 每秒造成1点伤害
            player.TakeDamage(1f);
            Debug.Log($"受到 1 点伤害！当前生命值: {player.currentHealth}");

            // 等待1秒，再进行下一次伤害
            yield return new WaitForSeconds(1f);
            elapsedTime += 1f;
        }

        // 效果结束后，恢复玩家状态（仅当玩家存活时）
        if (player.currentHealth > 0)
        {
            player.isInIceTrap = false;
            player.isFrozen = false;
        }

        // 陷阱可以重复使用重置
        if (trapCooldown > 0)
        {
            yield return new WaitForSeconds(trapCooldown);
            _isTrapActive = true;
            gameObject.SetActive(true);
        }
    }
}
    