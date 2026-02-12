using UnityEngine;
using System;

public class FireBullet : MonoBehaviour
{
    public float speed;
    public Vector2 direction;
    public Action onHit;
    public int damage = 1;
    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        
    }

    public void Shoot(Vector2 dir, float spd)
    {
        direction = dir;
        speed = spd;
        rb.velocity = direction * speed;
        gameObject.SetActive(true);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
            return;
        if (other.CompareTag("player") || other.CompareTag("Wall") || other.CompareTag("Firewall"))
        {
            if (other.CompareTag("player"))
            {
                PlayerController p = other.GetComponent<PlayerController>();
                if (p != null)
                    p.TakeDamage(damage);
            }

            onHit?.Invoke();
            gameObject.SetActive(false);
        }
    }
}
