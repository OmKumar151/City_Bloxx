
using UnityEngine;
using System.Collections;

public class CameraFollow : MonoBehaviour
{
    [Header("Target")]
    [Tooltip("Assign the Crane Pulley transform here.")]
    public Transform target;

    [Header("Camera Settings")]
    [Min(0f)] public float smoothSpeed = 4f;
    public float offsetY = 2.5f;
    public bool onlyMoveUp = true;

    [Header("Opening Camera Pan")]
    [Tooltip("The position where the camera starts the opening animation.")]
    public Transform openingStartPoint;

    [Min(0.1f)]
    public float openingPanDuration = 2f;

    public AnimationCurve openingPanCurve =
        AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    private float highestCameraY;
    private bool isOpeningPanPlaying;

    private void Awake()
    {
        highestCameraY = transform.position.y;
    }

    private void Start()
    {
        if (openingStartPoint == null)
        {
            Debug.LogWarning(
                "CameraFollow: Opening Start Point is not assigned. " +
                "The camera will use its current position.",
                this
            );

            return;
        }

        if (target == null)
        {
            Debug.LogError(
                "CameraFollow: Assign the Crane Pulley to Target.",
                this
            );

            return;
        }

        // Start at the assigned intro position.
        transform.position = openingStartPoint.position;

        // Stop normal following while the intro pan is playing.
        isOpeningPanPlaying = true;

        StartCoroutine(PlayOpeningPan());
    }

    private IEnumerator PlayOpeningPan()
    {
        Vector3 startPosition = transform.position;

        // The original destination is the target (crane pulley),
        // adjusted by the existing vertical camera offset.
        Vector3 endPosition = GetTargetCameraPosition();

        float elapsed = 0f;

        while (elapsed < openingPanDuration)
        {
            elapsed += Time.deltaTime;

            float progress = Mathf.Clamp01(
                elapsed / openingPanDuration
            );

            float curvedProgress =
                openingPanCurve.Evaluate(progress);

            transform.position = Vector3.Lerp(
                startPosition,
                endPosition,
                curvedProgress
            );

            yield return null;
        }

        // Finish exactly at the destination calculated for the intro.
        transform.position = endPosition;

        highestCameraY = transform.position.y;
        isOpeningPanPlaying = false;
    }

    private Vector3 GetTargetCameraPosition()
    {
        return new Vector3(
            target.position.x,
            target.position.y + offsetY,
            transform.position.z
        );
    }

    private void LateUpdate()
    {
        if (target == null || isOpeningPanPlaying)
            return;

        float desiredY = target.position.y + offsetY;

        if (onlyMoveUp)
            desiredY = Mathf.Max(desiredY, highestCameraY);

        float blend = 1f -
            Mathf.Exp(-smoothSpeed * Time.deltaTime);

        float newY = Mathf.Lerp(
            transform.position.y,
            desiredY,
            blend
        );

        if (onlyMoveUp)
            newY = Mathf.Max(newY, transform.position.y);

        transform.position = new Vector3(
            transform.position.x,
            newY,
            transform.position.z
        );

        highestCameraY = Mathf.Max(
            highestCameraY,
            newY
        );
    }

    public void SetTarget(Transform newTarget)
    {
        if (newTarget != null)
            target = newTarget;
    }

    public void ResetHighestCameraY()
    {
        highestCameraY = transform.position.y;
    }
}
