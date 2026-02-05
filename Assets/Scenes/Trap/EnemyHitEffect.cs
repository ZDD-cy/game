using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 敌人受击效果处理脚本（专门对接5种陷阱）
/// 依赖你原有Enemy的字段：hp、moveSpeed、currentSpeed、isFrozen、isInIceTrap
/// 依赖你原有Enemy的方法：TakeDamage()、ApplyBurn()、ResetSpeed()
/// </summary>
public class EnemyHitEffect : MonoBehaviour
{
    [Header("受击特效配置")]
    public ParticleSystem normalHitEffect;   // 通用受击粒子（尖刺/隐藏/落石通用）
    public ParticleSystem burnEffect;        // 灼烧特效（火焰陷阱专属）
    public ParticleSystem freezeEffect;      // 冰冻特效（冰冻陷阱专属）
    public ParticleSystem knockbackEffect;   // 击退特效（尖刺陷阱专属）
    public ParticleSystem rockHitEffect;     // 重击击飞特效（落石陷阱专属）

    [Header("受击音效配置")]
    public AudioClip hitClip;        // 通用受击音效
    public AudioClip burnClip;       // 灼烧音效
    public AudioClip freezeClip;     // 冰冻音效
    public AudioClip knockbackClip;  // 击退音效
    public AudioClip rockHitClip;    // 落石重击音效

    [Header("受击参数配置")]
    public float hitStunTime = 0.2f; // 受击僵直时间
    public float rockKnockupForce = 6f; // 落石击飞力度
    public float burnEffectInterval = 0.5f; // 灼烧特效间隔

    private Enemy enemy;
    private Rigidbody2D rb;
    private AudioSource audioSource;
    private bool isBurning;          // 灼烧状态标记
    private Coroutine burnCoroutine; // 灼烧协程缓存
   

    void Awake()
    {
        // 获取你原有Enemy组件和必要组件
        enemy = GetComponent<Enemy>();
        rb = GetComponent<Rigidbody2D>();
        if (!GetComponent<AudioSource>()) gameObject.AddComponent<AudioSource>();
        audioSource = GetComponent<AudioSource>();

        // 初始化特效（防止空引用报错）
        InitParticle(normalHitEffect);
        InitParticle(burnEffect);
        InitParticle(freezeEffect);
        InitParticle(knockbackEffect);
        InitParticle(rockHitEffect);
    }

    #region 通用工具方法（特效+音效播放）
    // 初始化粒子（停止初始播放）
    private void InitParticle(ParticleSystem particle)
    {
        if (particle == null) return;
        particle.Stop();
        var mainMoudle = particle.main;
        mainMoudle.loop= false;
    }

    // 播放单次粒子特效（位置跟随敌人）
    private void PlayOneShotParticle(ParticleSystem particle)
    {
        if (particle == null) return;
        particle.transform.position = transform.position;
        particle.Play();
    }

    // 播放持续粒子特效（循环播放）
    private void PlayLoopParticle(ParticleSystem particle)
    {
        if (particle == null) return;
        particle.transform.position = transform.position;
        var mainMoudle = particle.main;
        mainMoudle.loop = true;
        particle.Play();
    }

    // 停止持续粒子特效
    private void StopLoopParticle(ParticleSystem particle)
    {
        if (particle == null) return;
        particle.Stop();
    }

    // 播放音效
    private void PlayHitSound(AudioClip clip)
    {
        if (audioSource == null || clip == null) return;
        audioSource.PlayOneShot(clip);
    }
    #endregion

    #region 1. 通用受击效果（所有陷阱基础伤害都调用这个）
    /// <summary>
    /// 基础受击反馈（尖刺/隐藏/火焰触发伤害时调用）
    /// </summary>
    public void OnNormalHit()
    {
        if (enemy == null || enemy.hp <= 0) return;
        // 播放特效+音效
        PlayOneShotParticle(normalHitEffect);
        PlayHitSound(hitClip);
        // 受击僵直（暂时停移）
        StartCoroutine(HitStunCoroutine());
    }
    #endregion

