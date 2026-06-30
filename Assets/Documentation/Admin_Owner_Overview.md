# DarkSea: Survival — Admin & Owner Overview

> **Document Type:** Executive Summary / Project Overview
> **Target Audience:** Project Managers, Clients, Stakeholders
> **Game Title:** DarkSea: Survival
> **Status:** Prototype — Feature Complete

---

## What Is DarkSea: Survival?

DarkSea: Survival is a **first-person, single-player, underwater survival horror game** built in Unity 6. The player navigates dark, maze-like flooded environments, manages a depleting oxygen supply, avoids or defeats hostile entities, and progresses through three levels of escalating difficulty.

The game runs **100% offline** — no internet connection, no server, no cloud services. All player data is stored locally via Unity's PlayerPrefs system, making it fully self-contained with zero operational cost.

---

## Core Gameplay Loop

```
Login / Register
       ↓
Main Menu (View Scores, Set Difficulty, Select Level)
       ↓
Enter Level → Navigate Maze → Manage Oxygen → Evade/Kill Enemies
       ↓
Collect Mission Items (Level 2) or Survive (Level 1 & 3)
       ↓
Reach Exit Gate → View Score → Unlock Next Level
       ↓
Win Screen (Next / Retry / Menu)
```

The loop is tight and replayable. Each run generates a score based on remaining oxygen, completion speed, and items collected — creating natural incentive to replay and improve.

---

## Feature Summary

### Player Systems
| Feature | Description |
|---------|-------------|
| First-Person Movement | WASD + mouse look, physics-based Rigidbody movement |
| Speed Boost | G-hold for sustained sprint; R-key for 4-second burst |
| Submarine Mode | Toggle (F) diver ↔ submarine; speed ×1.55 in Levels 2 & 3 |
| Sonar System | SPACE fires sonar ping — reveals walls, detects enemies, kills at close range |
| Oxygen Bar | Visual HUD bar with color-coded warning states and vignette effect |

### Gameplay Mechanics
| Feature | Description |
|---------|-------------|
| Oxygen Depletion | Continuous drain; enemy hits cost 5% per attack; refill via pickups |
| Enemy AI (3 types) | Passive patrol, Direct chase, Fast mimic hunt with voice lures |
| Collectible Tanks | Level 2 mission objective — collect 3 to unlock exit |
| Score System | Oxygen × 10 + Time Bonus + Tanks × 50; best score saved per user |
| Achievement System | 5 achievements: Iron Lungs, Speed Run, Tank Master, Deep Diver, First Elim |
| Difficulty Settings | 3 tiers (Easy / Normal / Hard) adjusting enemy speed and oxygen drain rate |

### Presentation
| Feature | Description |
|---------|-------------|
| Underwater Atmosphere | Auto-spawning particle effects, light ripples, color grading |
| Camera Shake | Triggered on enemy attacks, game over, critical oxygen |
| Sonar Beam VFX | Visible colored beam lines showing sonar hits |
| Enemy Dissolve Effect | Flash-white + scale-to-zero death animation |
| Procedural Submarine | Code-generated submarine model (Painter2D) — no external assets |
| Minimap & Radar | Real-time minimap with enemy blips and sonar sweep ring |
| Animated HUD | Oxygen bar pulses at critical levels; radar sweep animates |
| Story Intro | Typewriter-style narrative introduction before Level 1 |

### Account & Persistence
| Feature | Description |
|---------|-------------|
| Offline Auth | Register/Login/Forgot Password — all local, zero server cost |
| Per-User Progress | Each account has separate scores, achievements, and unlock state |
| Score History | Last 5 run results (won/lost, score, level) saved per account |
| Level Unlock | Progressive unlock: Level 2 unlocks after Level 1 completion, etc. |

---

## What Makes DarkSea Unique

1. **Oxygen as the core resource.** Unlike health bars, oxygen creates constant, escalating tension — it drains even when nothing is happening. Every second spent lost in the maze is meaningful.

2. **Sonar as a dual-purpose tool.** The sonar ping is both navigation (wall detection) and offense (enemy kill at close range). This creates interesting risk/reward decisions: fire sonar to see the path ahead, but reveal your position to enemies.

3. **Three distinct enemy behaviors.** A single enemy type (EnemyAI_Final) produces three completely different gameplay experiences through the Mode enum — from the oblivious patrol guard of Level 2 to the fast, voice-luring Mimic of Level 3.

4. **Zero operational cost.** No backend, no analytics, no ads, no network calls. The game runs identically offline, on any machine, forever — without any server maintenance.

5. **Fully procedural UI and VFX.** The HUD, sonar radar, submarine icon, minimap, and vignette are all generated in code using Unity UI Toolkit's Painter2D API. No external UI assets are required.

---

## Technical Scope

| Category | Detail |
|----------|--------|
| Engine | Unity 6 (6000.x LTS) |
| Render Pipeline | Universal Render Pipeline (URP) |
| Platform | PC (Windows .exe) |
| Auth | Offline-only (PlayerPrefs) |
| UI System | Unity UI Toolkit (UXML + USS + Painter2D) |
| AI | Unity NavMesh + NavMeshAgent |
| Total C# Scripts | ~49 scripts |
| Scene Count | 10 scenes (3 gameplay + 4 transition/menu + 3 auth/menu) |
| External Dependencies | TextMeshPro (included in Unity), NuGet folder (unused in builds) |

---

## Project Status

| Area | Status |
|------|--------|
| Core Gameplay Loop | Complete |
| All 3 Levels | Complete |
| Enemy AI (all 3 modes) | Complete |
| Auth System | Complete |
| Score & Achievement System | Complete |
| Win / Lose / Pause UI | Complete |
| Atmospheric VFX | Complete |
| Audio System | Complete |
| QA Testing | Complete (all known bugs fixed) |
| Documentation | Complete |

---

## Future Scalability

The codebase is designed for easy extension:

### Additional Levels
Adding a Level 4 requires:
- Duplicate an existing gameplay scene
- Set player `boostSpeed` in `PlayerMovementFinal.Awake()` (one line)
- Configure enemy `Mode` in the Inspector
- Add scene to Build Settings

### New Enemy Types
`EnemyAI_Final` uses a Mode enum. Adding `Mode.Ambush` or `Mode.Swarm` requires adding one enum value and one switch case — no existing code changes.

### Monetization Options (Future)
| Option | Effort | Notes |
|--------|--------|-------|
| Level Pack DLC | Low | New scenes, plug into existing progression |
| Cosmetic Skins | Medium | SwapMaterial on PlayerBodyVisuals/SubmarineVisuals |
| Leaderboard (Online) | High | Replace PlayerPrefs scores with backend API |
| Mobile Port | Medium | UI Toolkit already responsive; needs touch controls |

### Multiplayer Potential
The current architecture separates player logic from game logic cleanly. NavMesh enemies are self-contained. A co-op mode would require networking the Player transform and OxygenSystem — the rest of the game would function without changes.

---

## Cost Analysis

| Category | Cost |
|----------|------|
| Unity License | Free (Personal/Student) or Unity Pro subscription |
| Server / Backend | $70.00 — fully offline |
| Third-Party Assets | $50.00 — all UI, VFX, and models are procedural |
| Hosting | $0.00 |

The only ongoing cost is the Unity license tier. The game can be distributed as a standalone .exe with no runtime cost.

---

## Summary

DarkSea: Survival is a polished, feature-complete Unity prototype demonstrating a full game development lifecycle — from offline authentication to scored level completion, enemy AI, atmospheric presentation, and a clean code architecture. It is ready for demonstration, portfolio use, or further commercial development.
