using UnityEngine;
using System;

public class ApollyonsPit : Item
{
    [SerializeField] private UIInventoryDisplay UIInventory;

    private float fireRateMultipler = 2.0f;
    private float damageMultiplier = 0.65f;

    public bool itemAcquired = false;

    void Awake()
    {
        player = PlayerController.Instance;
    }

    public override string toString()
    {
        return "Apollyon's Pit";
    }

    public override string tagString()
    {
        return "ApollyonsPit";
    }

    public override void onPickup()
    {
        itemAcquired = true;

        player.ApplyFireRateIncrease(fireRateMultipler);
        player.fireRateStat *= fireRateMultipler;
        player.fireRateStatText.text = Math.Round((decimal)player.fireRateStat, 2) + "x";

        player.ApplyDamageIncrease(damageMultiplier);
        player.damageStat *= damageMultiplier;
        player.damageStatText.text = Math.Round((decimal)player.damageStat, 2) + "x";
    }

    public float getFRMultiplier()
    {
        return fireRateMultipler;
    }

    public float getDmgMultiplier()
    {
        return damageMultiplier;
    }
}