    #region 2. 尖刺陷阱专属受击效果（击退+僵直）
    /// <summary>
    /// 尖刺陷阱击退效果（陷阱传入击退方向和力度）
    /// </summary>
    public void OnSpikeKnockback(Vector2 knockDir, float force)
    {
        if (enemy == null || enemy.hp <= 0 || rb == null) return;
        // 播放击退特效+音效
        PlayOneShotParticle(knockbackEffect);
        PlayHitSound(knockbackClip);
        // 执行击退逻辑（清空原有速度，添加击退力）
        rb.velocity = Vector2.zero;
        rb.AddForce(knockDir * force, ForceMode2D.Impulse);
        // 击退僵直
        StartCoroutine(HitStunCoroutine());
    }
    #endregion

    #region 3. 火焰陷阱专属受击效果（灼烧+持续特效）
    /// <summary>
    /// 火焰陷阱灼烧效果（陷阱传入灼烧伤害和时长）
    /// </summary>
    public void OnFireBurn(int burnDmg, float duration)
    {
        if (enemy == null || enemy.hp <= 0 || isBurning) return;
        // 标记灼烧状态，播放持续灼烧特效+音效
        isBurning = true;
        PlayLoopParticle(burnEffect);
        PlayHitSound(burnClip);
        // 启动灼烧特效循环协程（同步伤害节奏）
        if (burnCoroutine != null) StopCoroutine(burnCoroutine);
        burnCoroutine = StartCoroutine(BurnEffectCoroutine(duration));
    }

    /// <summary>
    /// 灼烧特效循环（和灼烧伤害同步反馈）
    /// </summary>
    private IEnumerator BurnEffectCoroutine(float duration)
    {
        float timer = 0;
        while (timer < duration && enemy.hp > 0)
        {
            timer += burnEffectInterval;
            // 每间隔播放一次小受击粒子，强化灼烧反馈
            PlayOneShotParticle(normalHitEffect);
            PlayHitSound(burnClip);
            yield return new WaitForSeconds(burnEffectInterval);
        }
        // 灼烧结束，停止特效，重置状态
        isBurning = false;
        StopLoopParticle(burnEffect);
        burnCoroutine = null;
    }
    #endregion

    #region 4. 冰冻陷阱专属受击效果（冰冻特效+减速反馈）
    /// <summary>
    /// 冰冻陷阱触发效果（进入陷阱时调用）
    /// </summary>
    public void OnFreezeEnter()
    {
        if (enemy == null || enemy.hp <= 0 || enemy.isFrozen) return;
        // 播放冰冻特效+音效
        PlayLoopParticle(freezeEffect);
        PlayHitSound(freezeClip);
        // 冰冻僵直（动画减速视觉反馈，不需要改Enemy核心逻辑）
        transform.localScale = new Vector3(transform.localScale.x * 1.05f, transform.localScale.y * 1.05f, 1);
    }

    /// <summary>
    /// 冰冻陷阱退出效果（离开陷阱时调用）
    /// </summary>
    public void OnFreezeExit()
    {
        if (enemy == null) return;
        // 停止冰冻特效，恢复缩放
        StopLoopParticle(freezeEffect);
        transform.localScale = new Vector3(transform.localScale.x / 1.05f, transform.localScale.y / 1.05f, 1);
    }

    /// <summary>
    /// 冰冻持续冻伤反馈（每秒触发一次）
    /// </summary>
    public void OnFreezeDamage()
    {
        if (enemy == null || enemy.hp <= 0) return;
        PlayOneShotParticle(normalHitEffect); // 冻伤小粒子反馈
    }
    #endregion

    #region 5. 落石陷阱专属受击效果（重击击飞+高额伤害反馈）
    /// <summary>
    /// 落石陷阱重击效果（落石落地命中时调用）
    /// </summary>
    public void OnRockHit()
    {
        if (enemy == null || enemy.hp <= 0 || rb == null) return;
        // 播放重击特效+音效
        PlayOneShotParticle(rockHitEffect);
        PlayHitSound(rockHitClip);
        // 向上击飞效果（落石专属）
        rb.velocity = Vector2.zero;
        rb.AddForce(Vector2.up * rockKnockupForce, ForceMode2D.Impulse);
        // 重击僵直（比普通受击久一点）
        StartCoroutine(HitStunCoroutine(hitStunTime * 2));
    }
    #endregion

    #region 6. 隐藏陷阱专属受击效果（牵引僵直+显形伤害反馈）
    /// <summary>
    /// 隐藏陷阱牵引效果（触发牵引时调用）
    /// </summary>
    public void OnPullStart()
    {
        if (enemy == null || enemy.hp <= 0) return;
        // 播放通用受击特效+音效（隐藏陷阱伤害反馈）
        PlayOneShotParticle(normalHitEffect);
        PlayHitSound(hitClip);
        // 牵引僵直（减速视觉反馈）
        StartCoroutine(PullStunCoroutine());
    }

