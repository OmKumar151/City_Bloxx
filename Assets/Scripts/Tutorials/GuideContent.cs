using System.Collections.Generic;
using System.Text;
using UnityEngine;

/// <summary>
/// Every tutorial, the "i" game guide and the wording the Info Panel uses,
/// in one place. Numbers (floors, unlock populations, roof scores) are read
/// from the live game where possible so the text never goes out of date.
/// </summary>
public static class GuideContent
{
    // Tutorial ids. Each one is shown once, the first time it triggers.
    public const string Welcome = "WELCOME";
    public const string CityIntro = "BUILDING_SELECTION";
    public const string PlaceBuilding = "PLACE_BUILDING";
    public const string BuildTower = "GAMEPLAY_BUILD";
    public const string QuickGame = "QUICK_PLAY";
    public const string NewBuilding = "NEW_BUILDING";
    public const string Stars = "STARS";

    /// <summary>The "i" guide. Never marked completed.</summary>
    public const string Guide = "GUIDE";

    public static readonly string[] AllTutorialIds =
    {
        Welcome, CityIntro, PlaceBuilding, BuildTower, QuickGame, NewBuilding, Stars
    };

    // =========================================================
    // BUILDING FACTS
    // =========================================================

    private static readonly string[] BuildingNames = { "Blue", "Red", "Green", "Yellow" };
    private static readonly string[] BuildingColours = { "#5AB0FF", "#FF6B6B", "#62DB74", "#FFD54A" };
    private static readonly int[] FallbackFloors = { 10, 20, 30, 40 };
    private static readonly int[] FallbackRoofScores = { 360, 750, 1140, 1560 };

    private const string Gold = "#FFCC4D";

    private static BuildingGameplayConfig[] configs;

    /// <summary>LevelSelect hands over its configs so floors/scores are exact.</summary>
    public static void RegisterBuildingConfigs(BuildingGameplayConfig[] buildingConfigs)
    {
        if (buildingConfigs != null && buildingConfigs.Length > 0)
            configs = buildingConfigs;
    }

    public static string BuildingName(int index)
    {
        return index >= 0 && index < BuildingNames.Length ? BuildingNames[index] : "Unknown";
    }

    /// <summary>Building name in its own colour, e.g. "Red" in red.</summary>
    public static string Coloured(int index)
    {
        if (index < 0 || index >= BuildingNames.Length)
            return "Unknown";

        return "<color=" + BuildingColours[index] + ">" + BuildingNames[index] + "</color>";
    }

    public static int Floors(int index)
    {
        BuildingGameplayConfig config = GetConfig(index);

        if (config != null)
            return config.floorsToComplete;

        return index >= 0 && index < FallbackFloors.Length ? FallbackFloors[index] : 0;
    }

    public static int SpecialRoofScore(int index)
    {
        BuildingGameplayConfig config = GetConfig(index);

        if (config != null)
            return config.specialRoofScoreThreshold;

        return index >= 0 && index < FallbackRoofScores.Length ? FallbackRoofScores[index] : 0;
    }

    public static int IndexOf(GridCell.BuildingType type)
    {
        switch (type)
        {
            case GridCell.BuildingType.Blue: return 0;
            case GridCell.BuildingType.Red: return 1;
            case GridCell.BuildingType.Green: return 2;
            case GridCell.BuildingType.Yellow: return 3;
            default: return -1;
        }
    }

    public static int IndexOfBuildingId(string buildingId)
    {
        for (int i = 0; i < BuildingNames.Length; i++)
        {
            if (string.Equals(buildingId, BuildingNames[i], System.StringComparison.OrdinalIgnoreCase))
                return i;
        }

        return -1;
    }

    /// <summary>Where a building may go, e.g. "must touch a Blue and a Red".</summary>
    public static string PlacementRule(int index)
    {
        switch (index)
        {
            case 0: return "can go on any square";
            case 1: return "must touch a " + Coloured(0);
            case 2: return "must touch a " + Coloured(0) + " and a " + Coloured(1);
            case 3: return "must touch a " + Coloured(0) + ", " + Coloured(1) + " and " + Coloured(2);
            default: return string.Empty;
        }
    }

