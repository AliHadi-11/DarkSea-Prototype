# DarkSea: Survival — Player Guide

> **Genre:** First-Person Underwater Survival Horror
> **Platform:** PC (Windows)
> **Difficulty:** Adjustable (Easy / Normal / Hard)

---

## Table of Contents

1. [Story & Setting](#story--setting)
2. [Controls](#controls)
3. [Core Mechanics](#core-mechanics)
4. [Level Breakdown](#level-breakdown)
5. [Win & Lose Conditions](#win--lose-conditions)
6. [Tips & Strategies](#tips--strategies)

---

## Story & Setting

You are an underwater diver trapped in a series of flooded maze-like structures deep beneath the ocean. Your oxygen supply is limited and something is hunting you in the dark. Navigate each level, manage your oxygen, and escape before the darkness swallows you whole.

---

## Controls

| Key | Action |
|-----|--------|
| `W` | Move Forward |
| `S` | Move Backward |
| `A` | Strafe Left |
| `D` | Strafe Right |
| `Mouse` | Look / Rotate camera |
| `G` (Hold) | Speed Boost (sprint) |
| `R` | Temporary Sprint Burst (+5 speed for 4 seconds, Level 2 & 3 only) |
| `SPACE` | Fire Sonar Ping — reveals walls & detects enemies |
| `F` | Toggle Submarine Mode (Level 2 & 3 only — faster movement) |
| `ESC` / `P` | Pause / Resume Game |

---

## Core Mechanics

### Oxygen System

Your oxygen percentage is displayed on the HUD at all times. It drains continuously — faster on higher difficulty settings.

| Oxygen Level | Status |
|---|---|
| 100% – 31% | Normal — green bar |
| 30% | WARNING notification appears |
| 20% | CRITICAL — red bar, camera vignette effect activates, warning text flashes |
| 0% | **GAME OVER** — "Out of Oxygen" |

**How to refill oxygen:**
- Pick up glowing **Oxygen Tanks** floating in the maze. Each tank restores **40% oxygen**.
- In Level 2 and 3, the tanks are mission-critical **Collectible Tanks** (restores **30% oxygen**).

**Enemy attacks also drain oxygen:** Each enemy hit costs you **5% oxygen** with a 1.5-second gap between hits. Watch the HUD closely after an encounter.

---

### Sonar System

Press `SPACE` to fire a sonar pulse. This is your most powerful tool:

- **Wall Detection:** The sonar beam bounces off walls and shows you the distance ahead — essential for navigating dark corridors.
- **Enemy Detection:** If an enemy is within sonar range (~20 meters), it gets highlighted and a warning is displayed.
- **Enemy Kill:** If an enemy is within **3.5 meters** when you fire the sonar, the pulse destroys it. Use this to eliminate threats in close quarters.

A visible **sonar beam** (colored line) fires from your position toward the target. A blue beam = wall/scan. Red beam = enemy hit.

---

### Minimap & Radar

A minimap is displayed in the corner of the HUD showing:
- Your current position and facing direction (arrow)
- Enemy positions as red blips
- A sonar sweep ring that expands after each sonar ping

---

### Submarine Mode (Level 2 & 3)

Press `F` to deploy your personal submarine. While in submarine mode:
- Your diver body is hidden and replaced with a procedural submarine model
- Movement speed increases by **1.55×**
- A HUD indicator appears at the bottom-left showing "SUBMARINE" status
- Press `F` again to return to diver mode

The submarine automatically deactivates if your oxygen runs out.

---

### Difficulty Settings

Accessible from the Main Menu settings panel:

| Difficulty | Enemy Speed | Oxygen Drain |
|---|---|---|
| Easy | 0.7× base | 0.7× base |
| Normal | 1.0× base | 1.0× base |
| Hard | 1.4× base | 1.4× base |

---

## Level Breakdown

### Level 1 — DarkSea (The Beginning)

**Objective:** Survive and reach the Exit Gate.

**What to Expect:**
- Single enemy in **Chase mode** — it will pursue you directly once it detects you.
- One Oxygen Tank pickup available in the maze.
- Dark, claustrophobic underwater maze corridors.
- This is the tutorial level — learn the sonar and oxygen mechanics here.

**Enemy Behavior:**
The enemy patrols until it detects you, then enters a direct chase. It will follow you relentlessly. Use sonar at close range to kill it, or simply outrun it to the exit.

**Exit:** Walk into the glowing gate at the end of the maze. The gate pulses with a teal-green light.

**Score Formula:** `(Remaining Oxygen × 10) + Time Bonus + (Tanks × 50)`

---

### Level 2 — The Flooded Lab

**Objective:** Collect **3 Oxygen Tanks** then reach the Exit Gate.

**What to Expect:**
- 3 enemies in **Passive mode** — they patrol fixed routes and do NOT chase you. However, they will attack you if you get within 2.5 meters.
- 3 glowing Collectible Tanks scattered through the maze. You must collect ALL THREE before the exit unlocks.
- The exit gate glows **red** (locked) until all 3 tanks are collected, then turns **green** (unlocked) with a burst effect.
- Tank count is shown in the HUD.

**Strategy:** The enemies are predictable — watch their patrol routes and time your tank collection between their passes. Use Submarine mode (`F`) for speed to collect tanks quickly.

**Exit:** The gate unlocks automatically when 3 tanks are collected. A notification appears: "EXIT UNLOCKED."

---

### Level 3 — The Abyss

**Objective:** Survive the Mimic and reach the Exit Gate.

**What to Expect:**
- Up to 5 enemies in **Hunt mode** — the most aggressive state. They move faster than chase enemies.
- The Mimic enemy plays **distress voice clips** at intervals ("Help me...", "I can hear you...") to lure and confuse you. Do not follow the voices blindly.
- 7 Collectible Tanks available but not required for exit. Collect them to keep your oxygen topped up.
- The maze is longer and more complex.

**Enemy Behavior:**
Hunt-mode enemies are relentless. They move at `huntSpeed` (faster than Level 1 enemies). They periodically play audio lures. Each attack drains 5% oxygen with a 1.5-second cooldown — multiple enemies attacking simultaneously can drain oxygen rapidly.

**Strategy:** Stay mobile. Use sonar to kill enemies at close range. Prioritize collecting tanks to offset oxygen drain from attacks. The exit is navigable without collecting all 7 tanks.

---

## Win & Lose Conditions

### You Win When:
- You walk through the glowing **Exit Gate** while meeting all level requirements.
- A **Win Screen** appears showing your score, remaining oxygen, and time taken.
- Buttons: **Next Level**, **Retry**, **Main Menu**.

### You Lose When:
- Your **oxygen reaches 0%** (Game Over: "Out of Oxygen")
- **Game Over screen** appears with options to **Restart** or go to **Main Menu**.

### Scoring System

| Component | Points |
|---|---|
| Remaining Oxygen | × 10 per % |
| Time Bonus | `max(0, 200 - seconds) × 2` |
| Collectible Tanks | × 50 each |

Your best score per level is saved to your account and shown in the Main Menu scoreboard.

---

## Achievements

| Achievement | Condition |
|---|---|
| **Iron Lungs** | Complete a level with 60%+ oxygen remaining |
| **Speed Run** | Complete a level in under 90 seconds |
| **Tank Master** | Collect all required tanks in Level 2 |
| **Deep Diver** | Complete Level 3 (the final level) |

Achievements are shown as notification pop-ups and saved to your account permanently.

---

## Tips & Strategies

1. **Always watch your oxygen bar.** The vignette effect and red color are your last warning — by that point you have less than 20% left.
2. **Use sonar sparingly in Level 3.** Hunt enemies are faster — you may not always have time to aim before firing.
3. **In Level 2, memorize patrol routes.** The passive enemies don't chase, but they can block corridors.
4. **Submarine mode is fastest.** In Levels 2 and 3, toggle submarine mode for quick tank collection runs.
5. **The Exit Gate glows.** Look for the teal-green light in the distance — it always shows where the exit is.
6. **Multiple hits are dangerous.** If two Hunt enemies are on you simultaneously, you lose 10% oxygen every 1.5 seconds. Prioritize escape over collection.
7. **Speed Boost `R` has a cooldown.** Use it strategically to escape enemy proximity, not for general movement.

---

*Good luck, diver. The abyss is watching.*
