using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;

public class Bomber : Enemy
{
    public float maxHealthBasic = 50;
    public float baseDamageBasic = 0f;
    public float speedBasic = 10f;
    public float collectibleDropChanceBasic = 0.25f;
    public float rangeBasic = 0;
    public int spawnChanceBasic = 3;
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

    // Bomber specific vars
    private bool isThrowing = false;
    private bool reposition = true;
    public GameObject projectilePrefab;
    public Transform throwPoint;
    public float escapeDistance = 10f;
    float launchSpeed = 15f;
    float minLaunchAngle = 60f;

    protected override void StartAux(){}
    
    protected override void UpdateAI()
    {
        if (playerTransform != null && navMeshAgent.enabled)
        {
            // If throwing dont cancel
            if (isThrowing)
            {
                Vector3 direction = (playerTransform.position - transform.position).normalized;
                direction.y = 0f;
                transform.rotation = Quaternion.LookRotation(direction);
                return;
            }

            // Check if player is closing in if so escape
            if (Vector3.Distance(transform.position, playerTransform.position) <= escapeDistance)
            {
                reposition = true;
            }

            // Move if not in a good spot
            if (reposition)
            {
                if (!navMeshAgent.pathPending && navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance)
                {
                    MoveToPatrolZone();
                }
            }
            // else attack
            else
            {
                // Face the player
                Vector3 direction = (playerTransform.position - transform.position).normalized;
                direction.y = 0f;
                transform.rotation = Quaternion.LookRotation(direction);
                if (!isThrowing)
                {
                    StartCoroutine(ThrowAtPlayer());
                }
            }
        }
    }

    private void MoveToPatrolZone()
    {
        // Select point to patrol to (cannot be too close to player)
        PatrolPoint[] allZones = FindObjectsOfType<PatrolPoint>();
        List<PatrolPoint> zones = new List<PatrolPoint>();
        foreach(PatrolPoint zone in allZones)
        {
            if(Vector3.Distance(zone.transform.position, playerTransform.position) > escapeDistance)
            {
                zones.Add(zone);
            }
        }

        if (zones.Count == 0) return;
        PatrolPoint selectedZone = zones[Random.Range(0, zones.Count)];

        if (NavMesh.SamplePosition(selectedZone.GetRandomPoint(), out NavMeshHit hit, 2f, NavMesh.AllAreas))
        {
            navMeshAgent.SetDestination(hit.position);
            reposition = false;
        }
        else
        {
            // Retry
            MoveToPatrolZone();
        }
    }
    

    private System.Collections.IEnumerator ThrowAtPlayer()
    {
        // Set default throw settings for a lob throw
        launchSpeed = 15f;
        minLaunchAngle = 60f;

        // Line of sight check
        Vector3 losDir = (playerTransform.position + Vector3.up * 1f) - throwPoint.position; // aiming toward player's upper body
        if (Physics.Raycast(throwPoint.position, losDir.normalized, out RaycastHit hit, losDir.magnitude))
        {
            if (hit.collider.CompareTag("Player"))
            {
                // Clean shot, switch to a beam
                launchSpeed = 10f;
                minLaunchAngle = -30f;
            }
        }
        
        float selectedAngle;
        Vector3 toTargetXZ;
        CalculateThrow(throwPoint.position, launchSpeed, minLaunchAngle, out selectedAngle, out toTargetXZ);
        if (selectedAngle != -999f){
            isThrowing = true;
            anim.SetTrigger("attack");
            yield return new WaitForSeconds(2f);
        }
    }

    public void ReleaseProjectile()
    {
        // Throw projectile
        if (projectilePrefab && throwPoint && playerTransform)
        {  
            // Get angle needed to hit player, if valid throw the bomb
            float selectedAngle;
            Vector3 toTargetXZ;
            CalculateThrow(throwPoint.position, launchSpeed, minLaunchAngle, out selectedAngle, out toTargetXZ);
            if (selectedAngle != -999f)
            {
                GameObject proj = Instantiate(projectilePrefab, throwPoint.position, Quaternion.identity);
                Rigidbody rb = proj.GetComponent<Rigidbody>();
                Vector3 velocity = toTargetXZ.normalized * launchSpeed * Mathf.Cos(selectedAngle);
                velocity.y = launchSpeed * Mathf.Sin(selectedAngle);
                rb.linearVelocity = velocity;
            }
        }
        isThrowing = false;
    }

}