    /// <summary>"a Blue and a Red" from a list of building indexes.</summary>
    public static string JoinBuildings(IList<int> indexes)
    {
        if (indexes == null || indexes.Count == 0)
            return string.Empty;

        StringBuilder text = new StringBuilder("a ");

        for (int i = 0; i < indexes.Count; i++)
        {
            if (i > 0)
                text.Append(i == indexes.Count - 1 ? " and a " : ", a ");

            text.Append(Coloured(indexes[i]));
        }

        return text.ToString();
    }

    private static BuildingGameplayConfig GetConfig(int index)
    {
        if (configs == null || index < 0 || index >= configs.Length)
            return null;

        return configs[index];
    }

    // =========================================================
    // REWARDS
    // =========================================================

    public static string RewardName(PopulationProgressionManager.RewardType reward)
    {
        switch (reward)
        {
            case PopulationProgressionManager.RewardType.BlueBuilding: return Coloured(0) + " Building";
            case PopulationProgressionManager.RewardType.RedBuilding: return Coloured(1) + " Building";
            case PopulationProgressionManager.RewardType.GreenBuilding: return Coloured(2) + " Building";
            case PopulationProgressionManager.RewardType.YellowBuilding: return Coloured(3) + " Building";
            case PopulationProgressionManager.RewardType.BlueStar: return Coloured(0) + " Star";
            case PopulationProgressionManager.RewardType.RedStar: return Coloured(1) + " Star";
            case PopulationProgressionManager.RewardType.GreenStar: return Coloured(2) + " Star";
            case PopulationProgressionManager.RewardType.YellowStar: return Coloured(3) + " Star";
            default: return reward.ToString();
        }
    }

    public static bool IsStar(PopulationProgressionManager.RewardType reward)
    {
        return reward == PopulationProgressionManager.RewardType.BlueStar ||
               reward == PopulationProgressionManager.RewardType.RedStar ||
               reward == PopulationProgressionManager.RewardType.GreenStar ||
               reward == PopulationProgressionManager.RewardType.YellowStar;
    }

    /// <summary>Building index a reward belongs to (its colour).</summary>
    public static int RewardBuildingIndex(PopulationProgressionManager.RewardType reward)
    {
        switch (reward)
        {
            case PopulationProgressionManager.RewardType.BlueBuilding:
            case PopulationProgressionManager.RewardType.BlueStar: return 0;
            case PopulationProgressionManager.RewardType.RedBuilding:
            case PopulationProgressionManager.RewardType.RedStar: return 1;
            case PopulationProgressionManager.RewardType.GreenBuilding:
            case PopulationProgressionManager.RewardType.GreenStar: return 2;
            case PopulationProgressionManager.RewardType.YellowBuilding:
            case PopulationProgressionManager.RewardType.YellowStar: return 3;
            default: return -1;
        }
    }

    /// <summary>The next locked reward, in unlock order. False when all are unlocked.</summary>
    public static bool TryGetNextReward(
        out PopulationProgressionManager.RewardType reward,
        out int population)
    {
        reward = PopulationProgressionManager.RewardType.BlueBuilding;
        population = 0;

        PopulationProgressionManager progression = PopulationProgressionManager.Instance;

        if (progression == null)
            return false;

        foreach (PopulationProgressionManager.RewardType candidate in
                 System.Enum.GetValues(typeof(PopulationProgressionManager.RewardType)))
        {
            if (progression.IsRewardUnlocked(candidate))
                continue;

            reward = candidate;
            population = progression.GetCheckpointPopulation(candidate);
            return true;
        }

        return false;
    }

    public static int CurrentPopulation()
    {
        return PopulationProgressionManager.Instance != null
            ? PopulationProgressionManager.Instance.GetCurrentPopulation()
            : 0;
    }

    // =========================================================
    // TUTORIALS AND GUIDE
    // =========================================================

    /// <summary>
    /// Builds a tutorial (or the guide) with up-to-date numbers.
    /// buildingIndex is the building it is about, where that matters.
    /// </summary>
    public static TutorialManager.TutorialDefinition Build(string id, int buildingIndex = -1)
    {
        switch (id)
        {
            case Welcome: return BuildWelcome();
            case CityIntro: return BuildCityIntro();
            case PlaceBuilding: return BuildPlaceBuilding(buildingIndex);
            case BuildTower: return BuildBuildTower(buildingIndex);
            case QuickGame: return BuildQuickGame();
            case NewBuilding: return BuildNewBuilding(buildingIndex);
            case Stars: return BuildStars(buildingIndex);
            case Guide: return BuildGuide();
            default: return null;
        }
    }

