using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(LineRenderer))]
public class PlayerTargetAttack : MonoBehaviour
{
    [Header("目标选取方案（二选一，勾选即启用）")]
    public bool isRightClickSelect = false; // 方案1：右键手动选目标
    public bool isAutoFindNearest = true;  // 方案2：自动选最近敌人（默认启用）

    [Header("自动寻敌配置")]
    public float autoFindRange = 10f; // 自动寻敌最大范围（超出不选）

    [Header("攻击计时配置（默认2s）")]
    public float attackInterval = 2f; // 首次攻击/后续叠加间隔（默认2s）

    [Header("虚实线实体化配置")]
    public Color dashLineColor = Color.white; // 虚线颜色（未确认目标）
    public Color solidLineColor = Color.red;   // 实线颜色（已确认目标）
    public float lineWidth = 0.1f;            // 线宽度（实体化）
    public float dashLineGap = 0.2f;          // 虚线间隔（越小越密）

    private LineRenderer targetLine; // 实体化线渲染组件
    private Transform currentTarget; // 当前锁定目标
    private EnemyStatus targetStatus; // 目标状态脚本
    private Coroutine attackCoroutine; // 攻击计时协程（独立）
    private Material lineMaterial; // 新增：虚线材质（解决LineRenderer虚线不显示问题）

    [Header("射线直接伤害配置")]
    public float rayDamage = 5f; // 每次射线命中的伤害值
    public float rayRange = 15f; // 射线最大距离（建议比自动寻敌范围大）

    void Awake()
    {
        // 初始化实体化线渲染（虚实线核心）
        targetLine = GetComponent<LineRenderer>();
        targetLine.enabled = false; // 初始隐藏
        targetLine.widthMultiplier = lineWidth;
        targetLine.positionCount = 2; // 两点一线：玩家→目标
        targetLine.useWorldSpace = true; // 世界空间，跟随玩家/目标移动

        // 新增：初始化虚线材质（关键修复：Unity默认材质不支持虚线，需自定义）
        lineMaterial = new Material(Shader.Find("Sprites/Default"));
        targetLine.material = lineMaterial;

        // 初始为虚线
        SetLineAsDash();
    }

    void Update()
    {
        // 修复：二选一逻辑（避免同时启用两个方案冲突）
        if (isRightClickSelect && !isAutoFindNearest)
        {
            RightClickSelectTarget(); // 方案1：右键手动选（仅单开）
        }
        else if (isAutoFindNearest && !isRightClickSelect)
        {
            AutoFindNearestEnemy(); // 方案2：自动选最近（默认单开）
        }
        else if (isRightClickSelect && isAutoFindNearest)
        {
            Debug.LogWarning("[警告] 两个选敌方案不能同时启用！默认使用自动寻敌");
            isRightClickSelect = false;
            AutoFindNearestEnemy();
        }

        // 修复：实时更新虚实线位置（玩家→目标，实体化跟随）
        if (currentTarget != null && targetLine != null)
        {
            targetLine.SetPositions(new[] { transform.position, currentTarget.position });
        }
        // 新增：锁定目标时，每帧发射射线造成直接伤害
        if (currentTarget != null)
        {
            Debug.Log("Take damage");
            RayDirectDamage();
        }
        // 目标失焦检测（死亡/超出范围/销毁，自动清空）
        CheckTargetValid();
    }

