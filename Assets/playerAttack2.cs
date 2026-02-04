using UnityEngine;


    // ... [保持原有变声明不变] ...
public class PlayerAttackController : MonoBehaviour
{
    // 1. 公开变量：可以在Unity编辑器里调整
    [Header("攻击设置")]
    public float attackRange = 10f;           // 攻击范围
    public float damagePerSecond = 20f;       // 每秒伤害
    public KeyCode attackKey = KeyCode.Mouse0; // 攻击按键（默认鼠标左键）

    [Header("图层设置")]
    public LayerMask targetLayer;            // 主机所在的图层
    public LayerMask wallLayer;              // 防火墙所在的图层

    [Header("引用")]
    public Transform attackPoint;            // 攻击起始点（如果不设置，默认用玩家位置）

    // 2. 私有变量：内部使用
    private GameObject currentTarget;        // 当前攻击的目标
    private bool isAttacking = false;        // 是否正在攻击
    private int currentLinkId = -1;          // 当前连接线的ID
    void Start()
    {
        Debug.Log("[PlayerAttackController] 脚本已启动");
        if (attackPoint == null) attackPoint = transform;
        // FIX: 将LinkManager检查从Start移到具体调用处，避免启动时就报错打断游戏
    }

    void Update()
    {
        HandleInput();
        if (isAttacking)
        {
            UpdateAttack(); // FIX: 确保只有isAttacking为true时才会更新攻击
        }
    }

    void HandleInput()
    {
        // FIX: 使用更明确的逻辑，按下时尝试攻击，松开时一定停止
        if (Input.GetKeyDown(attackKey))
        {
            Debug.Log($"[按键按下] 开始攻击流程");
            StartAttack();
        }
        
        // FIX: 无论攻击是否成功建立，松开按键都应执行清理
        if (Input.GetKeyUp(attackKey))
        {
            Debug.Log($"[按键松开] 强制停止攻击");
            StopAttack();
        }
    }

    void StartAttack()
    {
        Debug.Log("[开始攻击] 阶段1: 射线检测");
        // FIX: 每次开始攻击都先重置状态，确保干净
        currentTarget = null;
        
        Vector3 mousePos = GetMouseWorldPosition();
        Vector2 attackDirection = (mousePos - attackPoint.position).normalized;
        RaycastHit2D hit = Physics2D.Raycast(attackPoint.position, attackDirection, attackRange, targetLayer);

        // ========== 关键修复区域 ==========
        if (hit.collider == null) // 1. 什么都没点中
        {
            Debug.LogWarning("[攻击失败] 射线未命中任何目标。攻击流程终止。");
            isAttacking = false; // FIX: 明确设置为false，阻止UpdateAttack执行
            return; // FIX: 直接返回，不执行后续任何代码
        }

        HostController host = hit.collider.GetComponent<HostController>();
        if (host == null) // 2. 点中了，但不是主机（比如点中了其他意外物体）
        {
            Debug.LogWarning($"[攻击失败] 目标 {hit.collider.name} 不是主机，缺少HostController组件。");
            isAttacking = false;
            return;
        }
  if (!host.isAlive) // 3. 点中了，但主机已死亡
        {
            Debug.LogWarning($"[攻击失败] 目标 {hit.collider.name} 已死亡。");
            isAttacking = false;
            return;
        }

        if (IsPathBlocked(attackPoint.position, hit.collider.transform.position)) // 4. 路径被阻挡
        {
            Debug.LogWarning($"[攻击失败] 路径被防火墙阻挡。");
            isAttacking = false;
            return;
        }
        // ========== 关键修复区域结束 ==========

        // 所有检查都通过，正式建立攻击
        Debug.Log($"[攻击建立] 所有检查通过，锁定目标: {hit.collider.name}");
        currentTarget = hit.collider.gameObject;
        isAttacking = true; // FIX: 只有在这里才设置为true
        CreateLinkToTarget();
    }

