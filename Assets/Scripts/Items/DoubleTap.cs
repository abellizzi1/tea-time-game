using UnityEngine;
using System;

public class DoubleTap : Item
{
    public bool itemAcquired = false;
    private float damageMultiplier = 0.7f;

    void Awake()
    {
        player = PlayerController.Instance;
    }

    public override string toString()
    {
        return "Double Tap";
    }

    public override string tagString()
    {
        return "DoubleTap";
    }

    public override void onPickup()
    {
        player.ModifyPelletCount();
        player.ApplyDamageIncrease(damageMultiplier);
        player.damageStat *= damageMultiplier;
        player.damageStatText.text = Math.Round((decimal)player.damageStat, 2) + "x";
        itemAcquired = true;
    }
}
