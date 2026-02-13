
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// 冰霜Buff数据类
[System.Serializable]
public class FrostBuffData
{
    public int currentLayer;       // 当前Buff层数
    public float buffDuration = 8f;    // 每层持续时间
    public float buffTimer;       // Buff倒计时
    public float normalDissipateSpeed = 1f; // 正常消散速度
    public float trapDissipateSpeed = 2f;   // 机关激活后消散速度（一倍加速）
    public float currentDissipateSpeed;     // 当前实际消散速度

    // 减速/属性削减配置
    public float slowPerLayer = 0.15f; // 1-3层每层15%减速
    public float attrReducePerLayer = 0.15f; //4-6层每层15%全属性削减
    public float maxAttrReduce = 1f;   //7层100%全属性削减
    public float dotDamage = 5f;       //7层持续伤害（每秒）
    public float dotInterval = 1f;     //持续伤害间隔

    // Buff效果标记
    public bool isDashForbidden; //是否封禁冲刺
    public bool isMaxLayer;      //是否达到7层（满层）
    
}

public class FrostBoss : MonoBehaviour
{
    public FightActive fightActive;
    public Transform playerTransform;
    [Header("BOSS基础配置")]
    public float moveSpeed = 8f;               // BOSS基础移速
    public List<Transform> movePathPoints;     // BOSS移动路径点（一次性冰霜陷阱）
    public Transform frostTrapPrefab;          // 冰霜陷阱预制体（路径残留）
    public LayerMask playerLayer;              // 玩家图层
    public float playerCheckRange = 10f;       // 检测玩家范围

    [Header("冰霜Buff配置")]
    public FrostBuffData frostBuff;            // 冰霜Buff数据

    [Header("技能冷却配置")]
    public float iceBlastCD = 10f;             // 冰爆冷却
    public float snowFlakeCD = 6f;             // 雪花冷却
    public float pulseCD = 40f;                // 脉冲冷却
    private float iceBlastTimer;
    private float snowFlakeTimer;
    private float pulseTimer;


    [Header("技能范围配置")]
    public float iceBlastRange = 3f;           // 冰爆3×3范围
    public float snowFlakeAngle = 90f;         // 雪花90度扇形
    public float snowFlakeRange = 15f;         // 雪花扇形射程
    public GameObject snowFlakeBullet;         // 雪花弹幕预制体
    public Transform bulletSpawnPoint;         // 弹幕生成点

    [Header("机关交互")]
    public bool isAnyTrapActive = false;       // 是否有角落机关激活
    public int activeTrapCount = 0;            // 激活的机关数量（最多4个）

    // 组件/目标引用
    private Transform player;
    private Rigidbody2D rb;
    private Coroutine dotCoroutine;            // 7层持续伤害协程
    private int currentPathIndex = 0;          // 当前移动路径索引
    public BossfightOverlayManager BOM;
    
    void Start()
    {
       //添加携程
        StartCoroutine(CastSnowFlake());
        StartCoroutine(MoveAlongPathAndCreateTrap());
        // 初始化组件
        rb = GetComponent<Rigidbody2D>();
        if (rb == null) rb = gameObject.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        rb.bodyType = RigidbodyType2D.Dynamic;

        // 初始化Buff
        frostBuff.currentDissipateSpeed = frostBuff.normalDissipateSpeed;
        frostBuff.buffTimer = frostBuff.buffDuration;

        // 初始化技能计时器
        iceBlastTimer = 0;
        snowFlakeTimer = 0;
        pulseTimer = 0;

        // 查找玩家
        player = FindFirstObjectByType<Player>()?.transform;

        // 开始沿路径移动并生成冰霜陷阱
        StartCoroutine(MoveAlongPathAndCreateTrap());
    }


