using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Collections.Generic;

public class PlayerLockEnemy : MonoBehaviour
{
    [Header("锁敌设置")]
    public float lockRange = 5f;
    public LayerMask enemyLayer;
    public KeyCode switchTargetKey = KeyCode.Tab;

    // 当前选中的攻击目标，供外部攻击脚本调用
    public Transform currentAttackTarget { get; private set; }
    private List<Transform> allLockedEnemies = new List<Transform>();
    private int currentTargetIndex = -1;

    void Start()
    {
        if (enemyLayer == 0)
        {
            Debug.LogWarning("未设置敌人层！请在Inspector面板选择Enemy层", this);
            enemyLayer = LayerMask.GetMask("Enemy");
        }
    }

    void Update()
    {
        UpdateAllLockedEnemies();
        CheckInput();
    }

    void UpdateAllLockedEnemies()
    {
        allLockedEnemies.Clear();
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(transform.position, lockRange, enemyLayer);
        foreach (Collider2D col in hitEnemies)
        {
            if (col.gameObject.activeInHierarchy)
                allLockedEnemies.Add(col.transform);
        }

        // 目标消失时清空选中状态
        if (currentAttackTarget != null && !allLockedEnemies.Contains(currentAttackTarget))
        {
            currentAttackTarget = null;
            currentTargetIndex = -1;
        }
    }

    void CheckInput()
    {
        if (Input.GetKeyDown(switchTargetKey) && allLockedEnemies.Count > 0)
        {
            SwitchAttackTarget();
        }
    }

    void SwitchAttackTarget()
    {
        currentTargetIndex = (currentTargetIndex + 1) % allLockedEnemies.Count;
        currentAttackTarget = allLockedEnemies[currentTargetIndex];
        Debug.Log($"已切换目标：{currentAttackTarget.name}");
    }

    // 外部调用：获取当前所有锁定的敌人
    public List<Transform> GetAllLockedEnemies() => allLockedEnemies;

    // Gizmos调试
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, lockRange);
        if (currentAttackTarget != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, currentAttackTarget.position);
        }
    }
}
