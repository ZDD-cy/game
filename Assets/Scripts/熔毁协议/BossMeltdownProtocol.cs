using UnityEngine;
using System.Collections.Generic;

public class BossMeltdownProtocol : MonoBehaviour
{
    #region 基础属性
    [Header("基础属性")]
    public int maxHP = 500;
    public int baseATK = 5;
    private int currentHP;
    private Rigidbody2D rb;
    public Transform player;

    private bool isKilledByMe = false;

    private bool isDead = false;
    #endregion

    #region 被动-安全检测（热量槽核心）
    [Header("被动-安全检测")]
    public int maxHeat = 10;
    public int currentHeat;
    public float heatDecayDelay = 3f;
    private float lastHeatAddTime;
    private float heatDecayTimer;
    public float damageReductionPerStack = 0.05f;
    public float attackBoostPerStack = 0.05f;
    private float finalDamageReduction;
    private float finalAttackBoost;
    #endregion

    #region 被动-星火（燃烧Debuff）
    [Header("被动-星火")]
    public float burnDuration = 3f;
    public float burnDamagePerSecond = 1f;
    #endregion

    #region 技能1-特制型防火墙
    [Header("技能1-特制型防火墙")]
    public float firewallCD = 20f;
    private float firewallCDTimer;
    public float firewallCastTime = 1f;
    public float firewallDuration = 15f;
    public float firewallDisableTime = 5f;
    public GameObject firewallPrefab;
    private bool isFirewallActive;
    private float firewallActiveTimer;
    private GameObject currentFirewall;
    #endregion

    #region 技能2-火种
    [Header("技能2-火种")]
    public float fireSeedCD = 6f;
    private float fireSeedCDTimer;
    public float fireSeedCastTime = 1f;
    public GameObject fireBulletPrefab;
    public GameObject fireTilePrefab;
    public int bulletCount = 9;
    public float bulletSpeed = 7f;
    public float coneAngle = 45f;

    private List<GameObject> bulletPool = new List<GameObject>();
    #endregion

    #region 技能3-余烬
    [Header("技能3-余烬")]
    public float emberCastTime = 1f;
    public int emberBulletCount = 18;
    public float emberBulletSpeed = 6f;
    private bool isHeatZero;
    #endregion

    #region AI逻辑
    [Header("AI配置")]
    public float skillCheckInterval = 1f;
    private float skillCheckTimer;
    #endregion

    public BossfightOverlayManager BOM;
    
    private void Start()
    {
        currentHP = maxHP;
        isDead = false;

        rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.gravityScale = 0;
        }

        currentHeat = 0;
        lastHeatAddTime = Time.time;
        heatDecayTimer = 0;

