using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FrostDebuff : MonoBehaviour
{
    [Header("寒霜Debuff配置")]
    public float frostDuration = 5f; // 基础消散耗时
    private float currentFrostTime;
    private bool isFrosted; // 是否处于寒霜状态
    private Collider2D playerCol;
    private ThawAnchorManager thawManager;

    void Awake()
    {
        playerCol = GetComponent<Collider2D>();
        // 获取全局解冻锚管理器（需保证场景中有Manager对象）
        thawManager = FindObjectOfType<ThawAnchorManager>();
    }

    // 外部调用：给玩家添加寒霜Debuff（如敌人攻击时调用）
    public void AddFrostDebuff()
    {
        isFrosted = true;
        currentFrostTime = frostDuration;
        // 可添加你的Debuff效果：如减速、特效等
        // Example: GetComponent<PlayerMove>().speed *= 0.5f;
    }

    void Update()
    {
        if (!isFrosted) return;

        // 核心：判断是否在激活的解冻锚范围内，决定消散速度
        float thawSpeed = 1f;
        if (thawManager != null && thawManager.IsInActiveThawArea(playerCol))
        {
            thawSpeed = 2f; // 范围内：消散速度翻倍（耗时减半）
        }

        // 更新Debuff剩余时间
        currentFrostTime -= Time.deltaTime * thawSpeed;

        // Debuff消散完成
        if (currentFrostTime <= 0)
        {
            isFrosted = false;
            // 恢复玩家状态：如恢复速度、关闭特效等
            // Example: GetComponent<PlayerMove>().speed /= 0.5f;
        }
    }

    // 外部获取Debuff状态（供其他脚本调用，如UI显示）
    public bool IsFrosted()
    {
        return isFrosted;
    }
}
