using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class PatrolPoint : MonoBehaviour
{
    private BoxCollider zone;

    void Awake()
    {
        zone = GetComponent<BoxCollider>();
        zone.isTrigger = true;
    }

    // Returns a random point within the box bounds (local space converted to world)
    public Vector3 GetRandomPoint()
    {
        Vector3 center = zone.center + transform.position;
        Vector3 size = zone.size * 0.5f;

        float x = Random.Range(-size.x, size.x);
        float y = Random.Range(-size.y, size.y);
        float z = Random.Range(-size.z, size.z);

        return center + new Vector3(x, y, z);
    }
}