    /// <summary>
    /// 隐藏陷阱牵引结束效果
    /// </summary>
    public void OnPullEnd()
    {
 
        enemy.ResetSpeed(); // 调用你Enemy的重置速度方法
    }
    #endregion

    #region 状态协程（僵直+牵引逻辑）
    // 通用受击僵直
    private IEnumerator HitStunCoroutine(float stunTime = 0)
    {
        float targetTime = stunTime > 0 ? stunTime : hitStunTime;
        float originSpeed = enemy.currentSpeed;
        enemy.currentSpeed = 0; // 僵直时停止移动
        yield return new WaitForSeconds(targetTime);
        // 僵直结束，恢复速度（排除冰冻状态）
        if (enemy != null && !enemy.isFrozen)
        {
            enemy.currentSpeed = originSpeed;
        }
    }

    // 隐藏陷阱牵引僵直
    private IEnumerator PullStunCoroutine()
    {
        float pullStunTime = 1.5f; // 对应隐藏陷阱显形时长
        float originSpeed = enemy.moveSpeed;
        enemy.currentSpeed = originSpeed * 0.4f; // 牵引时减速
        yield return new WaitForSeconds(pullStunTime);
        if (enemy != null) enemy.ResetSpeed();
    }
    #endregion

    #region 外部调用接口（给陷阱脚本调用的方法，重点！）
    /// <summary>
    /// 陷阱通用伤害调用入口（对接TakeDamage）
    /// </summary>
    public void DoHit() => OnNormalHit();

    /// <summary>
    /// 尖刺击退调用入口
    /// </summary>
    public void DoSpikeKnockback(Vector2 dir, float force) => OnSpikeKnockback(dir, force);

    /// <summary>
    /// 火焰灼烧调用入口（对接ApplyBurn）
    /// </summary>
    public void DoFireBurn(int dmg, float duration) => OnFireBurn(dmg, duration);

    /// <summary>
    /// 冰冻进入/退出调用入口
    /// </summary>
    public void DoFreezeEnter() => OnFreezeEnter();
    public void DoFreezeExit() => OnFreezeExit();
    public void DoFreezeDamage() => OnFreezeDamage();

    /// <summary>
    /// 落石重击调用入口
    /// </summary>
    public void DoRockHit() => OnRockHit();

    /// <summary>
    /// 隐藏陷阱牵引调用入口
    /// </summary>
    public void DoPullStart() => OnPullStart();
    public void DoPullEnd() => OnPullEnd();
    #endregion

    // 防止协程泄漏
    void OnDestroy()
    {
        if (burnCoroutine != null) StopCoroutine(burnCoroutine);
        StopAllCoroutines();
    }
    //补充携程
    // 这个方法由 Enemy 类调用
    public void ApplyBurn(float burnDamage, float burnDuration)
    {
        // 在这里实现燃烧的逻辑和特效
        // 1. 播放火焰粒子特效
        // 2. 启动一个协程来处理持续伤害
        StartCoroutine(BurnCoroutine(burnDamage, burnDuration));
    }

    private IEnumerator BurnCoroutine(float damagePerSecond, float duration)
    {
        float timer = 0;
        Enemy enemy = GetComponent<Enemy>();
        while (timer < duration)
        {
            enemy.TakeDamage((int)(damagePerSecond * Time.deltaTime));
            timer += Time.deltaTime;
            yield return null;
        }
    }

    // 这个方法由 Enemy 类调用
    public void ApplySlow(float slowAmount, float slowDuration)
    {
        // 在这里实现减速的逻辑和特效
        // 1. 播放冰霜粒子特效
        // 2. 启动一个协程来处理减速效果
        StartCoroutine(SlowCoroutine(slowAmount, slowDuration));
    }

    private IEnumerator SlowCoroutine(float slowAmount, float duration)
    {
        Enemy enemy = GetComponent<Enemy>();

        // 保存原始速度
        float originalSpeed = enemy.moveSpeed;
        // 应用减速
        enemy.moveSpeed *= (1 - slowAmount);

        yield return new WaitForSeconds(duration);

        // 恢复原始速度
        enemy.moveSpeed = originalSpeed;
    }
}