        firewallCDTimer = 0;
        fireSeedCDTimer = 0;
        isFirewallActive = false;
        isHeatZero = false;
    }

    private void Update()
    {
        if (isDead || player == null) return;

        UpdateHeatSystem();
        UpdateSkillCD();
        UpdateAIskillLogic();
        CheckEmberTrigger();

        if (isFirewallActive && currentFirewall != null)
        {
            firewallActiveTimer += Time.deltaTime;
            if (firewallActiveTimer >= firewallDuration)
            {
                CloseFirewall();
            }
        }
    }

    #region 热量系统
    private void UpdateHeatSystem()
    {
        finalDamageReduction = currentHeat * damageReductionPerStack;
        finalAttackBoost = currentHeat * attackBoostPerStack;

        if (currentHeat > 0 && Time.time - lastHeatAddTime >= heatDecayDelay)
        {
            heatDecayTimer += Time.deltaTime;
            if (heatDecayTimer >= 1f)
            {
                currentHeat = Mathf.Max(0, currentHeat - 1);
                heatDecayTimer = 0;
            }
        }
    }

    public void AddHeat(int value)
    {
        if (isDead) return;
        currentHeat = Mathf.Min(maxHeat, currentHeat + value);
        lastHeatAddTime = Time.time;
        heatDecayTimer = 0;
    }
    #endregion

    #region 技能冷却
    private void UpdateSkillCD()
    {
        if (firewallCDTimer > 0) firewallCDTimer -= Time.deltaTime;
        if (fireSeedCDTimer > 0) fireSeedCDTimer -= Time.deltaTime;
    }
    #endregion

    #region AI 技能逻辑
    private void UpdateAIskillLogic()
    {
        skillCheckTimer += Time.deltaTime;
        if (skillCheckTimer < skillCheckInterval) return;
        skillCheckTimer = 0;

        if (firewallCDTimer <= 0 && !isFirewallActive)
        {
            CastFirewall();
        }
        else if (fireSeedCDTimer <= 0)
        {
            CastFireSeed();
        }
    }
    #endregion

    #region 技能1 防火墙 —— 不销毁，只隐藏复用
    private void CastFirewall()
    {
        if (isDead) return;

        firewallCDTimer = firewallCD;
        isFirewallActive = true;
        firewallActiveTimer = 0;

        if (firewallPrefab != null)
        {
            if (currentFirewall == null)
            {
                currentFirewall = Instantiate(firewallPrefab, transform.position, Quaternion.identity,  transform);
            }
            else
            {
                currentFirewall.transform.position = transform.position;
                currentFirewall.SetActive(true);
            }
        }
    }

    private void CloseFirewall()
    {
        isFirewallActive = false;
        if (currentFirewall != null)
        {
            currentFirewall.SetActive(false);
            
        }
    }
    #endregion

    #region 技能2 火种 —— 对象池复用，不销毁
    private void CastFireSeed()
    {
        if (isDead) return;

        fireSeedCDTimer = fireSeedCD;

        Vector2 dir = (player.position - transform.position).normalized;
        float baseAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        float startAngle = baseAngle - coneAngle / 2;
        float angleStep = coneAngle / (bulletCount - 1);

        for (int i = 0; i < bulletCount; i++)
        {
            float angle = startAngle + i * angleStep;
            Vector2 shootDir = new Vector2(
                Mathf.Cos(angle * Mathf.Deg2Rad),
                Mathf.Sin(angle * Mathf.Deg2Rad)
            );

            GameObject bullet = GetBulletFromPool();
            bullet.transform.position = transform.position;
            bullet.SetActive(true);

            Rigidbody2D brb = bullet.GetComponent<Rigidbody2D>();
            if (brb != null)
                brb.velocity = shootDir * bulletSpeed;
        }
    }

    private GameObject GetBulletFromPool()
    {
        foreach (var b in bulletPool)
        {
            if (!b.activeInHierarchy)
                return b;
        }

        GameObject newBullet = Instantiate(fireBulletPrefab);
        newBullet.SetActive(false);
        bulletPool.Add(newBullet);
        return newBullet;
    }
    #endregion

    #region 技能3 余烬 —— 复用子弹池
    private void CheckEmberTrigger()
    {
        if (currentHeat == 0 && !isHeatZero)
        {
            isHeatZero = true;
            CastEmber();
        }
        if (currentHeat > 0)
            isHeatZero = false;
    }

    private void CastEmber()
    {
        if (isDead) return;

        float angleStep = 360f / emberBulletCount;

        for (int i = 0; i < emberBulletCount; i++)
        {
            float angle = i * angleStep;
            Vector2 dir = new Vector2(
                Mathf.Cos(angle * Mathf.Deg2Rad),
                Mathf.Sin(angle * Mathf.Deg2Rad)
            );

            GameObject bullet = GetBulletFromPool();
            bullet.transform.position = transform.position;
            bullet.SetActive(true);

            Rigidbody2D brb = bullet.GetComponent<Rigidbody2D>();
            if (brb != null)
                brb.velocity = dir * emberBulletSpeed;
        }
    }
    #endregion

    //受伤 & 死亡 —— 只有这里真正销毁
    public void TakeDamage(int damage)
    {
        if (isDead) return;

        float realDamage = damage * (1 - finalDamageReduction);
        currentHP -= Mathf.RoundToInt(realDamage);
        currentHP = Mathf.Max(currentHP, 0);

        if (currentHP <= 0)
            Die();
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;

        Debug.Log("BOSS 已被击败！");
        Destroy(gameObject);
    }
    void OnDisable()
    {
        if(BOM!=null)BOM.PassFight();
    }
}

    //private void OnDestroy()
    //{
    //    if (!isKilledByMe)
    //    {
    //        // 打印调用栈，定位是谁在销毁 Boss
    //        Debug.LogError("⚠️ 警告：Boss 被非法销毁！调用栈：\n" + StackTraceUtility.ExtractStackTrace());
    //        #endregion


    //    }
    //}
