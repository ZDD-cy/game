using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomTrigger : MonoBehaviour
{
    // 引用你的 FightActive 脚本
    public FightActive fightActive;

    // 当玩家进入触发器时
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("player") && fightActive != null)
        {
            // 调用 FightActive 里的 StartFight() 方法
            fightActive.StartFight();
            // 触发一次后就禁用触发器，防止重复触发
            GetComponent<Collider2D>().enabled = false;
        }
    }
}
