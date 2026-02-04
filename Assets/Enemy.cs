
using System.Collections;
using System.Collections.Generic;
using Unity.PlasticSCM.Editor.WebApi;
using UnityEngine;

public enum EnemyState
{
    Idle, Patrol, Chase, Attack, Dead
}


public class Enemy: MonoBehaviour
{
    // 巡逻相关变量
    public Transform[] patrolPoints;
    private int currentPatrolIndex = 0;
    public float moveSpeed = 3f;

    // 玩家目标
    private Transform player;
    IEnumerator FollowPlayer()
    {
        while (player != null)
        {
            transform.position = Vector3.MoveTowards(transform.position, player.position, moveSpeed * Time.deltaTime);
            yield return null;
        }
    }



    public bool isInIceTrap = false;
        [Header("敌人基础属性")]
        public float hp = 20f;
        public float maxHp = 20f;
        public float currentSpeed;
        public float normalSpeed = 2f;
        public int coin = 2;

        public void ResetSpeed()
    {
        currentSpeed = moveSpeed;
    }


    [Header("受击反馈")]
        public Color hitColor = Color.red;
        public float hitFlashTime = 0.1f;
        public GameObject damagePopupPrefab;

       
        
        [Header("AI配置")]
        public float chaseRange = 8f;
        public float attackRange = 3f;
        private int currentHealth;
     private EnemyState currentState;
     private float currentStateTime;
     private bool facingRight = true; // 默认朝右
     public float idleTurnInterval = 2f; // 每隔2秒转头一次


    [Header("受击反馈")]
    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private Color originalColor;
    internal bool isFrozen;


    // 检查玩家是否在视野范围内
    public class EnemyAI : MonoBehaviour
    {
        public EnemyState currentState;

        // 视野参数
        public float sightRange = 5f;       // 视野半径
        public float sightAngle = 110f;     // 扇形视野角度
        public float attackRange = 1.5f;    // 攻击范围

        // 巡逻参数
        public float patrolSpeed = 2f;
        public float patrolWaitTime = 2f;
        private Vector3 patrolTarget;
        private float patrolTimer;

        private Transform player;
        private float currentStateTime;
        private float idleTurnInterval;
        private bool facingRight;

        void Start()
        {
            player = GameObject.FindGameObjectWithTag("Player").transform;
            currentState = EnemyState.Idle; // 初始状态
            SetRandomPatrolTarget();
        }

        void Update()
        {
            switch (currentState)
            {
                case EnemyState.Idle:
                    IdleLogic();
                    break;
                case EnemyState.Patrol:
                    PatrolLogic();
                    break;
                case EnemyState.Attack:
                    AttackLogic();
                    break;
            }
        }

        // 原地
        void IdleLogic()
        {
            // 停止移动
            GetComponent<Rigidbody2D>().velocity = Vector2.zero;
            if (currentStateTime >= idleTurnInterval)
            {
                facingRight = !facingRight;
                transform.localScale = new Vector3(facingRight ? 1 : -1, 1, 1);
                currentStateTime = 0;
            }


            // 检测玩家是否在视野内
            if (IsPlayerInSight() && IsPlayerInAttackRange())
            {
                currentState = EnemyState.Attack;
            }
            else
            {
                // 随机切换到巡逻状态
                patrolTimer += Time.deltaTime;
                if (patrolTimer >= patrolWaitTime)
                {
                    currentState = EnemyState.Patrol;
                    patrolTimer = 0;
                }

            }
        }

        // 巡逻逻辑
        void PatrolLogic()
        {
            // 向巡逻目标移动
            Vector2 direction = (patrolTarget - transform.position).normalized;
            GetComponent<Rigidbody2D>().velocity = direction * patrolSpeed;

            // 到达目标后切换回Idle
            if (Vector2.Distance(transform.position, patrolTarget) < 0.2f)
            {
                currentState = EnemyState.Idle;
                SetRandomPatrolTarget();
            }

            // 检测玩家是否在视野内
            if (IsPlayerInSight() && IsPlayerInAttackRange())
            {
                currentState = EnemyState.Attack;
            }
        }

        // 攻击逻辑（你已写好的部分）
        void AttackLogic()
        {
            // 执行攻击动作（你的攻击代码）
            Debug.Log("攻击玩家！");

            // 检测玩家是否离开攻击范围
            if (!IsPlayerInSight() || !IsPlayerInAttackRange())
            {
                currentState = EnemyState.Idle; // 回到原地待命
            }
        }

        // 扇形视野检测
        bool IsPlayerInSight()
        {
            Vector2 directionToPlayer = player.position - transform.position;
            float angle = Vector2.Angle(transform.right, directionToPlayer.normalized);

            // 角度在扇形范围内 + 距离在视野内 + 无障碍物阻挡
            if (angle < sightAngle / 2 && directionToPlayer.magnitude < sightRange)
            {
                RaycastHit2D hit = Physics2D.Raycast(transform.position, directionToPlayer.normalized, sightRange);
                if (hit.collider != null && hit.collider.CompareTag("Player"))
                {
                    return true;
                }
            }
            return false;
        }

        // 攻击范围检测
        bool IsPlayerInAttackRange()
        {
            return Vector2.Distance(transform.position, player.position) <= attackRange;
        }

        // 设置随机巡逻目标
        void SetRandomPatrolTarget()
        {
            patrolTarget = transform.position + new Vector3(Random.Range(-3f, 3f), Random.Range(-3f, 3f), 0);
        }
    }


   

    // 受伤
    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
    }

   

   

    

    

   
       //受击反馈
  //受伤红闪
            IEnumerator FlashCoroutine()
            {
                sr.color = hitColor;
                yield return new WaitForSeconds(hitFlashTime);
                sr.color = originalColor;
            }
  //弹出伤害
            void ShowDamagePopup(int damage)
            {
                if (damagePopupPrefab == null) return;
                GameObject popup = Instantiate(damagePopupPrefab, transform.position, Quaternion.identity);
                popup.GetComponent<DamagePopup>().SetDamage(damage);
                Destroy(popup, 1f);
            }
  //碰撞检测
            void OnCollisionEnter2D(Collision2D other)
            {
                if (other.collider.CompareTag("Wall") || other.collider.CompareTag("Trap"))
                {
                    rb.velocity = Vector2.zero;
                }
            }
        }

  
