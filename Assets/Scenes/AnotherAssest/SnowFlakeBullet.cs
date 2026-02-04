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
    public float minScale = 0.8f;
    public float maxScale = 1.2f;
    public float scaleSpeed = 1f;
    public float fadeOutTime = 2f;
    public float fadeDuration = 1f;

    [Header("伤害设置")]
    public int damage = 10;

    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private Vector2 moveDir;
    private float travelDistance;
    private float scaleOffset;
    private float timer;

    void Start()
    {
        // 初始化组件
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        if (rb == null) rb = gameObject.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0;

        // 随机缩放偏移，让每个雪花效果不同
        scaleOffset = Random.Range(0f, Mathf.PI * 2f);
        timer = 0;
        travelDistance = 0;

        // 超时自动销毁
        Destroy(gameObject, 5f);
    }

    void Update()
    {
        // 飞行逻辑
        Vector2 moveStep = moveDir * moveSpeed * Time.deltaTime;
        rb.velocity = moveStep;
        travelDistance += moveStep.magnitude;

        // 超出射程销毁
        if (travelDistance >= maxRange)
        {
            Destroy(gameObject);
        }

        // 旋转效果
        transform.Rotate(0, 0, rotateSpeed * Time.deltaTime);

        // 呼吸式缩放
        float scale = Mathf.Lerp(minScale, maxScale, Mathf.Sin(Time.time * scaleSpeed + scaleOffset) * 0.5f + 0.5f);
        transform.localScale = Vector3.one * scale;

        // 透明度渐变
        timer += Time.deltaTime;
        if (timer >= fadeOutTime && sr != null)
        {
            float alpha = Mathf.Lerp(1f, 0f, (timer - fadeOutTime) / fadeDuration);
            Color color = sr.color;
            color.a = alpha;
            sr.color = color;

            if (alpha <= 0)
            {
                Destroy(gameObject);
            }
        }
    }

    // 设置弹幕飞行方向
    public void SetDirection(Vector2 dir)
    {
        moveDir = dir.normalized;
    }

    // 碰撞玩家
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponent<PlayerController>()?.TakeDamage(damage);
            Destroy(gameObject);
        }
    }
}
