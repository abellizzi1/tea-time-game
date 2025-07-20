using UnityEngine;
using System;

public class DeepPockets : Item
{
    private float ammoBonus = 0.1f;

    public bool itemAcquired = false;

    void Awake()
    {
        player = PlayerController.Instance;
    }

    public override string toString()
    {
        return "Deep Pockets";
    }

    public override string tagString()
    {
        return "DeepPockets";
    }

    public override void onPickup()
    {
        player.ApplyAmmoBonus(ammoBonus);

        itemAcquired = true;

        Debug.Log("Deep Pockets Acquired, ammo capacity increased by: " + ammoBonus);
    }

    public float getAmmoBonus()
    {
        return ammoBonus;
    }
}
