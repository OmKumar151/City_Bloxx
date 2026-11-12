
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Tap (phone) or click (PC) anywhere to drop the hanging block.
/// </summary>
public class CraneInput : MonoBehaviour
{
    private void Update()
    {
        if (!WasPressedThisFrame(out Vector2 pressPosition))
            return;

        // Taps on the guide (or its "i" button) are not drops.
        TutorialManager tutorials = TutorialManager.Instance;

        if (tutorials != null && tutorials.BlocksGameplayInput(pressPosition))
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
    private static bool WasPressedThisFrame(out Vector2 position)
    {
        position = Vector2.zero;

        Touchscreen touch = Touchscreen.current;

        if (touch != null && touch.primaryTouch.press.wasPressedThisFrame)
        {
            position = touch.primaryTouch.position.ReadValue();
            return true;
        }

        Mouse mouse = Mouse.current;

        if (mouse != null && mouse.leftButton.wasPressedThisFrame)
        {
            position = mouse.position.ReadValue();
            return true;
        }

        return false;
    }
}
