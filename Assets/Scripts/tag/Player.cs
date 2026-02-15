using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    //陷阱适配
    public float currentSpeed;
    public float hp;
    public bool isFrozen;
    public bool isBurning;
    public bool isInIceTrap;
    public bool isInFireTrap;
    public GameObject damagePopupPrefab;
    public bool InBossScene;
    public BossfightOverlayManager BOM;
    public Slider healthBar;
    public Camera cam;
    public void ResetSpeed()
    {
        currentSpeed = moveSpeed;
    }

    // 新增 ApplySlow 方法
    public void ApplySlow(float slowAmount, float duration)
    {
        Debug.Log($"减速效果被调用：速率{slowAmount},持续{duration}秒");
        StartCoroutine(SlowCoroutine(slowAmount, duration));
    }

    // 减速协程
    private IEnumerator SlowCoroutine(float slowAmount, float duration)
    {
        currentSpeed = moveSpeed * (1 - slowAmount);
        yield return new WaitForSeconds(duration);
        currentSpeed = moveSpeed; // 恢复原速度
    }

    // 新增 ApplyBurn 方法（燃烧持续伤害）
    public void ApplyBurn(float damagePerSecond, float duration)
    {
        StartCoroutine(BurnCoroutine(damagePerSecond, duration));
    }

    // 燃烧协程
    private IEnumerator BurnCoroutine(float damagePerSecond, float duration)
    {
        float timer = 0f;
        while (timer < duration)
        {
            TakeDamage((int)(damagePerSecond * Time.deltaTime));
            timer += Time.deltaTime;
            yield return null;
        }
    }

    [Header("移动设置")]
    [SerializeField] public float moveSpeed = 3f;

    [Header("属性设置")]
    [SerializeField] public int maxHealth = 100;
    public float currentHealth;

    // 组件引用
    public Rigidbody2D rb;
    public Vector2 movement;

    void Start()
    {
        // 获取组件
        rb = GetComponent<Rigidbody2D>();

        // 初始化血量
        currentHealth = maxHealth;
        Debug.Log("玩家初始化完成，生命值: " + currentHealth + "/" + maxHealth);
        
        // 初始化速度
        ResetSpeed();
        healthBar.value = currentHealth;
    }

    void Update()
    {
        // 获取WASD输入
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");

        // 确保斜向移动速度和正交移动速度一致
        if (movement.magnitude > 1)
        {
            movement.Normalize();
        }
        healthBar.value = currentHealth / maxHealth;
    }

    void FixedUpdate()
    {
        // 应用移动
        rb.MovePosition(rb.position + movement * (currentSpeed * Time.fixedDeltaTime));
    }

    // 受伤
    public void TakeDamage(float damage)
    {
        currentHealth = (int)Mathf.Max(0, currentHealth - damage);

        if (currentHealth <= 0)
        {
            Die();
        }
        if (damagePopupPrefab != null)
        {
            // 在玩家头顶生成伤害弹窗
            Vector3 spawnPos = transform.position + new Vector3(0, 1.5f, 0);
            GameObject popup = Instantiate(damagePopupPrefab, spawnPos, Quaternion.identity);

            // 调用预制体上的脚本设置伤害值
            if (popup.TryGetComponent<DamagePopup>(out var dp))
            {
                dp.SetDamage((int)damage); // 传入伤害值
            }
        }
        if (currentHealth <= 0)
        {
            Die();
        }
    }


    private void Die()
    {
        Debug.Log("玩家死亡！");
        BOM.FailFight();
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
