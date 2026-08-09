using TMPro;
using UnityEngine;

public class InfoPanelUI : MonoBehaviour
{
    public TMP_Text descriptionText;

    public void Show(LevelData level)
    {
        descriptionText.text = level.description;
    }
}