    void Update()
    {
        if (player == null || rb == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        // 1. 玩家在检测范围内，且在最大追踪距离内 → 开始追逐
        if (distanceToPlayer <= playerCheckRange && distanceToPlayer <= playerCheckRange)
        {
            Debug.Log("检测到玩家，开始追逐");
            if (fightActive != null)
            {
                fightActive.isFightActive = true;
            }

            // 计算方向并移动
            Vector2 direction = (player.position - transform.position).normalized;
            rb.velocity = direction * moveSpeed;
        }
        // 2. 玩家超出了最大追踪距离，或者不在检测范围内 → 停止追逐
        else
        {
            rb.velocity = Vector2.zero;
            // 可以在这里添加战斗状态结束的逻辑
            if (fightActive != null)
            {
                fightActive.isFightActive = false;
            }
        }
    
        // 技能冷却计时
        if (fightActive != null && fightActive.isFightActive)
        {
            UpdateSkillTimers();
            // 检测并释放技能
            CheckAndCastSkills();
            // 更新冰霜Buff（层数/时长/效果）
            UpdateFrostBuff();
            // 机关激活时刷新Buff消散速度
            UpdateBuffDissipateSpeed();
        }

        #region 路径移动+冰霜陷阱生成
        /*// 沿路径移动，每到一个点生成冰霜陷阱（路径一次性）
        IEnumerator MoveAlongPathAndCreateTrap()
        {
            if (movePathPoints == null || movePathPoints.Count == 0) yield break;

            while (currentPathIndex < movePathPoints.Count)
            {
                Transform targetPoint = movePathPoints[currentPathIndex];
                // 移动到目标点
                while (Vector2.Distance(transform.position, targetPoint.position) > 0.1f)
                {
                    transform.position = Vector2.MoveTowards(transform.position, targetPoint.position, moveSpeed * Time.deltaTime * (1 - GetCurrentSlowRate()));
                    yield return null;
                }
                // 生成冰霜陷阱
                Instantiate(frostTrapPrefab, targetPoint.position, Quaternion.identity);
                currentPathIndex++;
            }
            // 路径走完后，转向追击玩家
            yield return null;
        }*/
        #endregion

        #region 冰霜Buff核心逻辑（叠加/刷新/消散/分层效果）
        void UpdateFrostBuff()
        {
            if (frostBuff.currentLayer <= 0)
            {
                // 无Buff时重置所有效果
                ResetFrostBuffEffect();
                return;
            }

            // Buff倒计时（消散速度由机关决定）
            frostBuff.buffTimer -= Time.deltaTime * frostBuff.currentDissipateSpeed;
            if (frostBuff.buffTimer <= 0)
            {
                // 计时结束减一层，刷新倒计时
                frostBuff.currentLayer--;
                frostBuff.buffTimer = frostBuff.buffDuration;
                // 层数变化更新效果
                UpdateFrostBuffEffect();
            }
        }

        // 更新Buff消散速度（有机关激活则一倍加速）
        void UpdateBuffDissipateSpeed()
        {
            frostBuff.currentDissipateSpeed = isAnyTrapActive ? frostBuff.trapDissipateSpeed : frostBuff.normalDissipateSpeed;
        }
    }
    // 叠加Buff（BOSS每次攻击调用，刷新时长+层数+1）
    private void AddFrostBuffLayer()
    {
        frostBuff.currentLayer = Mathf.Min(frostBuff.currentLayer + 1, 7); // 最多7层
        frostBuff.buffTimer = frostBuff.buffDuration; // 刷新持续时间
        UpdateFrostBuffEffect(); // 更新分层效果
    }

    // 更新Buff分层效果
    void UpdateFrostBuffEffect()
    {
        // 重置所有效果，再根据当前层数重新赋值
        frostBuff.isDashForbidden = false;
        frostBuff.isMaxLayer = false;
        StopDOTCoroutine();

        if (frostBuff.currentLayer is >= 1 and <= 3)
        {
            // 1-3层：每层15%减速，无其他效果
        }
        else if (frostBuff.currentLayer is >= 4 and <= 6)
        {
            // 4-6层：减速+每层15%全属性削减+封禁冲刺
            frostBuff.isDashForbidden = true;
        }
        else if (frostBuff.currentLayer >= 7)
        {
            // 7层：100%全属性削减+封禁冲刺+持续伤害
            frostBuff.isDashForbidden = true;
            frostBuff.isMaxLayer = true;
            StartDOTCoroutine();
        }
    }

    // 重置Buff效果
    void ResetFrostBuffEffect()
    {
        frostBuff.isDashForbidden = false;
        frostBuff.isMaxLayer = false;
        StopDOTCoroutine();
    }

    // 获取当前减速比例（供移动/技能使用）
    public float GetCurrentSlowRate()
    {
        return Mathf.Clamp01(frostBuff.currentLayer * frostBuff.slowPerLayer);
    }

    // 获取当前属性削减比例
    public float GetCurrentAttrReduceRate()
    {
        if (frostBuff.currentLayer <= 3) return 0;
        if (frostBuff.currentLayer >= 7) return frostBuff.maxAttrReduce;
        return Mathf.Clamp01((frostBuff.currentLayer - 3) * frostBuff.attrReducePerLayer);
    }
    #endregion

    #region 7层持续伤害（DOT）
    void StartDOTCoroutine()
    {
        dotCoroutine ??= StartCoroutine(DOTCoroutine());
    }

    void StopDOTCoroutine()
    {
        if (dotCoroutine != null)
        {
            StopCoroutine(dotCoroutine);
            dotCoroutine = null;
        }
    }

    IEnumerator DOTCoroutine()
    {
        while (frostBuff.isMaxLayer)
        {
            // 给玩家施加持续伤害（可替换为玩家受击方法）
            if (Vector2.Distance(transform.position, player.position) < playerCheckRange)
            {
                player.GetComponent<Player>()?.TakeDamage((int)frostBuff.dotDamage);
            }
            yield return new WaitForSeconds(frostBuff.dotInterval);
        }
    }
    #endregion

    #region 技能组逻辑（冰爆+雪花+脉冲）
    void UpdateSkillTimers()
    {
        iceBlastTimer += Time.deltaTime;
        snowFlakeTimer += Time.deltaTime;
        pulseTimer += Time.deltaTime;
    }

    void CheckAndCastSkills()
    {
        // 冰爆：3×3范围伤害，冷却10s，起手2s
        if (iceBlastTimer >= iceBlastCD)
        {
            StartCoroutine(CastIceBlast());
            iceBlastTimer = 0;
        }

        // 雪花：90度扇形弹幕，冷却6s，起手2s
        if (snowFlakeTimer >= snowFlakeCD)
        {
            StartCoroutine(CastSnowFlake());
            snowFlakeTimer = 0;
        }

        // 脉冲：全屏0伤害，叠Buff，冷却40s，无起手
        if (pulseTimer >= pulseCD)
        {
            CastPulse();
            pulseTimer = 0;
        }
    }

    // 1.冰爆：起手2s → 3×3范围伤害 → 叠Buff
    IEnumerator CastIceBlast()
    {
        yield return new WaitForSeconds(2f); // 起手2s
        // 检测3×3范围内的玩家
        Collider2D[] hitPlayers = Physics2D.OverlapCircleAll(transform.position, iceBlastRange, playerLayer);
        foreach (var hit in hitPlayers)
        {
            hit.GetComponent<Player>()?.TakeDamage(20); // 可自定义伤害
        }
        AddFrostBuffLayer(); // 攻击叠一层Buff
    }
    //2.雪花
    IEnumerator CastSnowFlake()
    {
        // 保险：如果战斗未激活，直接退出协程
        if (fightActive == null || !fightActive.isFightActive)
        {
            Debug.LogWarning("战斗未激活，取消雪花技能释放");
            yield break;
        }

        // 保险：如果玩家或生成点未设置，直接退出
        if (player == null || bulletSpawnPoint == null)
        {
            Debug.LogError("玩家或子弹生成点未设置！");
            yield break;
        }

        Debug.Log("CastSnowFlake 协程开始执行");
        yield return new WaitForSeconds(2f);
        Debug.Log("雪花技能触发，开始生成");

        // 1. 计算从生成点指向玩家的方向和角度
        Vector2 dirToPlayer = (player.position - bulletSpawnPoint.position).normalized;
        float angleToPlayer = Mathf.Atan2(dirToPlayer.y, dirToPlayer.x) * Mathf.Rad2Deg;

        // 2. 以玩家方向为中心，计算扇形的起始和结束角度
        float startAngle = angleToPlayer - snowFlakeAngle / 2;
        float angleStep = 5f;

        // 3. 生成扇形弹幕
        for (float angle = startAngle; angle <= startAngle + snowFlakeAngle; angle += angleStep)
        {
            float radian = angle * Mathf.Deg2Rad;
            Vector2 dir = new Vector2(Mathf.Cos(radian), Mathf.Sin(radian));

            GameObject bullet = Instantiate(snowFlakeBullet, bulletSpawnPoint.position, Quaternion.Euler(0, 0, angle));
            if (bullet != null)
            {
                Debug.Log("子弹已生成");
                bullet.GetComponent<SnowFlakeBullet>()?.SetDirection(dir, snowFlakeRange);
                Debug.Log($"子弹生成位置: {bullet.transform.position}");
            }
            else
            {
                Debug.LogError("子弹生成失败");
            }
        }
        AddFrostBuffLayer();
    }

    // 3.脉冲：全屏打击 → 0伤害 → 强制叠Buff
    void CastPulse()
    {
        // 全屏检测玩家（无伤害，仅叠Buff）
        Collider2D[] hitPlayers = Physics2D.OverlapCircleAll(transform.position, Mathf.Infinity, playerLayer);
        foreach (var hit in hitPlayers)
        {
            AddFrostBuffLayer(); // 脉冲叠一层Buff
        }
    }
      // 路径携程
    IEnumerator MoveAlongPathAndCreateTrap()
    {
        // 在这里编写沿路径移动并生成陷阱的逻辑
        Debug.Log("开始沿路径移动并生成陷阱");

        // 示例逻辑：等待2秒
        yield return new WaitForSeconds(2f);

        // 你的具体实现...
    }

    #endregion

    #region 机关交互接口（供角落机关脚本调用）
    // 机关激活/关闭时调用（传入true=激活，false=关闭）
    public void OnTrapStateChange(bool isActive)
    {
        activeTrapCount = isActive ? Mathf.Min(activeTrapCount + 1, 4) : Mathf.Max(activeTrapCount - 1, 0);
        // 只要有一个机关激活，就开启加速消散
        isAnyTrapActive = activeTrapCount > 0;
    }
    #endregion

    // 技能范围
    void OnDrawGizmosSelected()
    {
        // 冰爆范围
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, iceBlastRange);
        // 雪花扇形范围
        Gizmos.color = Color.white;
        DrawFanShape(bulletSpawnPoint != null ? bulletSpawnPoint.position : transform.position, snowFlakeRange, snowFlakeAngle);
    }

