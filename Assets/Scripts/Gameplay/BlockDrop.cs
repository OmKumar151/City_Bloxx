using UnityEngine;

/// <summary>
/// No longer used: CraneInput (in the Gameplay scene) drops the block for
/// both touch and mouse. Kept only because the block prefabs still have
/// this component; it can be removed from CurrentBlock and RoofBlock.
///
/// It used the old Input Manager (Input.GetMouseButtonDown), which throws
/// every frame because the project uses the new Input System only.
/// </summary>
public class BlockDrop : MonoBehaviour
{
}
