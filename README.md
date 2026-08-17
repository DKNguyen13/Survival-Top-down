# Survival Top-down

A Unity top-down survival game prototype where the player fights against waves of melee and ranged enemies using shooting, bombs, and dash abilities.

## Requirements

* Unity `2022.3.62f3` LTS
* Cinemachine `2.10.7`
* TextMesh Pro `3.0.7`
* DOTween

Main scene:

```text
Assets/Scenes/Gameplay.unity
```

The `Gameplay` scene is already included in Build Settings.

---

## Getting Started

1. Clone the repository.

```bash
git clone https://github.com/DKNguyen13/Survival-Top-down.git
```

2. Open **Unity Hub**.
3. Select **Add project from disk**.
4. Select the project root folder.
5. Open the project with Unity `2022.3.62f3` or another compatible Unity 2022.3 LTS version.
6. Open:

```text
Assets/Scenes/Gameplay.unity
```

7. Press **Play**.

If TextMesh Pro resources are missing:

```text
Window > TextMeshPro > Import TMP Essential Resources
```

---

## Controls

### Mobile / UI

| Control       | Action                                |
| ------------- | ------------------------------------- |
| Left Joystick | Move and rotate                       |
| Shoot         | Fire three projectiles                |
| Bomb          | Place a bomb                          |
| Dash          | Dash forward and trigger an explosion |

### Keyboard

| Input               | Action |
| ------------------- | ------ |
| `WASD` / Arrow Keys | Move   |
| `Space`             | Shoot  |
| `Q`                 | Bomb   |
| `E`                 | Dash   |

Shooting and dashing use the player's current `forward` direction.

A 180° direction change takes approximately one second due to the configured rotation speed.

---

## Gameplay

### Player

* HP: `500`
* Movement Speed: `2 units/s`
* Rotation Speed: `180°/s`
* Armor and Damage Multiplier are handled through a centralized damage formula.

### Shoot

The player fires three projectiles at:

```text
-15°
  0°
+15°
```

Properties:

* Maximum charges: `3`
* Charge recovery: `1 charge / 3 seconds`
* Shot interval: `0.5 seconds`

The shot interval works independently from the charge recovery system.

### Bomb

* Delay: `2 seconds`
* Base Damage: `50`
* Explosion Radius: `5`
* Cooldown: `12 seconds`

### Dash

* Distance: `3 units`
* Duration: `0.5 seconds`
* Explosion Damage: `15`
* Explosion Radius: `3`
* Cooldown: `6 seconds`

---

## Enemies

### Melee Enemy

* HP: `220`
* Movement Speed: `3`
* Attack Range: `1.3`
* Attack Cone: `50°`
* Base Damage: `30`

Behavior:

```text
Chase -> Attack -> Recovery -> Chase
```

### Ranged Enemy

* HP: `180`
* Movement Speed: `2.7`
* Attack Distance: `3`

Poison projectile:

* Speed: `10`
* Range: `5`
* Damage: `30 per tick`
* Total ticks: `4`

The first poison tick is applied immediately on hit, followed by three additional ticks at one-second intervals.

Reapplying poison refreshes its duration instead of stacking additional poison damage.

### Enemy FSM

Enemies use a finite state machine with:

```text
Chase
Attack
Recovery
```

Living enemies continuously face the player.

---

## Wave System

A new wave starts only after every enemy in the current wave has been defeated.

Each defeated enemy grants:

```text
30 EXP
```

### Wave Setup

| Wave    | Enemies                |
| ------- | ---------------------- |
| Wave 1  | 1 Melee                |
| Wave 2  | 1 Melee + 1 Ranged     |
| Wave 3+ | 3–4 Melee + 1–2 Ranged |

The first two waves are intentionally simplified as tutorial waves.

If strict enemy counts are required for every wave, remove the tutorial branches from:

```csharp
WaveManager.GetEnemyCounts()
```

---

## Progression

Level requirement:

```text
100 EXP
```

Excess EXP is carried over after leveling up.

Each level grants:

```text
+40 Max HP
+40 Current HP
+2 Armor
+0.1 Damage Multiplier
```

