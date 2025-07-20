using System.Collections;
using UnityEngine;

/// <summary>
/// Handles the life‑cycle and behaviour of a *single* bullet instance.
/// A bullet is spawned by <c>PlayerWeapon</c>, given an initial <see cref="velocity"/>, and then:
/// <list type="bullet">
///   <item>Moves every frame using simple kinematics (no rigidbody).</item>
///   <item>Performs a ray‑cast each frame to detect hits along its path.</item>
///   <item>Renders a tracer line via <see cref="LineRenderer"/> (instant‑hit feel).</item>
///   <item>Deals damage to an <c>Enemy</c> if one is hit.</item>
///   <item>Returns itself to the object‑pool after a short lifetime / impact.</item>
/// </list>
/// This script purposefully avoids <c>Rigidbody</c> physics to keep bullets deterministic and
/// performant even at very high speeds.
/// </summary>
[RequireComponent(typeof(LineRenderer))]
public class Bullet : MonoBehaviour
{
    /// <summary>
    /// Bullet velocity in world‑space (units / second).
    /// Set by <c>PlayerWeapon</c> when the bullet is spawned.
    /// </summary>
    [HideInInspector] public Vector3 velocity;

    // Cached data
    private Vector3 startPosition;         // Where the bullet started this flight (for tracer)
    private LineRenderer line;             // Tracer / bullet trail
    private bool hitSomething = false;     // True once we register a hit (prevents double‑damage)
    private WeaponData sourceWeaponData;   // Stats of the weapon that fired this bullet
    private ItemManager itemManager;

    #region Unity life_cycle

    /// <summary>
    /// Unity callback when the object is enabled (spawned or reused from pool).
    /// Resets state, caches components, and starts a timed auto‑despawn coroutine.
    /// </summary>
    private void OnEnable()
    {
        hitSomething = false;
        startPosition = transform.position;

        // Cache the LineRenderer the first time.
        if (line == null)
        {
            line = GetComponent<LineRenderer>();
        }

        line.positionCount = 2; // two‑point line (start ↔ end)
        line.enabled = false; // hide until we actually draw a tracer

        // Auto‑deactivate even if we hit nothing (e.g. missed shot)
        StartCoroutine(DestroyAfterLifetime());
    }

    /// <summary>
    /// Per‑frame logic: move the bullet forward and detect collisions via ray‑cast.
    /// We ray‑cast the distance we plan to travel this frame to ensure we do not skip
    /// thin colliders ("tunnelling").
    /// </summary>
    private void Update()
    {
        // Exit early if we already hit something this frame / bullet is stopped.
        if (hitSomething) return;

        // Predict next position assuming no obstacles.
        Vector3 nextPosition = transform.position + velocity * Time.deltaTime;

        // Ray‑cast ahead to detect hit along our path this frame.
        if (Physics.Raycast(transform.position,
                            velocity.normalized,
                            out RaycastHit hit,
                            velocity.magnitude * Time.deltaTime))
        {
            // Move bullet to exact impact point.
            transform.position = hit.point;

            // Draw tracer from spawn to impact.
            DrawLine(startPosition, hit.point);

            // Deal damage if we struck an Enemy.
            if (hit.collider.TryGetComponent<Enemy>(out Enemy enemy))
            {
                if (PlayerController.Instance.TryGetComponent<FinalDestination>(out var finalDestination) &&
                        finalDestination.getItemAcquired() &&
                        Random.value <= finalDestination.getCritChance())
                {
                    Debug.Log("----------- Crit! -----------");
                    enemy.TakeDamage(2 * sourceWeaponData.damage);
                    finalDestination.UIFeedback(sourceWeaponData.fireRate);
                    PlayerController.Instance.playerAudio.PlayCritSound();
                }
                else
                {
                    enemy.TakeDamage(sourceWeaponData.damage);
                }
            }
            else if (hit.collider.TryGetComponent<Barrel>(out Barrel barrel))
            {
                if (PlayerController.Instance.TryGetComponent<FinalDestination>(out var finalDestination) &&
                        finalDestination.getItemAcquired() &&
                        Random.value <= finalDestination.getCritChance())
                {
                    Debug.Log("----------- Crit! -----------");
                    barrel.TakeDamage(2 * sourceWeaponData.damage);
                    finalDestination.UIFeedback(sourceWeaponData.fireRate);
                    PlayerController.Instance.playerAudio.PlayCritSound();
                }
                else
                {
                    barrel.TakeDamage(sourceWeaponData.damage);
                }
            }

            hitSomething = true;

            // Quickly hide bullet (pool) after a tiny delay to let tracer show.
            StartCoroutine(DeactivateAfterDelay(0.05f));
        }
        else // No hit this frame – just move forward.
        {
            transform.position = nextPosition;
        }
    }

    #endregion

    #region Helper methods

    /// <summary>
    /// Renders/updates the tracer line from <paramref name="start"/> to <paramref name="end"/>.
    /// </summary>
    private void DrawLine(Vector3 start, Vector3 end)
    {
        if (line == null) line = GetComponent<LineRenderer>();

        line.SetPosition(0, start);
        line.SetPosition(1, end);
        line.enabled = true;
    }

    /// <summary>
    /// Waits for the bullet lifetime (no hit), draws full tracer, then deactivates the object.
    /// Using pooling instead of <c>Destroy</c> avoids GC spikes.
    /// </summary>
    private IEnumerator DestroyAfterLifetime()
    {
        yield return new WaitForSeconds(0.1f); // flight time before despawn

        if (!hitSomething)
        {
            DrawLine(startPosition, transform.position); // missed shot tracer
        }

        yield return new WaitForSeconds(0.05f); // let tracer be visible briefly
        gameObject.SetActive(false); // return to pool
    }

    /// <summary>
    /// Deactivates the bullet after <paramref name="delay"/> seconds – used when we hit something.
    /// </summary>
    private IEnumerator DeactivateAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        gameObject.SetActive(false);
    }

    /// <summary>
    /// Injects the firing weapon's stats so the bullet knows how much damage to deal, etc.
    /// Called immediately after the bullet is fetched from the pool.
    /// </summary>
    public void SetWeaponData(WeaponData data)
    {
        sourceWeaponData = data;
    }

    public void SetItemManager(ItemManager manager)
    {
        itemManager = manager;
    }
    #endregion
}
