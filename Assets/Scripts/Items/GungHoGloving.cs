using UnityEngine;
using System;

public class GungHoGloves : Item
{
    private float reloadMultiplier = 0.75f;

    public bool itemAcquired = false;

    void Awake()
    {
        player = PlayerController.Instance;
    }

    public override string toString()
    {
        return "Gung Ho Gloves";
    }

    public override string tagString()
    {
        return "GungHoGloves";
    }

    public override void onPickup()
    {
        player.ApplyReloadSpeedDecrease(reloadMultiplier);
        player.reloadSpeedStat *= reloadMultiplier;
        player.reloadSpeedStatText.text = Math.Round((decimal)player.reloadSpeedStat, 2) + "x";

        itemAcquired = true;
    }

    public float getReloadMultiplier()
    {
        return reloadMultiplier;
    }
}
