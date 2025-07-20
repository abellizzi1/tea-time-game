using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public Vector3 SelectSpawnPoint()
    {
        float x = Random.Range(transform.position.x - transform.localScale.x / 2, transform.position.x + transform.localScale.x / 2);
        float y = transform.position.y;
        float z = Random.Range(transform.position.z - transform.localScale.z / 2, transform.position.z + transform.localScale.z / 2);
        return new Vector3(x, y, z);
    }
}
