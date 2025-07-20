using UnityEngine;

public class CoeurDeVie : Item
{
    [SerializeField] private UIInventoryDisplay UIInventory;
    private float HealthBonus = 25f;

    public bool itemAcquired = false;

    void Awake()
    {
        player = PlayerController.Instance;
    }

    public override string toString()
    {
        return "Coeur De Vie";
    }

    public override string tagString()
    {
        return "CoeurDeVie";
    }

    public override void onPickup()
    {
        player.maxHealth += HealthBonus;
        player.currentHealth = player.maxHealth;

        player.healthBar.SetMaxHealth(player.maxHealth);
        player.healthBar.SetHealth(player.currentHealth);

        itemAcquired = true;

        Debug.Log("Coeur De Vie Acquired, max health is now: " + player.maxHealth);
    }

    public float getHealthBonus()
    {
        return HealthBonus;
    }
}