    void CreateLinkToTarget()
    {
        // FIX: 双重保险，再次检查目标
        if (currentTarget == null)
        {
            Debug.LogError("[创建连接] 错误：currentTarget为空！");
            isAttacking = false;
            return;
        }

        // FIX: 检查LinkManager是否存在
        if (LinkManager.Instance == null)
        {
            Debug.LogError("[创建连接] 错误：LinkManager实例为空！请确保场景中有LinkManager对象。");
            // 不终止攻击，但没有视觉效果
            return;
        }

        currentLinkId = LinkManager.Instance.CreateLink(
            attackPoint.position,
            currentTarget.transform.position,
            transform
        );
        
        if (currentLinkId != -1)
        {
            LinkManager.Instance.SetLinkState(currentLinkId, LinkState.Attacking);
            Debug.Log($"[创建连接] 成功，连接线ID: {currentLinkId}");
        }
        else
        {
            Debug.LogWarning("[创建连接] LinkManager返回了无效ID (-1)。");
        }
    }

    void UpdateAttack()
    {
        // FIX: 在每帧更新攻击时也进行严格的空值检查
        if (!isAttacking)
        {
            return; // 第一道防线
        }

        if (currentTarget == null)
        {
            Debug.LogWarning("[更新攻击] 当前目标丢失，停止攻击。");
            StopAttack();
            return;
        }
          HostController host = currentTarget.GetComponent<HostController>();
        if (host == null || !host.isAlive)
        {
            Debug.Log($"[更新攻击] 目标已失效。");
            StopAttack();
            return;
        }

        if (IsPathBlocked(attackPoint.position, currentTarget.transform.position))
        {
            Debug.LogWarning($"[更新攻击] 路径被新出现的墙阻挡。");
            StopAttack();
            return;
        }

        // 更新连接线位置
        if (LinkManager.Instance != null && currentLinkId != -1)
        {
            LinkManager.Instance.UpdateLinkPositions(
                currentLinkId,
                attackPoint.position,
                currentTarget.transform.position
            );
        }

        // 造成伤害
        ApplyDamageToTarget();
    }

    void ApplyDamageToTarget()
    {
        // FIX: 再次检查，确保安全
        if (currentTarget == null) return;
        
        HostController host = currentTarget.GetComponent<HostController>();
        if (host != null)
        {
            float damageThisFrame = damagePerSecond * Time.deltaTime;
            host.TakeDamage(damageThisFrame);
            // Debug.Log($"[造成伤害] {damageThisFrame:F2} 点"); // 可关闭以减少日志
        }
    }

    void StopAttack()
    {
        Debug.Log("[停止攻击] 清理所有资源");
        
        // 断开连接线
        if (LinkManager.Instance != null && currentLinkId != -1)
        {
            LinkManager.Instance.BreakLink(currentLinkId);
        }
        
        // FIX: 重置所有状态变量
        currentLinkId = -1;
        isAttacking = false;
        currentTarget = null;
    }

    // ... [IsPathBl    // 11. 检查路径是否被阻挡（关键函数）
    bool IsPathBlocked(Vector3 from, Vector3 to)
    {
        // 发射一条从起点到终点的射线，只检测墙层
        RaycastHit2D hit = Physics2D.Linecast(from, to, wallLayer);
        
        bool isBlocked = hit.collider != null;
        
        if (isBlocked)
        {
            Debug.Log($"[PlayerAttackController] 路径被阻挡，阻挡物: {hit.collider.name}");
        }
        
        return isBlocked;
    }

    // 12. 辅助函数：获取鼠标在世界坐标中的位置
    Vector3 GetMouseWorldPosition()
    {
        // 获取鼠标在屏幕上的位置
        Vector3 mouseScreenPos = Input.mousePosition;
        
        // 设置鼠标的Z坐标为相机的距离（2D游戏中通常是0）
        mouseScreenPos.z = -Camera.main.transform.position.z;
        
        // 将屏幕坐标转换为世界坐标
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(mouseScreenPos);
        
        return worldPos;
    }

    // 新增：一个调试方法，可以在编辑器中调用
    public void ForceStopAttackForDebug()
    {
        StopAttack();
        Debug.Log("[调试] 强制停止攻击已执行");
    }
}