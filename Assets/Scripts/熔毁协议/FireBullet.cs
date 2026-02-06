using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class FireBullet : MonoBehaviour
{
    public float speed;
    public Vector2 direction;
    public Action onHit; // 命中回调
    public int damage = 1;
    private Rigidbody2D rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        rb.velocity = direction * speed;
        Destroy(gameObject, 5f); // 保底销毁
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("player"))
        {
            other.GetComponent<PlayerController>().TakeDamage(damage);
            onHit?.Invoke(); // 触发命中回调（生成地砖）
        }
        else if (other.CompareTag("Wall") || other.CompareTag("Firewall"))
        {
            onHit?.Invoke();
        }
    }
}
