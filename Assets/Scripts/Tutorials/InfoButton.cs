using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Put this on any Button to make it open the game guide ("i"). A scene
/// with one of these hides the floating "i" button TutorialManager adds.
/// </summary>
[RequireComponent(typeof(Button))]
public class InfoButton : MonoBehaviour
{
    private void Awake()
    {
        GetComponent<Button>().onClick.AddListener(OpenGuide);
    }

    public void OpenGuide()
    {
        if (TutorialManager.Instance != null)
            TutorialManager.Instance.ShowGuide();
    }
}
