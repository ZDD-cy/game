using UnityEngine;

public class EnemyAttackDOT : MonoBehaviour
{
    [Header("DOT 设置")]
    public float damagePerSecond = 2f;
    public float dotDuration = 5f;

    // 这个字段是给 EnemyAutoTarget 用来设置目标的
    public Transform target;

    private float _timer;
    private bool _isDoT;
    private Player _targetPlayer;
    private EnemyAutoTarget _autoTarget;

    private void Start()
    {
        _autoTarget = GetComponent<EnemyAutoTarget>();
    }

    private void Update()
    {
        // DOT 伤害逻辑
        if (_isDoT && _targetPlayer != null)
        {
            if (_timer < dotDuration)
            {
                _timer += Time.deltaTime;
                _targetPlayer.hp -= damagePerSecond * Time.deltaTime;
                 Debug.Log($"对玩家造成 {damagePerSecond * Time.deltaTime} 点伤害");
            }
            else
            {
                _isDoT = false;
                 Debug.Log("DOT效果结束");
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 只对当前锁定的目标触发DOT
        if (other.CompareTag("Player") && other.transform == target)
        {
            _targetPlayer = other.GetComponent<Player>();
            if (_targetPlayer != null)
            {
                ApplyDOT(damagePerSecond, dotDuration);
                Debug.Log("DOT启动，对锁定目标造成伤害");
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player") && other.transform == target)
        {
            _isDoT = false;
            _targetPlayer = null;
            Debug.Log("目标离开，DOT停止");
        }
    }

    public void ApplyDOT(float damage, float time)
    {
        damagePerSecond = damage;
        dotDuration = time;
        _timer = 0f;
        _isDoT = true;
    }
}
