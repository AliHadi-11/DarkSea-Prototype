# GAME ANALYSIS REPORT
## DarkSea: Survival
### Prepared By: Senior Game Design Consultant
### Classification: Publisher / Investor Presentation
### Date: June 2026

---

> **DISCLAIMER:** This report is based on full source code access, codebase review, and white-box QA testing. Where assumptions are made due to absence of live player data or analytics, they are explicitly marked as **[ASSUMPTION]**.

---

## SECTION 1 — EXECUTIVE SUMMARY

### What Is DarkSea: Survival?

DarkSea: Survival is a **first-person, single-player, underwater survival horror game** developed in Unity 6 for the PC (Windows) platform. The player assumes the role of a stranded underwater diver navigating procedurally-lit maze environments while managing a continuously depleting oxygen supply, evading or eliminating hostile AI entities, and racing to reach the level exit. The game spans three progressively difficult levels with distinct enemy behavior archetypes and a mission-based progression structure.

**Genre:** Survival Horror / First-Person Puzzle Exploration
**Platform:** PC (Windows)
**Engine:** Unity 6, Universal Render Pipeline (URP)
**Target Audience:** Ages 14–30, fans of survival horror (Alien: Isolation, Subnautica, SOMA), players who enjoy tension-based resource management gameplay
**Development Status:** Feature-Complete Prototype — Ready for playtesting and pre-production evaluation

### Overall Verdict

DarkSea: Survival is a **technically competent and mechanically sound prototype** that demonstrates a cohesive game design vision. The oxygen-as-core-resource mechanic is well-executed and creates genuine tension. However, as a prototype, content depth (3 levels), absence of custom art assets, and limited replayability currently position it below commercial release threshold. With focused post-prototype investment in content and presentation, this title has credible market potential in the indie survival horror segment.

> ### Overall Rating: **7.0 / 10**
> *Solid foundation. Strong mechanics. Needs content and art investment for commercial viability.*

---

## SECTION 2 — CORE GAMEPLAY ANALYSIS

### 2.1 Core Loop Breakdown

```
[Register / Login]
       ↓
[Main Menu] → Difficulty Selection → Level Select
       ↓
[Enter Level] → Navigate Maze (WASD + Mouse)
       ↓
[Manage Oxygen] ← drains continuously, enemy attacks drain further
       ↓
[Use Sonar] → detect enemies / navigate walls / kill at close range
       ↓
[Complete Objective] → Level 1 & 3: Reach Exit | Level 2: Collect 3 Tanks → Reach Exit
       ↓
[Score Screen] → Oxygen × 10 + Time Bonus + Tanks × 50
       ↓
[Next Level Unlock] → Repeat with higher difficulty
```

The loop is **tight and intentional**. Every system feeds back into the core tension: oxygen drains, enemies accelerate the drain, sonar is both navigation and combat, and the exit is the only relief. This is a well-designed pressure loop for the survival horror genre.

---

### 2.2 Mechanics — Strengths

**Oxygen as Depletion Currency (Strength: High)**
Unlike conventional health systems, oxygen creates *passive* tension — the player is losing even while standing still. This prevents the "safe corner camping" behavior common in horror games and forces constant movement. The 5% per enemy hit cost is well-calibrated: painful enough to matter, not so punishing as to feel unfair.

**Sonar as Dual-Purpose Tool (Strength: High)**
Using the sonar ping for both spatial navigation (wall distance detection) and combat (close-range enemy elimination) is an elegant design. It creates a genuine risk-reward decision: fire sonar to see the path, but potentially reveal enemy positions. This single mechanic adds meaningful depth without complexity overhead.

**Three Enemy Behavior Modes (Strength: Medium-High)**
Using a single `EnemyAI_Final` script with a Mode enum (Passive/Chase/Hunt) to produce three distinct gameplay feelings is technically efficient and design-smart. Level 1's chasing enemy, Level 2's patrol-only guard, and Level 3's voice-luring Mimic each require different player strategies, preventing the game from feeling repetitive across its three levels.

**Difficulty Scaling (Strength: Medium)**
Adjustable enemy speed and oxygen drain multipliers (Easy: 0.7×, Normal: 1.0×, Hard: 1.4×) allow the game to serve both casual and hardcore audiences without content duplication. This is particularly important for a prototype seeking broader audience feedback.

