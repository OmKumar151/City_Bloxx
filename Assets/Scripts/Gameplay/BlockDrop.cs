using UnityEngine;

public class BlockDrop : MonoBehaviour
{
    private bool dropped = false;

    private void Update()
    {
        if (dropped)
            return;

        if (Input.GetMouseButtonDown(0))
        {
            dropped = true;

            // Release the block through the crane controller.
            // BlockSwing is no longer needed because the crane
            // now controls the hanging block's movement.
            if (CraneController.Instance != null)
            {
                CraneController.Instance.ReleaseBlock();
            }
            else
            {
                Debug.LogWarning(
                    "BlockDrop: CraneController instance was not found."
                );
            }
        }
    }
}