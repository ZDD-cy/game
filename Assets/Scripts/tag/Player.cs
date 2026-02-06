using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    //陷阱适配
    public float currentSpeed;
    public float hp;
    public bool isFrozen;
    public bool isInIceTrap;
      public void ResetSpeed()
    {
        currentSpeed = moveSpeed;
    }

    // 新增 ApplySlow 方法
    public void ApplySlow(float slowAmount, float duration)
    {
        StartCoroutine(SlowCoroutine(slowAmount, duration));
    }

    // 减速协程
    public IEnumerator SlowCoroutine(float slowAmount, float duration)
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
    public IEnumerator BurnCoroutine(float damagePerSecond, float duration)
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
    [SerializeField] public int maxHealth = 20;
    public int currentHealth;

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
    }

    void FixedUpdate()
    {
        // 应用移动
        rb.MovePosition(rb.position + movement * moveSpeed * Time.fixedDeltaTime);
    }

    // 受伤
    public void TakeDamage(float damage)
    {
        currentHealth = (int)Mathf.Max(0, currentHealth - damage);
        Debug.Log("受到 " + damage + " 点伤害！当前生命值: " + currentHealth + "/" + maxHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void Die()
    {
        Debug.Log("玩家死亡！");
        gameObject.SetActive(false);
    }
}
