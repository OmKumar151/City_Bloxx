# City_Bloxx (working title)

A portrait mobile tower-stacking and city-building game made with Unity.

A crane swings each block overhead, and you tap to drop it. Stacking blocks squarely builds a tall, stable tower. Landing off-centre makes the tower sway, and missing costs a life. Finished buildings are placed on a city grid, where they add population and unlock new building types and cities.

---

## Features

- **Crane stacking:** a swinging crane with timing-based drops, perfect-landing streaks, auto-centring on near-perfect drops, and swing speed that rises with progress.
- **Tower sway:** off-centre landings make the whole tower lean and sway, and perfect landings steady it again.
- **City building:** place finished buildings (blue, red, green, yellow) on a grid map to grow each city's population and unlock rewards and new cities.
- **Quick Play:** an endless mode from the main menu that keeps your best score.
- **Tutorials and in-game guide:** one-time tutorials on first contact with each feature, plus an "i" guide you can open at any time.
- **Haptics:** crisp vibration feedback on Android and the iOS Taptic Engine, tuned in a single config asset.
- **Audio:** music for each scene, and a rising chime for each step of a perfect-landing streak.
- **Responsive layout:** safe-area aware UI and a camera that fits tall phones and wide tablets.
- **Monetisation and cloud saves (optional, behind defines):** in-app purchases to unlock cities early, AdMob ads, and Google Play Games cloud saves.

## Requirements

| | |
|---|---|
| Unity | **6000.4.10f1** (Unity 6) |
| Platforms | iOS 15+ (primary), Android 7.1+ (API 25, ARM64) |
| Orientation | Portrait only |
| Packages | Unity In-App Purchasing 5.5, Input System, TextMesh Pro |
| Assets | Audio and image files are stored with **Git LFS** |

## Getting started

```bash
git lfs install
git clone https://github.com/OmKumar151/City_Bloxx.git
```

1. Open the project folder in Unity Hub with Unity **6000.4.10f1**.
2. Open `Assets/Scenes/Main Menu.unity` and press **Play**.
3. To try tablet and phone layouts, use **Window > General > Device Simulator**.

## Scenes

The scenes, in build order:

| Scene | Purpose |
|---|---|
| `Main Menu` | Title screen, settings, Quick Play |
| `LevelSelect` | City grid: place buildings, track population, unlock cities |
| `Gameplay1` | Crane stacking gameplay (story and Quick Play) |

## Project structure

```
Assets/
├── Audio/              Music and sound effects
├── BuildingConfigs/    Per-building gameplay tuning (BuildingGameplayConfig assets)
├── Plugins/iOS/        Native Taptic Engine bridge
├── Prefabs/            Blocks and reusable objects
├── Resources/          HapticsConfig, BillingMode
├── Scenes/             Main Menu, LevelSelect, Gameplay1
├── Scripts/
│   ├── Core/           GameManager, AudioManager, SaveStore, Haptics, camera and safe-area fitting
│   ├── Gameplay/       Crane, blocks, landing, tower sway, Quick Play, gameplay UI
│   ├── LevelSelect/    City grid, buildings, population, navigation, map saving
│   ├── Services/       Purchases, ads, Play Games (all optional)
│   ├── Tutorials/      Tutorial flow and game guide
│   └── Editor/         Build-time helpers
└── Sprites/            Game art and UI
```

## Configuration

### Tuning

- **Building difficulty:** `Assets/BuildingConfigs/*.asset` (one per building colour).
- **Haptics:** `Assets/Resources/HapticsConfig.asset`.
- **Crane, camera and sway:** components on the `Crane`, `Main Camera` and tower objects in `Gameplay1`.

### Scripting define symbols

Optional services are compiled in only when their define is set in **Project Settings > Player > Other Settings > Scripting Define Symbols**. Without them, the game runs fully offline with no ads or purchases.

| Define | Enables | Needs |
|---|---|---|
| `CITYBLOXX_IAP` | Real store purchases on device. In the Editor, purchases are always simulated. | Unity IAP package; products set up in App Store Connect or the Play Console |
| `CITYBLOXX_ADMOB` | Banner and rewarded ads | Google Mobile Ads Unity plugin, and real ad unit IDs in `AdsService.cs` |
| `CITYBLOXX_GPGS` | Google Play Games sign-in and cloud save (Android only) | Google Play Games plugin |

> All saved progress must be written through `SaveStore`, never straight to `PlayerPrefs`, so that cloud save can capture it.

## Building

### iOS

1. **File > Build Profiles**, choose **iOS**, then **Build**.
2. Open the generated Xcode project, set your signing team, and archive.
3. Before submitting:
   - Set the bundle identifier in Player Settings.
   - Raise the build number for each upload. Version **1.0** is the launch version.

### Android

Android isn't being published yet. When it is, create an upload keystore, set it under **Player Settings > Publishing Settings**, and build an `.aab` for the Play Console.

## Release checklist

- [ ] Final app name and bundle identifier
- [ ] Real AdMob app ID and ad unit IDs (replace Google's test IDs in `AdsService.cs`)
- [ ] IAP products created in App Store Connect with matching product IDs
- [ ] App icons and launch screen
- [ ] Privacy details: App Tracking Transparency prompt and App Store privacy labels (needed once ads are added)

## License

All rights reserved. This is a private project, and no licence is granted to use, copy or distribute the code or assets.
