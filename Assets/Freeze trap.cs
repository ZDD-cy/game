using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Freezetrap : MonoBehaviour
{   
    [Header("±ù¶³ÏÝÚåÅäÖÃ")]
    public float slowDownRate = 0.6f; 
    public float damagePerSec = 2f;   
    public float freezeDuration = 3f; 
    public float trapCD = 1f;         

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
            if (enemy != null && !enemy.isFrozen)
            {
                enemy.isInIceTrap = true;
                enemy.isFrozen = true;
               
                enemy.currentSpeed = enemy.moveSpeed * (1 - slowDownRate);
                
                StartCoroutine(EnemyTakeDamage(enemy));
                
                _isTrapActive = false;
            }
        }
    }

   
    void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            Enemy enemy = other.GetComponent<Enemy>();
            if (enemy != null && !enemy.isFrozen)
            {
                enemy.isFrozen = true;
                enemy.currentSpeed = enemy.moveSpeed * (1 - slowDownRate);
            }
        }
    }

   
    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            Enemy enemy = other.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.isInIceTrap = false;
                
                Invoke(nameof(RecoverEnemySpeed), freezeDuration);

                void RecoverEnemySpeed()
                {
                    if (!enemy.isInIceTrap) 
                    {
                        enemy.isFrozen = false;
                        enemy.currentSpeed = enemy.moveSpeed;
                    }
                }
            }
        }
    }


    IEnumerator EnemyTakeDamage(Enemy enemy)
    {
        float damageTimer = 0;
 
        while (enemy.isInIceTrap || enemy.isFrozen)
        {
            damageTimer += Time.deltaTime;
          
            if (damageTimer >= 1f)
            {
                enemy.hp -= damagePerSec;
                damageTimer = 0;
              
                if (enemy.hp <= 0)
                {
                    enemy.isFrozen = false;
                    enemy.currentSpeed = enemy.moveSpeed;
                    yield break;
                }
            }
            yield return null;
        }
    }
}


