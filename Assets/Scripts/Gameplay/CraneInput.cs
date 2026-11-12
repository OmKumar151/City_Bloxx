
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Tap (phone) or click (PC) anywhere to drop the hanging block.
/// </summary>
public class CraneInput : MonoBehaviour
{
    private void Update()
    {
        if (!WasPressedThisFrame())
            return;

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

    // Phones have no mouse (Mouse.current is null there), so check the
    // touchscreen as well as the mouse.
    private static bool WasPressedThisFrame()
    {
        Touchscreen touch = Touchscreen.current;

        if (touch != null && touch.primaryTouch.press.wasPressedThisFrame)
            return true;

        Mouse mouse = Mouse.current;

        return mouse != null && mouse.leftButton.wasPressedThisFrame;
    }
}
