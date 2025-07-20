using UnityEngine;

public class BombLogic : MonoBehaviour
{
    public GameObject particleEffects;
    public GameObject impactWarning;
    public float radius = 2f;
    public float damage = 25f;

    private GameObject activeIndicator;
    private Rigidbody rb;
    private bool hasHit = false;

    // Audio mangagement
    private AudioSource audioSource;
    public AudioClip sound;
    
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        Vector3 predLanding = FindDangerZone();
        activeIndicator = Instantiate(impactWarning, predLanding, impactWarning.transform.rotation);
        
        // Scale damage with cycle
        PlayerController player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();;
        if(player != null)
        {
            damage *= (1 + (.25f * player.numCyclesCompleted));
        }

        // Audio setup
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        // Configure 3D audio settings
        audioSource.spatialBlend = 1f; // 1 = fully 3D
        audioSource.rolloffMode = AudioRolloffMode.Linear;
        audioSource.minDistance = 1f;
        audioSource.maxDistance = 5f;
        audioSource.playOnAwake = false;

    }

    void OnCollisionEnter(Collision collision)
    {
        // Create expolosion
        Instantiate(particleEffects, transform.position, Quaternion.identity);
        if(sound != null)
        {
            AudioSource.PlayClipAtPoint(sound, transform.position);
        }

        // Deal damage
        Collider[] colliders = Physics.OverlapSphere(transform.position, radius);
        foreach(Collider obj in colliders)
        {
            if(obj.CompareTag("Player") && !hasHit)
            {
                hasHit = true;
                PlayerController player = obj.GetComponent<PlayerController>();
                player.ApplyDamage(damage);
                break;
            }
        }

        // Remove instance
        Destroy(activeIndicator);
        Destroy(gameObject);
    }

    private Vector3 FindDangerZone()
    {
        Vector3 position = transform.position;
        Vector3 currentVelocity = rb.linearVelocity;
        float simTime = 0f;
        float maxTime = 10f;
        float timestep = 0.1f;

        while(simTime < maxTime)
        {
            // Predict next position and velo
            Vector3 nextVelo = currentVelocity + Physics.gravity * timestep;
            Vector3 nextPos = position + currentVelocity * timestep;
            
            Vector3 direction = nextPos - position;
            float distance = direction.magnitude;

            Ray ray = new Ray(position, direction.normalized);
            RaycastHit[] collisions = Physics.RaycastAll(ray, distance);

            foreach(RaycastHit hit in collisions)
            {
                if (hit.collider.CompareTag("Ground") || hit.collider.CompareTag("LavaDoor"))
                {
                    return hit.point + Vector3.up * 0.1f;
                }
            }
            // Update for next loop
            position = nextPos;
            currentVelocity  = nextVelo;
            simTime += timestep;
        }
        return Vector3.zero;
    }

}
