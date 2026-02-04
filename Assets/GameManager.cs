using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("锁定攻击资源配置")]
    public Material dashLineMaterial;  
    public Material solidLineMaterial;  
    public GameObject bulletPrefab;    

    [Header("游戏全局状态")]
    public bool isGameActive = true;
    public int enemyCount = 0;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); 
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void UpdateEnemyCount(int change)
    {
        enemyCount += change;
        if (enemyCount <= 0)
        {
            enemyCount = 0;
            OnAllEnemiesDefeated();
        }
    }

    void OnAllEnemiesDefeated()
    {
        isGameActive = false;
        Debug.Log("所有敌人已消灭，关卡胜利！");

    }
}
