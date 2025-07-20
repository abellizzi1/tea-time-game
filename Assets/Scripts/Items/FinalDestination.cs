using UnityEngine;

public class FinalDestination : Item
{
    private UIInventoryDisplay uiInventoryDisplay;

    private float critChance = 0.10f;

    public bool itemAcquired = false;

    void Awake()
    {
        player = PlayerController.Instance;
        uiInventoryDisplay = UIInventoryDisplay.Instance;
    }

    public override string toString()
    {
        return "Final Destination";
    }

    public override string tagString()
    {
        return "FinalDestination";
    }

    public override void onPickup()
    {
        itemAcquired = true;
    }

    public float getCritChance()
    {
        return critChance;
    }

    public bool getItemAcquired()
    {
        return itemAcquired;
    }

    public void UIFeedback(float fireRate)
    {
        uiInventoryDisplay.StartIndicator(60f / fireRate, "FinalDestination");
    }

    public void playCritNoise()
    {
        player.playerAudio.PlayCritSound();
    }
}
