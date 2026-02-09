using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DamagePopup : MonoBehaviour
{
    [Tooltip("伤害数字的文本组件")]
    [SerializeField] private TextMeshPro damageText;

    [Tooltip("伤害数字弹出后上升的速度")]
    [SerializeField] private float moveSpeed = 3f;

    [Tooltip("伤害数字存在的时间（秒）")]
    [SerializeField] private float lifetime = 3f;
    
    [SerializeField] private float fadeDuration = 1f;

    private float timer;

    void Start()
    {
        float randomAngle = Random.Range(-20, 20);
        float randomX = Random.Range(-3, 3)*0.1f;
        var vector3 = transform.position;
        vector3.x += randomX;
        vector3.y += 1;
        transform.position = vector3;
        transform.Rotate(0, 0, randomAngle);
    }

    void Update()
    {
        if (timer < lifetime)
        {
            // 计算时间比例
            float t = timer / lifetime;
        
            // 使用缓出函数
            float easedT = 1 - Mathf.Pow(1 - t, 2);
        
            // 计算当前帧的移动速度（随时间递减）
            float currentSpeed = moveSpeed * (1 - easedT);
        
            // 插值到目标位置
            transform.Translate(Vector2.up * (currentSpeed * Time.deltaTime));
            
            timer += Time.deltaTime;
            if (timer >= lifetime)
            {
                StartCoroutine(FadeOutCoroutine());
            }
        }
    }

    // 设置要显示的伤害值
    public void SetDamage(float damage)
    {
        string formatted = damage.ToString("0.00"); 
        damageText.text = formatted;
    }

    private IEnumerator FadeOutCoroutine()
    {
        float elapsedTime = 0f;
        Color startColor = damageText.color;
        
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeDuration);
            
            damageText.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
            yield return null;
        }
        
        Destroy(gameObject);
        // 可选：禁用文本
        //textToFade.gameObject.SetActive(false);
    }
}
