using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpikeTrap : MonoBehaviour
{
    [Header("¼â´ÌÏÝÚåÅäÖÃ")]
    public int damage = 3;              
    public bool isContinuousDamage = true; 
    public float damageInterval = 1f;    
    public float trapCD = 0.5f;          
    public bool hasKnockback = true;     
    public float knockbackForce = 5f;    

    private float _cdTimer;
    private float _damageTimer;
    private bool _isTrapActive = true;

    void Update()
    {
        if (!_isTrapActive)
        {
            _cdTimer += Time.deltaTime;
            if (_cdTimer >= trapCD)
            {
                _isTrapActive = true;
                _cdTimer = 0;
            }
        }

        if (isContinuousDamage)
        {
            _damageTimer += Time.deltaTime;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy") && _isTrapActive)
        {
            Enemy enemy = other.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
                if (hasKnockback)
                {
                    ApplyKnockback(enemy);
                }
                if (!isContinuousDamage)
                {
                    _isTrapActive = false;
                }
                _damageTimer = 0;
            }
        }
    }

    void OnTriggerStay2D(Collider2D other)
    {
        if (isContinuousDamage && other.CompareTag("Enemy"))
        {
            Enemy enemy = other.GetComponent<Enemy>();
            if (enemy != null && _damageTimer >= damageInterval)
            {
                enemy.TakeDamage(damage);
                _damageTimer = 0;
            }
        }
    }

    void ApplyKnockback(Enemy enemy)
    {
        Rigidbody2D rb = enemy.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            Vector2 knockbackDir = (enemy.transform.position - transform.position).normalized;
            rb.AddForce(knockbackDir * knockbackForce, ForceMode2D.Impulse);
        }
    }
}

