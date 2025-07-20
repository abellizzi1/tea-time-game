using UnityEngine;
using System;

public class CannonballSplash : Item
{
    public bool itemAcquired = false;
    public Vector3 newBulletSize = new Vector3(0.25f, 0.25f, 0.25f);
    private float damageMultiplier = 1.1f;

    void Awake()
    {
        player = PlayerController.Instance;
    }

    public override string toString()
    {
        return "Cannonball Splash";
    }

    public override string tagString()
    {
        return "CannonballSplash";
    }

    public override void onPickup()
    {
        itemAcquired = true;
        player.ApplyDamageIncrease(damageMultiplier);
        player.damageStat *= damageMultiplier;
        player.damageStatText.text = Math.Round((decimal)player.damageStat, 2) + "x";
    }

    public bool getItemAcquired()
    {
        return itemAcquired;
    }
}
