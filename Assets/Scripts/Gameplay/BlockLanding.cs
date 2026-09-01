using UnityEngine;

public class BlockLanding : MonoBehaviour
{
    private bool hasLanded = false;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Only react to building/foundation objects
        if (!collision.gameObject.CompareTag("Building"))
            return;

        // Prevent this block from being processed multiple times
        if (hasLanded)
            return;

        hasLanded = true;

        Debug.Log("BLOCK LANDED: " + gameObject.name);

        // Stop the block
        Rigidbody2D rb = GetComponent<Rigidbody2D>();

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;

            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.freezeRotation = true;
        }

        // Make sure the block stays in the building
        gameObject.tag = "Building";

        // Tell GameManager this is now the highest placed floor
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetLastPlacedBlock(gameObject);
            GameManager.Instance.AddFloor();
            GameManager.Instance.AddScore(50);

            Debug.Log(
                "Camera target changed to: " +
                gameObject.name
            );
        }

        // Ask crane for next block
        if (CraneController.Instance != null)
        {
            CraneController.Instance.PrepareNextBlock();
        }
    }
}