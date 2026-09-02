# Super Mario — Wwise Project Guide

**For sound designers.** This is the project you'll be authoring audio for. It's a
working Super Mario Bros. remake where **every sound comes from Wwise** — there is
no audio left in Unity at all.

Your job is the Wwise project. You should not need to write a single line of C#.

---

## The one thing to understand first

The game code has already been wired to *ask* Wwise for a lot of things. Each of
those hooks is an empty slot in Unity's Inspector, waiting for you to point it at
something you've authored.

**An empty slot is silent, not broken.** The game checks whether a slot has been
filled before it does anything, so nothing crashes and nothing errors. That means
you can author one sound at a time and hear each one switch on as you connect it.

The workflow is always the same:

1. Author the Event / State / Switch / Game Parameter in **Wwise**
2. Generate SoundBanks
3. In **Unity**, select the `Level Manager` object and pick your new object from
   the dropdown in the matching slot
4. Press Play

---

## Setup

You need both of these, matching exactly:

| | Version |
|---|---|
| Unity | `6000.6.0f1` |
| Wwise | `2025.1.10` |

- **Unity project:** open `Examples/Dadiu-SuperMario/` in Unity Hub
- **Wwise project:** `Dadiu-SuperMario_WwiseProject/Dadiu-SuperMario_WwiseProject.wproj`

**Generate SoundBanks in Wwise before you press Play the first time.** Banks are
not stored in git — a fresh clone has none, and without them you get no sound at
all plus a wall of red errors.

### If the Wwise Picker dropdown is empty in Unity

The picker can read either from the Wwise project files or live from Wwise
Authoring over WAAPI. If it's set to live and Wwise isn't running, you get an
empty list. Open **Window → Wwise → Wwise Picker** and switch the dropdown in the
top-left from `WwiseAuthoring` to `FileSystem`. Or just leave Wwise open.

---

## Playing the game

You can press Play on **any scene** — `World 1-1`, `World 1-3`, `Test Scene`,
whatever you're working on. You don't have to start from the Main Menu.

Scenes live in `Assets/Scenes/`:

- `Main Menu` → `Level Start Screen` → a level → `Game Over Screen`
- Levels: `World 1-1`, `World 1-1 - Underground`, `World 1-2`,
  `World 1-2 - Underground`, `World 1-2 - Castle Cut`, `World 1-3`, `World 1-4`
- `Test Scene` is a sandbox — a good place to try things without breaking a level

Controls are in the original `README.md`.

---

## How the music works

This is the most developed part of the project and worth understanding before you
touch it.

The music is **one continuous interactive-music playback** that starts once and
keeps running across scene loads. Levels do **not** stop and start their own
music.

```
MUS_PlayMainPlaylist  (the only "play" Event)
  └─ MUS_MainSwitch          switches on the MarioState State group
      └─ MUS_Levels_Sw       switches on the Levels State group
          ├─ ST_Level_101 → the 1-1 playlist (intro / A / B1 / C / B2)
          ├─ ST_Level_102 → MUS_Level102_Sw_01
          ├─ ST_Level_103 → MUS_Lvl103
          └─ ST_Level_104 → MUS_Lvl104
```

**A level picks its music by setting a State, not by playing a different Event.**
Each level scene has `ST_Current Level` filled in on its Level Manager, and Wwise
transitions to the right music when that State changes.

So if you want to change what World 1-3 sounds like, you edit `MUS_Lvl103` in
Wwise. You don't touch Unity at all.

---

## What the game already tells you

This is your playground. Everything below is already computed and published by
the game — it just needs something in Wwise listening to it.

All of these slots are on the **`Level Manager`** object in each level scene
(open a level, find `Level Manager` in the Hierarchy, look at the Inspector).

### Game Parameters (RTPCs)

| Slot | What it gives you | Ideas |
|---|---|---|
| `RTPC_ Level Progress` | 0–100 across the level, left edge to flagpole | Build intensity as the player nears the end |
| `RTPC_ Stomp Chain` | Consecutive stomps without landing | The classic Mario escalation — pitch each stomp up |
| `RTPC_ Fall Speed` | 0–100, how fast he was falling when he landed | One landing sound that covers a hop and a long drop |
| `RTPC_ Height` | 0–100, how high up he is | Thinner air, more reverb up high |
| `RTPC_ Coins` | 0–99, resets at the 1-up | Coin pitch climbing across a level |
| `RTPC_ Danger Nearby` | Live enemies within 8 units | A tension layer that follows the level, not the clock |
| `RTPC_ Time Left` | The countdown | Tempo, filter, a ticking layer under 100 |
| `RTPC_ Mario Speed` | Current run speed *(on the Mario object)* | Footstep rate, wind |

