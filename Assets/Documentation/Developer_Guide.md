# DarkSea: Survival — Developer Guide

> **Engine:** Unity 6 (6000.x)
> **Render Pipeline:** Universal Render Pipeline (URP)
> **UI System:** Unity UI Toolkit (UIElements / UXML / USS)
> **Language:** C# (.NET Standard 2.1)
> **Auth / Persistence:** PlayerPrefs (fully offline)

---

## Table of Contents

1. [Architecture Overview](#architecture-overview)
2. [Scene Flow & Load Order](#scene-flow--load-order)
3. [Script Dependency Map](#script-dependency-map)
4. [Core Systems — Deep Dive](#core-systems--deep-dive)
   - [Auth System (PlayerPrefs)](#auth-system-playerprefs)
   - [Player Movement](#player-movement)
   - [Oxygen System](#oxygen-system)
   - [Enemy AI](#enemy-ai)
   - [Level Exit & Scoring](#level-exit--scoring)
   - [UI Toolkit Implementation](#ui-toolkit-implementation)
   - [Audio Manager](#audio-manager)
   - [Scene Fader & Transitions](#scene-fader--transitions)
5. [Static Fields & Lifecycle Notes](#static-fields--lifecycle-notes)
6. [Auto-Spawn Pattern](#auto-spawn-pattern)
7. [Key Constants & Tuning Values](#key-constants--tuning-values)
8. [Known Design Quirks](#known-design-quirks)

---

## Architecture Overview

DarkSea uses a **manager-singleton pattern** for global systems and **auto-spawn** `[RuntimeInitializeOnLoadMethod]` for lightweight per-scene effects. There is no scene-persistent GameManager object — state is either in static fields (reset carefully on scene load) or in `PlayerPrefs`.

```
PlayerPrefs (disk)
    └── Auth keys:    PREF_USER_<username>  = password
    └── Progress:     U_<user>_UnlockedLevel, U_<user>_BestScore_L1..3
    └── Achievements: U_<user>_Achievement_<name>
    └── Scores:       U_<user>_RecentScores
    └── Session:      CurrentPlayer (non-prefixed — cleared on logout)

Static Singletons (scene-persistent via DontDestroyOnLoad)
    └── AudioManager       — music, ambience, breathing, sfx
    └── SceneFader         — fade-to-black overlay
    └── CameraShake        — shake controller
    └── NotificationUI     — HUD notification cards
    └── GameMessage        — world-space flash message

Per-Scene MonoBehaviours (destroyed with scene)
    └── OxygenSystem       — oxygen drain, game over broadcast
    └── EnemyAI_Final      — NavMesh enemy (one per enemy GO)
    └── LevelExit          — win trigger + scoring
    └── PlayerMovementFinal — rigidbody player controller
    └── PauseMenuUI        — ESC pause handler
```

---

## Scene Flow & Load Order

Scenes must be registered in Build Settings in this exact order:

| Index | Scene Name | Purpose |
|-------|-----------|---------|
| 0 | `RegisterScene` | Account creation (UGUI-based RegisterManager) |
| 1 | `LoginScene` | Login (UGUI-based LoginTabSystem) OR uses AuthUI |
| 2 | `AuthScene` *(if using AuthUI.cs)* | Combined register+login via UI Toolkit |
| 3 | `MainMenu` | Welcome screen, level select, settings, scores |
| 4 | `Level1_Transition` | 3-second cutscene → auto-loads DarkSea |
| 5 | `DarkSea` | Level 1 gameplay |
| 6 | `Level2_Transition` | Cutscene → auto-loads Level_2 |
| 7 | `Level_2` | Level 2 gameplay |
| 8 | `Level3_Transition` | Cutscene → auto-loads Level_3 |
| 9 | `Level_3` | Level 3 gameplay |

**TransitionManager.cs** handles all cutscene scenes. Assign `nextSceneName` and `waitTime` (default 3s) in the Inspector.

---

## Script Dependency Map

```
PlayerMovementFinal
    ├── reads:  DifficultyManager.EnemySpeedMultiplier (indirect)
    ├── calls:  CameraShake.Shake()
    ├── calls:  SonarBeam.Shoot()
    └── reads:  OxygenSystem (for sonar kill check)

OxygenSystem
    ├── static: IsGameOver (bool) — broadcast to EnemyAI_Final
    ├── calls:  AudioManager.StopAll()
    ├── calls:  GameOverUI.Show()
    ├── calls:  NotificationUI.Show()
    ├── calls:  CameraShake.Shake()
    └── calls:  ScoreHistory.Push()

EnemyAI_Final
    ├── reads:  OxygenSystem.IsGameOver (static)
    ├── reads:  DifficultyManager.EnemySpeedMultiplier
    ├── calls:  OxygenSystem.currentOxygen (direct field write)
    ├── calls:  CameraShake.Shake()
    ├── calls:  SonarBeam.Shoot()
    └── calls:  NotificationUI.Show()

LevelExit
    ├── reads:  TankCollector.Collected (static int)
    ├── calls:  PlayerData.SetInt() / GetInt()
    ├── calls:  ScoreHistory.Push()
    ├── calls:  AchievementSystem.Unlock()
    ├── calls:  AudioManager.StopAll()
    └── calls:  WinPanelUI.Show()

LevelExitEffect  [visual layer — attaches alongside LevelExit]
    ├── reads:  TankCollector.Collected (for lock state)
    ├── reads:  LevelExit.requireTanks / requiredTanks
    ├── calls:  NotificationUI.Show()
    └── calls:  CameraShake.Shake()

AudioManager  [DontDestroyOnLoad singleton]
    ├── static: Instance
    └── manages: music, ambience, breathing, sfx AudioSources

SceneFader  [DontDestroyOnLoad singleton]
    └── static: FadeAndLoad(sceneName)
```

---

## Core Systems — Deep Dive

### Auth System (PlayerPrefs)

The game uses a **two-layer key scheme** to keep accounts isolated:

#### Key Format

```
Password storage:  PlayerPrefs["PREF_USER_<username>"] = plaintext password
Session key:       PlayerPrefs["CurrentPlayer"]        = logged-in username (non-prefixed)
Progress keys:     PlayerData.Key("BestScore_L1")      → "U_<user>_BestScore_L1"
```

#### PlayerData Static Class

```csharp
// All game-progress keys are automatically prefixed with the logged-in user:
string PlayerData.Key(string baseKey)
    → "U_" + CurrentPlayer + "_" + baseKey

PlayerData.GetInt("UnlockedLevel", 1)   // reads PlayerPrefs["U_ali_UnlockedLevel"]
PlayerData.SetInt("BestScore_L2", 850)  // writes PlayerPrefs["U_ali_BestScore_L2"]
```

This means two different accounts never share scores, achievements, or unlock state.

#### Auth Flow (AuthUI.cs — UI Toolkit)

```
Register Tab:
  User fills Username + Password + Confirm
  → Validates: min 3 chars, passwords match, username not taken
  → Saves: PlayerPrefs["PREF_USER_<username>"] = password
  → Redirects: MainMenu via SceneFader.FadeAndLoad()

Login Tab:
  User fills Username + Password
  → Checks: PlayerPrefs.HasKey("PREF_USER_<username>")
  → Compares: stored password == entered password
  → Sets: PlayerPrefs["CurrentPlayer"] = username
  → Redirects: MainMenu

Forgot Password (Login tab only):
  User types username in the username field
  → Reads: PlayerPrefs["PREF_USER_<username>"]
  → Displays: password in green feedback label
```

**Security Note:** Passwords are stored in plaintext. This is intentional for a prototype — do not use in production.

---

### Player Movement

**Script:** `PlayerMovementFinal.cs`
**Requires:** `Rigidbody` component

Movement uses `Rigidbody.MovePosition()` — NOT `transform.position` — to preserve physics collision.

```csharp
// Speed auto-detection in Awake() — set per scene
if (boostSpeed <= 0f) {
    boostSpeed = sceneName == "Level_2" ? 15f : sceneName == "Level_3" ? 16f : 12f;
}

// Move input
float h = Input.GetAxis("Horizontal");
float v = Input.GetAxis("Vertical");
Vector3 move = (transform.forward * v + transform.right * h).normalized * moveSpeed;
rb.MovePosition(rb.position + move * Time.fixedDeltaTime);

// G-hold boost
float currentSpeed = Input.GetKey(KeyCode.G) ? boostSpeed : moveSpeed;
```

**Mouse look** rotates the player GameObject horizontally and the camera vertically.

**Sonar (SPACE):**
- Fires a `SonarBeam` raycast visual
- `OverlapSphere(sonarRange=20f)` finds all enemies
- Enemies within `killRange=3.5f` → `EnemyDeath.Dissolve()`

---

### Oxygen System

**Script:** `OxygenSystem.cs`
**Attach to:** Player GameObject

```
Update() flow:
  1. If isDead → return
  2. currentOxygen -= oxygenDecreaseSpeed * DifficultyManager.OxygenDrainMultiplier * Time.deltaTime
  3. Update HUD text + color
  4. Show WARNING notification at ≤30%
  5. Show CRITICAL + CameraShake at ≤20%
  6. If currentOxygen ≤ 0 → ShowGameOver("OUT OF OXYGEN!")

ShowGameOver(reason):
  1. isDead = true
  2. IsGameOver = true  ← broadcast to all EnemyAI_Final instances
  3. AudioManager.StopAll()
  4. CameraShake.Shake(0.50f, 0.18f)
  5. ScoreHistory.Push(level, 0, false)  ← record loss
  6. gameOverUI.Show(reason)
  7. Time.timeScale = 0f
```

**Static bool `IsGameOver`** is the global stop signal. It is checked at the TOP of `EnemyAI_Final.Update()` before any other logic. Reset it in `GameOverUI.OnRestart()` and `GameOverUI.OnMenu()` before scene reload — otherwise enemies are permanently frozen in the new scene.

**Public API:**
```csharp
oxygenSystem.RefillOxygen(float amount)  // clamps to 100f
oxygenSystem.ShowGameOver(string reason) // manual trigger (e.g. from traps)
oxygenSystem.currentOxygen              // direct field — EnemyAI_Final writes to it
```

---

### Enemy AI

**Script:** `EnemyAI_Final.cs`
**Requires:** `NavMeshAgent` component
**Scene must have:** Baked NavMesh (Window > AI > Navigation > Bake)

#### Three Modes

```csharp
public enum Mode { Passive, Chase, Hunt }
```

| Mode | Level | Behavior |
|------|-------|----------|
| `Passive` | Level 2 | Patrols waypoints, never chases, but attacks at range |
| `Chase` | Level 1 | Direct pursuit, attacks at `catchRange` |
| `Hunt` | Level 3 | Fast pursuit + periodic mimic audio lures |

#### Update() Flow

```
1. Check OxygenSystem.IsGameOver → stop audio + agent → return
2. If player == null → return
3. Countdown _attackTimer
4. Calculate distance to player
5. If distance < approachWarningRange (12m) → NotificationUI "ENEMY APPROACHING!" + alert burst
6. Switch(mode):
   Passive → Patrol()
   Chase   → SetDestination(player) + if dist < catchRange → AttackPlayer()
   Hunt    → SetDestination(player) + PlayLure() + if dist < catchRange → AttackPlayer()
```

#### Attack Logic

```csharp
void AttackPlayer() {
    if (_attackTimer > 0f) return;      // 1.5s cooldown
    _attackTimer = attackCooldown;       // reset cooldown
    CameraShake.Shake(0.35f, 0.12f);    // screen shake
    SonarBeam.Shoot(from, to, EnemyColor);  // red attack beam
    OxygenSystem ox = player.GetComponent<OxygenSystem>();
    ox.currentOxygen -= oxygenDrainPct; // 5% drain
    if (ox.currentOxygen < 0f) ox.currentOxygen = 0f;
    NotificationUI.Show("ENEMY ATTACK! -5% OXYGEN", Danger);
}
```

#### Lure Audio (Hunt Mode Only)

```csharp
void PlayLure() {
    lureTimer += Time.deltaTime;
    if (lureTimer >= lureInterval && !audioSource.isPlaying) {
        audioSource.clip = lureClips[Random.Range(0, lureClips.Length)];
        audioSource.Play();  // NOT PlayOneShot — must be stoppable by audioSource.Stop()
    }
}
```

**Critical:** Use `audioSource.Play()` (not `PlayOneShot`) so that `audioSource.Stop()` in the `IsGameOver` check can silence it.

#### NavMesh Setup Per Enemy

- `NavMeshAgent.baseOffset = 0.52f` — lifts agent above NavMesh surface so Mutant feet align with floor
- `NavMeshAgent.speed` set from `chaseSpeed` or `huntSpeed` × `DifficultyManager.EnemySpeedMultiplier`

---

### Level Exit & Scoring

**Script:** `LevelExit.cs`
**Attach to:** Exit Gate GameObject with BoxCollider (Is Trigger = ON)

#### Inspector Fields

```csharp
public int levelNumber;          // 1, 2, or 3
public string nextSceneName;     // "Level2_Transition", "Level3_Transition", or ""
public bool isFinalLevel;        // true for Level 3
public bool requireTanks;        // true for Level 2
public int requiredTanks = 3;    // Level 2: need 3 tanks
public WinPanelUI winPanelUI;
public OxygenSystem oxygenScript;
public GameObject player;
public GameObject[] enemies;
```

#### Win Trigger Flow

```
OnTriggerEnter(Player):
  1. If requireTanks && TankCollector.Collected < requiredTanks → show GameMessage, return
  2. finished = true
  3. UnlockNext() → PlayerData.SetInt("UnlockedLevel", levelNumber + 1)
  4. ShowComplete(isFinal):
     a. AudioManager.StopAll()
     b. StopEverything() → disables PlayerMovementFinal + all EnemyAI_Final agents
     c. Calculate score: (oxygen×10) + timeBonus + (tanks×50)
     d. Save best score: PlayerData.SetInt("BestScore_L" + levelNumber, total)
     e. ScoreHistory.Push(levelNumber, total, true)  ← record win
     f. AchievementSystem.Unlock() checks
     g. Time.timeScale = 0f
     h. winPanelUI.Show(playerName, total, oxygen, isFinal, nextSceneName)
```

#### TankCollector

```csharp
public static class TankCollector {
    public static int Collected;  // static int — reset in LevelExit.Start()
}
```

`TankCollector.Collected` is reset to 0 in `LevelExit.Start()` so it always starts clean for each scene.

---

### UI Toolkit Implementation

The game uses **Unity UI Toolkit** (not legacy UGUI) for all gameplay HUD and menus.

#### Architecture

Each UI panel is a separate `UIDocument` with its own `.uxml` + `.uss` pair:

| Document | UXML | USS | Script |
|----------|------|-----|--------|
| Auth Screen | `AuthPanel.uxml` | `AuthPanel.uss` | `AuthUI.cs` |
| Notification Cards | `NotificationPanel.uxml` | `NotificationPanel.uss` | `NotificationUI.cs` |
| Win Screen | `WinPanel.uxml` | `WinPanel.uss` | `WinPanelUI.cs` |
| Game Over Screen | `GameOverPanel.uxml` | — | `GameOverUI.cs` |
| Pause Menu | `PausePanel.uxml` | — | `PauseMenuUI.cs` |

#### UXML/USS Pattern

```csharp
// In OnEnable() — query elements after UIDocument is ready
var root = GetComponent<UIDocument>().rootVisualElement;
_loginBtn = root.Q<Button>("btn-login");
_loginBtn.clicked += OnLoginClicked;

// In OnDisable() — always unsubscribe to prevent memory leaks
_loginBtn.clicked -= OnLoginClicked;
```

#### Dynamic UI (Auto-spawned, no UXML)

Several scripts create their UI entirely in code at runtime and attach to an existing scene `UIDocument`'s `PanelSettings`:

- `GameHUDExtras.cs` — timer, enemy count, objective label
- `SubmarineToggle.cs` — submarine HUD panel with Painter2D icon
- `AdvancedHUD.cs` — oxygen bar, sonar radar (Painter2D)
- `MinimapHUD.cs` — minimap with enemy blips
- `OxygenVignette.cs` — full-screen vignette overlay

These scripts find the scene's `UIDocument` by querying `FindObjectsByType<UIDocument>()`, copy its `PanelSettings`, then add their own `UIDocument` component at a higher `sortingOrder`.

#### Notification System

```csharp
// Usage from any script:
NotificationUI.Show("MESSAGE TEXT", NotificationUI.NotifType.Warning);

// Available types:
public enum NotifType { Kill, Warning, Danger }
// CSS classes: notif-kill, notif-warning, notif-danger (defined in NotificationPanel.uss)
```

Cards slide in from the right, display for 2.8 seconds, then slide out. Multiple cards stack vertically.

---

### Audio Manager

**Script:** `AudioManager.cs`
**Pattern:** `DontDestroyOnLoad` singleton

Manages 4 AudioSources: `musicSource`, `ambienceSource`, `breathingSource`, `sfxSource`.

```csharp
AudioManager.StopAll()           // stops all 4 sources
AudioManager.PlayEnemyAlert()    // one-shot sfx
```

**Breathing audio** is scaled dynamically in `Update()` based on `OxygenSystem.currentOxygen`:
- > 50%: breathing quiet
- 30–50%: breathing audible
- < 30%: breathing loud + fast

**Note:** Enemy `AudioSource` components are NOT managed by `AudioManager`. They are stopped via `OxygenSystem.IsGameOver` check in `EnemyAI_Final.Update()` or explicitly in `LevelExit.StopOneEnemy()`.

---

### Scene Fader & Transitions

**Script:** `SceneFader.cs`
**Pattern:** `DontDestroyOnLoad` singleton (UI Toolkit overlay, black fullscreen panel)

```csharp
// Load a scene with fade out → in:
SceneFader.FadeAndLoad("Level_2");

// Fade in on scene load happens automatically in Start()
```

**Transition scenes** (Level1_Transition, etc.) use `TransitionManager.cs`:
```csharp
public string nextSceneName = "DarkSea";
public float waitTime = 3f;
// Calls SceneFader.FadeAndLoad(nextSceneName) after waitTime seconds
```

---

## Static Fields & Lifecycle Notes

Static fields survive scene loads. These MUST be manually reset:

| Field | Script | Reset Where |
|-------|--------|------------|
| `IsGameOver` | `OxygenSystem` | `GameOverUI.OnRestart()`, `GameOverUI.OnMenu()`, `OxygenSystem.RestartGame()` |
| `TankCollector.Collected` | `LevelExit` (inner class) | `LevelExit.Start()` |
| `NotificationUI.Instance` | `NotificationUI` | Auto (Awake singleton guard) |
| `AudioManager.Instance` | `AudioManager` | Auto (DontDestroyOnLoad) |
| `SceneFader.Instance` | `SceneFader` | Auto (DontDestroyOnLoad) |
| `CameraShake.Instance` | `CameraShake` | Auto-spawn per scene |

---

## Auto-Spawn Pattern

Several scripts use `[RuntimeInitializeOnLoadMethod]` to self-register without scene setup:

```csharp
[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
static void AutoSpawn() {
    string scene = SceneManager.GetActiveScene().name;
    if (!scene.Contains("Level_2") && !scene.Contains("Level_3")) return;
    new GameObject("SpeedBoost").AddComponent<SpeedBoost>();
}
```

Scripts using this pattern: `OxygenTankVisual`, `SpeedBoost`, `UnderwaterAtmosphere`, `UnderwaterParticles`, `UnderwaterLightRipple`, `SubmarineToggle`, `GameHUDExtras`, `CameraShake`.

---

## Key Constants & Tuning Values

| System | Parameter | Value | Where to Change |
|--------|-----------|-------|----------------|
| Oxygen | Drain speed | `2f/s` | `OxygenSystem.oxygenDecreaseSpeed` in Inspector |
| Enemy | Catch range | `2.5f` | `EnemyAI_Final.catchRange` in Inspector |
| Enemy | Oxygen drain per hit | `5f%` | `EnemyAI_Final.oxygenDrainPct` in Inspector |
| Enemy | Attack cooldown | `1.5f s` | `EnemyAI_Final.attackCooldown` in Inspector |
| Enemy | Approach warning | `12f m` | `EnemyAI_Final.approachWarningRange` in Inspector |
| Enemy | Lure interval | `6f s` | `EnemyAI_Final.lureInterval` in Inspector |
| Tank pickup | Refill amount | `40f%` | `OxygenPickup.refillAmount` in Inspector |
| CollectibleTank | Refill amount | `30f%` | `CollectibleTank.cs` constant |
| Sonar | Kill range | `3.5f` | `PlayerMovementFinal.killRange` in Inspector |
| Sonar | Detection range | `20f` | `SonarSystem.sonarRange` in Inspector |
| Score | Time bonus base | `200` | `LevelExit.cs` line constant |
| Player | DarkSea speed | `12f` | Auto in `PlayerMovementFinal.Awake()` |
| Player | Level_2 speed | `15f` | Auto in `PlayerMovementFinal.Awake()` |
| Player | Level_3 speed | `16f` | Auto in `PlayerMovementFinal.Awake()` |
| NavMesh | Base offset | `0.52f` | `NavMeshAgent.baseOffset` on each enemy |

---

## Known Design Quirks

1. **Two auth systems coexist:** `RegisterManager.cs` + `LoginTabSystem.cs` (UGUI-based, legacy) and `AuthUI.cs` (UI Toolkit, active). The active system uses `PREF_USER_` prefix; the legacy system uses bare keys. An account created in one system cannot log in via the other.

2. **`applyRootMotion = false` required on Mutant animator.** The Mutant character asset has root motion enabled by default, which fights NavMeshAgent position. Disable it in the Animator component on each enemy.

3. **OxygenTankVisual targets both pickup types.** `OxygenTankVisual.cs` finds both `OxygenPickup` (DarkSea) and `CollectibleTank` (Level_2/3) objects via `FindObjectsByType` and applies the billboard sprite to both. The `OxygenTank.png` must be in `Assets/Resources/`.

4. **SonarSystem vs PlayerMovementFinal:** Sonar logic is duplicated in both scripts. `SonarSystem.cs` is a standalone older version. `PlayerMovementFinal.cs` contains the active sonar implementation. `SonarSystem.cs` may be safely deleted if it is not attached to any scene GameObject.

5. **`TankCollector.Collected` is a static int.** It resets in `LevelExit.Start()`. Do NOT call `LevelExit` initialization before the scene fully loads or the counter will reset mid-play.