    private static TutorialManager.TutorialDefinition Make(
        string id, string name, params TutorialManager.TutorialStep[] steps)
    {
        return new TutorialManager.TutorialDefinition
        {
            tutorialId = id,
            displayName = name,
            steps = new List<TutorialManager.TutorialStep>(steps)
        };
    }

    private static TutorialManager.TutorialStep Step(string title, string message, string target = null)
    {
        return new TutorialManager.TutorialStep
        {
            title = title,
            message = message,
            targetId = target,
            showHighlight = !string.IsNullOrEmpty(target)
        };
    }

    private static string Em(string text)
    {
        return "<color=" + Gold + "><b>" + text + "</b></color>";
    }

    // ---------------------------------------------------------

    private static TutorialManager.TutorialDefinition BuildWelcome()
    {
        return Make(Welcome, "Welcome",
            Step("WELCOME TO CITY BLOXX!",
                "Your goal: " + Em("build the biggest city you can") + ".\n\n" +
                "Every building in your city is a tower you stack yourself, one floor at a time. " +
                "The better you stack, the more people move in."),
            Step("TWO WAYS TO PLAY",
                Em("BUILD CITY") + " is the main game. Build towers, place them in your city and grow " +
                "its population to unlock new buildings, stars and new cities.\n\n" +
                Em("QUICK GAME") + " is one endless tower. Stack as high as you can and beat your best score."),
            Step("HOW TO STACK",
                "A floor swings from the crane. " + Em("Tap anywhere") + " to drop it.\n\n" +
                "Land it right on top of the floor below. A drop close to the centre is a " + Em("PERFECT") +
                ": it's worth more, and perfects in a row build a combo multiplier."),
            Step("NEED HELP?",
                "Tap the " + Em("i") + " button at any time to open the game guide: goals, buildings, " +
                "stars and what every button does.\n\nHave fun building!"));
    }

    private static TutorialManager.TutorialDefinition BuildCityIntro()
    {
        return Make(CityIntro, "Your City",
            Step("THIS IS YOUR CITY",
                "Each square on this grid holds one building. Fill it with buildings to grow your " +
                Em("population") + ": the number of people living in your city.",
                "Grid"),
            Step("POPULATION",
                "The top bar shows your population and how close you are to the next unlock.\n\n" +
                "Population unlocks new buildings, stars and, later, a whole new city.",
                "TopBar"),
            Step("CHOOSE A BUILDING",
                "Use the " + Em("arrow buttons") + " to pick a building, then press " + Em("OK") +
                " to start building it.\n\nYou start with " + Coloured(0) + ". " + Coloured(1) + ", " +
                Coloured(2) + " and " + Coloured(3) + " unlock as your city grows.",
                "BuildingSelection"),
            Step("THE FOUR BUILDINGS",
                BuildingTable() + "\n\"Touch\" includes corners. Taller buildings are harder to stack, but " +
                "a good score on them brings in many more people."),
            Step("BUILD IT, THEN PLACE IT",
                "Press " + Em("OK") + " to play the tower. Stack every floor and the building is yours: you " +
                "come back here to place it, and its " + Em("score becomes its residents") + ".\n\n" +
                "Lose all your lives and the tower is lost. Nothing is added, but you can try again.",
                "OkButton"),
            Step("RESET CITY",
                "This button clears the " + Em("whole city") + ": every building, all its population " +
                "and its unlocks. It can't be undone, so only use it to start this city over.",
                "ResetButton"));
    }

