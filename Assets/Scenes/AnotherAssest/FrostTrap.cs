using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FrostTrap : MonoBehaviour
{
    [Header("机关配置")]
    public FrostBoss frostBoss;       // 关联的寒霜BOSS
    public KeyCode activateKey = KeyCode.E; // 激活按键
    public Image uiHint;              // 机关激活提示UI
    private bool isActive = false;

    void Start()
    {
        if (uiHint != null) uiHint.enabled = false;
    }

    void Update()
    {
        // 玩家靠近+按按键激活/关闭机关
        Collider2D[] players = Physics2D.OverlapCircleAll(transform.position, 3f, LayerMask.GetMask("Player"));
        if (players.Length > 0)
        {
            if (uiHint != null) uiHint.enabled = true;
            if (Input.GetKeyDown(activateKey))
            {
                isActive = !isActive;
                // 同步机关状态给BOSS
                frostBoss?.OnTrapStateChange(isActive);
                // 机关视觉反馈
               SpriteRenderer sr =GetComponent<SpriteRenderer>();   
                if (sr != null)
                {
                    sr.color = isActive ? Color.cyan : Color.white;
                }
            }
        }
        else
        {
            if (uiHint != null) uiHint.enabled = false;
        }
    }

    // 机关检测范围
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 3f);
    }
}

