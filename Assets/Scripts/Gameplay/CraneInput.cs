
using UnityEngine;
using UnityEngine.InputSystem;

public class CraneInput : MonoBehaviour
{
    private void Update()
    {
        if (Mouse.current == null ||
            !Mouse.current.leftButton.wasPressedThisFrame)
        {
            return;
        }

        CraneController crane = CraneController.Instance;

        if (crane == null)
        {
            Debug.LogWarning(
                "CraneInput: CraneController instance was not found.",
                this
            );
            return;
        }

        crane.ReleaseBlock();
    }
}