    private static TutorialManager.TutorialDefinition BuildPlaceBuilding(int index)
    {
        string building = index >= 0 ? Coloured(index) + " building" : "building";

        return Make(PlaceBuilding, "Place Your Building",
            Step("YOU BUILT IT!",
                "Your " + building + " is finished. Now choose where it goes in your city.\n\n" +
                "Use the " + Em("arrows") + " to move around the grid. Squares that light up are " +
                "places it's allowed.",
                "Grid"),
            Step("PLACE OR REPLACE",
                "Press " + Em("OK") + " to place it.\n\nYou can also place it on an existing " +
                "building to " + Em("replace") + " it. The old building and its residents are removed, " +
                "so only replace a building with a better one.",
                "OkButton"),
            Step("CANCEL BUILD",
                "This button " + Em("throws the new building away") + " and you lose its residents.\n\n" +
                "Only use it if you don't want this building in your city.",
                "DiscardButton"));
    }

    private static TutorialManager.TutorialDefinition BuildBuildTower(int index)
    {
        string building = index >= 0
            ? "your " + Coloured(index) + " building: " + Em(Floors(index) + " floors")
            : "your building";

        return Make(BuildTower, "Build The Tower",
            Step("BUILD THE TOWER",
                "Stack " + building + ".\n\nA floor swings from the crane. Wait for the right " +
                "moment and " + Em("tap anywhere") + " to drop it."),
            Step("AIM FOR PERFECT",
                "Land each floor right on top of the one below. Close to the centre counts as a " +
                Em("PERFECT") + " and scores more.\n\nPerfects in a row raise your combo (up to " +
                Em("x2") + "), but the next floor must land before the timer bar runs out."),
            Step("DON'T MISS",
                "A floor that misses the tower costs a " + Em("life") + ". Lose all your lives and " +
                "the building fails.\n\nOff-centre landings also make the tower sway, which makes " +
                "the next drop harder."),
            Step("FINISH IT",
                "Land the roof to complete the building. Your score becomes its " + Em("residents") +
                " when you place it in your city.\n\nA perfect roof gives bonus residents!"));
    }

    private static TutorialManager.TutorialDefinition BuildQuickGame()
    {
        return Make(QuickGame, "Quick Game",
            Step("QUICK GAME",
                "Pick a colour and stack forever: there's " + Em("no top floor") + ".\n\n" +
                Em("Tap anywhere") + " to drop each floor. The game ends when you run out of lives."),
            Step("BEAT YOUR BEST",
                Em("Perfect") + " landings score more and build a combo. The swing speeds up as the " +
                "tower grows.\n\nYour " + Em("best score") + " is saved. Try to beat it!"));
    }

    private static TutorialManager.TutorialDefinition BuildNewBuilding(int index)
    {
        string name = index >= 0 ? Coloured(index) : "new";

        string body = index >= 0
            ? "The " + name + " building is " + Em(Floors(index) + " floors") + " tall and " +
              PlacementRule(index) + " (corners count).\n\n"
            : string.Empty;

        return Make(NewBuilding, "New Building",
            Step("NEW BUILDING UNLOCKED!",
                body + "Taller buildings are harder to stack, but they can hold far more people. " +
                "Select it with the " + Em("arrows") + ".",
                "BuildingSelection"));
    }

    private static TutorialManager.TutorialDefinition BuildStars(int index)
    {
        string colour = index >= 0 ? Coloured(index) : "a colour's";

        string detail = index >= 0
            ? "Next time you build a " + Coloured(index) + " building, score " +
              Em(SpecialRoofScore(index).ToString("N0") + " or more") + " and it's finished with a " +
              Em("special roof") + "."
            : "Score high enough on that colour's building and it's finished with a " +
              Em("special roof") + ".";

        return Make(Stars, "Stars",
            Step("YOU EARNED A STAR!",
                "You earned the " + colour + " Star.\n\n" + detail + "\n\n" +
                "Each colour has its own star. Grow your population to collect all four."));
    }

    // ---------------------------------------------------------
    // The "i" guide
    // ---------------------------------------------------------

    /// <summary>Guide page each scene opens on (the rest are a swipe away).</summary>
    public const int GuidePageGoal = 0;
    public const int GuidePageStacking = 1;
    public const int GuidePageCity = 3;
    public const int GuidePageQuickGame = 8;

