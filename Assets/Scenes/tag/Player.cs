using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("移动设置")]
    [SerializeField] private float moveSpeed = 3f;

    [Header("属性设置")]
    [SerializeField] private int maxHealth = 20;
    private int currentHealth;

    // 组件引用
    private Rigidbody2D rb;
    private Vector2 movement;

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
    public void TakeDamage(int damage)
    {
        currentHealth = Mathf.Max(0, currentHealth - damage);
        Debug.Log("受到 " + damage + " 点伤害！当前生命值: " + currentHealth + "/" + maxHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log("玩家死亡！");
        gameObject.SetActive(false);
    }
}