---

## UI

Implemented UI features:

* Player HP bar
* EXP bar
* Player level
* Current wave
* Remaining enemy count
* Virtual joystick
* Shoot button
* Bomb button
* Dash button
* Skill charge indicators
* Skill cooldown indicators
* Enemy world-space health bars
* Game Over screen
* Play Again button

The Game Over screen displays the player's reached level and wave.

`Play Again` reloads the gameplay scene and restores `Time.timeScale`.

---

## Bonus Features

The prototype also includes:

* Object pooling
* Runtime pooled VFX
* Cinemachine camera shake
* Audio source pooling
* 2D and 3D SFX support
* Randomized SFX pitch
* Same-SFX spam prevention
* Looping BGM
* BGM fade transitions
* UI animations
* Primitive placeholder characters

Runtime VFX is implemented for:

```text
Projectile Hit
Poison
Bomb
Dash
Melee Attack
Level Up
```

---

## Project Structure

```text
Assets/Runtime
├── Core
│   ├── Bootstrap
│   ├── FSM
│   └── Pooling
│
├── Data
│   ├── Audio
│   ├── Enemy
│   └── Player
│
├── Gameplay
│   ├── AudioManager
│   ├── Combat
│   ├── Enemy
│   ├── Player
│   ├── UI
│   ├── VFX
│   └── Wave
│
├── Audio
├── Materials
└── Prefabs
```

---

## Architecture

### Player

`PlayerController`

Initializes and coordinates the player FSM.

`PlayerMotor`

Handles:

* Movement
* Rotation
* Gravity
* Dash movement

`PlayerSkills`

Handles:

* Skill charges
* Cooldowns
* Skill execution

`PlayerStats`

Handles:

* Health
* Armor
* Damage multiplier
* Poison effects

`PlayerProgression`

Handles:

* EXP
* Level progression

### Enemy

`EnemyController`

Initializes and coordinates the enemy FSM.

`EnemyMotor`

Handles:

* Movement
* Facing
* Gravity

`EnemyAttack`

Base class for melee and ranged attacks.

### Systems

`WaveManager`

Handles:

* Wave spawning
* Living enemy tracking
* EXP rewards

`ObjectPooling`

Shared object pool for:

* Player projectiles
* Poison projectiles
* Bombs
* Melee enemies
* Ranged enemies

`PrototypeEffects`

Handles pooled runtime VFX using shared materials.

`AudioManager`

Handles:

* SFX AudioSource pooling
* 2D / 3D SFX
* Random pitch
* SFX spam prevention
* Background music

---

## Configuration

Gameplay values are stored in ScriptableObject configuration assets:

```text
Assets/GameData/Player/PlayerConfig.asset

Assets/GameData/Enemy/MeleeConfig.asset
Assets/GameData/Enemy/RangedConfig.asset

Assets/GameData/Audio/AudioLibrary.asset
```

Prefabs reference their corresponding configuration assets instead of duplicating values across multiple components.

---

## Optimization

The project includes several runtime optimizations:

* Player projectile pooling
* Poison projectile pooling
* Bomb pooling
* Enemy pooling
* VFX pooling
* AudioSource pooling
* VFX prewarming
* Reusable shared materials
* Enemy pool separation by type
* Area damage enemy deduplication

Runtime VFX uses:

```csharp
sharedMaterial
```

instead of creating separate material instances for every effect.

SFX sources are reused instead of being created and destroyed during combat.

Area damage also ensures an enemy with multiple colliders is only processed once.

---
---

## Build

1. Open:

```text
File > Build Settings
```

2. Make sure the following scene is included in `Scenes In Build`:

```text
Assets/Scenes/Gameplay.unity
```

3. Select the target platform:

```text
Windows, Mac, Linux
```

or:

```text
Android
```

4. Select **Switch Platform** if required.
5. Select **Build**.

---

## Tech Stack

* Unity `2022.3.62f3`
* C#
* Cinemachine
* TextMesh Pro
* DOTween
* ScriptableObject-based configuration
* Finite State Machine
* Object Pooling
