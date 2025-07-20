using UnityEngine;

public class TeaBagDrop : MonoBehaviour
{
    public Vector3 startOffset = new Vector3(0f, 5f, 0f); // how far above to start
    public float dropSpeed = 1f;
    public float settleThreshold = 0.01f;

    private Vector3 targetPosition;
    private bool dropping = true;
    private Vector3 velocity = Vector3.zero;

    void Start()
    {
        // Save where we want to end up
        targetPosition = transform.position;

        // Start above the target position
        transform.position = targetPosition + startOffset;
    }

    void Update()
    {
        if (dropping)
        {
            // Smoothly move toward the target position
            transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, 0.5f);

            // Stop once we're close enough
            if (Vector3.Distance(transform.position, targetPosition) < settleThreshold)
            {
                transform.position = targetPosition;
                dropping = false;
            }
        }
    }
}
