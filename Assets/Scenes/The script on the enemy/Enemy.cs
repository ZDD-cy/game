
using System.Collections;
using System.Collections.Generic;
using Unity.PlasticSCM.Editor.WebApi;
using UnityEngine;
using static firetrap;
public enum EnemyState
{
    Patrol, // 巡逻
    Stay    // 停留原地
}

[RequireComponent(typeof(Rigidbody2D), typeof(SpriteRenderer))]
public class Enemy : MonoBehaviour
{
    // ============== 基础配置 ==============
    [Header("基础属性")]
    public float moveSpeed = 5f;       // 移速
    public float hp = 100f;         // 血量
    private float currentHp;

    
    // ============== 巡逻-停留状态配置 ==============
    [Header("巡逻-停留参数")]
    public EnemyState currentState;    // 当前状态（初始设为巡逻）
    public float patrolRadius = 8f;    // 扇形巡逻半径
    public float patrolAngle = 120f;   // 扇形巡逻角度
    public float patrolDuration = 4f;  // 巡逻持续时长（到时间切换停留）
    public float stayDuration = 2f;    // 停留持续时长（到时间切换巡逻）
    private float stateTimer;          // 状态计时器
    private Vector2 patrolTarget;      // 巡逻目标点

    // ============== 组件引用 ==============
    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private Transform player;
    private DamagePopup damagePopup;

    //void ShowDamagePopup(float damage)
    //{
    //    // 如果没有预制体，直接返回
    //    if (damagePopupPrefab == null) return;

    //    // 在敌人位置生成伤害弹出
    //    GameObject popup = Instantiate(damagePopupPrefab, transform.position, Quaternion.identity);
    //    popup.GetComponent<DamagePopup>().SetDamage(Mathf.RoundToInt(damage));
    //    Destroy(popup, 1f);
    //}
    void Start()
    {
        damagePopup =GetComponent<DamagePopup>();
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
    }


    void FixedUpdate()
    {
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
    }

        public GameObject damagePopupPrefab; // 伤害数字预制体

        // 新增 TakeDamage 方法
        public void TakeDamage(int damage)
        {
            // 扣血
            currentHp -= damage;
            // 显示伤害数字
            ShowDamagePopup(damage);
            // 判断是否死亡
            if (currentHp <= 0)
            {
                Die();
            }
        }

        // 辅助方法：显示伤害数字
        private void ShowDamagePopup(float damage)
        {
            if (damagePopupPrefab != null)
            {
                GameObject popup = Instantiate(damagePopupPrefab, transform.position, Quaternion.identity);
                popup.GetComponent<DamagePopup>().SetDamage(Mathf.RoundToInt(damage));
                Destroy(popup, 1f);
            }
        }

        // 辅助方法：处理死亡逻辑
        private void Die()
        {
            // 可以在这里添加死亡动画、掉落物品等逻辑
            Destroy(gameObject);
        }

    }






