**Submarine Mode (Strength: Medium)**
The F-key submarine toggle providing 1.55× speed is a creative mobility tool that adds a layer of strategic choice in Levels 2 and 3. It differentiates the later game feel from Level 1.

---

### 2.3 Mechanics — Weaknesses

**Content Depth — 3 Levels Only (Weakness: Critical)**
Three levels with an estimated 10–25 minutes of total play time per run is insufficient for commercial release. **[ASSUMPTION]** A typical indie survival horror title requires 2–4 hours of core content minimum for $5–$10 pricing. This is the single largest gap between current prototype and commercial product.

**No Failure Variety (Weakness: Medium)**
The only lose condition is oxygen depletion (including enemy drain). There are no trap hazards, fall damage, environmental dangers, or time limits with consequences beyond oxygen cost. This limits tension variety — experienced players will quickly learn the single win/loss axis and optimize around it, reducing long-term engagement.

**Enemy AI Predictability (Weakness: Medium)**
Chase mode enemies move in a direct line to the player with no path variation beyond NavMesh routing. Experienced players can learn to "kite" enemies and consistently eliminate them via sonar. Level 3's Hunt mode voice lures add flavor, but experienced players learn to ignore them. A patrol enemy (Level 2) that never pursues the player reduces threat perception in that level.

**No Player Progression Between Runs (Weakness: Medium)**
Scores are saved, achievements unlock, and levels unlock — but there is no between-run progression (unlockable abilities, upgrades, loadouts) that would incentivize repeated play beyond score chasing. **[ASSUMPTION]** Replayability after all achievements are unlocked is low.

**Maze Layout Static Per Scene (Weakness: Low-Medium)**
The maze geometry is fixed per scene. Players who play Level 2 twice will know exactly where all three tanks and the exit are. Procedural maze generation or randomized tank placement would significantly increase replay value.

---

### 2.4 Player Engagement & Replayability

| Metric | Assessment | Score |
|--------|-----------|-------|
| First-run tension | High — oxygen system creates immediate pressure | 8/10 |
| 2nd–5th run interest | Medium — score chasing, achievement hunting | 6/10 |
| Long-term replayability | Low — fixed maps, no progression loop | 4/10 |
| Skill ceiling | Medium — sonar timing, enemy kiting | 6/10 |

**Overall Replayability: 5.5/10**

The game is strongest on the first playthrough when maps are unknown and enemy behaviors are unfamiliar. Replayability drops sharply after content mastery. For a commercial product, roguelite elements or randomized layout generation would be the highest-value investment.

---

## SECTION 3 — TECHNICAL EVALUATION

### 3.1 Engine & Technology

| Component | Technology | Assessment |
|-----------|-----------|-----------|
| Engine | Unity 6 (6000.x LTS) | Excellent choice — LTS ensures long support window |
| Render Pipeline | URP | Appropriate for scope; performant on mid-range hardware |
| UI System | UI Toolkit (UXML/USS) | Modern, scalable — superior to legacy UGUI for this use case |
| Enemy AI | NavMesh + NavMeshAgent | Industry standard; reliable; adequate for current scope |
| Persistence | PlayerPrefs (offline) | Appropriate for prototype; limitation for commercial release |
| Audio | Unity AudioSource (managed) | Functional; no middleware (FMOD/Wwise) — acceptable at this scope |

### 3.2 Code & Architecture Quality

Having reviewed all 49 C# scripts in the codebase, the following assessment applies:

**Strengths:**
- **Auto-spawn pattern** (`[RuntimeInitializeOnLoadMethod]`) is used consistently for lightweight systems — reduces scene setup burden and human error
- **Static singleton pattern** is correctly implemented with `DontDestroyOnLoad` for persistent systems (AudioManager, SceneFader, CameraShake)
- **`PlayerData` key-prefixing system** cleanly isolates per-user data without a database — elegant offline solution
- **`OxygenSystem.IsGameOver` broadcast flag** is a simple, effective signal pattern across enemy instances
- **`EnemyAI_Final` Mode enum** shows good design thinking — one class, multiple behaviors, no code duplication
- **UI Toolkit Painter2D** used for procedural HUD elements (sonar radar, oxygen bar, minimap, submarine icon) — no external UI asset dependency

