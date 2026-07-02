using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;

    public float smoothSpeed = 2f;

    public float offsetY = 3f;

    private void LateUpdate()
    {
        if (target == null)
            return;

        Vector3 desiredPosition =
            new Vector3(
                transform.position.x,
                target.position.y + offsetY,
                transform.position.z
            );

        transform.position =
            Vector3.Lerp(
                transform.position,
                desiredPosition,
                smoothSpeed * Time.deltaTime
            );
    }
}