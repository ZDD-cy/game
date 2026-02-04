using System;
using System.Collections;
using System.Collections.Generic;
using Unity.PlasticSCM.Editor.WebApi;
using UnityEngine;

public enum EnemyState
{
    Patrol, Chase, Attack, Dead
}


public class Enemy: MonoBehaviour
{
    // 巡逻相关变量
    public Transform[] patrolPoints;
    private int currentPatrolIndex = 0;
    public float moveSpeed = 3f;

    // 玩家目标
    private Transform player;


        public bool isInIceTrap = false;
        [Header("敌人基础属性")]
        public float hp = 20f;
        public float maxHp = 20f;
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
        
        [Header("AI配置")]
        public float chaseRange = 8f;
        public float attackRange = 3f;
        private int currentHealth;
    private EnemyState currentState;

    private enum EnemyState { Patrol, Chase, Attack,
            Dead
        }



        void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            sr = GetComponent<SpriteRenderer>();
            originalColor = sr.color;
        }

        void Start()
        {
            player = GameObject.FindGameObjectWithTag("Player")?.transform;
            if (player != null)
            {
                StartCoroutine(FollowPlayer());
                player = GameObject.FindGameObjectWithTag("Player").transform;
            }
        }

    void Update()
    {
        if (currentHealth <= 0) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        switch (currentState)
        {
            case EnemyState.Patrol:
                Enemy.Patrol();
                break;
            case EnemyState.Chase:
                Enemy.ChasePlayer();
                break;
            case EnemyState.Attack:
                Enemy.AttackPlayer();
                break;
                StateTimeCheck();  //状态超时自动回复
                switch (currentState)
                {
                    case EnemyState.Patrol:
                        Patrol1();
                        break;
                    case EnemyState.Chase:
                        ChasePlayer1();
                        break;
                    case EnemyState.Attack:
                        AttackPlayer1();
                        break;
                }

                if (distanceToPlayer <= attackRange)
                {
                    currentState = EnemyState.Attack;
                }
                else if (distanceToPlayer <= chaseRange)
                {
                    currentState = EnemyState.Chase;
                }
                else
                {
                    currentState = EnemyState.Patrol;
                }

        }
    }
    
private void Patrol1()
    {
        if (patrolPoints == null || patrolPoints.Length == 0) return;

        Transform targetPoint = patrolPoints[currentPatrolIndex];
        transform.position = Vector3.MoveTowards(transform.position, targetPoint.position, moveSpeed * Time.deltaTime);
        transform.LookAt(targetPoint);

        if (Vector3.Distance(transform.position, targetPoint.position) < 0.5f)
        {
            currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
        }
    }
    private void ChasePlayer1()
    {
        if (player == null) return;

        transform.position = Vector3.MoveTowards(transform.position, player.position, moveSpeed * Time.deltaTime);
        transform.LookAt(player);
    }
    private void AttackPlayer1()
    {
        if (player == null) return;
        transform.LookAt(player);
        GetComponent<Attack>()?.StartLockOn(player);
    }



    // 受伤
    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
    }

    // 死亡
   

    private static void AttackPlayer()
        {
            throw new NotImplementedException();
        }

        private static void ChasePlayer()
        {
            throw new NotImplementedException();
        }

        private static void Patrol()
        {
            throw new NotImplementedException();
        }

        private void StateTimeCheck()
        {
            throw new NotImplementedException();
        }

       //陷阱受击逻辑
        public void TakeDamage(int damage, bool isKnockback = false, float knockbackForce = 5f)
        {
            if (hp <= 0) return;

            hp -= damage;
            ShowDamagePopup(damage);    //头顶受击
            HitFlash();       //播放受击闪烁

            if (isKnockback && rb != null)
            {
                Vector2 knockDir = (transform.position - player.position).normalized;
                rb.AddForce(knockDir * knockbackForce, ForceMode2D.Impulse);
            }

        }
        

        private void Die()
        {
            // 停止所有行为
            currentState = EnemyState.Dead;
            // 通知 GameManager 更新敌人数量
            GameManager.Instance?.UpdateEnemyCount(-1);
            // 销毁敌人对象
            Destroy(gameObject, 0.5f);
        }
        private void HitFlash()
        {
            // 获取 SpriteRenderer 组件
            SpriteRenderer sr = GetComponent<SpriteRenderer>();
            if (sr == null) return;

            // 保存原始颜色
            Color originalColor = sr.color;
            // 设置为白色
            sr.color = Color.white;
            // 0.1秒后恢复颜色
            Invoke(nameof(ResetColor), 0.1f);
        }

        // 恢复原始颜色
        private void ResetColor()
        {
            SpriteRenderer sr = GetComponent<SpriteRenderer>();
            if (sr == null) return;
            sr.color = originalColor;
        }


        private void ShowDamagePopup(int damage)
        {
            // 这里可以替换为你自己的伤害弹窗预制体
            Debug.Log($"Enemy took {damage} damage!");

            // 如果有预制体，可以这样实例化：
            // GameObject popup = Instantiate(damagePopupPrefab, transform.position, Quaternion.identity);
            // popup.GetComponent<TextMesh>().text = damage.ToString();
        }

        public void ApplyBurn(int burnDamage, float duration)
        {
            StartCoroutine(BurnCoroutine(burnDamage, duration));
        }
        private IEnumerator BurnCoroutine(int burnDamage, float duration)
        {
            float timer = 0f;
            while (timer < duration && hp > 0)
            {
                // 每0.5秒造成一次燃烧伤害
                hp -= burnDamage;
                ShowDamagePopup(burnDamage);
                HitFlash();
                yield return new WaitForSeconds(0.5f);
                timer += 0.5f;
            }
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

    


