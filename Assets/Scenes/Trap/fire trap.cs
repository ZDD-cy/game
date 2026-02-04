using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class firetrap : MonoBehaviour
{
    [Header("火焰陷阱配置")]
    public int triggerDamage = 2;
    public int burnDamage = 1;          
    public float burnDuration = 3f;     
    public float trapCD = 1.5f;         
    public bool hasAOE = false;         
    public float aoeRange = 2f;         
    public float aoeDamage = 3;         

    private float _cdTimer;
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
    }
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy") && _isTrapActive)
        {
            Enemy enemy = other.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(triggerDamage);
                // 调用受击脚本的灼烧效果
                other.GetComponent<EnemyHitEffect>().DoFireBurn(burnDamage, burnDuration);
                if (hasAOE)
                {
                    ApplyAOEDamage();
                }
                _isTrapActive = false;
            }
        }
    }


    void ApplyAOEDamage()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, aoeRange);
        foreach (Collider2D col in colliders)
        {
            if (col.CompareTag("Enemy"))
            {
                Enemy enemy = col.GetComponent<Enemy>();
                if (enemy != null)
                {
                    enemy.TakeDamage((int)aoeDamage);
                    // 调用受击脚本的灼烧效果
                    col.GetComponent<EnemyHitEffect>().DoFireBurn(burnDamage, burnDuration);
                }
            }
        }
    }
}

