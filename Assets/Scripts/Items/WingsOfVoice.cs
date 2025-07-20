using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class WingsOfVoice : Item
{
    private UIInventoryDisplay UIInventory;
    private RootMotionControlScript rootMotionControl;
    private PlayerController playerController;

    public float dashDuration = 0.5f;
    public KeyCode dashKey = KeyCode.LeftControl;
    public float boostedAnimationSpeed = 3f;
    public float boostedRootMovementSpeed = 3f;
    private float dashCooldown = 7.5f;
    private bool itemAcquired = false;
    private LayerMask dashCollisionMask;
    private bool isDashing = false;
    private float originalAnimSpeed;
    private float originalRootSpeed;

    public override string toString()
    {
        return "Wings of Voice";
    }

    public override string tagString()
    {
        return "WingsOfVoice";
    }

    void Awake()
    {
        playerController = PlayerController.Instance;
        rootMotionControl = playerController.rootMotionControl;
        UIInventory = UIInventoryDisplay.Instance;
    }

    void Start()
    {
        // How to assign to multiple layers if needed in the future: LayerMask.GetMask("Layer1", "Layer2", ...)
        dashCollisionMask = LayerMask.GetMask("Obstacle", "Enemy");
        // Save original values since root's values will be modified when dashing
        originalAnimSpeed = rootMotionControl.animationSpeed;
        originalRootSpeed = rootMotionControl.rootMovementSpeed;
    }

    void Update()
    {
        // No dashing in the shop
        if (itemAcquired && dashCooldown == 0.0f && Input.GetKeyDown(dashKey) && !isDashing && !playerController.getIsInShop())
        {
            doDash();
        }

        if (dashCooldown > 0.0f)
        {
            dashCooldown -= Time.deltaTime;
        }
        else
        {
            dashCooldown = 0.0f;
        }
    }

    public override void onPickup()
    {
        Debug.Log("PICKED IT UP");
        itemAcquired = true;
        dashCooldown = 0.0f;
    }

    public void resetCooldown()
    {
        dashCooldown = 7.5f;
        UIInventory.StartCooldown(dashCooldown, "WingsOfVoice");
    }

    public bool isCooledDown()
    {
        return dashCooldown == 0.0f;
    }

    public bool hasItem()
    {
        return itemAcquired;
    }

    public KeyCode getInputKey()
    {
        return dashKey;
    }

    private void doDash()
    {
        isDashing = true;
        StartCoroutine(DashCoroutine());
    }

    private void StopDash(float originalAnimSpeed, float originalRootSpeed)
    {
        if (!isDashing) { return; }
        rootMotionControl.animationSpeed = originalAnimSpeed;
        rootMotionControl.rootMovementSpeed = originalRootSpeed;
        isDashing = false;
        resetCooldown();
    }

    private IEnumerator DashCoroutine()
    {
        playerController.playerAudio.PlayDashSound();
        // Set temporary animation and root movement speed
        rootMotionControl.animationSpeed = boostedAnimationSpeed;
        rootMotionControl.rootMovementSpeed = boostedRootMovementSpeed;

        // Wait for dash duration then restore original speed values
        yield return new WaitForSeconds(dashDuration);
        StopDash(originalAnimSpeed, originalRootSpeed);
    }

    public void OnPlayerTriggerEnter(Collider other)
    {
        if (isDashing && ((1 << other.gameObject.layer) & dashCollisionMask) != 0)
        {
            Debug.Log("Dash interrupted by: " + other.gameObject.name);
            StopDash(originalAnimSpeed, originalRootSpeed);
        }
    }
}
