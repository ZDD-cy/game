
using System.Collections;
using System.Collections.Generic;
using Unity.PlasticSCM.Editor.WebApi;
using UnityEngine;
using static firetrap;
public enum EnemyState
{
    Patrol, // 巡逻
    Stay,    // 停留原地
    Chase    //追逐
}

[RequireComponent(typeof(Rigidbody2D), typeof(SpriteRenderer))]
public class Enemy : MonoBehaviour
{
    // ============== 基础配置 ==============
    [Header("基础属性")]
    public float moveSpeed = 5f;       // 移速
    public float hp = 100f;         // 血量
    private float currentHp;
    [Header("追击设置")]
    public float chaseSpeed = 7f; // 追击速度
    public float attackRange = 2f; // 攻击范围
    public float attackCooldown = 1.5f; // 攻击冷却
    private float attackTimer;

    // ============== 巡逻-停留状态配置 ==============
    [Header("巡逻-停留参数")]
    public EnemyState currentState;    // 当前状态（初始设为巡逻）
    public float patrolRadius = 8f;    // 扇形巡逻半径
    public float patrolAngle = 120f;   // 扇形巡逻角度
    public float patrolDuration = 4f;  // 巡逻持续时长（到时间切换停留）
    public float stayDuration = 2f;    // 停留持续时长（到时间切换巡逻）
    private float stateTimer;          // 状态计时器
    private Vector2 patrolTarget;      // 巡逻目标点
    public DamagePopup damagePopupPrefab;

    // ============== 组件引用 ==============
    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private Transform player;
    private DamagePopup damagePopup;
    private EnemyAutoTarget autoTarget;  

    void ShowDamagePopup(float damage)
    {
      
            if (damagePopupPrefab == null) return;
            DamagePopup popup = Instantiate(damagePopupPrefab, transform.position, Quaternion.identity);

            // 直接调用组件方法，无需 GetComponent
            popup.SetDamage(Mathf.RoundToInt(damage));

        // 销毁整个游戏对象
        Destroy(popup.gameObject, 1f);
    }

    void Start()
    {
        damagePopup = GetComponent<DamagePopup>();
        // 初始化
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        currentHp = hp;
        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        // 初始状态设为巡逻，并生成第一个巡逻点
        currentState = EnemyState.Patrol;
        GenerateSectorPatrolTarget();
        stateTimer = 0;
        Debug.Log("敌人初始状态：巡逻");
        autoTarget = GetComponent<EnemyAutoTarget>(); // 获取索敌脚本
        if (autoTarget == null)
        {
            Debug.LogError("未找到 EnemyAutoTarget 脚本！", this);
        }
    }


    void FixedUpdate()
    {
        // 优先检查：如果锁定了玩家，直接进入追击状态
        if (autoTarget != null && autoTarget.currentTarget != null)
        {
            if (currentState != EnemyState.Chase)
            {
                SwitchToState(EnemyState.Chase);
            }
            RunChaseLogic(); // 执行追击逻辑
        }
        // 状态机核心逻辑：单状态器切换（巡逻→停留→巡逻）
        switch (currentState)
        {
            case EnemyState.Patrol:
                RunPatrolLogic();
                break;
            case EnemyState.Stay:
                RunStayLogic();
                break;
        }
    }


    // ============== 巡逻状态逻辑 ==============
    private void RunPatrolLogic()
    {
        // 向巡逻点移动
        Vector2 direction = (patrolTarget - (Vector2)transform.position).normalized;
        rb.velocity = direction * moveSpeed;
        FlipToFaceDirection(direction); // 面向移动方向

        // 巡逻计时：到时间切换为停留
        stateTimer += Time.deltaTime;
        if (stateTimer >= patrolDuration)
        {
            SwitchToState(EnemyState.Stay);
        }
    }


    // ============== 停留状态逻辑 ==============
    private void RunStayLogic()
    {
        // 停止移动
        rb.velocity = Vector2.zero;

        // 停留计时：到时间切换为巡逻（生成新巡逻点）
        stateTimer += Time.deltaTime;
        if (stateTimer >= stayDuration)
        {
            GenerateSectorPatrolTarget(); // 生成新的扇形巡逻点
            SwitchToState(EnemyState.Patrol);
        }
    }



    // ============== 追击状态逻辑 ==============
    private void RunChaseLogic()
    {
        if (autoTarget.currentTarget == null) return;

        attackTimer -= Time.fixedDeltaTime;

        // 计算与玩家的距离
        float distance = Vector2.Distance(transform.position, autoTarget.currentTarget.position);

        // 1. 计算方向，并强制忽略Y轴，保证水平追击
        Vector2 direction = (autoTarget.currentTarget.position - transform.position).normalized;
        direction.y = 0; // 关键：忽略垂直方向的影响
        direction.Normalize(); // 重新归一化，确保速度大小正确

        if (distance > attackRange)
        {
            // 2. 距离大于攻击范围，向玩家直线移动
            rb.velocity = direction * chaseSpeed;

            // 3. 优化翻转逻辑，避免微小方向导致的抖动转圈
            if (Mathf.Abs(direction.x) > 0.1f)
            {
                FlipToFaceDirection(direction);
            }
        }
        else
        {
            // 距离小于等于攻击范围，停止移动并尝试攻击
            rb.velocity = Vector2.zero;
            if (attackTimer <= 0)
            {
                AttackPlayer(); // 执行攻击
                attackTimer = attackCooldown; // 重置冷却
            }
        }
    }


    // ============== 攻击玩家方法 ==============
    private void AttackPlayer()
    {
        EnemyAttackDOT attackDOT = GetComponent<EnemyAttackDOT>();
        if (attackDOT != null)
        {
            attackDOT.ApplyDOT(5f, 2f); // 示例：每秒5点伤害，持续2秒
            Debug.Log("敌人发动攻击！");
        }
    }


    // ============== 状态切换工具方法 ==============
    private void SwitchToState(EnemyState newState)
    {
        currentState = newState;
        stateTimer = 0; // 重置计时器
        Debug.Log($"敌人状态切换：{newState}");
    }


    // ============== 辅助方法：生成扇形巡逻点 ==============
    private void GenerateSectorPatrolTarget()
    {
        // 随机生成扇形内的方向（基于敌人当前朝向）
        float randomAngle = Random.Range(-patrolAngle / 2f, patrolAngle / 2f);
        Vector2 direction = Quaternion.Euler(0, 0, randomAngle) * transform.right;

        // 随机生成半径内的距离
        float randomDist = Random.Range(patrolRadius * 0.5f, patrolRadius);
        patrolTarget = (Vector2)transform.position + direction * randomDist;
    }


    // ============== 辅助方法：面向移动方向 ==============
    private void FlipToFaceDirection(Vector2 direction)
    {
        sr.flipX = direction.x < 0; // 方向向左则翻转Sprite
        if (sr != null)
        {
            // 只有当水平方向分量足够大时才翻转，避免抖动
            if (Mathf.Abs(direction.x) > 1f)
            {
                sr.flipX = direction.x < 0;
            }
        }
    }


    //修复
    void MoveTowards(Vector2 targetPos, float speed)
    {
        Vector2 direction = (targetPos - (Vector2)transform.position).normalized;
        // 如果距离小于 0.1，就停止移动
        if (Vector2.Distance(transform.position, targetPos) < 0.1f)
        {
            rb.velocity = Vector2.zero;
            return;
        }
        rb.velocity = direction * speed;
        FlipToFaceDirection(direction);
    }
   
}

























