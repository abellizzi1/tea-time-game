using UnityEngine;
using System;

public class PumpedUpKicks : Item
{
    private float speedMultiplier = 1.05f;

    public bool itemAcquired = false;

    void Awake()
    {
        player = PlayerController.Instance;
    }

    public override string toString()
    {
        return "Pumped Up Kicks";
    }

    public override string tagString()
    {
        return "PumpedUpKicks";
    }

    public override void onPickup()
    {
        player.ApplySpeedIncrease(speedMultiplier);
        player.speedStat *= speedMultiplier;
        player.speedStatText.text = Math.Round((decimal)player.speedStat, 2) + "x";
        itemAcquired = true;
    }

    public float getSpeedMultiplier()
    {
        return speedMultiplier;
    }
}
