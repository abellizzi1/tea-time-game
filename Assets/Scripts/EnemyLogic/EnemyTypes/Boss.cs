using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using TMPro;

public class Boss : Enemy
{
    public float maxHealthBasic = 7500;
    public float baseDamageBasic = 0f;
    public float speedBasic = 0f;
    public float collectibleDropChanceBasic = 0.0f;
    public float rangeBasic = 9999999f;
    public int spawnChanceBasic = 0;
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

    // Boss room variables
    public EnemySpawnController spawner;
    private Coroutine phase2 = null;
    private Coroutine phase3 = null;
    private Coroutine bossRise = null;
    private Coroutine bossSink = null;
    private GameObject lw;
    public static bool bossDead;
    public GameObject projectilePrefab;
    public Transform throwPoint;
    private bool rainFire = false;
    private Coroutine throwing;
    private Coroutine rainingFire;
    public float launchSpeed = 15f;
    public float minLaunchAngle = -60f;
    public BoxCollider bombZone;
    public TextMeshProUGUI warningText;

    // UI Elements
    [SerializeField] private GameObject bossCanvas;
    [SerializeField] private HealthBar healthBar;

    protected override void StartAux()
    {
        healthBar.SetMaxHealth((int)health);
        healthBar.SetHealth((int)health);
        bossCanvas.SetActive(true);
        warningText.gameObject.SetActive(false);

        bossDead = false;

        lw = GameObject.FindWithTag("LavaWarning");
        if (lw != null)
        {
            Debug.Log("Disabling");
            lw.SetActive(false);
        }
    }


    protected override void UpdateAI()
    {
        healthBar.SetHealth(health);

        if (health <= 0)
        {
            bossCanvas.SetActive(false);
            bossDead = true;
        }

        // If there's a reference to the playerTransform...
        if (playerTransform != null)
        {
            // Look at player
            Vector3 direction = playerTransform.position - transform.position;
            direction.y = 0;
            if (direction.sqrMagnitude > 0.001f) 
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), Time.deltaTime * 45f);
            }

            // Check if time for phase change
            if(phase3 == null && health/maxHealth < .3)
            {
                bossSink = StartCoroutine(BossSink());
                phase3 = StartCoroutine(RaiseBossColumns());
            }
            else if(phase2 == null && health/maxHealth < .7)
            {
                bossSink = StartCoroutine(BossSink());
                phase2 = StartCoroutine(OpenBossLavaDoors());
            }

            // Phase 1-2 attack
            if(bossRise == null && bossSink == null && !rainFire)
            {
                if(throwing == null)
                {
                    throwing = StartCoroutine(ThrowAtPlayer());
                }
            }
            
            // Phase 3 attack
            if(rainFire && rainingFire == null)
            {
                rainingFire = StartCoroutine(RainFire());
            }
        }
    }

    private System.Collections.IEnumerator ThrowAtPlayer()
    {
        float selectedAngle;
        Vector3 toTargetXZ;
        CalculateThrow(throwPoint.position, launchSpeed, minLaunchAngle, out selectedAngle, out toTargetXZ);
        if (selectedAngle != -999f){
            anim.SetTrigger("attack");
            yield return new WaitForSeconds(UnityEngine.Random.Range(3,6));
        }
        throwing = null;
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
    }

    private System.Collections.IEnumerator RainFire()
    {

        int count = 0;
        while(true)
        {
            count++;
            Vector3 center = bombZone.center + bombZone.transform.position;
            Vector3 size = bombZone.size * 0.45f;

            float x = Random.Range(-size.x, size.x);
            float z = Random.Range(-size.z, size.z);
            Vector3 spawnPos = center + new Vector3(x, 0, z);
            Instantiate(projectilePrefab, spawnPos, Quaternion.identity);

            if(count % 20 == 0 && playerTransform != null)
            {
                spawnPos = new Vector3(playerTransform.position.x, center.y, playerTransform.position.z);
                Instantiate(projectilePrefab, spawnPos, Quaternion.identity);
            }
            yield return new WaitForSeconds(.15f);
        }
        yield return null;
    }




    IEnumerator OpenBossLavaDoors()
    {
        GameObject[] lavaDoors = GameObject.FindGameObjectsWithTag("LavaDoor");
        float targetXbase = 20f;
        float speed = 2f;   
        bool done = false;
        while(true)
        {
            foreach (GameObject door in lavaDoors)
            {
                float targetX = targetXbase;
                if (door.transform.position.x < 0)
                {
                    targetX = -Mathf.Abs(targetXbase);
                }
                float direction = Mathf.Sign(targetX - door.transform.position.x);
                door.transform.position += Vector3.right * direction * speed * Time.deltaTime;

                // Check if we’ve reached or passed the targetX
                if ((direction > 0 && door.transform.position.x >= targetX) ||
                    (direction < 0 && door.transform.position.x <= targetX))
                {
                    // Snap to exact targetX and disable the object
                    door.transform.position = new Vector3(targetX, door.transform.position.y, door.transform.position.z);
                    door.gameObject.SetActive(false);
                    done = true;
                }
                if(done)
                {
                    transform.position = new Vector3(1f, -25f, -2f);
                    bossRise = StartCoroutine(BossRise());
                    yield break;
                }
            }
            yield return null;
        }
    }

    IEnumerator RaiseBossColumns()
    {
        GameObject[] cols = GameObject.FindGameObjectsWithTag("BossColumns");
        GameObject lava = GameObject.FindGameObjectWithTag("Lava");
        float moveSpeed = 1.5f; // units per second

        if (lw != null)
        {
            warningText.gameObject.SetActive(true);
            lw.SetActive(true);
        }
        yield return new WaitForSeconds(8f);

        while(true)
        {
            lava.transform.position = Vector3.MoveTowards(
                lava.transform.position,
                new Vector3(lava.transform.position.x, 10f, lava.transform.position.z),
                moveSpeed * Time.deltaTime
            );
            
            foreach (GameObject col in cols)
            {
                col.transform.position = Vector3.MoveTowards(
                    col.transform.position,
                    new Vector3(col.transform.position.x, 6.5f, col.transform.position.z),
                    moveSpeed * Time.deltaTime
                );
                if (col.transform.position.y >= 6.5f)
                {
                    transform.position = new Vector3(6.5f, -10f, -8f);
                    RemoveEnemies();
                    warningText.gameObject.SetActive(false);
                    rainFire = true;
                    bossRise = StartCoroutine(BossRise());
                    yield break;
                }
            }
            yield return null;
        }
    }

    IEnumerator BossRise()
    {
        Vector3 startPos = transform.position;
        Vector3 endPos =  startPos + Vector3.up * 16f;
        float speed = 2.5f;
        damageable = true;

        while (Vector3.Distance(transform.position, endPos) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(transform.position, endPos, speed * Time.deltaTime);
            yield return null;
        }

        // Snap to exact final position
        transform.position = endPos;
        bossRise = null;
    }

    IEnumerator BossSink()
    {
        damageable = false;
        Vector3 startPos = transform.position;
        Vector3 endPos =  startPos + Vector3.down * 15f;
        float speed = 5f;

        while (Vector3.Distance(transform.position, endPos) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(transform.position, endPos, speed * Time.deltaTime);
            yield return null;
        }

        // Snap to exact final position
        transform.position = endPos;
        bossSink = null;
    }

    private void RemoveEnemies()
    {
        spawner.Stop();

        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject enemy in enemies)
        {
            if(enemy != this.gameObject)
            {
                Destroy(enemy);
            }
        }
    }
}