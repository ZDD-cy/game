using UnityEngine;

// 敌人自动索敌、锁定玩家脚本 2D版
public class EnemyAutoTarget : MonoBehaviour
{
    [Header("索敌设置")]
    public float detectRange = 8f; // 索敌半径
    public LayerMask playerLayer; // 玩家层（选Player）
    public string playerTag = "Player"; // 玩家标签

    [Header("锁定设置")]
    public bool alwaysLock = true; // 一旦锁定就一直跟随
    public bool onlyLockInRange = true; // 只在范围内锁定

    // 外部获取当前锁定目标
    public Transform currentTarget;
    public float targetCheckTimer;
    public float checkInterval = 2f; // 每2秒检测一次目标

    private EnemyAttackDOT enemyAttackDOT;

    private void Start()
    {
        // 获取你身上的DOT攻击脚本
        enemyAttackDOT = GetComponent<EnemyAttackDOT>();
        // 初始搜索一次玩家
        FindplayerTarget();
    }

    private void Update()
    {
        targetCheckTimer += Time.deltaTime;
        if (targetCheckTimer >= checkInterval)
        {
            targetCheckTimer = 0;
            CheckTargetValid();
        }
    }

    private void CheckTargetValid()
    {
        if (currentTarget == null)
        {
            FindplayerTarget();
            return;
        }

        float dis = Vector2.Distance(transform.position, currentTarget.position);

        if (onlyLockInRange && dis > detectRange)
        {
            currentTarget = null;
            // 空检查：确保enemyAttackDOT不为空再访问
            if (enemyAttackDOT != null)
            {
                enemyAttackDOT.target = null;
            }
        }
        else
        {
            if (enemyAttackDOT != null)
            {
                enemyAttackDOT.target = currentTarget;
            }
        }
    }

    /// 自动搜索玩家目标
    public void FindplayerTarget()
    {
        GameObject player = GameObject.FindGameObjectWithTag(playerTag);

        if (player != null)
        {
            Debug.Log($"找到玩家: {player.name}", player);
            Transform playerTrans = player.transform;
            float distance = Vector2.Distance(transform.position, playerTrans.position);

            if (distance <= detectRange)
            {
                currentTarget = playerTrans;
                if (enemyAttackDOT != null) enemyAttackDOT.target = currentTarget;
                Debug.Log($"成功锁定目标: {currentTarget.name}");
            }
            else
            {
                currentTarget = null;
                if (enemyAttackDOT != null) enemyAttackDOT.target = null;
                Debug.Log("玩家在索敌范围外");
            }
        }
        else
        {
            currentTarget = null;
            if (enemyAttackDOT != null) enemyAttackDOT.target = null;
            Debug.LogError($"没有找到标签为 '{playerTag}' 的玩家对象！");
        }
    }

    // Scene 视图绘制索敌范围，方便调试
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectRange);
    }
}
