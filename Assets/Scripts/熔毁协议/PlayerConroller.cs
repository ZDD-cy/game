using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public bool isBurning;
    private float burnTimer;
    private int currentHP;

    // BOSS调用：施加燃烧Debuff
    public void ApplyBurn(float duration, float damagePerSecond)
    {
        if (isBurning) return; // 不可叠加
        isBurning = true;
        burnTimer = 0;
        StartCoroutine(BurnCoroutine(duration, damagePerSecond));
    }

    private IEnumerator BurnCoroutine(float duration, float damagePerSecond)
    {
        while (burnTimer < duration)
        {
            burnTimer += Time.deltaTime;
            if (burnTimer >= 1f)
            {
                currentHP = Mathf.Max(currentHP - (int)damagePerSecond, 0);
                burnTimer = 0;
            }
            yield return null;
        }
        isBurning = false;
    }

    // 玩家受击逻辑（可自行扩展）
    public void TakeDamage(int damage)
    {
        currentHP = Mathf.Max(currentHP - damage, 0);
    }
}