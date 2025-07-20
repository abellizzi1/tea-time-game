using UnityEngine;

public class EnemyHitbox : MonoBehaviour
{
    public Enemy parent;

    // Apply damage to player with a hit
    private void OnCollisionEnter(Collision collision)
   {
        if (collision.gameObject.CompareTag("Player") && parent.getAttackState())
        {
            parent.DoDamage(collision.gameObject.GetComponent<PlayerController>());

            // If damage is done mark out of attacking state to avoid multiple hit detections
            parent.DeactivateHitbox();
        }
   }
}
