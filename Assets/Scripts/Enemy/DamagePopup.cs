using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DamagePopup : MonoBehaviour
{
    [Tooltip("伤害数字的文本组件")]
    [SerializeField] private TextMeshPro damageText;

    [Tooltip("伤害数字弹出后上升的速度")]
    [SerializeField] private float moveSpeed = 2f;

    [Tooltip("伤害数字存在的时间（秒）")]
    [SerializeField] private float lifetime = 1f;

    [Tooltip("伤害数字上升时的旋转角度（度/秒）")]
    [SerializeField] private float rotateSpeed = 360f;

    private float timer;

    void Update()
    {
        // 让伤害数字向上移动
        transform.Translate(Vector3.up * moveSpeed * Time.deltaTime);
        // 让伤害数字旋转
        transform.Rotate(0, 0, rotateSpeed * Time.deltaTime);

        // 计时，时间到了就销毁
        timer += Time.deltaTime;
        if (timer >= lifetime)
        {
            Destroy(gameObject);
        }
    }

    // 设置要显示的伤害值
    public void SetDamage(int damage)
    {
        damageText.text = damage.ToString();
    }
}