**Weaknesses:**
- **Two parallel auth systems** coexist (`RegisterManager/LoginTabSystem` in legacy UGUI, `AuthUI.cs` in UI Toolkit) with **incompatible PlayerPrefs key formats**. This is a technical debt that could confuse future developers or produce a broken auth experience if the wrong scene is loaded
- **No scene-level null safety net** — several scripts rely on `FindWithTag("Player")` or `FindFirstObjectByType<>()` which return null silently if the GameObject is missing from the scene. No centralized scene validation
- **`SonarSystem.cs` is dead code** — sonar logic is duplicated in `PlayerMovementFinal.cs`. The standalone script is never called but adds maintenance overhead
- **`LevelManager.cs` is dead code** — win path via `LevelManager.EnemyKilled()` was never wired after the attack system was changed from kill-on-contact to oxygen drain

**Architecture Score: 7.5/10** — Well-structured for a prototype; legacy dead code should be cleaned before further development.

### 3.3 Performance Assessment

**[ASSUMPTION — no profiler data available]**

| Area | Expected Performance | Risk Level |
|------|---------------------|-----------|
| NavMesh pathfinding (5 enemies max) | Very Low CPU overhead | Low |
| Particle systems (UnderwaterAtmosphere, OxygenTankVisual) | Moderate | Low-Medium |
| UI Toolkit Painter2D redraws | Low (cached well) | Low |
| `FindWithTag` / `FindObjectsByType` in some Update() loops | Minor overhead | Low |
| Memory leaks | None identified | Low |

No significant performance risks identified at current scope. The game would run comfortably on a 2015-era mid-range PC.

### 3.4 Known Technical Risks

| Risk | Severity | Status |
|------|----------|--------|
| `IsGameOver` not reset → frozen enemies on restart | Critical | **Fixed** |
| F-key conflict between SpeedBoost and SubmarineToggle | Critical | **Fixed** |
| Scene name `"Level2"` vs `"Level_2"` mismatch in HUD/score | High | **Fixed** |
| `TankCollector.Collected` reset race (Start vs Awake) | Medium | **Fixed** |
| `PauseMenu.Resume()` re-enabling game after death | Medium | **Fixed** |
| Two auth systems with incompatible key formats | Medium | **Identified — not merged** |
| Dead code (`SonarSystem.cs`, `LevelManager.cs`) | Low | **Identified — not deleted** |

**All critical and high bugs have been resolved. Two medium technical debts remain documented.**

---

## SECTION 4 — PRESENTATION & UX

### 4.1 Art Style

**Honest Assessment: This is the prototype's weakest area.**

The game currently uses:
- Procedurally generated VFX (particles, lights, Painter2D HUD) — technically impressive but visually generic
- No custom character art — player body is procedural, enemies are Unity Mutant asset
- No custom environment art — maze walls use a stone/block texture (`medieval_blocks_03_diff_4k`)
- No custom logo, splash screen, or main menu visual identity

The underwater atmosphere achieved through `UnderwaterAtmosphere.cs`, `UnderwaterLightRipple.cs`, and `UnderwaterParticles.cs` creates a functional mood, but a commercial product in the survival horror genre requires custom art direction to stand out. The Subnautica visual language (bioluminescent deep ocean) sets a high bar that procedural particles alone cannot meet.

**Art Score: 5.5/10** — Functional but generic. Needs dedicated art pass for commercial release.

### 4.2 UI / UX

**Positive:**
- UI Toolkit implementation is clean and modern
- Notification card system (slide-in, color-coded) is well-designed and informative
- Sonar radar, minimap, and oxygen bar are clearly readable
- Forget Password feature is thoughtfully included for the offline auth system
- HUD extras (timer, enemy count, objective label) add meaningful contextual information

**Negative:**
- No tutorial or guided first-level instruction — players are dropped into the maze with no contextual explanation of sonar, oxygen management, or submarine mode **[HIGH PRIORITY FIX]**
- Auth screen (UI Toolkit) and some game screens (legacy UGUI elements if present) may have visual inconsistency
- Tab-cycling in auth fields works but has no visual "active field" highlight beyond cursor position
- No accessibility options (subtitles, colorblind mode, remappable keys)

**UX Score: 6.5/10** — Good information architecture; onboarding gap is the biggest UX risk.

### 4.3 Audio

