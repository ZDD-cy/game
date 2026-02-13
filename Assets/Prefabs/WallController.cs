using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class WallController : MonoBehaviour
{
    // 升起后的目标位置
    public Vector3 raisedPosition;
    // 降下后的目标位置
    public Vector3 loweredPosition;

    public float moveSpeed = 2f;
    private bool isWallActive = false;

    // 本房间的敌人
    public List<EnemyStatus> enemiesInRoom = new();

    private void Start()
    {
        // 一开始墙是降下的
        transform.position = loweredPosition;
        
    }

    private void Update()
    {
        if (isWallActive)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                raisedPosition,
                moveSpeed * Time.deltaTime
            );
        }
        else
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                loweredPosition,
                moveSpeed * Time.deltaTime
            );
        }
    }

    // 玩家进入房间，升起墙
    public void RaiseWall()
    {
        isWallActive = true;
        Debug.Log("wall shou be ring now!");
    }

    // 敌人清完，降下墙
    public void LowerWall()
    {
        isWallActive = false;
    }

    // 检查敌人是否全死了
    public void CheckAllEnemiesDead()
    {
        foreach (var enemy in enemiesInRoom)
        {
            if (!enemy.isDead)
                return;
        }
        LowerWall();
    }
}
