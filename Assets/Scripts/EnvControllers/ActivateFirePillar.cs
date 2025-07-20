using UnityEngine;

public class ActivateFirePillar : MonoBehaviour
{
    public GameObject firePillar;
    public int playerProximityCount = 0;
    public string playerTag = "Player"; // Adjust this if your player uses a different tag

    private bool playerInRange = false;

    void Start()
    {
        if (firePillar != null)
            firePillar.SetActive(false); // Hide the fire pillar at start
    }

    void Update()
    {
        // Optionally, do something while the player is in range
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            playerProximityCount++;
            playerInRange = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            playerInRange = false;
        }
    }

    public void buttonPressed()
    {
        if (firePillar != null)
            firePillar.SetActive(true); // Show the fire pillar
    }
}
