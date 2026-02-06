using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Collections.Generic;

public class ThawAnchorManager : MonoBehaviour
{
    [Header("解冻锚配置")]
    public List<ThawAnchor> thawAnchors = new List<ThawAnchor>(); // 按顺时针顺序拖拽四个塔
    public float activeDuration = 8f; // 单个塔激活时长
    public float switchInterval = 2f; // 切换塔的间隔（无塔激活的过渡时间）

    private int currentActiveIndex = 0; // 当前激活的塔索引
    private float timer;
    private bool isSwitching; // 是否处于切换过渡阶段

    void Start()
    {
        // 初始化所有塔为未激活
        foreach (var anchor in thawAnchors)
        {
            if (anchor != null)
            {
                anchor.SetAnchorActive(false);
            }
        }
        // 激活第一个塔
        if (thawAnchors.Count >= 1)
        {
            thawAnchors[currentActiveIndex].SetAnchorActive(true);
        }
        timer = activeDuration;
    }

    void Update()
    {
        if (thawAnchors.Count < 4)
        {
            Debug.LogWarning("请在Inspector面板按顺时针顺序添加4个解冻锚！");
            return;
        }

        timer -= Time.deltaTime;

        // 激活时长到，进入切换过渡
        if (timer <= 0 && !isSwitching)
        {
            thawAnchors[currentActiveIndex].SetAnchorActive(false);
            isSwitching = true;
            timer = switchInterval;
        }
        // 过渡时间到，激活下一个塔（顺时针）
        else if (timer <= 0 && isSwitching)
        {
            currentActiveIndex = (currentActiveIndex + 1) % 4; // 取模实现循环
            thawAnchors[currentActiveIndex].SetAnchorActive(true);
            isSwitching = false;
            timer = activeDuration;
        }
    }

    // 供外部获取：是否在某个解冻锚的激活范围内
    public bool IsInActiveThawArea(Collider2D targetCol)
    {
        foreach (var anchor in thawAnchors)
        {
            if (anchor != null && anchor.IsActive && anchor.CheckInRange(targetCol))
            {
                return true;
            }
        }
        return false;
    }
}

