
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// ��˪Buff������
[System.Serializable]
public class FrostBuffData
{
    public int currentLayer = 0;       // ��ǰBuff����
    public float buffDuration = 8f;    // ÿ�����ʱ��
    public float buffTimer = 0f;       // Buff����ʱ
    public float normalDissipateSpeed = 1f; // ������ɢ�ٶ�
    public float trapDissipateSpeed = 2f;   // ���ؼ������ɢ�ٶȣ�һ�����٣�
    public float currentDissipateSpeed;     // ��ǰʵ����ɢ�ٶ�

    // ����/������������
    public float slowPerLayer = 0.15f; // 1-3��ÿ��15%����
    public float attrReducePerLayer = 0.15f; //4-6��ÿ��15%ȫ��������
    public float maxAttrReduce = 1f;   //7��100%ȫ��������
    public float dotDamage = 5f;       //7������˺���ÿ�룩
    public float dotInterval = 1f;     //�����˺����

    // BuffЧ�����
    public bool isDashForbidden = false; //�Ƿ������
    public bool isMaxLayer = false;      //�Ƿ�ﵽ7�㣨���㣩

    
}

public class FrostBoss : MonoBehaviour
{
    public FightActive fightActive;
    public Transform playerTransform;
    [Header("BOSS��������")]
    public float moveSpeed = 8f;               // BOSS��������
    public List<Transform> movePathPoints;     // BOSS�ƶ�·���㣨һ���Ա�˪���壩
    public Transform frostTrapPrefab;          // ��˪����Ԥ���壨·��������
    public LayerMask playerLayer;              // ���ͼ��
    public float playerCheckRange = 10f;       // �����ҷ�Χ

    [Header("��˪Buff����")]
    public FrostBuffData frostBuff;            // ��˪Buff����

    [Header("������ȴ����")]
    public float iceBlastCD = 10f;             // ������ȴ
    public float snowFlakeCD = 6f;             // ѩ����ȴ
    public float pulseCD = 40f;                // ������ȴ
    private float iceBlastTimer;
    private float snowFlakeTimer;
    private float pulseTimer;


    [Header("���ܷ�Χ����")]
    public float iceBlastRange = 3f;           // ����3��3��Χ
    public float snowFlakeAngle = 90f;         // ѩ��90������
    public float snowFlakeRange = 15f;         // ѩ���������
    public GameObject snowFlakeBullet;         // ѩ����ĻԤ����
    public Transform bulletSpawnPoint;         // ��Ļ���ɵ�

    [Header("���ؽ���")]
    public bool isAnyTrapActive = false;       // �Ƿ��н�����ؼ���
    public int activeTrapCount = 0;            // ����Ļ������������4����

    // ���/Ŀ������
    private Transform player;
    private Rigidbody2D rb;
    private Coroutine dotCoroutine;            // 7������˺�Э��
    private int currentPathIndex = 0;          // ��ǰ�ƶ�·������


    
    
        
    
    void Start()
    {
       //����Я��
        StartCoroutine(CastSnowFlake());
        StartCoroutine(MoveAlongPathAndCreateTrap());
        // ��ʼ�����
        rb = GetComponent<Rigidbody2D>();
        if (rb == null) rb = gameObject.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        rb.bodyType = RigidbodyType2D.Dynamic;

        // ��ʼ��Buff
        frostBuff.currentDissipateSpeed = frostBuff.normalDissipateSpeed;
        frostBuff.buffTimer = frostBuff.buffDuration;

        // ��ʼ�����ܼ�ʱ��
        iceBlastTimer = 0;
        snowFlakeTimer = 0;
        pulseTimer = 0;

        // �������
        player = FindFirstObjectByType<Player>()?.transform;

        // ��ʼ��·���ƶ������ɱ�˪����
        StartCoroutine(MoveAlongPathAndCreateTrap());
    }

