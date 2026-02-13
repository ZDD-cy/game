using System.Collections;
using UnityEngine;

public class EnemyAttackDOT : MonoBehaviour
{
    [Header("DOT 设置")]
    public float damagePerSecond = 20f; // 每秒伤害
    public float dotDuration = 3f;       // 每次DOT持续时间
    public float damageInterval = 0.5f;  // 每0.5秒触发一次伤害

    // 这个target会被EnemyAutoTarget脚本自动赋值
    public Transform target;            // 敌人锁定的目标

    private Player _targetPlayer;
    private Coroutine _dotCoroutine;
    private EnemyAutoTarget _autoTarget;

    private void Start()
    {
        // 获取自动索敌组件
        _autoTarget = GetComponent<EnemyAutoTarget>();
        if (_autoTarget != null)
        {
            // 初始时同步一次目标
            target = _autoTarget.currentTarget;
        }
    }

    private void Update()
    {
        // 每帧同步目标，确保和EnemyAutoTarget的currentTarget一致
        if (_autoTarget != null && target != _autoTarget.currentTarget)
        {
            target = _autoTarget.currentTarget;
            _targetPlayer = target?.GetComponent<Player>();
        }
    }

    /// <summary>
    /// 应用持续伤害效果（保持你原有的方法签名）
    /// </summary>
    /// <param name="damagePerSecond">每秒伤害</param>
    /// <param name="duration">持续时间</param>
    public void ApplyDOT(float damagePerSecond, float duration)
    {
        this.damagePerSecond = damagePerSecond;
        this.dotDuration = duration;

        // 从当前锁定的target获取玩家
        _targetPlayer = target?.GetComponent<Player>();

        if (_targetPlayer != null)
        {
            if (_dotCoroutine != null)
                StopCoroutine(_dotCoroutine);
            _dotCoroutine = StartCoroutine(DOTCoroutine());
            Debug.Log($"✅ 应用DOT: 每秒{damagePerSecond}点伤害，持续{duration}秒，目标: {_targetPlayer.name}");
        }
        else
        {
            Debug.LogWarning("❌ ApplyDOT 失败: 当前没有锁定的玩家目标");
        }
    }

    /// <summary>
    /// DOT协程：即使目标短暂失效，也会完成剩余伤害
    /// </summary>
    private IEnumerator DOTCoroutine()
    {
        float timer = 0;
        while (timer < dotDuration)
        {
            if (_targetPlayer != null)
            {
                float damage = damagePerSecond * damageInterval;
                _targetPlayer.TakeDamage(damage);
                Debug.Log($"💥 对玩家造成 {damage:F2} 点伤害");
            }
            else
            {
                Debug.LogWarning("⚠️ DOT协程中目标丢失，但继续执行...");
            }

            yield return new WaitForSeconds(damageInterval);
            timer += damageInterval;
        }

        _dotCoroutine = null;
        Debug.Log("🔚 DOT结束");
    }

    // 清空之前的触发器逻辑，不再使用
    private void OnTriggerEnter2D(Collider2D other) { }
    private void OnTriggerExit2D(Collider2D other) { }
}
