using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Enemy : MonoBehaviour
{
    public bool isInIceTrap = false;
    [Header("敌人基础属性")]
    public float hp = 20f;
    public float maxHp = 100f;
    public float moveSpeed = 3f;
    public float currentSpeed;
    public int coin = 2;

    [Header("状态标记")]
    [HideInInspector] public bool isFrozen;
    [HideInInspector] public bool isBurning;
    [HideInInspector] public bool isInTrap;
    [HideInInspector] public bool isPulled;

    [Header("受击反馈")]
    public Color hitColor = Color.red;
    public float hitFlashTime = 0.1f;
    public GameObject damagePopupPrefab;

    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private Color originalColor;
    private float burnTimer;
    private float freezeTimer;
    private int currentBurnDamage;
    private Transform player;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        originalColor = sr.color;
        currentSpeed = moveSpeed;
    }

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (player != null)
        {
            StartCoroutine(FollowPlayer());
        }
    }

    void Update()
    {
        StateTimeCheck();
    }

    private void StateTimeCheck()
    {
        throw new NotImplementedException();
    }

    public void TakeDamage(int damage, bool isKnockback = false, float knockbackForce = 5f)
    {
        if (hp <= 0) return;

        hp -= damage;
        ShowDamagePopup(damage);
        HitFlash();

        if (isKnockback && rb != null)
        {
            Vector2 knockDir = (transform.position - player.position).normalized;
            rb.AddForce(knockDir * knockbackForce, ForceMode2D.Impulse);
        }

        if (hp <= 0)
        {
            GradientMode();
        }
    }

    private void GradientMode()
    {
        throw new NotImplementedException();
    }

    private void HitFlash()
    {
        throw new NotImplementedException();
    }

    private void ShowDamagePopup(int damage)
    {
        throw new NotImplementedException();
    }

    public void ApplyBurn(int burnDamage, float duration)
    {
        currentBurnDamage = burnDamage;
        burnTimer = duration;

        if (!isBurning)
        {
            isBurning = true;
            Coroutine coroutine = StartCoroutine(BurnCoroutine());
        }
    }

    private string BurnCoroutine()
    {
        throw new NotImplementedException();
    }

    public void ApplyFreeze(float slowRate, float duration)
    {
        isFrozen = true;
        freezeTimer = duration;
        currentSpeed = moveSpeed * (1 - slowRate);
        sr.color = new Color(0.6f, 1f, 1f);
    }

    public void ApplyPull(Vector2 trapPos, float pullForce, float pullRadius)
    {
        if (isPulled || hp <= 0) return;

        isPulled = true;
        StartCoroutine(PullCoroutine(trapPos, pullForce, pullRadius));
    }

    private string PullCoroutine(Vector2 trapPos, float pullForce, float pullRadius)
    {
        throw new NotImplementedException();
    }

    public void ResetAllStates()
    {
        isFrozen = false;
        isBurning = false;
        isPulled = false;
        isInTrap = false;

        currentSpeed = moveSpeed;
        burnTimer = 0;
        freezeTimer = 0;
        sr.color = originalColor;

        if (rb != null)
        {
            rb.velocity = Vector2.zero;
        }
    }
    public void ResetSpeed()
    {
        if (!isFrozen)
        {
            currentSpeed = moveSpeed;
        }
        isPulled = false;

        if (rb != null)
        {
            rb.velocity = Vector2.zero;
        }
    }


    IEnumerator FollowPlayer()
    {
        while (hp > 0 && player != null)
        {
            if (isPulled || isInTrap) yield return null;
            if (player == null) break;


            Vector2 moveDir = (player.position - transform.position).normalized;
            rb.velocity = moveDir * currentSpeed;


            if (moveDir.x > 0.1f) sr.flipX = false;
            else if (moveDir.x < -0.1f) sr.flipX = true;

            yield return null;
        }
        rb.velocity = Vector2.zero;

        IEnumerator BurnCoroutine()
        {
            while (isBurning && hp > 0)
            {
                if (burnTimer <= 0)
                {
                    isBurning = false;
                    sr.color = originalColor;
                    yield break;
                }

                burnTimer -= Time.deltaTime;
                if (burnTimer <= maxHp - 1f)
                {
                    TakeDamage(currentBurnDamage);
                    burnTimer = Mathf.Clamp(burnTimer, 0, maxHp);
                }
                sr.color = Color.Lerp(originalColor, Color.yellow, 0.5f);
                yield return null;
            }
            sr.color = originalColor;
            isBurning = false;
        }

        IEnumerator PullCoroutine(Vector2 trapPos, float pullForce, float pullRadius)
        {
            while (isPulled && hp > 0)
            {
                float distance = Vector2.Distance(transform.position, trapPos);
                if (distance > pullRadius || !isInTrap)
                {
                    ResetSpeed();
                    yield break;
                }

                Vector2 pullDir = (trapPos - (Vector2)transform.position).normalized;
                rb.velocity = pullDir * pullForce;
                yield return null;
            }
            ResetSpeed();
        }

        void StateTimeCheck()
        {
            if (isFrozen)
            {
                freezeTimer -= Time.deltaTime;
                if (freezeTimer <= 0)
                {
                    isFrozen = false;
                    currentSpeed = moveSpeed;
                    sr.color = originalColor;
                }
            }

            if (isBurning && burnTimer <= 0)
            {
                isBurning = false;
                sr.color = originalColor;
            }
        }

        void HitFlash()
        {
            StopCoroutine(nameof(FlashCoroutine));
            StartCoroutine(FlashCoroutine());
        }

        IEnumerator FlashCoroutine()
        {
            sr.color = hitColor;
            yield return new WaitForSeconds(hitFlashTime);
            sr.color = originalColor;
        }

        void ShowDamagePopup(int damage)
        {
            if (damagePopupPrefab == null) return;
            GameObject popup = Instantiate(damagePopupPrefab, transform.position, Quaternion.identity);
            popup.GetComponent<DamagePopup>().SetDamage(damage);
            Destroy(popup, 1f);
        }


        void Die()
        {
            ResetAllStates();
            StopAllCoroutines();
            // 播放死亡音效/特效;
            Destroy(gameObject);
        }
        void OnCollisionEnter2D(Collision2D other)
        {
            if (other.collider.CompareTag("Wall") || other.collider.CompareTag("Trap"))
            {
                rb.velocity = Vector2.zero;
            }
        }
    }

    internal void ApplySlow(float v, float revealDuration)
    {
        throw new NotImplementedException();
    }
}
