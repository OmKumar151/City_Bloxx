
using UnityEngine;

public class SwingPivotController : MonoBehaviour
{
    [Header("Pendulum Settings")]
    public float maxAngle = 35f;
    public float swingSpeed = 2f;

    private bool isSwinging = false;
    private float startAngle;

    private void Awake()
    {
        startAngle = transform.localEulerAngles.z;

        if (startAngle > 180f)
        {
            startAngle -= 360f;
        }
    }

    private void Update()
    {
        if (!isSwinging)
            return;

        // Smoothly swing around the pivot.
        float angle = startAngle +
                      Mathf.Sin(Time.time * swingSpeed) * maxAngle;

        transform.localRotation = Quaternion.Euler(0f, 0f, angle);
    }

    public void StartSwing()
    {
        isSwinging = true;
    }

    public void StopSwing()
    {
        isSwinging = false;
    }
}
