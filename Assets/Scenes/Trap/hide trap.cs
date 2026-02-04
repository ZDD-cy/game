using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class hidetrap : MonoBehaviour

{
    [Header("基础陷阱配置")]
    public TrapType trapType = TrapType.Spike;
    public int damage = 3;
    public float revealDuration = 1.5f; 
    public float trapCD = 2f;
    public bool isReusable = true;
    public Sprite revealSprite;

    [Header("牵引效果配置")]
    public bool enablePull = true;      
    public float pullForce = 8f;        
    public float pullRadius = 1.5f;     

    private SpriteRenderer sr;
    private Collider2D trapCollider;
    private Sprite originalSprite;
    private bool isActive = true;
    private bool isRevealed = false;
    private Enemy trappedEnemy;        

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        trapCollider = GetComponent<Collider2D>();
        originalSprite = sr.sprite;
        InitHide();
    }

    void InitHide()
    {
        if (revealSprite != null)
            sr.sprite = originalSprite; 
        else
            sr.color = Color.clear;  
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy") && isActive && !isRevealed)
        {
            trappedEnemy = other.GetComponent<Enemy>();
            if (trappedEnemy == null) return;

            isRevealed = true;
            isActive = false;
            RevealTrap();
            ApplyTrapEffect();
            if (enablePull) StartCoroutine(PullEnemyCoroutine());
            Invoke(nameof(ResetTrapLogic), revealDuration);
        }
    }

    IEnumerator PullEnemyCoroutine()
    {
        while (isRevealed && trappedEnemy != null && trappedEnemy.hp > 0)
        {
            float distance = Vector2.Distance(transform.position, trappedEnemy.transform.position);
            if (distance > pullRadius) break;

            Vector2 pullDir = (transform.position - trappedEnemy.transform.position).normalized;
            Rigidbody2D enemyRb = trappedEnemy.GetComponent<Rigidbody2D>();
            if (enemyRb != null)
            {
                enemyRb.velocity = pullDir * pullForce * Time.deltaTime * 100;
            }
            else
            {
                trappedEnemy.transform.position = Vector2.MoveTowards(
                    trappedEnemy.transform.position,
                    transform.position,
                    pullForce * Time.deltaTime
                );
            }
            yield return null;
        }
        if (trappedEnemy != null) trappedEnemy.ResetSpeed();
    }

    void ApplyTrapEffect()
    {
        trappedEnemy.TakeDamage(damage);
        switch (trapType)
        {
            case TrapType.Fire:
                trappedEnemy.ApplyBurn(damage / 2, revealDuration);
                break;
            case TrapType.Freeze:
                trappedEnemy.ApplySlow(0.7f, revealDuration);
                break;
            case TrapType.Spike:
                break;
        }
    }
//显性需添加粒子效果
    void RevealTrap()
    {
        if (revealSprite != null) sr.sprite = revealSprite;
        else sr.color = Color.white;
    }

    void ResetTrapLogic()
    {
        StopCoroutine(PullEnemyCoroutine());
        if (trappedEnemy != null)
        {
            trappedEnemy.ResetSpeed();
            trappedEnemy = null;
        }
        InitHide();
        isRevealed = false;
        if (isReusable) Invoke(nameof(ActivateTrap), trapCD);
        else trapCollider.enabled = false;
    }

    void ActivateTrap() => isActive = true;

    public enum TrapType
    {
        Spike, 
        Fire,  
        Freeze 
    }
}

