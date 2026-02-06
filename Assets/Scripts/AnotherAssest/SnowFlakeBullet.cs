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

    

    void OnDestroy()
    {
        Debug.Log($"子弹 {gameObject.name} 被销毁，当前场景时间: {Time.time}");
    }

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

        // 超时自动回收
        StartCoroutine(RecoverAfterTime(5f));
        // 新增超时回收协程
        IEnumerator RecoverAfterTime(float delay)
        {
            yield return new WaitForSeconds(delay);
            // 超时后执行回收
            travelDistance = 0;
            gameObject.SetActive(false);
        }
    }

    void Update()
    {
        // 飞行逻辑
        Vector2 moveStep = moveDir * moveSpeed * Time.deltaTime;
        rb.velocity = moveStep;
        travelDistance += moveStep.magnitude;

        // 超出射程回收
        if (travelDistance >= maxRange)
        {
            travelDistance = 0;
            gameObject.SetActive(false);    
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

                //  改为隐藏，以便对象池复用
                gameObject.SetActive(false);

                // 重置透明度，下次激活时能正常显示
                if (sr != null)
                {
                    color = sr.color;
                    color.a = 1f;
                    sr.color = color;
                }
                // 重置计时器
                timer = 0f;
            }
        }
    }

    // 设置弹幕飞行方向
    public void SetDirection(Vector2 dir, float snowFlakeRange)
    {

        // 重置飞行距离
        travelDistance = 0;
        moveDir = dir;

        // 恢复碰撞体和渲染
        Collider2D collider = GetComponent<Collider2D>();
        if (collider != null)
            collider.enabled = true;

        SpriteRenderer renderer = GetComponent<SpriteRenderer>();
        if (renderer != null)
            renderer.enabled = true; Debug.Log($"子弹 {gameObject.name} 开始飞行，方向: {dir}，射程: {maxRange}");
        moveDir = dir.normalized;
        maxRange = snowFlakeRange;
    }

    // 碰撞玩家
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponent<Player>()?.TakeDamage(damage);
            travelDistance = 0;
            gameObject.SetActive(false);
        }
    }
}
