using UnityEngine;
using System;

public class ProteinPowder : Item
{
    private float damageMultiplier = 1.4f;

    public bool itemAcquired = false;

    void Awake()
    {
        player = PlayerController.Instance;
    }

    public override string toString()
    {
        return "Protein Powder";
    }

    public override string tagString()
    {
        return "ProteinPowder";
    }

    public override void onPickup()
    {
        player.ApplyDamageIncrease(damageMultiplier);
        player.damageStat *= damageMultiplier;
        player.damageStatText.text = Math.Round((decimal)player.damageStat, 2) + "x";

        itemAcquired = true;
    }

    public float getDamageMultiplier()
    {
        return damageMultiplier;
    }
}
