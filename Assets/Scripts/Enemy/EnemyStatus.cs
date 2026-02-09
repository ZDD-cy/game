using UnityEngine;

public class EnemyStatus : MonoBehaviour
{
    [Header("基础属性")]
    public int hp = 20; // 敌人血量，可按需改
    public float currentHp;
    [Tooltip("输入敌人名字")]
    public string enemyName = "敌人";

    [Header("Debuff默认配置")]
    public int debuffPerSecDamage = 1; // 每秒Debuff伤害（默认1点）
    public float debuffDuration = 4f; // Debuff持续时间（默认4s）

    [Tooltip("伤害数字每多少秒出现一次")]
    [SerializeField] private float popupfreq = 0.2f;
    
    private int currentDebuffLayer; // 当前Debuff层数
    private float debuffTimer; // Debuff计时（每层独立，取最大值）
    private float popuptimer;
    private float deltadamage;

    public GameObject damagePopupPrefab;      //声明伤害预制体
    
    private void Start()
    {
        currentHp = hp;
    }
//
    void Update()
    {
        // Debuff持续计时，大于0则持续掉血
        if (debuffTimer > 0)
        {
            debuffTimer -= Time.deltaTime;
            popuptimer += Time.deltaTime;
            if (popuptimer >= popupfreq) {
                ShowDamagePopup(deltadamage);
                popuptimer = 0.0f;
                deltadamage = 0.0f;
            }
            TakeDamage(debuffPerSecDamage * Time.deltaTime); // 每秒掉血，浮点型避免帧跳
        }
        else
        {
            currentDebuffLayer = 0; // 计时结束，层数清零
        }
    }

    // 接收伤害（整数/浮点型兼容）
    public void TakeDamage(float damage)
    {
      
        float lastHp = currentHp;
        float finalDamage = damage;
        if (currentHp <= 0)
        {
            currentHp = Mathf.Max(currentHp, 0);
            Die();
        }
        //扣血
        deltadamage += finalDamage;
        currentHp -= finalDamage;
        Debug.Log($"【{enemyName} - 受到伤害】受击{finalDamage:F1}点 | 血量变化：{lastHp:F1} → {currentHp:F1} | 剩余：{currentHp:F1}/{hp:F1}");
    }

    // 添加Debuff（叠加层数，重置计时）
    public void AddDebuff()
    {
        currentDebuffLayer++;
        debuffTimer = debuffDuration; // 每次叠加重置持续时间（也可改为叠加时长，按需注释）
        // 若要叠加时长，替换上面一行：debuffTimer += debuffDuration;
    }

    // 敌人死亡逻辑（清空目标、销毁/回收）
    private void Die()
    {
        // 敌人死亡：销毁/回收，按需选择
        // Destroy(gameObject); 
        gameObject.SetActive(false);
        // 通知玩家脚本清空目标（避免空引用）
        FindObjectOfType<PlayerTargetAttack>().ClearTarget();
    }

    // 获取当前是否有Debuff
    public bool HasDebuff()
    {
        return currentDebuffLayer > 0 && debuffTimer > 0;
    }
    
    private void ShowDamagePopup(float damage)
    {
        if (damagePopupPrefab != null)
        {
            GameObject popup = Instantiate(damagePopupPrefab, transform.position, Quaternion.identity);
            popup.transform.SetParent(gameObject.transform);
            popup.GetComponent<DamagePopup>().SetDamage(damage);
        }
    }
}