    void Update()
    {
        if (player == null || fightActive == null) return;
        if (Vector2.Distance(transform.position, player.position) <= playerCheckRange)
        {
            Debug.Log("��⵽��ң���ʼ׷��");
        }
        if (player == null) return;
        // ��������ҵľ���
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        // 360�ȼ�⣺ֻҪ�ڷ�Χ�ھ���Ϊ��⵽
        if (distanceToPlayer <= playerCheckRange)
        {
            if (fightActive != null)
            {
                fightActive.isFightActive = true;
            }
            Vector2 directionToPlayer = player.position - transform.position;
            float signedAngle = Mathf.Atan2(directionToPlayer.y, directionToPlayer.x) * Mathf.Rad2Deg;
            Debug.Log($"��������Boss�ĽǶȣ�{signedAngle:F1}��");
        }

        // ������ȴ��ʱ
        if (fightActive.isFightActive)
        {
            UpdateSkillTimers();
            // ��Ⲣ�ͷż���
            CheckAndCastSkills();
            // ���±�˪Buff������/ʱ��/Ч����
            UpdateFrostBuff();
            // ���ؼ���ʱˢ��Buff��ɢ�ٶ�
            UpdateBuffDissipateSpeed();
        }

        #region ·���ƶ�+��˪��������
        // ��·���ƶ���ÿ��һ�������ɱ�˪���壨·��һ���ԣ�
        IEnumerator MoveAlongPathAndCreateTrap()
        {
            if (movePathPoints == null || movePathPoints.Count == 0) yield break;

            while (currentPathIndex < movePathPoints.Count)
            {
                Transform targetPoint = movePathPoints[currentPathIndex];
                // �ƶ���Ŀ���
                while (Vector2.Distance(transform.position, targetPoint.position) > 0.1f)
                {
                    transform.position = Vector2.MoveTowards(transform.position, targetPoint.position, moveSpeed * Time.deltaTime * (1 - GetCurrentSlowRate()));
                    yield return null;
                }
                // ���ɱ�˪����
                Instantiate(frostTrapPrefab, targetPoint.position, Quaternion.identity);
                currentPathIndex++;
            }
            // ·�������ת��׷�����
            yield return null;
        }
        #endregion

        #region ��˪Buff�����߼�������/ˢ��/��ɢ/�ֲ�Ч����
        void UpdateFrostBuff()
        {
            if (frostBuff.currentLayer <= 0)
            {
                // ��Buffʱ��������Ч��
                ResetFrostBuffEffect();
                return;
            }

            // Buff����ʱ����ɢ�ٶ��ɻ��ؾ�����
            frostBuff.buffTimer -= Time.deltaTime * frostBuff.currentDissipateSpeed;
            if (frostBuff.buffTimer <= 0)
            {
                // ��ʱ������һ�㣬ˢ�µ���ʱ
                frostBuff.currentLayer--;
                frostBuff.buffTimer = frostBuff.buffDuration;
                // �����仯����Ч��
                UpdateFrostBuffEffect();
            }
        }

        // ����Buff��ɢ�ٶȣ��л��ؼ�����һ�����٣�
        void UpdateBuffDissipateSpeed()
        {
            frostBuff.currentDissipateSpeed = isAnyTrapActive ? frostBuff.trapDissipateSpeed : frostBuff.normalDissipateSpeed;
        }
    }
    // ����Buff��BOSSÿ�ι������ã�ˢ��ʱ��+����+1��
    public void AddFrostBuffLayer()
    {
        frostBuff.currentLayer = Mathf.Min(frostBuff.currentLayer + 1, 7); // ���7��
        frostBuff.buffTimer = frostBuff.buffDuration; // ˢ�³���ʱ��
        UpdateFrostBuffEffect(); // ���·ֲ�Ч��
    }

    // ����Buff�ֲ�Ч��
    void UpdateFrostBuffEffect()
    {
        // ��������Ч�����ٸ��ݵ�ǰ�������¸�ֵ
        frostBuff.isDashForbidden = false;
        frostBuff.isMaxLayer = false;
        StopDOTCoroutine();

        if (frostBuff.currentLayer >= 1 && frostBuff.currentLayer <= 3)
        {
            // 1-3�㣺ÿ��15%���٣�������Ч��
        }
        else if (frostBuff.currentLayer >= 4 && frostBuff.currentLayer <= 6)
        {
            // 4-6�㣺����+ÿ��15%ȫ��������+������
            frostBuff.isDashForbidden = true;
        }
        else if (frostBuff.currentLayer >= 7)
        {
            // 7�㣺100%ȫ��������+������+�����˺�
            frostBuff.isDashForbidden = true;
            frostBuff.isMaxLayer = true;
            StartDOTCoroutine();
        }
    }

    // ����BuffЧ��
    void ResetFrostBuffEffect()
    {
        frostBuff.isDashForbidden = false;
        frostBuff.isMaxLayer = false;
        StopDOTCoroutine();
    }

    // ��ȡ��ǰ���ٱ��������ƶ�/����ʹ�ã�
    public float GetCurrentSlowRate()
    {
        return Mathf.Clamp01(frostBuff.currentLayer * frostBuff.slowPerLayer);
    }

    // ��ȡ��ǰ������������
    public float GetCurrentAttrReduceRate()
    {
        if (frostBuff.currentLayer <= 3) return 0;
        if (frostBuff.currentLayer >= 7) return frostBuff.maxAttrReduce;
        return Mathf.Clamp01((frostBuff.currentLayer - 3) * frostBuff.attrReducePerLayer);
    }
    #endregion

