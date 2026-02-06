using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PlayerInventory : MonoBehaviour
{
    private int coins = 0;
    private int beads = 0;

    public void AddCoins(int amount)
    {
        coins += amount;
        Debug.Log($"获得 {amount} 个硬币，当前总数: {coins}");
    }

    public void AddBeads(int amount)
    {
        beads += amount;
        Debug.Log($"获得 {amount} 个珠子，当前总数: {beads}");
    }

    public int GetCoinCount() => coins;
    public int GetBeadCount() => beads;
}


