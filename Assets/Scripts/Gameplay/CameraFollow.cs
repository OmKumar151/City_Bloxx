using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Target")]
    public Transform target;

    [Header("Camera Settings")]
    public float smoothSpeed = 4f;
    public float offsetY = 2.5f;

    [Header("Movement")]
    public bool onlyMoveUp = true;

    private float highestCameraY;

    private void Start()
    {
        highestCameraY = transform.position.y;
    }

    private void LateUpdate()
    {
        if (target == null)
            return;

        float wantedY = target.position.y + offsetY;

        // Camera should NEVER move downward.
        if (onlyMoveUp)
        {
            wantedY = Mathf.Max(wantedY, highestCameraY);
        }

        float newY = Mathf.Lerp(
            transform.position.y,
            wantedY,
            smoothSpeed * Time.deltaTime
        );

        // Safety: never move downward.
        if (onlyMoveUp)
        {
            newY = Mathf.Max(newY, transform.position.y);
        }

        transform.position = new Vector3(
            transform.position.x,
            newY,
            transform.position.z
        );

        highestCameraY = Mathf.Max(highestCameraY, newY);
    }

    public void SetTarget(Transform newTarget)
    {
        if (newTarget == null)
            return;

        target = newTarget;
    }
}