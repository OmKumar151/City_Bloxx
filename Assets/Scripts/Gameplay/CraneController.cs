using UnityEngine;

public class CraneController : MonoBehaviour
{
    public static CraneController Instance;

    [Header("References")]
    public Transform hook;
    public GameObject rope;
    public GameObject hangingBlockPrefab;
    public SwingPivotController swingPivot;

    private GameObject currentBlock;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        SpawnBlock();
    }

    public void SpawnBlock()
    {
        if (currentBlock != null)
            return;

        // Reset crane rotation
        if (hook != null)
            hook.localRotation = Quaternion.identity;

        if (swingPivot != null)
        {
            swingPivot.transform.localRotation = Quaternion.identity;
            swingPivot.StartSwing();
        }

        if (rope != null)
            rope.SetActive(true);

        currentBlock = Instantiate(
            hangingBlockPrefab,
            hook.position,
            Quaternion.identity
        );

        currentBlock.transform.SetParent(hook);
        currentBlock.transform.localPosition = Vector3.zero;
        currentBlock.transform.localRotation = Quaternion.identity;

        Rigidbody2D rb = currentBlock.GetComponent<Rigidbody2D>();

        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.gravityScale = 0;
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0;
            rb.freezeRotation = true;
        }
    }

    public void ReleaseBlock()
    {
        if (currentBlock == null)
            return;

        if (swingPivot != null)
            swingPivot.StopSwing();

        currentBlock.transform.SetParent(null);

        Rigidbody2D rb = currentBlock.GetComponent<Rigidbody2D>();

        if (rb != null)
        {
            rb.gravityScale = 1;
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0;
            rb.freezeRotation = true;
        }

        if (rope != null)
            rope.SetActive(false);

        currentBlock = null;
    }

    public void PrepareNextBlock()
    {
        Invoke(nameof(SpawnBlock), 0.3f);
    }
}