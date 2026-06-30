# DarkSea: Survival — Setup & Installation Guide

> **Target Audience:** Developers setting up the project for the first time
> **Engine:** Unity 6 (6000.x)

---

## Table of Contents

1. [Requirements](#requirements)
2. [Opening the Project](#opening-the-project)
3. [Required Packages](#required-packages)
4. [Build Settings — Scene Order](#build-settings--scene-order)
5. [NavMesh Setup](#navmesh-setup)
6. [Building the Final .exe](#building-the-final-exe)
7. [Troubleshooting Common Issues](#troubleshooting-common-issues)

---

## Requirements

### Minimum Software

| Requirement | Version | Notes |
|-------------|---------|-------|
| **Unity Editor** | 6000.x LTS (Unity 6) | Required. Do NOT use Unity 5, 2021, or 2022 — API differences will cause compile errors. |
| **Universal Render Pipeline** | Included with Unity 6 | Must be configured as the active render pipeline |
| **UI Toolkit** | Built into Unity 6 | No installation needed; already included |
| **TextMeshPro** | Latest (via Package Manager) | Required for all in-game text |
| **NavMesh** | AI Navigation package | Required for enemy pathfinding |
| **Git (optional)** | Any version | Only needed if cloning from a repository |

### Hardware (Development Machine)

| Component | Minimum |
|-----------|---------|
| OS | Windows 10 / 11 (64-bit) |
| RAM | 8 GB |
| GPU | Any DirectX 11 compatible GPU |
| Disk Space | ~2 GB for project + Unity installation |

---

## Opening the Project

### Step 1 — Install Unity Hub

Download and install **Unity Hub** from [unity.com](https://unity.com/download). Unity Hub manages multiple Unity versions.

### Step 2 — Install Unity 6

In Unity Hub:
1. Click **Installs** → **Install Editor**
2. Select **Unity 6000.x LTS**
3. Under modules, check:
   - **Microsoft Visual Studio Community** (or your preferred IDE)
   - **Windows Build Support (IL2CPP)** ← required for final .exe build
4. Click **Install**

### Step 3 — Open the Project

1. In Unity Hub, click **Open** → **Add project from disk**
2. Navigate to the `DarkSea_Prototype` folder
3. Select the folder (not a file inside it) and click **Open**
4. Unity will import all assets — this may take 2–5 minutes on first open

### Step 4 — Verify Render Pipeline

1. In Unity Editor: **Edit → Project Settings → Graphics**
2. Confirm **Scriptable Render Pipeline Settings** is set to a URP asset
3. If not: create one via **Assets → Create → Rendering → URP Asset**, then assign it

---

## Required Packages

Open **Window → Package Manager** and verify these are installed:

| Package | Where to Find | Status |
|---------|--------------|--------|
| **Universal RP** | Unity Registry | Should be pre-installed |
| **AI Navigation** | Unity Registry | Search "AI Navigation" — install if missing |
| **TextMeshPro** | Unity Registry | Search "TextMeshPro" — install if missing |
| **Input System** | Unity Registry | Optional; game uses legacy `Input.GetKey` — not required |

### TextMeshPro Essentials

After installing TextMeshPro, Unity will prompt:
> "Import TMP Essentials?"

Click **Import TMP Essentials**. This imports the default fonts used by the project.

### AI Navigation (NavMesh)

If enemies are not moving in play mode, the `AI Navigation` package may be missing:
1. **Window → Package Manager → Unity Registry**
2. Search: `AI Navigation`
3. Install the latest version

---

## Build Settings — Scene Order

The game requires scenes to be registered in **Build Settings** in a specific order. Wrong order = wrong scene loads on transitions.

**File → Build Settings → Add Open Scenes** OR drag scenes manually:

| Index | Scene Name | Path |
|-------|-----------|------|
| 0 | `AuthScene` (or `LoginScene`) | `Assets/Scenes/` |
| 1 | `MainMenu` | `Assets/Scenes/` |
| 2 | `Level1_Transition` | `Assets/Scenes/` |
| 3 | `DarkSea` | `Assets/Scenes/` |
| 4 | `Level2_Transition` | `Assets/Scenes/` |
| 5 | `Level_2` | `Assets/Scenes/` |
| 6 | `Level3_Transition` | `Assets/Scenes/` |
| 7 | `Level_3` | `Assets/Scenes/` |

> **Note:** If both `RegisterScene`/`LoginScene` (legacy UGUI) and `AuthScene` (UI Toolkit) exist, only include ONE in Build Settings — whichever is the active auth system. `AuthScene` with `AuthUI.cs` is the current active system.

**How to open Build Settings:**
1. **File → Build Settings** (`Ctrl+Shift+B`)
2. Click **Add Open Scenes** for each scene after opening it, OR
3. Drag scene assets from the Project window into the "Scenes In Build" list

---

## NavMesh Setup

NavMesh must be baked in each gameplay scene for enemies to pathfind.

### Check if NavMesh is baked:
- In the **Scene View**, enable **AI → Show NavMesh** (gizmo overlay button)
- NavMesh areas appear as a blue overlay. If no blue area is visible, it hasn't been baked.

### How to bake:

1. Open the gameplay scene (DarkSea, Level_2, or Level_3)
2. **Window → AI → Navigation** (opens the Navigation window)
3. Click the **Bake** tab
4. Set **Agent Radius:** `0.35` and **Agent Height:** `2.0`
5. Click **Bake**
6. Save the scene (**Ctrl+S**)

Repeat for all three gameplay scenes.

### Enemy NavMeshAgent Settings (per enemy):

Select each enemy GameObject in the Inspector:
- `NavMeshAgent` → **Base Offset:** `0.52` (prevents feet from floating)
- `NavMeshAgent` → **Speed:** Set via `EnemyAI_Final.chaseSpeed` or `huntSpeed`
- Animator → **Apply Root Motion:** `false` ← CRITICAL — must be unchecked or enemies will fight the NavMesh

---

## Building the Final .exe

### Step 1 — Switch Platform

**File → Build Settings → Platform: PC, Mac & Linux Standalone**
- Target Platform: `Windows`
- Architecture: `x86_64`

Click **Switch Platform** if not already selected.

### Step 2 — Player Settings

Click **Player Settings** in Build Settings:

| Setting | Recommended Value |
|---------|------------------|
| Company Name | Your name or studio name |
| Product Name | `DarkSea: Survival` |
| Version | `1.0.0` |
| Default Screen Width | `1920` |
| Default Screen Height | `1080` |
| Fullscreen Mode | `Fullscreen Window` |
| Run In Background | `OFF` |
| Scripting Backend | `Mono` (faster builds) or `IL2CPP` (smaller output) |

### Step 3 — Build

1. In Build Settings, click **Build** (not "Build and Run")
2. Choose an output folder (e.g., `Builds/Windows/`)
3. Unity will compile and package everything — takes 2–10 minutes

**Output files:**
```
Builds/Windows/
├── DarkSea Survival.exe        ← Launch this
├── DarkSea Survival_Data/      ← Required data folder
├── MonoBleedingEdge/           ← Mono runtime (if using Mono backend)
└── UnityCrashHandler64.exe     ← Auto-included by Unity
```

> The `_Data` folder must always be in the same directory as the `.exe`. Do not move the `.exe` without also moving the entire output folder.

### Step 4 — Test the Build

Run `DarkSea Survival.exe` and verify:
- [ ] Login screen appears
- [ ] Register a new account
- [ ] Login with that account
- [ ] Main Menu shows player name
- [ ] Level 1 loads via transition
- [ ] Enemy moves and attacks
- [ ] Oxygen depletes
- [ ] Sonar fires
- [ ] Tank pickup refills oxygen
- [ ] Exit gate completes the level
- [ ] Win screen shows score
- [ ] "Next Level" loads Level 2
- [ ] Level 2 tank requirement works
- [ ] Level 3 Hunt enemy plays audio lures
- [ ] Game Over screen restarts correctly (enemies not frozen)

---

## Troubleshooting Common Issues

### Enemies not moving
- **Cause:** NavMesh not baked OR `AI Navigation` package not installed
- **Fix:** Bake NavMesh in the scene (see NavMesh Setup above)

### Text appears as pink/magenta boxes
- **Cause:** TextMeshPro Essentials not imported
- **Fix:** **Window → TextMeshPro → Import TMP Essential Resources**

### Screen is black in play mode
- **Cause:** URP asset not assigned OR camera has wrong renderer
- **Fix:** Check **Edit → Project Settings → Graphics → Scriptable Render Pipeline Settings**

### Compiler errors on first open
- **Cause:** Missing packages (TextMeshPro or AI Navigation)
- **Fix:** Install missing packages via Package Manager, then Unity will recompile

### Login says "Account not found" after registering
- **Cause:** Two auth systems with different key formats are both active in scenes
- **Fix:** Ensure only `AuthUI.cs` scene (using `PREF_USER_` prefix) is in Build Settings — not the legacy `RegisterScene`/`LoginScene` pair

### Enemy audio continues after game over
- **Cause:** `PlayOneShot` audio cannot be stopped (this was fixed — ensure `EnemyAI_Final.cs` uses `audioSource.Play()` in `PlayLure()`, NOT `PlayOneShot`)
- **Fix:** Verify `PlayLure()` method uses `audioSource.clip = clip; audioSource.Play();`

### Enemies frozen after restarting from Game Over
- **Cause:** `OxygenSystem.IsGameOver` static bool not reset before scene reload
- **Fix:** Confirm `GameOverUI.cs` has `OxygenSystem.IsGameOver = false;` in both `OnRestart()` and `OnMenu()` methods

### Tank counter shows wrong number on Level 2 restart
- **Cause:** `TankCollector.Collected` was reset in `LevelExit.Start()` — fixed to `Awake()`
- **Fix:** Confirm `LevelExit.cs` resets `TankCollector.Collected = 0` in `Awake()`, not `Start()`

### Level 2/3 objective text shows wrong message
- **Cause:** `GameHUDExtras.cs` used `Contains("Level2")` instead of `Contains("Level_2")`
- **Fix:** Confirm the scene name check uses underscore: `Contains("Level_2")`

---

## Quick Reference — Key Scene Objects

For each gameplay scene, verify these GameObjects exist and are correctly configured:

| GameObject | Required Components | Notes |
|-----------|-------------------|-------|
| `Player` | PlayerMovementFinal, OxygenSystem, Rigidbody | Tag must be "Player" |
| `Enemy` | EnemyAI_Final, NavMeshAgent, AudioSource | Tag must be "Enemy"; baseOffset=0.52 |
| `ExitGate` | LevelExit, LevelExitEffect, BoxCollider (IsTrigger) | levelNumber must match scene |
| `GameOverUI` | GameOverUI, UIDocument | GameOverPanel.uxml assigned |
| `WinPanelUI` | WinPanelUI, UIDocument | WinPanel.uxml assigned |
| `PauseMenu` | PauseMenuUI, UIDocument | PausePanel.uxml assigned |
| `NotificationUI` | NotificationUI, UIDocument | NotificationPanel.uxml assigned |
| `AudioManager` | AudioManager, AudioSource ×4 | DontDestroyOnLoad — one instance only |

---

*For developer architecture details, see `Developer_Guide.md`. For gameplay instructions, see `Player_Guide.md`.*
