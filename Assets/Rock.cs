using System;

internal class Rock
{
    public Action OnRockLanded { get; internal set; }

    internal void SetDamage(int damage)
    {
        throw new NotImplementedException();
    }
}