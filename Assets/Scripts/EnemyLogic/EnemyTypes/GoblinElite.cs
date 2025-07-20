using UnityEngine;

public class GoblinElite : Enemy
{
    public float maxHealthBasic = 250;
    public float baseDamageBasic = 25f;
    public float speedBasic = 5f;
    public float collectibleDropChanceBasic = 0.5f;
    public float rangeBasic = 3.5f;
    public int spawnChanceBasic = 1;
    public AudioClip hurtSoundBasic;
    public AudioClip deathSoundBasic;

    protected override float maxHealth => maxHealthBasic;
    protected override float baseDamage => baseDamageBasic;
    protected override float speed => speedBasic;
    protected override float collectibleDropChance => collectibleDropChanceBasic;
    protected override float range => rangeBasic;
    protected override int spawnChance => spawnChanceBasic;
    protected override AudioClip hurtSound => hurtSoundBasic;
    protected override AudioClip deathSound => deathSoundBasic;

    protected override void StartAux() {}

    protected override void UpdateAI()
    {
        // If there's a reference to the playerTransform...
        if (playerTransform != null && navMeshAgent.enabled)
        {
            // Set the enemy's destination to the player's current position.
            navMeshAgent.SetDestination(playerTransform.position);

            bool attack = Vector3.Distance(transform.position, playerTransform.position) < range;
            anim.SetBool("attacking", attack);
            if (playerTransform != null)
            {
                // Look at player
                Vector3 direction = playerTransform.position - transform.position;
                direction.y = 0;
                if (direction.sqrMagnitude > 0.001f) 
                {
                    transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), Time.deltaTime * 45f);
                }
            }
            navMeshAgent.speed = 1.5f*speed;
        }
        else if (!navMeshAgent.enabled)
        {
            DeactivateHitbox();
            anim.SetBool("attacking", false);
            navMeshAgent.speed = speed;
        }
    }
}
