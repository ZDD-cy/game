using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class WallController : MonoBehaviour
{
    public List<EnemyStatus> enemiesInRoom = new List<EnemyStatus>();

    void Start()
    {
        // 一开始隐藏墙
        gameObject.SetActive(false);
    }

    // 玩家进来：显示墙
    public void RaiseWall()
    {
        gameObject.SetActive(true);
        Debug.Log("墙已显示！");
    }

    // 清完怪：隐藏墙
    public void LowerWall()
    {
        gameObject.SetActive(false);
    }

    // 检查所有敌人是否死亡
    public void CheckAllEnemiesDead()
    {
        foreach (var enemy in enemiesInRoom)
        {
            if (enemy != null && !enemy.isDead)
                return;
        }
        LowerWall();
    }
}
