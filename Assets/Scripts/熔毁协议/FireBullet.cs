using UnityEngine;
using System;
using Object = UnityEngine.Object;

public class FireBullet : MonoBehaviour
{
    public float speed;
    public Vector2 direction;
    public Action onHit;
    public int damage = 1;
    private Rigidbody2D rb;
    private float timer;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0;
    }

    public void Update()
    {
        timer += Time.deltaTime;
        if (timer >= 10f)
        {
            gameObject.SetActive(false);
            timer = 0;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
            return;
        if (other.CompareTag("Player") || other.CompareTag("Wall") || other.CompareTag("Firewall"))
        {
            if (other.CompareTag("Player"))
            {
                Player p = other.GetComponent<Player>();
                if (p != null)
                    p.TakeDamage(damage);
            }

            onHit?.Invoke();
            gameObject.SetActive(false);
        }
    }
}