     #region 方案1：右键手动选取敌对单位（修复后）
    private void RightClickSelectTarget()
    {
        // 仅响应右键按下（避免长按重复触发）
        if (Input.GetMouseButtonDown(1))
        {
            // 修复：鼠标坐标转换（2D游戏必须设置Z轴，否则射线检测失效）
            Vector3 mouseScreenPos = Input.mousePosition;
            if (Camera.main != null)
            {
                mouseScreenPos.z = -Camera.main.transform.position.z; // 适配2D相机距离
                Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(mouseScreenPos);

                // 修复：射线检测参数（方向设为Vector2.down，长度设为0，精准点击判定）
                RaycastHit2D hit = Physics2D.Raycast(
                    mouseWorldPos,
                    Vector2.down, // 2D点击检测常用方向，避免穿透
                    0.1f, // 极短距离，确保只命中点击的物体
                    1 << LayerMask.NameToLayer("Enemy") // 仅检测Enemy层，排除其他物体
                );

                // 调试日志：查看射线是否命中
                if (hit)
                {
                    Debug.Log(
                        $"[右键选敌] 命中物体：{hit.collider.name} | 图层：{LayerMask.LayerToName(hit.collider.gameObject.layer)} | 标签：{hit.collider.tag}");

                    Collider2D col = hit.collider;
                    // 双重验证：确保是Enemy标签+EnemyStatus组件
                    if (col != null && col.CompareTag("Enemy") && col.GetComponent<EnemyStatus>() != null)
                    {
                        SetTarget(col.transform);
                        Debug.Log($"[右键选敌成功] 锁定目标：{col.name}");
                    }
                    else
                    {
                        Debug.LogWarning($"[右键选敌失败] 命中物体不是有效敌人（缺少Enemy标签或EnemyStatus组件）");
                        ClearTarget(); // 清空无效目标
                    }
                }
                else
                {
                    Debug.LogWarning($"[右键选敌失败] 未命中任何Enemy层物体");
                    ClearTarget(); // 未命中时清空目标
                }
            }
        }
    }
    #endregion



    #region 方案2：自动选取最近的敌对单位（元气骑士风格）
    private void AutoFindNearestEnemy()
    {
        // 查找场景中所有Enemy标签+Enemy层的物体（双重过滤，避免误选）
        Collider2D[] allEnemies = Physics2D.OverlapCircleAll(transform.position, autoFindRange, 1 << LayerMask.NameToLayer("Enemy"));
        Transform nearestEnemy = null;
        float minDistance = Mathf.Infinity;

        // 遍历找到最近的敌人（排除死亡/失活的）
        foreach (var enemyCol in allEnemies)
        {
            EnemyStatus enemyStatus = enemyCol.GetComponent<EnemyStatus>();
            if (enemyStatus != null && enemyStatus.currentHp > 0 && enemyCol.gameObject.activeSelf)
            {
                float distance = Vector2.Distance(transform.position, enemyCol.transform.position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearestEnemy = enemyCol.transform;
                }
            }
        }

        // 锁定最近敌人（无敌人则清空）
        if (nearestEnemy != null)
        {
            SetTarget(nearestEnemy);
        }
        else
        {
            ClearTarget();
        }
    }
    #endregion

    #region 核心：设置目标+开启独立计时+虚实线切换
    public void SetTarget(Transform target)
    {
        // 若目标不变，不重复执行
        if (currentTarget == target) return;

        // 清空原有目标的计时
        StopAttackCoroutine();

        // 赋值新目标
        currentTarget = target;
        targetStatus = currentTarget.GetComponent<EnemyStatus>();

        // 开启目标线（先为虚线，计时结束变实线）
        targetLine.enabled = true;
        SetLineAsDash(); // 未完成首次攻击，虚线

        // 开启独立攻击计时协程（与Update解耦，避免卡顿）
        attackCoroutine = StartCoroutine(AttackTimerCoroutine());
    }
    #endregion

    #region 独立计时协程：首次2s攻击→变实线→每2s叠加Debuff
    private IEnumerator AttackTimerCoroutine()
    {
        // 首次攻击等待：固定2s（面板可改）
        yield return new WaitForSeconds(attackInterval);

        // 首次攻击：施加Debuff，虚线变实线（双重判空，避免报错）
        if (targetStatus != null && currentTarget != null && currentTarget.gameObject.activeSelf)
        {
            targetStatus.AddDebuff();
            SetLineAsSolid(); // 确认目标，实体实线
            Debug.Log($"[Debuff生效] 对 {currentTarget.name} 施加Debuff，持续叠加");
        }
        else
        {
            ClearTarget();
            yield break;
        }

        // 修复：循环叠加（添加目标有效性判断，避免空引用）
        while (currentTarget != null && targetStatus != null && targetStatus.currentHp > 0 && currentTarget.gameObject.activeSelf)
        {
            yield return new WaitForSeconds(attackInterval);
            targetStatus.AddDebuff(); // 叠加Debuff（重置/叠加时长，在EnemyStatus中配置）
            Debug.Log($"[Debuff叠加] 对 {currentTarget.name} 再次施加Debuff");
        }

        // 目标失效后清空
        ClearTarget();
    }
    #endregion

