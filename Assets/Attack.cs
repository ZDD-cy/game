using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Attack : MonoBehaviour
{
    [Header("¹¥»÷ÅäÖÃ")]
    public float lockTime = 1f;         
    public GameObject bulletPrefab;     
    public Transform firePoint;        
    public LineRenderer lineRenderer;   

    private Transform targetEnemy;
    private float lockTimer;
    private bool isLocking = false;

    public void StartLockOn(Transform enemy)
    {
        targetEnemy = enemy;
        lockTimer = 0f;
        isLocking = true;
        lineRenderer.enabled = true;
        lineRenderer.material = GameManager.Instance.dashLineMaterial;
    }

    void Update()
    {
        if (isLocking && targetEnemy != null)
        {
            lineRenderer.SetPosition(0, firePoint.position);
            lineRenderer.SetPosition(1, targetEnemy.position);

            lockTimer += Time.deltaTime;
            if (lockTimer >= lockTime)
            {
                lineRenderer.material = GameManager.Instance.solidLineMaterial;
                FireBullet();
                isLocking = false;
                Invoke("HideLine", 0.1f); 
            }
        }
    }

    void FireBullet()
    {
        Vector3 direction = (targetEnemy.position - firePoint.position).normalized;
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.LookRotation(direction));
        bullet.GetComponent<Bullet>().SetDirection(direction);
    }

    void HideLine()
    {
        lineRenderer.enabled = false;
    }
}

   


