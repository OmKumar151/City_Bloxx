using UnityEngine;

public class BlockSwing : MonoBehaviour
{
    public float moveDistance = 3f;
    public float moveSpeed = 2f;

    private Vector3 startPos;

    private void Start()
    {
        startPos = transform.position;
    }

    private void Update()
    {
        float x =
            Mathf.Sin(Time.time * moveSpeed)
            * moveDistance;

        transform.position =
            new Vector3(
                startPos.x + x,
                startPos.y,
                startPos.z
            );
    }
}