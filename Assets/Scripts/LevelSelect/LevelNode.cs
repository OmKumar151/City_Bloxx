using UnityEngine;
using UnityEngine.UI;

public class LevelNode : MonoBehaviour
{
    [Header("Level Data")]
    public LevelData levelData;

    [Header("UI")]
    public Image icon;
    public Image highlight;
    public GameObject lockIcon;

    public void Refresh()
    {
        if (icon != null)
            icon.sprite = levelData.buildingIcon;

        if (lockIcon != null)
            lockIcon.SetActive(!levelData.unlocked);
    }

    public void Select(bool selected)
    {
        if (highlight != null)
            highlight.enabled = selected;
    }
}