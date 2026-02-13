using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomTrigger : MonoBehaviour
{
    // 引用你的 FightActive 脚本
    public FightActive fightActive;
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && fightActive != null)
        {
            // 启动战斗
            fightActive.StartFight();
            // 关闭触发器防止重复触发
            GetComponent<Collider2D>().enabled = false;
        }
    }

}
