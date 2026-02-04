using System;
using UnityEngine;

internal class Rock :MonoBehaviour
{
    public Action OnRockLanded { get; internal set; }

    internal void SetDamage(int damage)
    {
        throw new NotImplementedException();
    }
}