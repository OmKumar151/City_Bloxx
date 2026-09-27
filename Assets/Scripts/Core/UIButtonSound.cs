using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class UIButtonSound : MonoBehaviour
{
    [Header("Sound")]
    [SerializeField] private bool playSound = true;

    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
    }

    private void OnEnable()
    {
        if (button != null)
        {
            button.onClick.AddListener(PlayClickSound);
        }
    }

    private void OnDisable()
    {
        if (button != null)
        {
            button.onClick.RemoveListener(PlayClickSound);
        }
    }

    private void PlayClickSound()
    {
        if (!playSound)
            return;

        if (AudioManager.Instance == null)
        {
            Debug.LogWarning(
                "UIButtonSound: AudioManager is not available."
            );

            return;
        }

        AudioManager.Instance.PlayButtonClick();
    }
}