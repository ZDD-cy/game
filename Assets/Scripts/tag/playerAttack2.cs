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
    private float[] dashLineSegments; // 虚线分段数组（实现实体虚线）

    void Awake()
    {
        // 初始化实体化线渲染（虚实线核心）
        targetLine = GetComponent<LineRenderer>();
        targetLine.enabled = false; // 初始隐藏
        targetLine.widthMultiplier = lineWidth;
        targetLine.positionCount = 2; // 两点一线：玩家→目标
        targetLine.useWorldSpace = true; // 世界空间，跟随玩家/目标移动
        // 初始化实体虚线分段（核心：让LineRenderer显示虚线）
        dashLineSegments = new float[] { 0, dashLineGap, dashLineGap, dashLineGap };
        SetLineAsDash(); // 初始为虚线
    }

    void Update()
    {
        // 二选一：目标选取逻辑
        if (isRightClickSelect)
        {
            RightClickSelectTarget(); // 方案1：右键手动选
        }
        if (isAutoFindNearest)
        {
            AutoFindNearestEnemy(); // 方案2：自动选最近（默认）
        }

        // 实时更新虚实线位置（玩家→目标，实体化跟随）
        if (currentTarget != null)
        {
            if (targetLine != null) ;
        }
        // 目标失焦检测（死亡/超出范围/销毁，自动清空）
        CheckTargetValid();
    }

    #region 方案1：右键手动选取敌对单位
    private void RightClickSelectTarget()
    {
        if (Input.GetMouseButtonDown(1))
        {
            Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(mouseWorldPos, Vector2.zero);

            if (hit)
            {
                Collider2D col = hit.collider;
                if (col != null && col.CompareTag("Enemy"))
                {
                    SetTarget(col.transform);
                }
            }
        }
    }

    #endregion

    #region 方案2：自动选取最近的敌对单位（元气骑士风格）
    private void AutoFindNearestEnemy()
    {
        // 查找场景中所有Enemy标签物体
        Collider2D[] allEnemies = Physics2D.OverlapCircleAll(transform.position, autoFindRange, 1 << LayerMask.NameToLayer("Enemy"));
        Transform nearestEnemy = null;
        float minDistance = Mathf.Infinity;

        // 遍历找到最近的敌人
        foreach (var enemyCol in allEnemies)
        {
            float distance = Vector2.Distance(transform.position, enemyCol.transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                nearestEnemy = enemyCol.transform;
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
        // 首次攻击：施加Debuff，虚线变实线
        if (targetStatus != null)
        {
            targetStatus.AddDebuff();
            SetLineAsSolid(); // 确认目标，实体实线
        }

        // 循环叠加：while (currentTarget != null && targetStatus != null)
        {
            yield return new WaitForSeconds(attackInterval);
            targetStatus.AddDebuff(); // 叠加Debuff（重置/叠加时长，在EnemyStatus中配置）
        }
    }
    #endregion
    #region 虚实线实体化核心：LineRenderer切换虚线/实线
    // 设置为虚线（未确认目标）
    private void SetLineAsDash()
    {
        // 正确颜色设置，无报错
        targetLine.startColor = dashLineColor;
        targetLine.endColor = dashLineColor;

        // 官方虚线模式，兼容所有Unity版本
        targetLine.textureMode = LineTextureMode.Tile;
        targetLine.widthMultiplier = lineWidth;
        targetLine.loop = false;

        // 更新线条位置
        targetLine.SetPositions(new Vector3[] { transform.position, currentTarget != null ? currentTarget.position : transform.position });
    }

    // 设置为实线（已确认目标，Debuff生效）
    private void SetLineAsSolid()
    {
        // 正确颜色设置，无报错
        targetLine.startColor = solidLineColor;
        targetLine.endColor = solidLineColor;

        // 官方实线模式
        targetLine.textureMode = LineTextureMode.Stretch;
        targetLine.widthMultiplier = lineWidth;
        targetLine.loop = false;
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
        // 目标失活/超出自动寻敌范围，清空
        if (!currentTarget.gameObject.activeSelf || Vector2.Distance(transform.position, currentTarget.position) > autoFindRange * 1.5f)
        {
            ClearTarget();
        }
    }
    // 清空当前目标（隐藏线、停止计时、重置状态）
    public void ClearTarget()
    {
        StopAttackCoroutine();
        currentTarget = null;
        targetStatus = null;
        targetLine.enabled = false; // 隐藏虚实线
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
}