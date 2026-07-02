using UnityEngine;

public class SwingPivotController : MonoBehaviour
{
    public float maxAngle = 35f;
    public float speed = 2f;

    private bool swinging = false;

    private void Update()
    {
        if (!swinging)
            return;

        float angle = Mathf.Sin(Time.time * speed) * maxAngle;

        transform.localRotation = Quaternion.Euler(0, 0, angle);
    }

    public void StartSwing()
    {
        swinging = true;
    }

    public void StopSwing()
    {
        swinging = false;
        transform.localRotation = Quaternion.identity;
    }
}