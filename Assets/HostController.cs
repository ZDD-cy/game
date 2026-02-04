using UnityEngine;

public class HostController : MonoBehaviour
{
    // 主机的核心属性
    [Header("核心属性")]
    public float maxHealth = 100.0f;     // 最大生命值
    public float currentHealth;          // 当前生命值
    public bool isAlive = true;          // 存活状态
    public HostType hostType = HostType.Domestic; // 主机类型

    // 警报相关（与之前设计的状态对应）
    [Header("警报阈值")]
    public float warningThreshold = 0.7f;  // 触发一级警报的生命值比例（70%）
    public float criticalThreshold = 0.3f;  // 触发二级警报的生命值比例（30%）
    private bool warningTriggered = false; // 一级警报是否已触发
    private bool criticalTriggered = false; // 二级警报是否已触发

    // 视觉反馈（可选）
    [Header("视觉反馈")]
    public SpriteRenderer hostSprite;    // 主机的Sprite渲染器（用于变色）
    public Color normalColor = Color.cyan;
    public Color warningColor = Color.yellow;
    public Color criticalColor = Color.red;

    // 奖励相关
    [Header("奖励设置")]
    public int computePowerReward = 10;   // 吞噬后获得的算力（经验值）
    public int dataFragmentReward = 5;    // 吞噬后获得的数据碎片（货币）

    // 事件委托（用于通知其他系统，非常重要！）
    public delegate void HostHealthChanged(float currentHealth, float maxHealth);
    public event HostHealthChanged OnHealthChanged;

    public delegate void HostDestroyed(HostController host);
    public event HostDestroyed OnDestroyed;

    void Start()
    {
        // 初始化生命值
        currentHealth = maxHealth;
        
        // 如果未指定SpriteRenderer，尝试获取
        if (hostSprite == null)
            hostSprite = GetComponent<SpriteRenderer>();
            
        // 设置初始颜色
        UpdateVisualState();
        
        Debug.Log($"{gameObject.name} 初始化完成，生命值: {currentHealth}/{maxHealth}");
    }

    // ========== 核心接口方法 ==========
    
    // PlayerAttackController 需要的伤害方法
    public void TakeDamage(float damage)
    {
        if (!isAlive) return; // 如果已经死亡，不再受到伤害

        // 应用伤害
        currentHealth -= damage;
        
        // 确保生命值不超出范围
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        
        // 更新视觉状态
        UpdateVisualState();
        
        // 触发警报检查
        CheckAlarmState();
        
        // 触发生命值变化事件
        OnHealthChanged?.Invoke(currentHealth, currentHealth / maxHealth);
        
        // 检查死亡
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    // 检查并触发警报状态
    void CheckAlarmState()
    {
        float healthRatio = currentHealth / maxHealth;
        
        // 检查一级警报（受威胁）
        if (!warningTriggered && healthRatio <= warningThreshold)
        {
            warningTriggered = true;
            TriggerWarningAlarm();
        }
        
        // 检查二级警报（严重受损）
        if (!criticalTriggered && healthRatio <= criticalThreshold)
        {
            criticalTriggered = true;
            TriggerCriticalAlarm();
        }
    }

    // 一级警报触发
    void TriggerWarningAlarm()
    {
        Debug.Log($"{gameObject.name} 触发一级警报！生命值低于 {warningThreshold * 100}%");
        
        // 这里可以：
        // 1. 改变行为状态
        // 2. 召唤巡逻者（未来实现）
        // 3. 播放警报音效
        // 4. 通知其他系统
        
        // 示例：播放一个简单的特效
        // 可以在Inspector中拖入一个粒子系统
    }

    // 二级警报触发
    void TriggerCriticalAlarm()
    {
        Debug.Log($"{gameObject.name} 触发二级警报！生命值低于 {criticalThreshold * 100}%");
        
        // 这里可以：
        // 1. 强化警报行为
        // 2. 召唤更多/更强的巡逻者
        // 3. 准备断网逻辑
    }

    // 死亡/被吞噬
    void Die()
    {
        if (!isAlive) return;
        
        isAlive = false;
        Debug.Log($"{gameObject.name} 被吞噬！");
        
        // 1. 触发被销毁事件
        OnDestroyed?.Invoke(this);
        
        // 2. 提供奖励（这里需要连接到你的游戏管理系统）
        // GameManager.Instance.AddComputePower(computePowerReward);
        // GameManager.Instance.AddDataFragments(dataFragmentReward);
        
        // 3. 播放死亡/吞噬动画（未来实现）
        // 4. 标记为"肉鸡"节点（未来实现）
        
        // 5. 销毁对象（或设置为不活跃）
        // 暂时先销毁
        Destroy(gameObject, 0.1f); // 延迟0.1秒销毁，确保所有事件被处理
    }

    // 更新视觉状态
    void UpdateVisualState()
    {
        if (hostSprite == null) return;
        
        float healthRatio = currentHealth / maxHealth;
        
        if (healthRatio > warningThreshold)
        {
            // 健康状态
            hostSprite.color = normalColor;
        }
        else if (healthRatio > criticalThreshold)
        {
            // 警告状态
            hostSprite.color = warningColor;
        }
        else
        {
            // 危险状态
            hostSprite.color = criticalColor;
        }
    }

    // ========== 工具方法 ==========
     // 获取生命值百分比
    public float GetHealthPercentage()
    {
        return currentHealth / maxHealth;
    }
    
    // 重置主机（可用于关卡重置）
    public void ResetHost()
    {
        currentHealth = maxHealth;
        isAlive = true;
        warningTriggered = false;
        criticalTriggered = false;
        UpdateVisualState();
    }
}

// 主机类型枚举（扩展用）
public enum HostType
{
    Domestic,      // 家用主机（无攻击能力）
    Enterprise,    // 企业服务器（基础防御）
    Military,      // 军用网络（主动攻击）
    Firewall,      // 防火墙（特殊）
    Router         // 路由器（特殊）
}
