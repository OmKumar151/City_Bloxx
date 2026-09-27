using UnityEngine;

public class HookController : MonoBehaviour
{
    [Header("Hook Settings")]
    public float blockOffset = 0.65f;

    // Position where the block should hang
    public Vector3 GetBlockPosition()
    {
        return transform.position + Vector3.down * blockOffset;
    }
}