The last two already existed and nothing in Wwise reads them yet.

### States

| Slot(s) | Suggested group | What it tracks |
|---|---|---|
| `ST_ Environment` | `Environment` | Overworld / Underground / Castle. **Set this per level scene** — it's the obvious one to hang reverb off |
| `ST_ Flow Playing`, `Paused`, `Level Complete`, `Time Up`, `Game Over` | `GameFlow` | Where the player is in the game. A pause State on your buses is much nicer than what the code does today |
| `ST_ Time Normal`, `ST_ Time Hurry` | `TimePressure` | Flips when the clock drops below 100 |
| `ST_ Lives By Count` (4 slots) | `MarioLives` | Lives remaining. This group already exists in the Wwise project and has never been used |

### Switches

| Slot(s) | Suggested group | Where it lives |
|---|---|---|
| `enemy Type` | `EnemyType` | On each **enemy prefab** — Goomba, Koopa, Winged Koopa, Shell, Piranha, Bowser |
| `sw Defeat Stomp / Shell / Fireball / Block / Starman` | `DefeatMethod` | On the Level Manager — how the enemy died |
| `sw Mario Small / Super / Fire` | `MarioSize` | On the Level Manager — applied before the jump sound |
| `surface` | `Surface` | On the **`Sound Material`** component you add to ground, brick, pipe and castle prefabs |

The two enemy Switches are the interesting pair: **one** `enemy Defeat Sound`
Event plus `EnemyType` × `DefeatMethod` covers every way every creature in the
game can die. That's six enemies times five causes behind a single Event.

### Events

`jump Sound`, `skid Sound`, `land Sound`, `enemy Defeat Sound`,
`checkpoint Sound`, `pipe Enter Sound`, `pipe Exit Sound`, `empty Block Sound`

Four of those are moments that have never made a sound in this game: Mario
**skidding** when he turns at speed, **landing**, passing a **checkpoint**, and
bumping a block that's already **empty**.

There are also plenty of Events already wired and playing placeholder synth
tones — coins, jumps, stomps, bumps, block breaks, powerups, the flagpole, the
death jingle. Replacing those with real sounds is the fastest visible win.

---

## Where to start

A sensible order, roughly easiest to most interesting:

1. **Replace the placeholder synth tones.** Coin, jump, stomp, bump, break block.
   Instant payoff, no new concepts.
2. **Environment State + reverb.** Pick an `ST_ Environment` State per level scene, hang an aux send
   off it. The underground echo sells itself.
3. **The enemy Switch matrix.** One Event, `EnemyType` × `DefeatMethod`. This is
   the concept most worth having in your hands.
4. **The stomp chain.** Drive pitch from `RTPC_ Stomp Chain` so each consecutive
   stomp rises. Very little work, very satisfying.
5. **A pause State on your buses**, driven by `ST_ Flow Paused`. Replaces a clumsy
   piece of game code with the Wwise-native approach.
6. **`RTPC_ Level Progress`** driving music intensity toward the flagpole.

---

## When there's no sound

Work down this list — it catches almost everything:

1. **Have you generated SoundBanks** since your last Wwise change?
2. **Is the slot actually filled** in the Inspector, or still `None`?
3. **Do the Wwise and Unity versions match** (2025.1.10 / 6000.6.0f1)?
4. **For music:** is `ST_Current Level` set on this scene's Level Manager?
5. **Did the project compile?** A red error in Unity's Console stops *everything*,
   including audio. Fix the error first.
6. Only then open the Wwise Profiler.

---

## Please don't break these

- **Don't reinstall or update the Wwise Unity Integration.** The project carries a
  small hand-written patch without which it will not compile, and reinstalling
  silently wipes it. If it happens, see `WWISE-INTEGRATION.md`.
- **Don't install the Wwise Addressables package**, even if Wwise offers it. This
  project doesn't use it and it breaks the build.
- **Don't delete the ScriptableObjects folder** (`Assets/Wwise/ScriptableObjects/`).
  Every slot you fill in the Inspector points at a file in there. Deleting it
  empties every slot in the whole project.
- **Coordinate before editing the same Work Unit** as someone else. Wwise Work
  Units are XML files and they merge badly in git.

---

For the technical detail behind any of this — how the wiring works in code, what
each field is driven by, the exact Wwise objects to author — see
`WWISE-INTEGRATION.md` in the same folder.
