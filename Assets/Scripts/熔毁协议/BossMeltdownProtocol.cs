using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class BossMeltdownProtocol : MonoBehaviour
{
    #region 基础属性
    [Header("基础属性")]
    public int maxHP = 500;
    public int baseATK = 5;
    private int currentHP;
    private Rigidbody2D rb;
    public Transform player; // 拖拽玩家对象
    #endregion

    #region 被动-安全检测（热量槽核心）
    [Header("被动-安全检测")]
    public int maxHeat = 10;
    public int currentHeat;
    public float heatDecayDelay = 3f;
    private float lastHeatAddTime;
    private float heatDecayTimer;
    private float damageReductionPerStack = 0.05f;
    private float attackBoostPerStack = 0.05f;
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
    public GameObject firewallPrefab; // 防火墙预制体（带BoxCollider2D+DamageZone）
    private bool isFirewallActive;
    #endregion

    #region 技能2-火种
    [Header("技能2-火种")]
    public float fireSeedCD = 6f;
    private float fireSeedCDTimer;
    public float fireSeedCastTime = 1f;
    public GameObject fireBulletPrefab; // 火焰子弹预制体（带Bullet脚本）
    public GameObject fireTilePrefab; // 火焰地砖预制体（带DamageZone）
    public int bulletCount = 9;
    public float bulletSpeed = 7f;
    public float coneAngle = 45f; // 锥形弹幕角度
    #endregion

    #region 技能3-余烬
    [Header("技能3-余烬")]
    public float emberCastTime = 1f;
    public int emberBulletCount = 18;
    public float emberBulletSpeed = 6f;
    private bool isHeatZero; // 热量归零标记
    #endregion

    #region AI逻辑
    [Header("AI配置")]
    public float skillCheckInterval = 1f;
    private float skillCheckTimer;
    #endregion

    private void Start()
    {
        // 初始化基础参数
        currentHP = maxHP;
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic; // BOSS设为运动学，防止被推动
        rb.gravityScale = 0;

        // 初始化热量槽
        currentHeat = 0;
        lastHeatAddTime = Time.time;
        heatDecayTimer = 0;

        // 初始化技能冷却
        firewallCDTimer = 0;
        fireSeedCDTimer = 0;
        isFirewallActive = false;
        isHeatZero = false;
    }

    private void Update()
    {
        if (player == null) return; // 玩家为空则停止所有逻辑

        // 核心被动更新
        UpdateHeatSystem();
        UpdateSkillCD();
        UpdateAIskillLogic();

        // 检测热量归零触发余烬
        CheckEmberTrigger();
    }

    #region 被动-热量槽核心逻辑
    // 更新热量槽（叠加/衰减/增益）
    private void UpdateHeatSystem()
    {
        // 自动更新增益
        finalDamageReduction = currentHeat * damageReductionPerStack;
        finalAttackBoost = currentHeat * attackBoostPerStack;

        // 热量衰减逻辑
        if (currentHeat <= 0) return;
        if (Time.time - lastHeatAddTime < heatDecayDelay) return;

        heatDecayTimer += Time.deltaTime;
        if (heatDecayTimer >= 1f)
        {
            currentHeat = Mathf.Max(currentHeat - 1, 0);
            heatDecayTimer = 0f;
        }
    }

    // 外部调用：叠加热量（玩家攻击/BOSS命中时）
    public void AddHeat(int amount = 1)
    {
        currentHeat = Mathf.Min(currentHeat + amount, maxHeat);
        lastHeatAddTime = Time.time;
        isHeatZero = false;
    }

    // 外部调用：BOSS受击计算（应用伤害减免）
    public float TakeDamage(float rawDamage)
    {
        float reducedDamage = rawDamage * (1 - finalDamageReduction);
        reducedDamage = Mathf.Max(reducedDamage, 1f); // 保底1点伤害
        currentHP = Mathf.Max(currentHP - (int)reducedDamage, 0);
        AddHeat(); // 玩家攻击命中，叠加热量
        if (currentHP <= 0) BossDie();
        return reducedDamage;
    }

    // 外部调用：获取BOSS强化后攻击力
    public float GetBoostedAttack()
    {
        return baseATK * (1 + finalAttackBoost);
    }
    #endregion

    #region 被动-星火：给玩家施加燃烧Debuff
    // BOSS攻击命中玩家时调用
    public void ApplyBurnToPlayer()
    {
        PlayerController playerCtrl = player.GetComponent<PlayerController>();
        if (playerCtrl != null)
        {
            playerCtrl.ApplyBurn(burnDuration, burnDamagePerSecond);
        }
    }
    #endregion

    #region 技能1-特制型防火墙
    private IEnumerator FirewallCoroutine()
    {
        isFirewallActive = true;
        yield return new WaitForSeconds(firewallCastTime); // 起手1秒

        // 生成十字防火墙（上、下、左、右，长度4/宽度1）
        Vector2[] directions = { Vector2.up, Vector2.down, Vector2.left, Vector2.right };
        foreach (var dir in directions)
        {
            GameObject wall = Instantiate(firewallPrefab, transform.position + (Vector3)dir * 2f, Quaternion.identity);
            // 设置墙体尺寸：根据方向调整宽高（上下为竖墙，左右为横墙）
            BoxCollider2D col = wall.GetComponent<BoxCollider2D>();
            col.size = dir == Vector2.up || dir == Vector2.down ? new Vector2(1f, 4f) : new Vector2(4f, 1f);
            wall.transform.SetParent(transform);
            wall.tag = "Firewall";
        }

        // 墙体持续15秒（可阻挡+可伤害）
        yield return new WaitForSeconds(firewallDuration);

        // 进入5秒失能状态（关闭碰撞和伤害）
        foreach (Transform child in transform)
        {
            if (child.CompareTag("Firewall"))
            {
                child.GetComponent<BoxCollider2D>().enabled = false;
                DamageZone dz = child.GetComponent<DamageZone>();
                if (dz != null) dz.enabled = false;
            }
        }

        // 失能结束后销毁墙体
        yield return new WaitForSeconds(firewallDisableTime);
        foreach (Transform child in transform)
        {
            if (child.CompareTag("Firewall")) Destroy(child.gameObject);
        }

        isFirewallActive = false;
    }
    #endregion

    #region 技能2-火种（锥形弹幕+火焰地砖）
    private IEnumerator FireSeedCoroutine()
    {
        yield return new WaitForSeconds(fireSeedCastTime); // 起手1秒

        // 计算BOSS到玩家的中轴方向
        Vector2 targetDir = (player.position - transform.position).normalized;
        float baseAngle = Mathf.Atan2(targetDir.y, targetDir.x) * Mathf.Rad2Deg;

        // 发射9发锥形弹幕，均匀分布在锥形角度内
        for (int i = 0; i < bulletCount; i++)
        {
            float curAngle = baseAngle - coneAngle / 2 + (coneAngle / (bulletCount - 1)) * i;
            Vector2 bulletDir = new Vector2(Mathf.Cos(curAngle * Mathf.Deg2Rad), Mathf.Sin(curAngle * Mathf.Deg2Rad));

            GameObject bullet = Instantiate(fireBulletPrefab, transform.position, Quaternion.identity);
            FireBullet bulletScript = bullet.GetComponent<FireBullet>();
            if (bulletScript != null)
            {
                bulletScript.speed = bulletSpeed;
                bulletScript.direction = bulletDir;
                // 子弹命中回调：生成火焰地砖
                bulletScript.onHit = () =>
                {
                    Instantiate(fireTilePrefab, bullet.transform.position, Quaternion.identity);
                    Destroy(bullet);
                };
            }
        }
    }
    #endregion

    #region 技能3-余烬（热量归零触发360°弹幕）
    private void CheckEmberTrigger()
    {
        if (currentHeat == 0 && !isHeatZero)
        {
            isHeatZero = true;
            StartCoroutine(EmberCoroutine());
        }
    }

    private IEnumerator EmberCoroutine()
    {
        yield return new WaitForSeconds(emberCastTime); // 起手1秒

        // 360°均匀发射18发弹幕
        float angleStep = 360f / emberBulletCount;
        for (int i = 0; i < emberBulletCount; i++)
        {
            float angle = angleStep * i * Mathf.Deg2Rad;
            Vector2 dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));

            GameObject bullet = Instantiate(fireBulletPrefab, transform.position, Quaternion.identity);
            FireBullet bulletScript = bullet.GetComponent<FireBullet>();
            if (bulletScript != null)
            {
                bulletScript.speed = emberBulletSpeed;
                bulletScript.direction = dir;
                bulletScript.onHit = () => Destroy(bullet);
            }
            Destroy(bullet, 3f); // 保底销毁，防止内存泄漏
        }
    }
    #endregion

    #region 技能通用逻辑
    private void UpdateSkillCD()
    {
        if (firewallCDTimer > 0) firewallCDTimer -= Time.deltaTime;
        if (fireSeedCDTimer > 0) fireSeedCDTimer -= Time.deltaTime;
    }

    // AI技能释放逻辑（每秒检测，优先冷却完成的技能）
    private void UpdateAIskillLogic()
    {
        skillCheckTimer += Time.deltaTime;
        if (skillCheckTimer >= skillCheckInterval)
        {
            skillCheckTimer = 0;
            // 优先释放防火墙，再释放火种
            if (firewallCDTimer <= 0 && !isFirewallActive)
            {
                StartCoroutine(FirewallCoroutine());
                firewallCDTimer = firewallCD;
            }
            else if (fireSeedCDTimer <= 0)
            {
                StartCoroutine(FireSeedCoroutine());
                fireSeedCDTimer = fireSeedCD;
            }
        }
    }
    #endregion

    #region 辅助逻辑
    // BOSS死亡逻辑
    private void BossDie()
    {
        // 可添加死亡特效、销毁自身、通关逻辑等
        Destroy(gameObject, 0.5f);
    }

    // Gizmos绘制：场景视图显示热量槽和技能范围（方便调试）
    private void OnDrawGizmos()
    {
        // 绘制热量槽（红色=当前热量，绿色=最大热量）
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transform.position + Vector3.up * 3, new Vector3(maxHeat, 0.5f, 0));
        Gizmos.color = Color.red;
        Gizmos.DrawCube(transform.position + Vector3.up * 3 - new Vector3((maxHeat - currentHeat) / 2, 0, 0), new Vector3(currentHeat, 0.5f, 0));

        // 绘制防火墙范围
        Gizmos.color = new Color(1f, 0.5f, 0f);
        Gizmos.DrawWireCube(transform.position, new Vector3(5, 5, 0));
    }
    #endregion
}