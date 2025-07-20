using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Freeze : Item
{
    private KeyCode freezeKey = KeyCode.F;
    private float freezeCooldown = 5.0f;
    private bool isFrozen = false;
    public float freezeDuration = 5f;

    private UIInventoryDisplay UIInventory;
    private PlayerController playerController;

    void Awake()
    {
        UIInventory = UIInventoryDisplay.Instance;
        playerController = PlayerController.Instance;
    }

    public override string toString()
    {
        return "Freeze";
    }

    public override string tagString()
    {
        return toString();
    }

    void Update()
    {
        if (!isFrozen && freezeCooldown == 0.0f && UIInventory.getItemCounts().ContainsKey("Freeze") && UIInventory.getItemCounts()["Freeze"] > 0 && Input.GetKeyDown(freezeKey) && !playerController.getIsInShop())
        {
            UIInventory.decrementItemCount("Freeze");
            doFreeze();
        }

        if (freezeCooldown > 0.0f)
        {
            freezeCooldown -= Time.deltaTime;
        }
        else
        {
            freezeCooldown = 0.0f;
        }
    }

    public override void onPickup()
    {
        Debug.Log("Freeze Acquired");
        playerController.itemText.SetText("Freeze", 5.0f);
        playerController.itemDescText.SetText("Press [F] to freeze enemies for 5 seconds!", 5.0f);
    }

    public void resetCooldown()
    {
        freezeCooldown = 5.0f;
        UIInventory.StartCooldown(freezeCooldown, "Freeze");
    }

    private void doFreeze()
    {
        isFrozen = true;
        StartCoroutine(FreezeCoroutine());
    }

    private void StopFreeze()
    {
        if (!isFrozen) { return; }

        playerController.EnableEnemies();

        isFrozen = false;
        resetCooldown();
    }

    private IEnumerator FreezeCoroutine()
    {
        playerController.DisableEnemies();

        // Wait for dash duration then restore original speed values
        yield return new WaitForSeconds(freezeDuration);
        StopFreeze();
    }
}
