
using UnityEngine;

public class HookController : MonoBehaviour
{
    [Header("Hook Settings")]
    [Min(0f)] public float blockOffset = 0.65f;

    public Vector3 GetBlockPosition()
    {
        return transform.position + Vector3.down * blockOffset;
    }
}
