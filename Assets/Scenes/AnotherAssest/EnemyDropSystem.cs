using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class DropItem
{
    public GameObject itemPrefab;
    public string itemName;
    public int minCount = 1;      // 最小掉落数量
    public int maxCount = 3;      // 最大掉落数量
}
public class EnemyDropSystem : MonoBehaviour
{
    [Header("掉落物预制体")]
    public DropItem coin;
    public DropItem bead;

    [Header("掉落参数")]
    public float dropRadius = 1f;
    public float dropForce = 2f;

    // 敌人死亡时调用此方法触发掉落
    public void OnEnemyDie()
    {
        // 1. 必掉1个硬币
        SpawnDropItem(coin.itemPrefab, 1);

        // 2. 处理额外掉落概率
        float randomValue = Random.value;

        // 30%概率额外掉落1个硬币（总共2个）
        if (randomValue <= 0.3f)
        {
            SpawnDropItem(coin.itemPrefab, 1);
        }
        // 10%概率额外掉落2个硬币（总共3个）
        else if (randomValue <= 0.4f)
        {
            SpawnDropItem(coin.itemPrefab, 2);
        }
        // 30%概率掉落1个珠子
        else if (randomValue <= 0.7f)
        {
            SpawnDropItem(bead.itemPrefab, 1);
        }
        // 10%概率掉落2个珠子
        else if (randomValue <= 0.8f)
        {
            SpawnDropItem(bead.itemPrefab, 2);
        }
        // 剩余20%概率无额外掉落
    }

    // 生成掉落物
    public void SpawnDropItem(GameObject prefab, int count)
    {
        if (prefab == null) return;

        for (int i = 0; i < count; i++)
        {
            Vector3 randomOffset = Random.insideUnitSphere * dropRadius;
            randomOffset.y = Mathf.Abs(randomOffset.y); // 确保向上偏移
            Vector3 spawnPos = transform.position + randomOffset;

            GameObject dropItem = Instantiate(prefab, spawnPos, Quaternion.identity);

            Rigidbody rb = dropItem.GetComponent<Rigidbody>();
            if (rb != null)
            {
                Vector3 forceDir = Random.insideUnitSphere * dropForce;
                forceDir.y = Mathf.Abs(forceDir.y);
                rb.AddForce(forceDir, ForceMode.Impulse);
            }

            Destroy(dropItem, 10f);
        }
    }
}

