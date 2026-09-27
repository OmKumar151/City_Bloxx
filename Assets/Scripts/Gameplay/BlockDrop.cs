using UnityEngine;

public class BlockDrop : MonoBehaviour
{
    private bool dropped = false;

    void Update()
    {
        if (!dropped && Input.GetMouseButtonDown(0))
        {
            dropped = true;

            BlockSwing swing = GetComponent<BlockSwing>();
            if (swing != null)
            {
                swing.enabled = false;
            }

            // IMPORTANT: release through the crane (not just locally), so
            // CraneController.currentBlock gets cleared. Without this the
            // crane thinks a block is still active and refuses to spawn
            // the next one — that's what was causing the game to get stuck.
            if (CraneController.Instance != null)
            {
                CraneController.Instance.ReleaseBlock();
            }
        }
    }
}