- AudioManager correctly manages 4 audio channels (music, ambience, breathing, SFX)
- Breathing audio dynamically scales with oxygen percentage — effective tension tool
- Enemy approach triggers audio alert — correct design
- Level 3 Mimic voice lures (if audio clips are assigned) add horror flavor
- No procedural or dynamic music system — static looping tracks **[ASSUMPTION]**
- No spatial audio (3D audio positioning) for enemy footsteps or ambient sounds

**Audio Score: 6.0/10** — Functional foundation. Spatial audio and dynamic music would significantly improve atmosphere.

### 4.4 First-Time User Experience (Onboarding)

**This is the most significant UX gap.**

A first-time player launching DarkSea: Survival must:
1. Register an account (no hint that they must do this first)
2. Log in
3. Enter the game
4. Figure out that SPACE fires sonar
5. Figure out that sonar at close range kills enemies
6. Figure out that G is boost and F is submarine
7. Discover the tank requirement in Level 2 by bumping into the locked gate

None of these mechanics are explained in-game beyond the `StoryIntroUI.cs` typewriter narrative (which focuses on story, not controls). The game assumes the player has read a manual.

For an indie title targeting casual-to-mid players, this is a significant barrier.

**Onboarding Score: 4.5/10** — Needs a tutorial level or contextual tooltips before commercial release.

---

## SECTION 5 — MARKET & COMPETITIVE ANALYSIS

### 5.1 Genre Landscape

The survival horror / atmospheric exploration genre is crowded but resilient:

| Competitor | Platform | Price | Key Differentiator |
|-----------|---------|-------|-------------------|
| Subnautica | PC/Console | $29.99 | Open world, crafting, deep narrative |
| SOMA | PC/Console | $29.99 | Philosophical narrative, no combat |
| Alien: Isolation | PC/Console | $39.99 | Cat-and-mouse AI, licensed IP |
| Iron Lung | PC | $5.99 | Minimalist, single mechanic, short |
| Dredge | PC/Console | $24.99 | Fishing + Lovecraftian horror, relaxed pacing |
| **DarkSea: Survival** | **PC** | **[TBD]** | **Offline, score-based, sonar mechanic** |

### 5.2 Competitive Positioning

DarkSea occupies a niche closest to **Iron Lung** — short, intense, mechanically focused, low asset budget. This is the most realistic competitive tier at current scope.

At 3 levels (~15–25 min playtime), the game cannot compete directly with Subnautica or Alien: Isolation. However, the score-based competitive loop and three difficulty tiers create a replay proposition that Iron Lung lacks.

### 5.3 Unique Selling Points (USPs)

| USP | Strength | Notes |
|-----|---------|-------|
| Oxygen-as-only-resource tension | High | Genuinely novel framing compared to health-bar competitors |
| Sonar dual-purpose mechanic | High | Navigation + combat in one button — elegant |
| Zero server dependency | Medium | Differentiator for privacy-conscious players; niche appeal |
| Offline auth system | Low-Medium | Feature parity with online games, but no competitive advantage |
| Submarine toggle mode | Medium | Adds tactical mobility option uncommon in the genre |

### 5.4 Target Audience Segments

| Segment | Fit | Notes |
|---------|-----|-------|
| Survival horror fans | High | Core audience — oxygen tension, enemy AI align well |
| Score-chasers / speedrunners | Medium | Timer + score system supports this, but 3 levels limits depth |
| Casual horror players | Low-Medium | Needs tutorial before this segment is accessible |
| Student / portfolio evaluators | High | Technical quality is strong for academic context |

---

## SECTION 6 — MONETIZATION & BUSINESS POTENTIAL

### 6.1 Current Revenue Model

None. This is a prototype with no monetization layer.

### 6.2 Viable Revenue Models

| Model | Effort | Revenue Potential | Recommendation |
|-------|--------|-----------------|----------------|
| **Premium (one-time purchase)** | Low | $2.99–$5.99 at current scope; $9.99–$14.99 with full content | **Recommended for indie release** |
| **Free + Level Pack DLC** | Medium | $0 base + $1.99–$3.99 per level pack | Good for player acquisition |
| **Steam Early Access** | Medium | Community building while content is added | Viable if roadmap is credible |
| **Subscription (Xbox Game Pass / PS Plus)** | High | Requires port + publisher relationship | Long-term goal only |
| **Mobile Port** | Medium-High | $0.99–$2.99; needs touch controls + UI scaling | UI Toolkit is mobile-compatible; feasible |
| **Ads / F2P** | Not Recommended | Incompatible with horror atmosphere | Avoid |

