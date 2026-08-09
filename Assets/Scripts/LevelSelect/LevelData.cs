using UnityEngine;

[System.Serializable]
public class LevelData
{
    [Header("Information")]
    public string levelName;
    [TextArea]
    public string description;

    [Header("Gameplay")]
    public int targetPopulation;
    public bool unlocked;

    [Header("Visuals")]
    public Sprite buildingIcon;
    public Sprite highlightIcon;

    [Header("Scene")]
    public string gameplayScene;
}