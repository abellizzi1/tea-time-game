using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAudio : MonoBehaviour
{
    [SerializeField] private AudioClip[] footstepTileClips;
    [SerializeField] private AudioClip[] footstepWoodClips;
    [SerializeField] private AudioClip pickupClip;
    [SerializeField] private AudioClip damageClip;
    [SerializeField] private AudioClip purchaseClip;
    [SerializeField] private AudioClip insufficientFundsClip;
    [SerializeField] private AudioClip wingsOfVoiceDashClip;
    [SerializeField] private AudioClip finalDestinationCritClip;

    private AudioSource audioSource;
    private PlayerController playerController;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        playerController = GetComponent<PlayerController>();
    }

    public void PlayFootstep()
    {
        // Only play footstep audio if player is on ground
        if (playerController.getIsGrounded())
        {
            // Play wood footsteps when in the shop
            if (playerController.getIsInShop() && footstepWoodClips.Length > 0)
            {
                int index = Random.Range(0, footstepWoodClips.Length);
                audioSource.PlayOneShot(footstepWoodClips[index]);
            }
            else if (footstepTileClips.Length > 0)
            {
                int index = Random.Range(0, footstepTileClips.Length);
                audioSource.PlayOneShot(footstepTileClips[index]);
            }
        }
    }

    public void PlayPickupSound()
    {
        audioSource.PlayOneShot(pickupClip);
    }

    public void PlayPurchaseSound()
    {
        audioSource.PlayOneShot(purchaseClip);
    }

    public void PlayInsufficientFundsSound()
    {
        audioSource.PlayOneShot(insufficientFundsClip);
    }
    
    public void PlayDamageSound()
    {
        audioSource.PlayOneShot(damageClip);
    }

    public void PlayDashSound()
    {
        audioSource.PlayOneShot(wingsOfVoiceDashClip);
    }

    public void PlayCritSound()
    {
        audioSource.PlayOneShot(finalDestinationCritClip);
    }
}
