using UnityEngine;
using UnityEngine.InputSystem;

public class CraneInput : MonoBehaviour
{
    void Update()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            CraneController.Instance.ReleaseBlock();
        }
    }
}