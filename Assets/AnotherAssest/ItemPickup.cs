using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ItemPickup : MonoBehaviour
{
    [Header("物品基础配置")]
    public ItemType itemType; // 选择是硬币还是珠子
    public int value = 1; // 拾取的数量
    public float pickupRange = 1.5f; // 自动拾取范围

    [Header("视觉反馈")]
    public float rotationSpeed = 90f; // 旋转
    public float floatAmplitude = 0.1f; // 漂浮
    public float floatFrequency = 1f; // 漂浮频率

    private Vector3 startPos;
    private Rigidbody rb;

    void Start()
    {
        startPos = transform.position;
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        // 自动旋转和漂浮效果
        transform.Rotate(0, rotationSpeed * Time.deltaTime, 0);
        transform.position = startPos + Vector3.up * Mathf.Sin(Time.time * floatFrequency) * floatAmplitude;

        // 检测玩家进入范围
        Collider[] colliders = Physics.OverlapSphere(transform.position, pickupRange);
        foreach (var col in colliders)
        {
            if (col.CompareTag("Player"))
            {
                OnPickup(col.gameObject);
                break;
            }
        }
    }

    void OnPickup(GameObject player)
    {
        // 通知玩家的Inventory添加物品
        PlayerInventory playerInventory = player.GetComponent<PlayerInventory>();
        if (playerInventory != null)
        {
            if (itemType == ItemType.Coin)
                playerInventory.AddCoins(value);
            else if (itemType == ItemType.Bead)
                playerInventory.AddBeads(value);
        }

        // 播放拾取音效或特效（可选）
        // AudioSource.PlayClipAtPoint(pickupSound, transform.position);

        Destroy(gameObject);
    }

    // Gizmos在编辑器里显示拾取范围
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, pickupRange);
    }
}

public enum ItemType
{
    Coin,
    Bead
}


