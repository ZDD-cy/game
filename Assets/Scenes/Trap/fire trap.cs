using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static firetrap;


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
   
    //接口
    public interface IDamageable

{
    // 所有可被攻击的对象都必须实现这个方法
    void TakeDamage(float damage);

    // 所有可以被燃烧的对象都必须实现这个方法
    void ApplyBurn(float burnDamage, float burnDuration);

    // 所有可以被减速的对象都必须实现这个方法
    void ApplySlow(float slowAmount, float slowDuration);
}

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
        if (other.CompareTag("player") && _isTrapActive)
        {
            Player player = other.GetComponent<Player>();
            if (player != null)
            {
                player.TakeDamage(triggerDamage);
                player.ApplyBurn(burnDamage, burnDuration);
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
            if (col.CompareTag("player"))
            {
                Player player = col.GetComponent<Player>();
                if (player != null)
                {
                    player.TakeDamage((int)aoeDamage);
                    player.ApplyBurn(burnDamage, burnDuration);
                }
            }
        }
    }
}

