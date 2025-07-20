using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Barrel : MonoBehaviour
{
    // Enemy Stats set in child class
    private float health = 100;
    public GameObject fire;
    private bool hasExploded = false;

    public float duration = 15f;
    public float damagePerSecond = 15f;
    public int playerDamagePerSecond = 10;
    public float damageInterval = 1f;
    public float radius = 2.5f;

    // Start is called before the first frame update.
    void Start()
    {
        fire.SetActive(false);
    }

    // Update is called once per frame.
    void Update()
    {

    }

    // ------Damage calculators----

    // Apply input damage to the enemy, kill if health empties
    public void TakeDamage(float dmg)
    {
        if (hasExploded == false)
        {
            health -= dmg;
            Debug.Log($"{gameObject.name} took {dmg} damage. Remaining: {health}");

            if (health <= 0)
            {
                Boom();
                hasExploded = true;
            }
        }
    }

    public void Boom()
    {
        fire.SetActive(true);
        StartCoroutine(DamageCoroutine());
    }


    private IEnumerator DamageCoroutine()
    {

        float elapsed = 0f;

        while (elapsed < duration)
        {
            Collider[] hitColliders = Physics.OverlapSphere(transform.position, radius);
            foreach (Collider col in hitColliders)
            {
                if (col.CompareTag("Enemy"))
                {
                    Enemy target = col.GetComponent<Enemy>();
                    if (target != null)
                    {
                        target.TakeDamage(damagePerSecond);
                    }
                }
                else if (col.CompareTag("Player"))
                {
                    PlayerController player = col.GetComponent<PlayerController>();
                    if (player != null)
                    {
                        player.ApplyDamage(playerDamagePerSecond);
                    }
                }
            }

            yield return new WaitForSeconds(damageInterval);
            elapsed += damageInterval;
        }
        fire.SetActive(false);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, radius); // 5f matches the explosion radius
    }
}
