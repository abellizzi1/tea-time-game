using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public abstract class Enemy : MonoBehaviour
{
    // Reference to the player's transform.
    protected Transform playerTransform;
    // Reference to PlayerController component in the Player
    private PlayerController playerController;

    // Reference to the NavMeshAgent component for pathfinding.
    protected NavMeshAgent navMeshAgent;

    // Reference to models animator
    protected Animator anim;
    
    // References to enemy texture for damage animation
    private Material mat;
    private Color originalColor;
    private Coroutine damageCoroutine;
    private Coroutine damageAudioCoroutine;

    // Reference to the collectible the enemy can drop
    public GameObject collectiblePrefab;

    private bool isAttacking = false;
    protected bool damageable = true;

    // Enemy Stats set in child class
    protected abstract float maxHealth { get; }
    protected float health;
    protected float damage;
    protected abstract float baseDamage { get; }
    protected abstract float speed { get; }
    protected abstract float collectibleDropChance { get; }
    protected abstract float range { get; }
    protected abstract int spawnChance { get; }

    // Audio mangagement
    private AudioSource audioSource;
    protected abstract AudioClip hurtSound {get; }
    protected abstract AudioClip deathSound {get; }


    // Start is called before the first frame update.
    void Start()
    {
        // Set Stats
        health = maxHealth;
        damage = baseDamage;
        PlayerController player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();;
        if(player != null)
        {
            health *= (1 + (.25f * player.numCyclesCompleted));
            damage *= (1 + (.25f * player.numCyclesCompleted));
        }

        // Get and store the NavMeshAgent component attached to this object.
        GameObject playerGameObject = GameObject.FindWithTag("Player");
        playerTransform = playerGameObject.transform;
        playerController = playerGameObject.GetComponent<PlayerController>();
        navMeshAgent = GetComponent<NavMeshAgent>();
        navMeshAgent.speed = speed;
        anim = GetComponent<Animator>();
        
        // Get reference to mesh render of enemy 
        SkinnedMeshRenderer smr = GetComponentInChildren<SkinnedMeshRenderer>();
        if (smr != null)
        {
            mat = smr.material; // Creates a unique instance
            originalColor = mat.color;
        }

        // Audio setup
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        // Configure 3D audio settings
        audioSource.spatialBlend = 1f; // 1 = fully 3D
        audioSource.rolloffMode = AudioRolloffMode.Linear;
        audioSource.minDistance = 1f;
        audioSource.maxDistance = 50f;
        audioSource.playOnAwake = false;

        // Run anything enemy specific
        StartAux();
    }
    protected virtual void StartAux() { }

    // Update is called once per frame.
    void Update()
    {
        anim.SetFloat("speed", navMeshAgent.velocity.magnitude);
        UpdateAI();
    }

    protected abstract void UpdateAI();

    // ------Damage calculators----

    // Apply input damage to the enemy, kill if health empties
    public void TakeDamage(float dmg)
    {
        if(damageable)
        {
            health -= dmg;
            if(damageCoroutine != null)
            {
                StopCoroutine(damageCoroutine);
                mat.color = originalColor;
            }
            damageCoroutine = StartCoroutine(PlayDamageAnimation());
            damageAudioCoroutine = StartCoroutine(PlayDamageAudio());
            Debug.Log($"{gameObject.name} took {dmg} damage. Remaining: {health}");
            if (health <= 0)
            {
                Die();
            }
        }
    }

    private IEnumerator PlayDamageAudio()
    {
        // Play audio (delay to not spam)
        if(hurtSound != null)
        {
            audioSource.PlayOneShot(hurtSound);
        }
        yield return new WaitForSeconds(0.5f);
        damageAudioCoroutine = null;
    }
    
    private IEnumerator PlayDamageAnimation()
    {
        // Flash red
        mat.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        mat.color = originalColor;

        damageCoroutine = null;
    }

    // Calculate damage that will be applied by enemy and return it
    public void DoDamage(PlayerController player)
    {

        // TODO: Possibly add in any damage scaling
        player.ApplyDamage(damage);
    }

    public void Die()
    {
        if (navMeshAgent != null && navMeshAgent.enabled && navMeshAgent.isOnNavMesh) {
            navMeshAgent.isStopped = true;
            navMeshAgent.velocity = Vector3.zero;
        }
        else
        {
            navMeshAgent.enabled = false;
        }
        // Make sure animation has an event to remove body
        anim.SetBool("dead", true);
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.enabled = false;
        }

        // Play death noise
        if(deathSound != null)
        {
            audioSource.PlayOneShot(deathSound);
        }
    }

    public void ClearBody()
    {
        if (Random.value < collectibleDropChance)
        {
            if (collectiblePrefab != null)
            {
                Instantiate(collectiblePrefab, transform.position, Quaternion.identity);
            }
        }
        Destroy(gameObject);
        playerController.incrementEnemiesKilled();
    }

    public void ActivateHitbox()
    {
        isAttacking = true;
    }
    public void DeactivateHitbox()
    {
        isAttacking = false;
    }
    public bool getAttackState()
    {
        return isAttacking;
    }

    public int getSpawnChance()
    {
        return spawnChance;
    }

    protected void CalculateThrow(Vector3 throwPoint, float launchSpeed, float minLaunchAngle, out float selectedAngle, out Vector3 toTargetXZ)
    {

        // Calculate throw angle
        selectedAngle = -999f;
        Vector3 toTarget = playerTransform.position - throwPoint;
        toTargetXZ = new Vector3(toTarget.x, 0, toTarget.z);
        float yOffset = toTarget.y;
        float distance = toTargetXZ.magnitude;

        float gravity = Mathf.Abs(Physics.gravity.y);
        float minAngleRad = minLaunchAngle * Mathf.Deg2Rad;

        float discriminant = launchSpeed * launchSpeed * launchSpeed * launchSpeed - gravity * (gravity * distance * distance + 2 * yOffset * launchSpeed * launchSpeed);

        // No valid way to reach target, try again next cycle
        if (discriminant > 0)
        {

            float sqrtDisc = Mathf.Sqrt(discriminant);

            // Two angle options
            float lowAngle = Mathf.Atan2(launchSpeed * launchSpeed - sqrtDisc, gravity * distance);
            float highAngle = Mathf.Atan2(launchSpeed * launchSpeed + sqrtDisc, gravity * distance);

            if (lowAngle >= minAngleRad)
            {
                selectedAngle = lowAngle;
            }
            else if (highAngle >= minAngleRad)
            {
                selectedAngle = highAngle;
            }
        }
    }
}
