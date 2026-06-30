# DarkSea: Survival — Project Documentation

> **Version:** Prototype 1.0  
> **Engine:** Unity 6 (URP)  
> **Platform:** PC (Windows)  
> **Connectivity:** Fully Offline — No internet, no Firebase, no Google. All data stored in Unity `PlayerPrefs`.

---

## Table of Contents

1. [How to Play & Controls](#1-how-to-play--controls)
2. [Full Feature List & Mechanics Breakdown](#2-full-feature-list--mechanics-breakdown)
3. [Technical Architecture & Code Flow](#3-technical-architecture--code-flow)
4. [Scene Structure](#4-scene-structure)
5. [Asset Map](#5-asset-map)
6. [Known Quirks & Developer Notes](#6-known-quirks--developer-notes)

---

## 1. How to Play & Controls

### Objective
You are a diver trapped in a dark underwater maze. Survive by managing your oxygen, collecting tanks, avoiding or escaping enemies, and reaching the **Exit Point** at the end of each level.

### Controls

| Key / Input | Action |
|---|---|
| `W` / `S` | Move Forward / Backward |
| `A` / `D` + Mouse X | Rotate Left / Right |
| `G` (hold) | Speed Boost (no animation change) |
| `SPACE` | Fire Sonar Beam (kills enemies in range) |
| `ESC` | Pause / Resume |

### Speed by Level (G Hold)
| Level | Boost Speed |
|---|---|
| DarkSea (Level 1) | 12 units/s |
| Level_2 | 15 units/s |
| Level_3 | 16 units/s |

### Objective Per Level

| Level | Goal |
|---|---|
| **DarkSea** | Reach the Exit Point before oxygen runs out |
| **Level_2** | Collect 3 hidden Oxygen Tanks, then reach the Exit |
| **Level_3** | Survive the Hunt-mode enemies and reach the Exit |

### Win / Lose Conditions

- **Win:** Player reaches the Exit Point with oxygen remaining.
- **Lose (Oxygen):** `currentOxygen` drops to 0 → Game Over screen.
- **Lose (Enemy):** Enemy contacts player → drains 5% oxygen per hit (1.5s cooldown). If oxygen hits 0 from attacks, Game Over.

---

## 2. Full Feature List & Mechanics Breakdown

### 2.1 Offline Authentication (PlayerPrefs)

- **Register:** Creates an account stored locally: `PlayerPrefs.SetString("PREF_USER_<username>", password)`
- **Login:** Reads saved password from PlayerPrefs, compares, sets `"CurrentPlayer"` on success.
- **Forgot Password:** User enters their username → system reads `"PREF_USER_<username>"` from PlayerPrefs and displays the saved password.
- **No network calls.** Completely offline.

PlayerPrefs keys used:
| Key | Value |
|---|---|
| `PREF_USER_<username>` | Plaintext password |
| `CurrentPlayer` | Currently logged-in username |
| `SavedName` | Last registered username |
| `DarkSea_HighScore` | High score for DarkSea |

### 2.2 Oxygen System (`OxygenSystem.cs`)

- Oxygen starts at 100% and decreases continuously (`oxygenDecreaseSpeed * DifficultyManager.OxygenDrainMultiplier`).
- **Warning at 30%:** Shows `NotificationUI` — "LOW OXYGEN!"
- **Critical at 20%:** Shows "CRITICAL OXYGEN!" + `CameraShake`
- **Dead at 0%:** Calls `ShowGameOver("OUT OF OXYGEN!")`
- `static bool IsGameOver` — broadcast flag so all enemies know the game ended.

Refill sources:
| Source | Amount |
|---|---|
| `OxygenPickup` (DarkSea) | +40% |
| `CollectibleTank` (Level_2/3) | +30% |

### 2.3 Sonar System (`SonarSystem.cs` + `SonarBeam.cs`)

- Player presses `SPACE` → fires a green sonar beam in the look direction.
- Beam is rendered via `LineRenderer` (two passes: outer glow + white inner core) with a fade-out over 0.5s.
- If beam hits an enemy within range → `EnemyDeath` triggered (enemy destroyed).
- `SonarBeam` is a singleton runner — `LaserOuter` / `LaserInner` are children of the runner and auto-destroyed when the runner is destroyed.
- Static event `SonarSystem.OnPinged` → `AudioManager` plays `sonarPing.mp3`.

### 2.4 Enemy AI (`EnemyAI_Final.cs`)

Three behaviour modes, set per-enemy in Inspector:

| Mode | Behaviour |
|---|---|
| `Passive` | Patrols waypoints, never attacks |
| `Chase` | Chases player, attacks on contact |
| `Hunt` | Chases at higher speed, plays mimic voice lure clips, attacks on contact |

**Attack Logic (revised):**
- Attack range: `catchRange = 2.5f` units
- On contact: drains `oxygenDrainPct = 5f`% from player oxygen
- Cooldown: `attackCooldown = 1.5f` seconds between attacks
- Visual: red `SonarBeam` + `CameraShake` on each hit
- Enemy continues chasing after hitting (does NOT stop)

**Game-Over Detection:**
- All enemies check `OxygenSystem.IsGameOver` every frame.
- When true: stop `NavMeshAgent`, stop `AudioSource`.

**Approach Warning:**
- Within `approachWarningRange` (12 units): "ENEMY APPROACHING!" notification + `enemyAlert` SFX + particle burst above enemy head.
- Cooldown: 10s between repeat warnings.

### 2.5 Enemy Visuals

- **Model:** Mutant humanoid FBX (`MutantModel`) — scaled (1.3, 1.3, 1.3)
- **NavMeshAgent:** `baseOffset = 0.52f` — lifts agent root so Mutant feet align with NavMesh floor
- **Animator:** `EnemyAnimator.controller` — driven by `EnemyAnimatorDriver.cs`
  - `Speed` float → set from `agent.velocity.magnitude`
  - `anim.speed = 0` when velocity < 0.1 (prevents animation while stationary)
- Old capsule `EnemyModel` was deleted; only `MutantModel` remains.

### 2.6 Oxygen Tank Visuals (`OxygenTankVisual.cs`)

- Auto-runs via `[RuntimeInitializeOnLoadMethod(BeforeSceneLoad)]`.
- On scene load: finds all `OxygenPickup` AND `CollectibleTank` GameObjects.
- Hides their `MeshRenderer` (cylinder hidden).
- Attaches `OxygenTankBillboard`: a sprite (`Assets/Resources/OxygenTank.png`) that:
  - Always faces the camera (billboard).
  - Bobs up/down sinusoidally.
  - Has a pulsing teal glow ring.

### 2.7 CollectibleTank (Mission Tanks — Level 2/3)

- `CollectibleTank.cs` — mission-objective tanks (not regular refills).
- On player trigger: increments `TankCollector.Collected`, refills +30% oxygen, destroys self.
- Level exit gate (`LevelExit.cs`) checks `TankCollector.Collected >= requiredTanks` before allowing exit.

### 2.8 MiniMap System

- `MapIcon` on MiniMap layer (Layer 6).
- Main Camera `cullingMask` excludes Layer 6 (binary 55 = `110111`) → red sphere NOT visible in gameplay, only on MiniMap camera.
- `MinimapHUD.cs` renders top-down view of player position.

### 2.9 Audio System (`AudioManager.cs`)

- Singleton, `DontDestroyOnLoad` — survives scene transitions.
- 4 audio channels: `_musicSrc`, `_ambienceSrc`, `_breathingSrc`, `_sfxSrc`
- Clips loaded from `Assets/Resources/Audio/`:
  - `bgMusic.mp3` — main menu loop
  - `ambience.mp3` — underwater gameplay ambience
  - `breathing.mp3` — player breathing (volume/pitch scale with oxygen danger)
  - `sonarPing.mp3` — sonar fire SFX
  - `enemyAlert.mp3` — enemy approach one-shot
- `StopAll()` stops AudioManager sources.
- Enemy-level `AudioSource` (for mimic lure clips) is stopped via `OxygenSystem.IsGameOver` check in `EnemyAI_Final.Update()`.

### 2.10 Difficulty System (`DifficultyManager.cs`)

- Static multipliers applied at Start:
  - `OxygenDrainMultiplier` — scales `oxygenDecreaseSpeed`
  - `EnemySpeedMultiplier` — scales `agent.speed`

### 2.11 Pause System (`PauseManager.cs`)

- `ESC` toggles pause: `Time.timeScale = 0` + `AudioListener.pause = true`.
- Cursor unlocked during pause.

### 2.12 Win / Score System

- `LevelExit.cs` — triggers when player enters Exit Point tag zone.
- `ScoreHistory.Push(level, score, won)` — stores result in `PlayerPrefs`.
- `WinPanelUI` shown on win; `GameOverUI` shown on death.

---

## 3. Technical Architecture & Code Flow

### 3.1 Scene Load Flow

```
Boot
 └─ Auth Scene (AuthPanel.uxml / AuthUI.cs)
      ├─ Register → saves "PREF_USER_<name>" to PlayerPrefs
      └─ Login    → sets "CurrentPlayer" → loads MainMenu
           └─ MainMenu Scene (MainMenuUI.cs)
                └─ Level Select (LevelSelectUI.cs)
                     ├─ DarkSea  (Level 1)
                     ├─ Level_2  (Level 2)
                     └─ Level_3  (Level 3)
```

### 3.2 Gameplay Scene Hierarchy (per level)

```
[Scene Root]
├─ Player                    ← PlayerMovementFinal, OxygenSystem, SonarSystem, Rigidbody
│   └─ Ch45 (model)          ← Animator (PlayerAnimator.controller)
├─ Enemy / Enemy(2) / ...    ← EnemyAI_Final, NavMeshAgent, EnemyAnimatorDriver
│   └─ MutantModel           ← Animator (EnemyAnimator.controller), scale (1.3,1.3,1.3)
├─ OxygenTank / CollectibleTank  ← CapsuleCollider(trigger), OxygenPickup / CollectibleTank
├─ ExitPoint                 ← LevelExit.cs, tag "ExitPoint"
├─ Canvas (HUD)              ← AdvancedHUD, OxygenText, TankCounterText
├─ NotificationPanel         ← NotificationUI.cs
└─ Camera                    ← CameraShake, cullingMask excludes Layer 6 (MiniMap)
```

### 3.3 Key Script Dependencies

```
PlayerMovementFinal
  ├─ uses SonarSystem (SPACE key)
  └─ drives Animator (Speed float, IsMoving bool)

OxygenSystem
  ├─ decrements currentOxygen
  ├─ calls GameOverUI.Show() on death
  ├─ static IsGameOver → read by all EnemyAI_Final instances
  └─ calls AudioManager.StopAll() via ShowGameOver

EnemyAI_Final
  ├─ reads OxygenSystem.IsGameOver (stops self)
  ├─ uses NavMeshAgent (pathfinding)
  ├─ calls AttackPlayer() → drains OxygenSystem.currentOxygen
  └─ fires SonarBeam (red) on attack

SonarSystem
  ├─ fires SonarBeam (green) on SPACE
  ├─ raycasts for enemies → calls EnemyDeath
  └─ broadcasts OnPinged event → AudioManager

OxygenTankVisual  [RuntimeInitializeOnLoadMethod]
  ├─ finds OxygenPickup + CollectibleTank
  └─ attaches OxygenTankBillboard (sprite + bob + glow)

AudioManager  [RuntimeInitializeOnLoadMethod, DontDestroyOnLoad]
  ├─ manages music/ambience/breathing/sfx
  └─ scales breathing volume with oxygen danger
```

### 3.4 PlayerPrefs Data Map

All offline data lives in PlayerPrefs (local registry on Windows):

| Key Pattern | Type | Purpose |
|---|---|---|
| `PREF_USER_<username>` | String | User's password |
| `CurrentPlayer` | String | Logged-in username |
| `SavedName` | String | Last registered name |
| `DarkSea_Score_<n>` | Int | Score history entry |
| `Difficulty` | Int | 0=Easy, 1=Normal, 2=Hard |

### 3.5 NavMesh Setup

- All 3 levels have baked NavMesh data in `Assets/Scenes/<LevelName>/NavMesh/`.
- Enemy `NavMeshAgent` settings:
  - `baseOffset = 0.52f` — aligns Mutant feet with floor
  - `height = 1.8f`, `radius = 0.4f`
  - `speed` scaled by `DifficultyManager.EnemySpeedMultiplier`

### 3.6 Animation Architecture

**Player (Ch45 skeleton):**
- Controller: `Assets/PlayerAnimator.controller`
- Parameters: `Speed` (float), `IsMoving` (bool)
- States: Idle → Walking (blend by Speed)
- `applyRootMotion = false` (walk has baked root motion → drift if enabled)

**Enemy (Mutant skeleton):**
- Controller: `Assets/EnemyAnimator.controller`
- Driver: `EnemyAnimatorDriver.cs` on MutantModel child
  - Reads `NavMeshAgent.velocity.magnitude` → sets `Speed`
  - Sets `anim.speed = 0` when stopped (freeze animation)

---

## 4. Scene Structure

| Scene | Build Index | Purpose |
|---|---|---|
| `Auth` | 0 | Register / Login screen |
| `MainMenu` | 1 | Main menu, settings, level select |
| `DarkSea` | 2 | Level 1 — basic survival maze |
| `Level_2` | 3 | Level 2 — collect 3 tanks to unlock exit |
| `Level_3` | 4 | Level 3 — Hunt-mode enemies, harder maze |

---

## 5. Asset Map

```
Assets/
├─ GameScripts/
│   ├─ PlayerMovementFinal.cs   — Player movement, boost, sonar trigger
│   ├─ OxygenSystem.cs          — Oxygen drain, game over, refill API
│   ├─ SonarSystem.cs           — Sonar fire logic, enemy kill raycast
│   ├─ SonarBeam.cs             — LineRenderer beam visual (singleton runner)
│   ├─ EnemyAI_Final.cs         — Enemy AI (Passive/Chase/Hunt), attack logic
│   ├─ EnemyAnimatorDriver.cs   — Drives enemy animator from NavAgent velocity
│   ├─ CollectibleTank.cs       — Mission tank pickup + counter
│   ├─ OxygenPickup.cs          — Simple oxygen refill pickup
│   ├─ OxygenTankVisual.cs      — Billboard sprite on all tanks (auto-init)
│   ├─ AudioManager.cs          — Singleton audio (DontDestroyOnLoad)
│   ├─ AuthUI.cs                — Offline register/login/forgot-password UI
│   ├─ LevelExit.cs             — Win condition trigger
│   ├─ LevelManager.cs          — Scene transition helpers
│   ├─ DifficultyManager.cs     — Speed/drain multipliers
│   ├─ NotificationUI.cs        — In-game popup notifications
│   ├─ CameraShake.cs           — Static camera shake API
│   ├─ GameOverUI.cs            — Game over panel controller
│   ├─ WinPanelUI.cs            — Win panel controller
│   ├─ PauseManager.cs          — ESC pause / resume
│   ├─ MinimapHUD.cs            — MiniMap top-down display
│   └─ ...
├─ UI/
│   ├─ AuthPanel.uxml / .uss    — Auth screen layout & styles
│   ├─ HUD.uxml / .uss          — In-game HUD
│   ├─ MainMenu.uxml / .uss     — Main menu
│   ├─ GameOverPanel.uxml / .uss
│   ├─ WinPanel.uxml / .uss
│   ├─ PausePanel.uxml / .uss
│   └─ LevelSelect.uxml / .uss
├─ Resources/
│   ├─ OxygenTank.png           — Tank billboard sprite
│   └─ Audio/
│       ├─ bgMusic.mp3
│       ├─ ambience.mp3
│       ├─ breathing.mp3
│       ├─ sonarPing.mp3
│       └─ enemyAlert.mp3
├─ Scenes/
│   ├─ DarkSea.unity
│   ├─ Level_2.unity
│   └─ Level_3.unity
├─ Animations/
│   ├─ PlayerAnimator.controller
│   └─ EnemyAnimator.controller
└─ DarkSea_Documentation.md     ← this file
```

---

## 6. Known Quirks & Developer Notes

### Offline Auth — Security Note
Passwords are stored in plaintext in `PlayerPrefs` (Windows Registry). This is intentional — the game is fully offline and single-device. Do NOT add network sync without encrypting passwords first.

### applyRootMotion = false (Player)
Ch45's Walk animation has baked root motion. If `applyRootMotion` is ever re-enabled, the player will drift forward indefinitely while walking. Keep it `false` — movement is driven by `Rigidbody.MovePosition`.

### Enemy baseOffset = 0.52f
Mutant model root pivot is at world Y=0, but the feet geometry starts at approximately -0.52 units below the root (at scale 1.3). The `NavMeshAgent.baseOffset = 0.52` lifts the agent root so feet land on the NavMesh surface. If the enemy appears to sink into the floor or float, adjust this value.

### MiniMap Layer
`MapIcon` objects are on **Layer 6 (MiniMap)**. Main Camera `cullingMask = 55` (binary `00110111`) excludes Layer 6. If the red dot ever appears in the main game view, check the cullingMask on Main Camera.

### SonarBeam Cleanup
`LaserOuter` and `LaserInner` are parented to the `SonarBeamRunner` singleton. Hard `Destroy(go, 0.7f)` calls serve as safety nets in case the coroutine is interrupted. Do not remove these safety Destroy calls.

### CollectibleTank vs OxygenPickup
- `CollectibleTank` — mission objective (Level 2/3): increments counter, required for exit gate.
- `OxygenPickup` — simple refill (DarkSea): no counter, just oxygen.
- `OxygenTankVisual` handles both types — adds billboard sprite to whichever it finds.

### TankCollector.Collected is static
`TankCollector.Collected` is a static int. It is **not reset between scene loads automatically**. It resets when a new scene initializes `CollectibleTank.Start()` — but if you load a level additively or restart without a full scene reload, the counter may carry over. Fix: call `TankCollector.Collected = 0` in a scene init script if needed.

### Enemy Audio (Phase 4 Fix)
`AudioManager.StopAll()` only stops AudioManager's own managed channels. Enemy-specific `AudioSource` components (used for lure clips in Hunt mode) are stopped separately by checking `OxygenSystem.IsGameOver` at the top of `EnemyAI_Final.Update()`.

---

*Documentation generated for DarkSea: Survival Prototype — June 2026*