### 6.3 Cost Analysis

| Category | Cost | Notes |
|----------|------|-------|
| Unity License | $0 (Personal) or ~$2,040/yr (Pro) | Personal is free under $100k revenue |
| Server / Backend | $70.00 (one-time setup estimate) | Currently offline; future online features |
| Third-Party Assets | $50.00 (estimate) | Current build is mostly procedural |
| Hosting / Distribution | $100 (Steam Direct fee, one-time) | Required for Steam release |
| Marketing (minimum viable) | $500–$2,000 | Trailer, social media, press kit |
| Art Contractor (recommended) | $1,500–$5,000 | Custom environment + character art pass |
| **Total to Indie Release** | **~$2,220–$7,260** | Assuming solo developer, Personal license |

### 6.4 Revenue Projection (Conservative Estimates)

**[ASSUMPTION — based on comparable indie titles on Steam]**

| Scenario | Units Sold | Price | Revenue |
|---------|-----------|-------|---------|
| Minimal (niche audience) | 200–500 | $4.99 | $1,000–$2,500 |
| Moderate (positive reviews) | 1,000–3,000 | $4.99 | $5,000–$15,000 |
| Strong (content expansion) | 5,000–15,000 | $9.99 | $50,000–$150,000 |

Steam takes 30% revenue share. The strong scenario requires content expansion to 2+ hours of gameplay minimum.

---

## SECTION 7 — RISK ASSESSMENT

### 7.1 Technical Risks

| Risk | Probability | Impact | Mitigation |
|------|------------|--------|-----------|
| Dual auth system causing player data confusion | Medium | High | Remove legacy `RegisterManager/LoginTabSystem` entirely; standardize on `AuthUI.cs` |
| NavMesh breaks after scene geometry change | Medium | Medium | Re-bake NavMesh after any level edit; document as required step |
| Unity 6 breaking changes in future LTS updates | Low | Medium | Pin Unity version; test before updating |
| Performance degradation with more enemies/particles | Low | Low | Profiler test with 10+ enemies before adding content |
| PlayerPrefs data loss on machine format | Medium | Medium | Add export/import save file feature before commercial release |

### 7.2 Market Risks

| Risk | Probability | Impact | Mitigation |
|------|------------|--------|-----------|
| Oversaturation of indie horror on Steam | High | Medium | Differentiate via sonar mechanic marketing + score-based gameplay |
| Players bounce due to no tutorial | High | High | Implement tutorial level before release — this is a known gap |
| 15-minute game perceived as too short | High | High | Content expansion is pre-requisite for commercial release |
| Underwater horror genre fatigue | Medium | Medium | Subnautica/Iron Lung success shows demand remains; positioning matters |

### 7.3 Design Risks

| Risk | Probability | Impact | Mitigation |
|------|------------|--------|-----------|
| Fixed maze = zero replayability after 2–3 runs | High | High | Randomize tank positions (Level 2) and add procedural maze option |
| Single enemy type (different modes) feels shallow | Medium | Medium | Add 1–2 visually distinct enemy variants with unique abilities |
| Sonar kill makes combat trivial once learned | Medium | Medium | Add enemies with sonar resistance or counter-ping ability |

---

## SECTION 8 — RECOMMENDATIONS

### 8.1 Short-Term Fixes (Pre-Release / Quick Wins)

| Priority | Action | Effort | Impact |
|----------|--------|--------|--------|
| HIGH | Add in-game control tutorial (first 30 seconds of Level 1) | 1–2 days | Very High |
| HIGH | Remove dead code (`SonarSystem.cs`, `LevelManager.cs`, `MenuController.cs`) | 2 hours | Medium (cleanliness) |
| HIGH | Consolidate auth system — delete `RegisterManager.cs` + `LoginTabSystem.cs`, use `AuthUI.cs` only | 1 day | High (reliability) |
| HIGH | Add "Press SPACE for Sonar" on-screen prompt in Level 1 | 2 hours | High |
| MEDIUM | Randomize CollectibleTank positions in Level 2 (select from preset waypoints) | 1 day | Medium (replayability) |
| MEDIUM | Add keyboard-remapping screen in settings | 2–3 days | Medium (accessibility) |
| LOW | Create custom game icon and splash screen | 4 hours | Medium (first impression) |
| LOW | Steam store page preparation (description, screenshots, trailer) | 3–5 days | High (marketing) |

