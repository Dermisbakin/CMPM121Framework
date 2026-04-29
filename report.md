# CMPM 121 Assignment 2 Report

## Architecture Diagram

```mermaid
classDiagram
    GameManager --> EnemySpawner : tracks run state and enemies
    EnemySpawner --> LevelSelector : reads selected level
    LevelSelector --> Levels : stores levels.json entries
    Levels --> Spawn : contains spawn rules
    EnemySpawner --> Enemy : reads enemies.json entries
    EnemySpawner --> SpawnPoint : chooses spawn location
    EnemySpawner --> EnemyController : assigns hp speed damage
    EnemyController --> Hittable : takes damage and dies
    PlayerController --> GameManager : reports player death
    RewardScreenManager --> GameManager : shows continue or return button
    WaveLabelController --> GameManager : shows wave and result text
```

## Architecture Description

The level and enemy data are loaded from JSON into small data classes. `LevelSelector` stores the available level definitions, while `EnemySpawner` reads enemy definitions and uses the selected level's spawn rules to create enemies each wave. Spawn rules can choose enemy type, count, HP, damage, delay, sequence, and spawn location.

`GameManager` stores the current game state, wave number, enemy count, and simple run stats. `EnemyController` handles enemy movement and attacks, while `PlayerController` starts the player stats and reports game over when the player dies. `RewardScreenManager` reuses the existing reward screen button for either continuing to the next wave or returning to the start after victory or defeat.

## Added Classes And Methods

- Added fields to `Enemy` for JSON enemy stats.
- Added `speed` and `damage` fields to `Spawn`.
- Added `LevelSelector.GetLevel`.
- Added wave/end-state fields and helper methods in `GameManager`.
- Added data-driven spawn helpers in `EnemySpawner`.
- Added enemy `damage` support and movement stopping in `EnemyController`.
- Updated `RewardScreenManager` and `WaveLabelController` to display wave, victory, and defeat states.

## Contributions

Todd Crandell fixed bugs and enemy movement behavior, including stopping enemies correctly while attacking and using configured enemy damage.

Branson Guan worked on the initial assignment implementation, including the level selection flow, level JSON structure, and wave spawning setup.

Saurav Shah worked on the initial assignment implementation, including enemy JSON data, enemy type setup, and spawn rule integration.
