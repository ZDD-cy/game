using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Player : MonoBehaviour
{

    public float speed = 3f;
    public int currentHealth;
    public int maxHealth = 20;

    void Start()
    {
        currentHealth = maxHealth;
    }

    void Update()
    {
        Move();

        if (Input.GetKeyDown(KeyCode.J))
        {
            Attack();
        }
    }

    void Move()
    {
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");
        Vector3 movement = new Vector3(horizontalInput, 0f, verticalInput);
        transform.Translate(movement * speed * Time.deltaTime);
    }

    void Attack()
    {
        Debug.Log("Player attacks!");
    }

    public void TakeDamage(int damage)
    {
        // 减少角色的血量
        currentHealth -= damage;
        Debug.Log($"Player takes {damage} damage. Current health: {currentHealth}");

        // 检查角色是否死亡
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    void Die()
    {
        Debug.Log("Player died!");
    }
}
