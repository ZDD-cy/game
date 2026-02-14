using UnityEngine;
using UnityEngine.UI;

public class EnemyStatus : MonoBehaviour
{
    [Header("基础属性")]
    public int hp = 20;
    public float currentHp;
    public string enemyName = "敌人";

    [Header("Debuff默认配置")]
    public int debuffPerSecDamage = 1;
    public float debuffDuration = 4f;
    public float popuptimer;

    [Tooltip("伤害数字每多少秒出现一次")]
    private float damageAccumulatedForDisplay = 0f; // 累计待显示的伤害
    public float damageThresholdForDisplay = 20f;   // 每累计满20点才显示一次


    private int currentDebuffLayer;
    private float debuffTimer;

    public GameObject damagePopupPrefab;
    public bool isDead = false;
    public Slider healthBar;
    public Canvas Rendercanvas;
    public Camera cam;

    private void Start()
    {
        currentHp = hp;
        healthBar.value = currentHp;
        GameObject RenderCamera = GameObject.FindGameObjectWithTag("MainCamera");
        if (RenderCamera != null)
        {
            cam = RenderCamera.GetComponent<Camera>();
        }
        else
        {
            Debug.LogError("No Main Camera!");
        }
        Rendercanvas.worldCamera = cam;
    }
    public float GetLastHp()
    {
        return currentHp;
    }
    void Update()
    {
        if (debuffTimer > 0)
        {
            debuffTimer -= Time.deltaTime;

            // 每帧计算这一帧的 Debuff 伤害
            float damageThisFrame = debuffPerSecDamage * Time.deltaTime;

            // 直接扣血
            currentHp = Mathf.Max(currentHp - damageThisFrame, 0);

            // 把这一帧的伤害加到“待显示累计值”里
            damageAccumulatedForDisplay += damageThisFrame;

            // 当累计伤害 >= 阈值时，一次性显示
            if (damageAccumulatedForDisplay >= damageThresholdForDisplay)
            {
                int displayDamage = Mathf.RoundToInt(damageAccumulatedForDisplay);
                ShowDamagePopup(displayDamage);
                damageAccumulatedForDisplay = 0f; // 重置累计
            }

            // 检查是否死亡
            if (currentHp <= 0 && !isDead)
            {
                Die();
            }

            // Debuff 结束时，把剩余的累计伤害也显示出来
            if (debuffTimer <= 0)
            {
                currentDebuffLayer = 0;
                if (damageAccumulatedForDisplay > 0)
                {
                    int displayDamage = Mathf.RoundToInt(damageAccumulatedForDisplay);
                    ShowDamagePopup(displayDamage);
                    damageAccumulatedForDisplay = 0f;
                }
            }
        }
    }




    // 保留你原来的双参数方法
    public void TakeDamage(float damage, float lastHp)
    {
        if (isDead) return;

        float finalDamage = damage;
        currentHp = Mathf.Max(currentHp - finalDamage, 0);

        // 把伤害累计起来
        damageAccumulatedForDisplay += finalDamage;

        // 当累计伤害 >= 阈值时，一次性显示
        if (damageAccumulatedForDisplay >= damageThresholdForDisplay)
        {
            int displayDamage = Mathf.RoundToInt(damageAccumulatedForDisplay);
            ShowDamagePopup(displayDamage);
            healthBar.value = currentHp / hp;
            damageAccumulatedForDisplay = 0f; // 重置累计
        }

        if (currentHp <= 0)
        {
            Die();
        }
    }

    public void AddDebuff()
    {
        currentDebuffLayer++;
        debuffTimer = debuffDuration;
    }

    private void Die()
    {
        isDead = true;
        gameObject.SetActive(false);
        FindObjectOfType<PlayerTargetAttack>().ClearTarget();
        WallController[] allWalls = FindObjectsOfType<WallController>();
        foreach (WallController wall in allWalls)
        {
            wall.CheckAllEnemiesDead();
        }
    }

    public bool HasDebuff()
    {
        return currentDebuffLayer > 0 && debuffTimer > 0;
    }

    private void ShowDamagePopup(float damage)
    {
        if (damagePopupPrefab != null)
        {
            Vector3 popupPosition = transform.position + new Vector3(0, 1f, 0);
            GameObject popup = Instantiate(damagePopupPrefab, popupPosition, Quaternion.identity);
            DamagePopup popupScript = popup.GetComponent<DamagePopup>();
            if (popupScript != null)
            {
                popupScript.SetDamage(damage);
            }
        }
    }
}
