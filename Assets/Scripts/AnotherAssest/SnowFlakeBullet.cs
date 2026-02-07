using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnowFlakeBullet : MonoBehaviour
{
    [Header("基础飞行设置")]
    public float moveSpeed = 8f;
    public float maxRange = 15f;

    [Header("视觉效果设置")]
    public float rotateSpeed = 90f;
    public float minScale = 250f;
    public float maxScale = 400f;
    public float scaleSpeed = 1f;

    [Header("伤害设置")]
    public int damage = 10;

    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private Vector2 moveDir;
    private float travelDistance;
    private float scaleOffset;

    // 初始化只执行一次
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();

        // 强制可见，解决看不见子弹
        if (sr != null)
        {
            sr.enabled = true;
            sr.sortingOrder = 30;
            Color c = sr.color;
            c.a = 1;
            sr.color = c;
        }

        // 物理固定
        if (rb != null)
        {
            rb.gravityScale = 0;
            rb.bodyType = RigidbodyType2D.Dynamic;
        }

        // 碰撞体
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.isTrigger = true;
        }
    }

    // 每次从对象池激活时重置状态
    void OnEnable()
    {
        // 重置位置Z轴，保证可见
        transform.position = new Vector3(transform.position.x, transform.position.y, 0);
        transform.localScale = Vector3.one;
        travelDistance = 0;
        scaleOffset = Random.Range(0, Mathf.PI * 2);

        // 强制开启渲染与碰撞
        if (sr != null) sr.enabled = true;
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = true;
    }

    void Update()
    {
        // 未激活不运行
        if (!gameObject.activeSelf) return;

        // 飞行
        rb.velocity = moveDir * moveSpeed;
        travelDistance += rb.velocity.magnitude * Time.deltaTime;

        // 超出射程 → 回收，不销毁
        if (travelDistance >= maxRange)
            RecycleBullet();

        // 旋转
        transform.Rotate(0, 0, rotateSpeed * Time.deltaTime);

        // 呼吸缩放
        float scale = Mathf.Lerp(minScale, maxScale, Mathf.Sin(Time.time * scaleSpeed + scaleOffset) * 0.5f + 0.5f);
        transform.localScale = new Vector3(scale, scale, 1);
    }

    // 【你要的：回收，不销毁！】
    void RecycleBullet()
    {
        rb.velocity = Vector2.zero;
        gameObject.SetActive(false);
    }

    // 适配你BOSS的2个参数调用，完全兼容，不报参数错
    public void SetDirection(Vector2 dir, float snowFlakeRange)
    {
        moveDir = dir.normalized;
        maxRange = snowFlakeRange;
        travelDistance = 0;
    }

    // 碰撞命中 → 回收，不销毁
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!gameObject.activeSelf) return;

        if (other.CompareTag("Player"))
        {
            Player p = other.GetComponent<Player>();
            if (p != null)
                p.TakeDamage(damage);

            // 命中也回收，不销毁
            RecycleBullet();
        }
    }
}

