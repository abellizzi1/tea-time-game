using UnityEngine;
using System;

public class SwiftExecution : Item
{
    private float fireRateMultipler = 1.15f;

    public bool itemAcquired = false;

    void Awake()
    {
        player = PlayerController.Instance;
    }

    public override string toString()
    {
        return "Swift Execution";
    }

    public override string tagString()
    {
        return "SwiftExecution";
    }

    public override void onPickup()
    {
        itemAcquired = true;

        player.ApplyFireRateIncrease(fireRateMultipler);
        player.fireRateStat *= fireRateMultipler;
        player.fireRateStatText.text = Math.Round((decimal)player.fireRateStat, 2) + "x";
    }

    public float getMultiplier()
    {
        return fireRateMultipler;
    }
}
