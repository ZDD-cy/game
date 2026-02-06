using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;

// 挂载在所有敌人身上的受伤+属性脚本
public class EnemyTakeDamage : MonoBehaviour
{
    [Header("敌人基础属性")]
    public float maxHp = 50f;    // 最大血量（公共，可外部调用）
    public float currentHp;      // 当前血量（公共，可外部调用）
    public string enemyName = "普通敌人"; // 可选：敌人名称
    public float defense = 2f;   // 可选：防御（减伤用）
    public GameObject damagePopupPrefab;      //声明伤害预制体                          // 声明伤害弹出预制体变量


    void Start()
    {
        currentHp = maxHp; // 初始化当前血量
    }

    // 受伤方法（外部攻击脚本调用）
    public void TakeDamage(float damage)
    {

        ShowDamagePopup(damage);
        float finalDamage = Mathf.Max(damage - defense, 0);
        currentHp -= finalDamage;
        Debug.Log($"{enemyName}受到{finalDamage}伤害，剩余血量：{currentHp:F1}");

        if (currentHp <= 0) Die();

    }

    // 敌人死亡
    void Die()
    {
        Destroy(gameObject);
    }
    // 辅助方法：显示伤害数字
    private void ShowDamagePopup(float damage)
    {
        if (damagePopupPrefab != null)
        {
            GameObject popup = Instantiate(damagePopupPrefab, transform.position, Quaternion.identity);
            popup.GetComponent<DamagePopup>().SetDamage(Mathf.RoundToInt(damage));
            Destroy(popup, 1f);
        }
    }
}
