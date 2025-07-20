using UnityEngine;

public class EnemySpawnController : MonoBehaviour
{
    public GameObject[] enemyModels;
    public EnemySpawner[] spawners;
    public float minSpawnDelay = 1f;
    public float maxSpawnDelay = 3f;
    public bool active = true;

    private int totalEnemyWeight = 0;

    void Start()
    {
        // Speed up spawns as game progresses
        PlayerController player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();;
        if(player != null)
        {
            minSpawnDelay *= Mathf.Exp(-.25f*player.numCyclesCompleted);
            maxSpawnDelay *= Mathf.Exp(-.25f*player.numCyclesCompleted);
        }

        ScheduleSpawn();
        foreach (GameObject enemyObj in enemyModels)
        {
            Enemy enemy = enemyObj.GetComponent<Enemy>();
            totalEnemyWeight += enemy.getSpawnChance();
        }
    }

    void ScheduleSpawn()
    {
        float delay = Random.Range(minSpawnDelay, maxSpawnDelay);
        Invoke(nameof(SpawnEnemy), delay);
    }

    void SpawnEnemy()
    {
        if (active)
        {

            // Select spawn location
            EnemySpawner spawner = spawners[Random.Range(0, spawners.Length)];
            Vector3 spawnPoint = spawner.SelectSpawnPoint();

            // Select enemy type
            int idx = UnityEngine.Random.Range(1, totalEnemyWeight + 1);
            int curr = 0;
            GameObject selectedEnemy = enemyModels[0];
            foreach (GameObject enemyObj in enemyModels)
            {
                Enemy enemy = enemyObj.GetComponent<Enemy>();
                curr += enemy.getSpawnChance();
                if (curr >= idx)
                {
                    selectedEnemy = enemyObj;
                    break;
                }
            }

            // Spawn enemy
            Instantiate(selectedEnemy, spawnPoint, Quaternion.identity);

            // Select time of next spawn
            ScheduleSpawn();
        }
    }

    public void Stop()
    {
        active = false;
    }

    public void StartSpawner()
    {
        active = true;
        ScheduleSpawn();
    }
}