### 8.2 Long-Term Roadmap

| Phase | Timeline | Features | Goal |
|-------|----------|----------|------|
| **v1.1 — Content** | 4–8 weeks | Add Level 4 & 5; new enemy variant (ambush type); randomized map sections | 45–60 min playtime |
| **v1.2 — Polish** | 2–4 weeks | Custom environment art pass; 3D spatial audio; dynamic music system | Commercial-grade presentation |
| **v1.3 — Replayability** | 3–5 weeks | Daily challenge mode; global leaderboard (optional online); unlock system (skins/upgrades) | Long-term retention |
| **v2.0 — Commercial** | 3–6 months | Full 2-hour campaign; Steam release; mobile port (Android/iOS) | Revenue generation |

### 8.3 Priority Ranking Summary

```
CRITICAL (Do Before Any Release):
  1. Tutorial / onboarding for new players
  2. Auth system consolidation (remove legacy)
  3. Content expansion to minimum 45 minutes

HIGH (Do Before Steam Release):
  4. Custom art pass (environment + splash)
  5. Randomized/procedural level elements
  6. SaveFile export/import (protect player data)

MEDIUM (Post-Launch v1.x):
  7. Spatial audio + dynamic music
  8. Additional enemy variants
  9. Daily challenge / leaderboard

LOW (v2.0+):
  10. Mobile port
  11. Co-op multiplayer
  12. Level editor
```

---

## SECTION 9 — FINAL SCORE CARD

| Category | Score /10 | Comments |
|----------|-----------|---------|
| **Core Concept & Vision** | 8.0 | Oxygen + sonar loop is tight, original, and well-executed |
| **Gameplay Mechanics** | 7.5 | Sonar dual-use and 3 enemy modes are strengths; no failure variety |
| **Level Design** | 6.0 | 3 levels is thin; fixed layouts kill replayability quickly |
| **Enemy AI Quality** | 7.0 | 3 distinct modes work well; predictable after first run |
| **Technical Architecture** | 7.5 | Clean, modern stack; some dead code and dual-auth debt |
| **Performance & Stability** | 8.5 | All known bugs fixed; no memory leaks; solid for scope |
| **UI / UX Design** | 6.5 | Clean HUD; zero onboarding is a serious gap |
| **Art & Presentation** | 5.5 | Procedural VFX is clever but not commercially competitive |
| **Audio** | 6.0 | Functional; lacks spatial audio and dynamic music |
| **Replayability** | 5.5 | Fixed maps, no progression loop; score-chasing has limited ceiling |
| **Market Potential** | 6.5 | Real niche exists; needs content + polish to access it |
| **Monetization Readiness** | 5.0 | Zero infrastructure; needs content expansion before any pricing |
| | | |
| **OVERALL** | **7.0 / 10** | Strong prototype. Needs content + art + onboarding for market. |

---

## CONSULTANT CONCLUSION

DarkSea: Survival is **an above-average game prototype** that successfully demonstrates the core mechanic loop, implements a full feature set (auth, scoring, achievements, VFX, audio, AI), and passes technical QA. For a prototype, this is genuinely impressive work.

The path to a commercial product is clear and achievable:

1. **Content first** — 3 levels is a demo, not a game. Minimum 5–7 levels for pricing at $4.99+.
2. **Tutorial second** — no player should ever be confused about SPACE=Sonar or the oxygen drain. This is a day-one fix.
3. **Art third** — the mechanics earn the player's attention; custom art will hold it.

The unique sonar mechanic, the oxygen tension loop, and the clean offline architecture give DarkSea: Survival a **credible foundation** that many prototypes lack. The question is not whether this game *can* be commercially viable — it can — but whether the development investment will be made to close the gap between current prototype and market-ready product.

**Recommendation: Greenlight for continued development. Target Steam Early Access at v1.1 (5+ levels, tutorial, basic art pass).**

---

*Report prepared based on full source code access (49 scripts), white-box QA testing, and codebase architecture review.*
*All financial projections and audience estimates are assumptions based on comparable indie market data and are not guarantees of performance.*
