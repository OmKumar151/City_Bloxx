
public static class GameplaySelection
{
    public static BuildingGameplayConfig SelectedBuilding { get; private set; }

    public static bool HasSelection => SelectedBuilding != null;

    public static void Select(BuildingGameplayConfig configuration)
    {
        SelectedBuilding = configuration;
    }

    public static void ClearSelection()
    {
        SelectedBuilding = null;
    }
}