    // 绘制扇形Gizmos（调试雪花技能）
    void DrawFanShape(Vector3 center, float radius, float angle)
    {
        float startAngle = transform.eulerAngles.z - angle / 2;
        Vector3 startPos = center + new Vector3(Mathf.Cos(startAngle * Mathf.Deg2Rad), Mathf.Sin(startAngle * Mathf.Deg2Rad), 0) * radius;
        Vector3 endPos = center + new Vector3(Mathf.Cos((startAngle + angle) * Mathf.Deg2Rad), Mathf.Sin((startAngle + angle) * Mathf.Deg2Rad), 0) * radius;
        Gizmos.DrawLine(center, startPos);
        Gizmos.DrawLine(center, endPos);
        int segments = 20;
        for (int i = 0; i <= segments; i++)
        {
            float currentAngle = startAngle + (angle / segments) * i;
            Vector3 pos = center + new Vector3(Mathf.Cos(currentAngle * Mathf.Deg2Rad), Mathf.Sin(currentAngle * Mathf.Deg2Rad), 0) * radius;
            if (i > 0)
            {
                float prevAngle = startAngle + (angle / segments) * (i - 1);
                Vector3 prevPos = center + new Vector3(Mathf.Cos(prevAngle * Mathf.Deg2Rad), Mathf.Sin(prevAngle * Mathf.Deg2Rad), 0) * radius;
                Gizmos.DrawLine(prevPos, pos);
            }
        }
    }
    void OnDisable()
    {
        if(BOM!=null)BOM.PassFight();
    }
}