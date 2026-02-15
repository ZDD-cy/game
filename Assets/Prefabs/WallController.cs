using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class WallController : MonoBehaviour
{
    void Start()
    {
        // 一开始隐藏墙
        gameObject.SetActive(false);
    }

    // 玩家进来：显示墙
    public void RaiseWall()
    {
        gameObject.SetActive(true);
    }

    // 清完怪：隐藏墙
    public void LowerWall()
    {
        gameObject.SetActive(false);
    }


}