    #region 虚实线实体化核心：LineRenderer切换虚线/实线
    // 设置为虚线（未确认目标）
    private void SetLineAsDash()
    {
        if (targetLine == null) return;

        // 颜色设置
        targetLine.startColor = dashLineColor;
        targetLine.endColor = dashLineColor;

        // 修复：虚线实现（使用材质主纹理缩放，兼容所有版本）
        lineMaterial.mainTexture = CreateDashTexture();
        lineMaterial.mainTextureScale = new Vector2(1 / dashLineGap, 1);

        // 更新线条位置
        targetLine.SetPositions(new[] { transform.position, currentTarget != null ? currentTarget.position : transform.position });
    }

    // 设置为实线（已确认目标，Debuff生效）
    private void SetLineAsSolid()
    {
        if (targetLine == null) return;

        // 颜色设置
        targetLine.startColor = solidLineColor;
        targetLine.endColor = solidLineColor;

        // 实线实现（清空纹理，使用纯色）
        lineMaterial.mainTexture = null;
    }

    // 新增：创建虚线纹理（关键修复：解决LineRenderer虚线不显示问题）
    private Texture2D CreateDashTexture()
    {
        Texture2D texture = new Texture2D(2, 1);
        texture.SetPixel(0, 0, Color.white);
        texture.SetPixel(1, 0, Color.clear);
        texture.Apply();
        return texture;
    }
    // 射线直接伤害：锁定目标后，每帧发射射线造成伤害
    private void RayDirectDamage()
    {
        if (currentTarget == null || targetStatus == null || targetStatus.currentHp <= 0)
            return;

        // 发射射线（玩家→目标，检测是否被阻挡）
        RaycastHit2D hit = Physics2D.Raycast(
            transform.position,
            (currentTarget.position - transform.position).normalized,
            rayRange,
            1 << LayerMask.NameToLayer("Enemy") // 只检测敌人
        );

        // 射线命中目标且无阻挡，造成直接伤害
        if (hit.collider != null && hit.transform == currentTarget)
        {
            targetStatus.TakeDamage(rayDamage * Time.deltaTime); // 帧平滑伤害
            Debug.Log($"[射线伤害] 对 {currentTarget.name} 造成 {rayDamage * Time.deltaTime:F2} 点伤害");
        }
    }

    #endregion

    #region 目标有效性检测+清空目标+停止计时
    // 检测目标是否有效（死亡/超出范围/销毁/失活）
    private void CheckTargetValid()
    {
        if (currentTarget == null || targetStatus == null)
        {
            ClearTarget();
            return;
        }

        // 目标失活/超出自动寻敌范围/死亡，清空
        if (!currentTarget.gameObject.activeSelf ||
            Vector2.Distance(transform.position, currentTarget.position) > autoFindRange * 1.5f ||
            targetStatus.currentHp <= 0)
        {
            ClearTarget();
            Debug.Log($"[目标失效] {currentTarget.name} 已死亡/超出范围，清空目标");
        }
    }

    // 清空当前目标（隐藏线、停止计时、重置状态）
    public void ClearTarget()
    {
        StopAttackCoroutine();
        currentTarget = null;
        targetStatus = null;
        if (targetLine != null)
        {
            targetLine.enabled = false; // 隐藏虚实线
        }
    }

    // 停止独立攻击计时协程
    private void StopAttackCoroutine()
    {
        if (attackCoroutine != null)
        {
            StopCoroutine(attackCoroutine);
            attackCoroutine = null;
        }
    }
    #endregion

    // Gizmos绘制：场景视图显示自动寻敌范围（地图设计时调试用）
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, autoFindRange);
    }

    // 新增：脚本销毁时释放材质（避免内存泄漏）
    private void OnDestroy()
    {
        if (lineMaterial != null)
        {
            Destroy(lineMaterial);
        }
    }
}