    #region 7������˺���DOT��
    void StartDOTCoroutine()
    {
        if (dotCoroutine == null)
        {
            dotCoroutine = StartCoroutine(DOTCoroutine());
        }
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
            // �����ʩ�ӳ����˺������滻Ϊ����ܻ�������
            if (Vector2.Distance(transform.position, player.position) < playerCheckRange)
            {
                player.GetComponent<Player>()?.TakeDamage((int)frostBuff.dotDamage);
            }
            yield return new WaitForSeconds(frostBuff.dotInterval);
        }
    }
    #endregion

    #region �������߼�������+ѩ��+���壩
    void UpdateSkillTimers()
    {
        iceBlastTimer += Time.deltaTime;
        snowFlakeTimer += Time.deltaTime;
        pulseTimer += Time.deltaTime;
    }

    void CheckAndCastSkills()
    {
        // ������3��3��Χ�˺�����ȴ10s������2s
        if (iceBlastTimer >= iceBlastCD)
        {
            StartCoroutine(CastIceBlast());
            iceBlastTimer = 0;
        }

        // ѩ����90�����ε�Ļ����ȴ6s������2s
        if (snowFlakeTimer >= snowFlakeCD)
        {
            StartCoroutine(CastSnowFlake());
            snowFlakeTimer = 0;
        }

        // ���壺ȫ��0�˺�����Buff����ȴ40s��������
        if (pulseTimer >= pulseCD)
        {
            CastPulse();
            pulseTimer = 0;
        }
    }

    // 1.����������2s �� 3��3��Χ�˺� �� ��Buff
    IEnumerator CastIceBlast()
    {
        yield return new WaitForSeconds(2f); // ����2s
        // ���3��3��Χ�ڵ����
        Collider2D[] hitPlayers = Physics2D.OverlapCircleAll(transform.position, iceBlastRange, playerLayer);
        foreach (var hit in hitPlayers)
        {
            hit.GetComponent<Player>()?.TakeDamage(20); // ���Զ����˺�
        }
        AddFrostBuffLayer(); // ������һ��Buff
    }
    //2.ѩ��
    IEnumerator CastSnowFlake()
    {
        // ���գ����ս��δ���ֱ���˳�Э��
        if (fightActive == null || !fightActive.isFightActive)
        {
            Debug.LogWarning("ս��δ���ȡ��ѩ�������ͷ�");
            yield break;
        }

        // ���գ������һ����ɵ�δ���ã�ֱ���˳�
        if (player == null || bulletSpawnPoint == null)
        {
            Debug.LogError("��һ��ӵ����ɵ�δ���ã�");
            yield break;
        }

        Debug.Log("CastSnowFlake Э�̿�ʼִ��");
        yield return new WaitForSeconds(2f);
        Debug.Log("ѩ�����ܴ�������ʼ����");

        // 1. ��������ɵ�ָ����ҵķ���ͽǶ�
        Vector2 dirToPlayer = (player.position - bulletSpawnPoint.position).normalized;
        float angleToPlayer = Mathf.Atan2(dirToPlayer.y, dirToPlayer.x) * Mathf.Rad2Deg;

        // 2. ����ҷ���Ϊ���ģ��������ε���ʼ�ͽ����Ƕ�
        float startAngle = angleToPlayer - snowFlakeAngle / 2;
        float angleStep = 5f;

        // 3. �������ε�Ļ
        for (float angle = startAngle; angle <= startAngle + snowFlakeAngle; angle += angleStep)
        {
            float radian = angle * Mathf.Deg2Rad;
            Vector2 dir = new Vector2(Mathf.Cos(radian), Mathf.Sin(radian));

            GameObject bullet = Instantiate(snowFlakeBullet, bulletSpawnPoint.position, Quaternion.Euler(0, 0, angle));
            if (bullet != null)
            {
                Debug.Log("�ӵ�������");
                bullet.GetComponent<SnowFlakeBullet>()?.SetDirection(dir, snowFlakeRange);
                Debug.Log($"�ӵ�����λ��: {bullet.transform.position}");
            }
            else
            {
                Debug.LogError("�ӵ�����ʧ��");
            }
        }
             AddFrostBuffLayer();
    }

    // 3.���壺ȫ����� �� 0�˺� �� ǿ�Ƶ�Buff
    void CastPulse()
    {
        // ȫ�������ң����˺�������Buff��
        Collider2D[] hitPlayers = Physics2D.OverlapCircleAll(transform.position, Mathf.Infinity, playerLayer);
        foreach (var hit in hitPlayers)
        {
            AddFrostBuffLayer(); // �����һ��Buff
        }
    }
      // ·��Я��
    IEnumerator MoveAlongPathAndCreateTrap()
    {
        // �������д��·���ƶ�������������߼�
        Debug.Log("��ʼ��·���ƶ�����������");

        // ʾ���߼����ȴ�2��
        yield return new WaitForSeconds(2f);

        // ��ľ���ʵ��...
    }

    #endregion

    #region ���ؽ����ӿڣ���������ؽű����ã�
    // ���ؼ���/�ر�ʱ���ã�����true=���false=�رգ�
    public void OnTrapStateChange(bool isActive)
    {
        if (isActive)
        {
            activeTrapCount = Mathf.Min(activeTrapCount + 1, 4);
        }
        else
        {
            activeTrapCount = Mathf.Max(activeTrapCount - 1, 0);
        }
        // ֻҪ��һ�����ؼ���Ϳ���������ɢ
        isAnyTrapActive = activeTrapCount > 0;
    }
    #endregion

    // ���ܷ�Χ
    void OnDrawGizmosSelected()
    {
        // ������Χ
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, iceBlastRange);
        // ѩ�����η�Χ
        Gizmos.color = Color.white;
        DrawFanShape(bulletSpawnPoint != null ? bulletSpawnPoint.position : transform.position, snowFlakeRange, snowFlakeAngle);
    }

    // ��������Gizmos������ѩ�����ܣ�
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
}