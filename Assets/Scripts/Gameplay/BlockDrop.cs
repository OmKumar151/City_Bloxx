using UnityEngine;

public class BlockDrop : MonoBehaviour
{
    private Rigidbody2D rb;
    private bool dropped = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        // Every new block starts hanging
        rb.gravityScale = 0;
    }

    void Update()
    {
        if (!dropped && Input.GetMouseButtonDown(0))
        {
            dropped = true;

            rb.gravityScale = 1;

            GetComponent<BlockSwing>().enabled = false;
        }
    }
}