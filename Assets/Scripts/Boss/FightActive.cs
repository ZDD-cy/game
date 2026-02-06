using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 战斗状态核心管理脚本
/// 负责检测玩家进入、激活战斗、以及战斗结束后的清理
/// </summary>
public class FightActive : MonoBehaviour
{
    [Header("战斗设置")]
    [Tooltip("战斗是否已经激活")]
    public bool isFightActive = false;

    // 引用
    private Transform player;
    private Collider2D bossCollider;

    void Start()
    {
        // 初始化
        player = FindFirstObjectByType<Player>()?.transform;
        bossCollider = GetComponent<Collider2D>();

        // 确保战斗一开始是关闭的
        isFightActive = false;
    }

    void Update()
    {
        if (player == null) return;
    }

    /// <summary>
    /// 开始战斗
    /// </summary>
    public void StartFight()
    {
        isFightActive = true;
        Debug.Log("【战斗状态】玩家进入房间，战斗已激活！");

        // 在这里可以添加战斗开始时的逻辑，比如：
        // 1. 锁定BOSS位置，不再移动
        // 2. 播放BOSS入场动画
        // 3. 启动技能计时器
    }

    /// <summary>
    /// 结束战斗
    /// </summary>
    public void EndFight()
    {
        isFightActive = false;
        Debug.Log("【战斗状态】战斗已结束！");

        // 在这里可以添加战斗结束时的逻辑，比如：
        // 1. 停止所有技能
        // 2. 播放BOSS死亡/退场动画
        // 3. 解锁玩家控制
    }

   
}