    private static TutorialManager.TutorialDefinition BuildGuide()
    {
        return Make(Guide, "Game Guide",
            Step("THE GOAL",
                Em("Build the biggest city you can.") + "\n\n" +
                "Stack towers floor by floor, place them in your city and grow its population. " +
                "Population unlocks new buildings, stars and new cities."),
            Step("STACKING A TOWER",
                Em("Tap anywhere") + " to drop the swinging floor.\n\n" +
                "Land it on the floor below. Near the centre is a " + Em("PERFECT") + ": more points, " +
                "and perfects in a row raise your combo up to " + Em("x2") + ". Keep the combo by " +
                "landing the next floor before the timer bar runs out."),
            Step("LIVES AND SWAY",
                "A floor that misses the tower costs a " + Em("life") + ". Lose them all and the tower " +
                "fails: nothing is added to your city.\n\n" +
                "Off-centre landings make the tower sway. Perfect landings calm it down."),
            Step("YOUR CITY",
                "Finish a tower and you place it on your city grid. The tower's " +
                Em("score becomes its residents") + ", and all residents together are your " +
                Em("population") + ".\n\nPlacing a building on another one " + Em("replaces") +
                " it, along with its residents."),
            Step("THE FOUR BUILDINGS",
                BuildingTable() + "\n\"Touch\" includes corners. If a building has nowhere to go, place " +
                "the buildings it needs first."),
            Step("UNLOCKS", UnlockLadder()),
            Step("STARS AND SPECIAL ROOFS",
                "Each colour has a " + Em("Star") + ", earned at a high population.\n\n" +
                "With a colour's star, score enough on that building and it gets a " +
                Em("special roof") + ":\n" + RoofTable()),
            Step("BUTTONS",
                Em("Arrows") + ": pick a building, or move around the grid while placing.\n" +
                Em("OK") + ": build the selected tower, or place your new building.\n" +
                Em("Cancel Build") + ": throw away the building you're placing, with its residents.\n" +
                Em("Reset City") + ": clear the whole city (buildings, population, unlocks).\n" +
                Em("Top arrows") + ": switch between unlocked cities."),
            Step("QUICK GAME",
                "One endless tower with " + Em("no top floor") + ". Stack until your lives run out " +
                "and beat your " + Em("best score") + ".\n\nQuick Game doesn't change your city: " +
                "it's pure stacking practice and high scores."));
    }

    private static string BuildingTable()
    {
        StringBuilder text = new StringBuilder();

        for (int i = 0; i < BuildingNames.Length; i++)
        {
            text.Append("- <b>").Append(Coloured(i)).Append("</b>: ")
                .Append(Floors(i)).Append(" floors, ").Append(PlacementRule(i)).Append('\n');
        }

        return text.ToString();
    }

    private static string RoofTable()
    {
        StringBuilder text = new StringBuilder();

        for (int i = 0; i < BuildingNames.Length; i++)
        {
            text.Append("- ").Append(Coloured(i)).Append(": score ")
                .Append(SpecialRoofScore(i).ToString("N0")).Append('+');

            if (i < BuildingNames.Length - 1)
                text.Append('\n');
        }

        return text.ToString();
    }

    private static string UnlockLadder()
    {
        PopulationProgressionManager progression = PopulationProgressionManager.Instance;

        if (progression == null)
        {
            return "Your city's population unlocks rewards in this order:\n\n" +
                   Coloured(1) + " Building, " + Coloured(2) + " Building, " + Coloured(0) + " Star, " +
                   Coloured(3) + " Building, then the " + Coloured(1) + ", " + Coloured(2) + " and " +
                   Coloured(3) + " Stars.\n\nOpen Build City to see how close you are.";
        }

        StringBuilder text = new StringBuilder();
        text.Append("Population ").Append(Em(CurrentPopulation().ToString("N0")))
            .Append(". Rewards in this city:\n");

        foreach (PopulationProgressionManager.RewardType reward in
                 System.Enum.GetValues(typeof(PopulationProgressionManager.RewardType)))
        {
            if (reward == PopulationProgressionManager.RewardType.BlueBuilding)
                continue;

            bool unlocked = progression.IsRewardUnlocked(reward);

            text.Append("- ").Append(RewardName(reward)).Append(": ")
                .Append(progression.GetCheckpointPopulation(reward).ToString("N0"))
                .Append(unlocked ? "  <color=#62DB74>(unlocked)</color>" : string.Empty)
                .Append('\n');
        }

        return text.ToString().TrimEnd('\n');
    }
}
