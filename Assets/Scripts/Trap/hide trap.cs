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
    private Player trappedplayer;        

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
        if (other.CompareTag("player") && isActive && !isRevealed)
        {
            trappedplayer = other.GetComponent<Player>();
            if (trappedplayer == null) return;

            isRevealed = true;
            isActive = false;
            RevealTrap();
            ApplyTrapEffect();
            if (enablePull) StartCoroutine(PullplayerCoroutine());
            Invoke(nameof(ResetTrapLogic), revealDuration);
        }
    }

    IEnumerator PullplayerCoroutine()
    {
        while (isRevealed && trappedplayer != null && trappedplayer.hp > 0)
        {
            float distance = Vector2.Distance(transform.position, trappedplayer.transform.position);
            if (distance > pullRadius) break;

            Vector2 pullDir = (transform.position - trappedplayer.transform.position).normalized;
            Rigidbody2D playerRb = trappedplayer.GetComponent<Rigidbody2D>();
            if (playerRb != null)
            {
                playerRb.velocity = pullDir * pullForce * Time.deltaTime * 100;
            }
            else
            {
                trappedplayer.transform.position = Vector2.MoveTowards(
                    trappedplayer.transform.position,
                    transform.position,
                    pullForce * Time.deltaTime
                );
            }
            yield return null;
        }
        if (trappedplayer != null) trappedplayer.ResetSpeed();
    }

    void ApplyTrapEffect()
    {
        trappedplayer.TakeDamage(damage);
        switch (trapType)
        {
            case TrapType.Fire:
                trappedplayer.ApplyBurn(damage / 2, revealDuration);
                break;
            case TrapType.Freeze:
                trappedplayer.ApplySlow(0.7f, revealDuration);
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
        StopCoroutine(PullplayerCoroutine());
        if (trappedplayer != null)
        {
            trappedplayer.ResetSpeed();
            trappedplayer = null;
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

