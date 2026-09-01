# Wwise Integration Notes — Dadiu-SuperMario

Course-specific documentation for this example project. The original upstream
`README.md` (controls, credits, screenshots) is left untouched — this file
covers the Unity/Wwise structure and the state of the audio integration, for
students and instructors working on this project as a Wwise exercise.

## Version mismatch — read this first

This project is **not** on the same versions as the main WAG course project:

| | Dadiu-SuperMario | Main course (WAG) |
|---|---|---|
| Unity | `2021.3.40f1` | `6000.2.12f1` |
| Wwise | `2023.1.6` build `8555` | `2025.1.3` build `9039` |
| Wwise Unity Integration | bundle `2023.1.6.3160`, version 19 | bundle `2025.1.3.3970`, version 21 |

You need a matching Unity **2021.3.40f1** install and a matching Wwise
**2023.1.6** Authoring install to open this project as-is. Opening the
`.wproj` in a newer Wwise will trigger a project upgrade — don't do that
casually if you intend to keep working across both this project and the WAG
project, since it's a one-way conversion. Treat this as its own sandbox
project, not something to open side-by-side with the same Wwise Authoring
window as WAG.

## Unity structure

- `Assets/Scenes/` — flow is `Main Menu` → `Level Start Screen` → one of
  `World 1-1`, `World 1-1 - Underground`, `World 1-2`, `World 1-2 - Castle Cut`,
  `World 1-2 - Underground`, `World 1-3`, `World 1-4` → `Game Over Screen` /
  `Time Up Screen`.
- `Assets/Scripts/GameStateManager.cs` — a `DontDestroyOnLoad` singleton
  created the first time a scene containing one runs (in practice, `Main
  Menu`). Carries lives/coins/score/timer/spawn-point across scene loads.
  **This is why level scenes used to fail when played directly**: pressing
  Play on `World 1-1` without going through `Main Menu` left no
  `GameStateManager` instance, and every script that read game state called
  `FindObjectOfType<GameStateManager>()` with no null check — instant
  `NullReferenceException`. `GameStateManager.GetOrCreate()` now creates one
  with new-game defaults (3 lives, 400 time, spawn point 0) and logs a warning
  if none was found; `LevelManager`, `LevelStartScreen`, `TimeUpScreen` and
  `GameOverScreen` all go through it, so any scene can be opened and tested on
  its own. The screens also stop assuming a `World x-y` scene name
  (`GameStateManager.WorldLabel()`), which is what used to throw an
  `IndexOutOfRangeException` on `Test Scene`. Entering through `Main Menu` is
  still the normal path — that's what carries score and lives between levels.
- `Assets/Scripts/LevelManager.cs` — per-level god object: HUD, timer,
  scoring, pause/unpause, respawn, powerup/powerdown state, **and both the
  legacy Unity audio system and the Wwise integration side by side**.
- `Assets/Scripts/WwiseBankLoader.cs` — loads `BNK_Main` at the start of every
  play session, so audio works no matter which scene you press Play on. Not
  upstream WAG or upstream Mario; added for this course.
- `Assets/Sounds/` — legacy audio source of truth: 18 music `.mp3`s
  (`01-main-theme-overworld.mp3` etc.) and 17 SFX `.wav`s (`smb_*.wav`),
  played via `AudioSource`/`AudioClip`.
- `Assets/Wwise/` — Wwise Unity Integration (SDK 2023.1.6), installed but
  only wired into two scripts (see below).

## Wwise project structure (`Dadiu-SuperMario_WwiseProject/`)

| Work unit / folder | Status |
|---|---|
| `Interactive Music Hierarchy/Default Work Unit.wwu` | **Built out.** Per-level `MusicSwitchContainer`s driven by a `Levels` state group (`MUS_Levels_Sw` → `MUS_Level101_Sw`, `MUS_Level102_Sw_01`, ...), a playlist container for World 1-1 with intro/loop segments (`MUS_101_Intro`, `MUS_101_A`, `MUS_101_B1`, `MUS_101_C`, `MUS_101_B2`), and Mario alive/dead segment pairs per level. This is a genuine interactive-music setup, not a stub. |
| `Events/Music.wwu` | `MUS_PlayMainPlaylist` (top-level "start the music state machine" event — posted once from `LevelManager.Start()` via `musicSource`), `MUS_Level101`, `MUS_Level102`. |
| `Events/MarioStates.wwu` | `EVT_MarioAlive`, `EVT_MarioDead` — **authored but never posted from any script.** |
| `Events/Debug.wwu` + `Actor-Mixer Hierarchy/Debug` | Placeholder synth test tones (`DB_Synth_*`) used to sanity-check the signal chain. Not game content. |
| `Actor-Mixer Hierarchy` (everything else) | **Empty.** None of the 17 gameplay SFX `.wav`s in `Assets/Sounds/` have been imported as Wwise Originals or turned into Sound objects/Events. |
| `States/` | `MarioState` (None/Small/Large/Star/Dead), `MarioLives` (00–03), `Levels` (None/101/102/103/104), `DayNight` (None/Day/Night — unused by any script; likely copied in as scaffolding, not yet load-bearing). |
| `Game Parameters/` | `RTPC_TimeLeft`, `RTPC_MarioSpeed` (both driven from code), `TimeOfDay` (unused). |
| `SoundBanks/` | Single bank, `BNK_Main`. |

## C# ↔ Wwise wiring

The legacy Unity audio system has been removed from the game code. There are
no `AudioSource`s or `AudioClip`s left in `Assets/Scripts/` — every sound is
an `AK.Wwise.Event` posted from code and bound in the Inspector with the
Wwise Picker.

`LevelManager.cs` owns the audio surface (34 Wwise-type fields, all of them
actually used); `Mario.cs` adds `RTPC_MarioSpeed` and `InAirSound`;
`GameOverScreen.cs` posts `WwGameOverMusic`.

`LevelManager`'s Event fields dropped their old `Ww` prefix — `WwcoinSound` is
now `coinSound`, `WwMusicSource` is `musicSource`, and so on — so the Inspector
reads "Coin Sound" instead of "Ww Coin Sound". The `[Header]` labels already say
these are Wwise fields, and the type shown next to each one is `AK.Wwise.Event`.
The pre-Wwise version of the game had `AudioClip` fields under exactly these
names, and they were still sitting in the Level Starter prefab and in six scene
files as stale serialized data; they had to be stripped in the same pass, or the
renamed Event fields would have deserialized an `AudioClip` reference and come up
empty. `RTPC_` and `ST_` keep their prefixes, since those match the Wwise object
names. `GameOverScreen.WwGameOverMusic` was left alone.

**A note on silence.** `AK.Wwise.Event.Post()` checks `IsValid()` first, so an
unbound (None) Event is a safe no-op rather than a crash. That means the game
runs *silently* until the Events below are authored in Wwise and picked in
the Inspector — "no sound" here almost always means an unpicked Event or an
ungenerated SoundBank, not a bug in the code.

### Playing a scene on its own

Any scene can be opened and played directly — `Test Scene`, `World 1-3`,
whatever a student is working on. Two things used to make that fail, and both
are handled now:

- **The SoundBank was never loaded.** `BNK_Main` is loaded by an `AkBank`
  component that only exists in the `Main Menu` scene, so pressing Play on a
  level scene loaded no bank and *every* `Post()` failed with
  `Could not post event (name: MUS_PlayMainPlaylist, ...). Please make sure to
  load or rebuild the appropriate SoundBank.`
  `Assets/Scripts/WwiseBankLoader.cs` now loads the bank once per play session.
  It has to happen very early: components post from `Awake` as well as `Start`
  (the `AkAmbient` on the `Firebar` prefab in `World 1-4` is one), so anything
  that waits for the scene to finish loading is already too late. It subscribes
  to `AkSoundEngineInitialization.Instance.initializationDelegate`, which fires
  at the end of `InitializeSoundEngine()` inside `AkInitializer.OnEnable`
  (script execution order `-100`) — before any `AkBank` (`-75`), `AkEvent` or
  `AkAmbient` (`0`) runs. Bank loads are reference-counted by `AkBankManager`,
  so the Main Menu's `AkBank` component is unaffected. Add any new bank names to
  `WwiseBankLoader.BankNames`.
- **No `GameStateManager`.** See the `GameStateManager.cs` note above:
  `GameStateManager.GetOrCreate()` spawns one with new-game defaults.

`Test Scene` is also enabled in Build Settings, so the die → `Level Start
Screen` → reload cycle can actually load it back. A scene that isn't in Build
Settings can be played in the Editor but cannot be reloaded by name at runtime.

### Timing constants replace `AudioClip.length`

Wwise Events don't expose their length to C# the way an `AudioClip` does, so
the handful of places that timed a coroutine off `clip.length` now use
serialized floats on `LevelManager` (`deadSoundDuration`,
`warningSoundDuration`, `pauseSoundDuration`, `flagpoleSoundDuration`,
`levelCompleteMusicDuration`, `castleCompleteMusicDuration`) and
`GameOverScreen` (`gameOverMusicDuration`). Defaults are measured from the
original files in `Assets/Sounds/`; retune them in the Inspector if the
authored Events come out a different length. The alternative — an
`AK_EndOfEvent` callback — is the more precise option if you'd rather not
maintain numbers by hand.

## Still to author in Wwise

The code side is complete; the Wwise project is not. These Events are posted
by code but **do not exist yet** in `Dadiu-SuperMario_WwiseProject`. Import
the matching `.wav`s from `Assets/Sounds/` into `Originals/SFX/`, build them
into Actor-Mixer objects, and create one Event each:

| Suggested work unit | Events to create |
|---|---|
| `Events/Player.wwu` | jump small, jump super, stomp, kick, fireball, powerup, powerup appears, pipe/powerdown, dead, 1-up |
| `Events/Enemies.wwu` | bowser falls, bowser fire |
| `Events/Objects.wwu` | coin, break block, bump, flagpole |
| `Events/UI.wwu` | pause (posted on both pause and unpause), hurry-up warning |
| `Events/Music.wwu` (extend) | starman, starman hurry, level complete, castle complete, game over |

Also needed: two volume Game Parameters (`RTPC_MusicVolume`,
`RTPC_SoundVolume`, authored 0–100) — the Main Menu volume sliders still
write the `musicVolume`/`soundVolume` PlayerPrefs, and `LevelManager` and
`GameOverScreen` now push those into these RTPCs instead of into
`AudioSource.volume`. Without them the sliders do nothing.

Suggested Actor-Mixer layout: one mixer per domain (`MX_Player`,
`MX_Enemies`, `MX_Objects`, `MX_UI`), leaving the existing `MX_Debug` synth
tones isolated as test content. The single `BNK_Main` SoundBank is fine at
this project's scale — there's no need for the Context/Region/DLC bank split
the main WAG project uses, since that split exists to teach runtime bank
loading.

### Already wired, previously orphaned

`EVT_MarioAlive` / `EVT_MarioDead` are now posted from `LevelManager` (level
start and Mario's death). This supersedes `MusicManagerWwise.cs`, which is left
in place but is no longer needed.

`ST_MarioStar` is now set when the starman powerup is active, and Mario's
size state is kept in sync via `UpdateMarioSizeState()` rather than being
forced to `ST_MarioSmall` on every level load.

### Per-level music

`MUS_PlayMainPlaylist` plays `MUS_MainSwitch`, which switches on `MarioState`
into `MUS_Levels_Sw`, which switches on the **`Levels`** State group:

| `Levels` State | plays |
|---|---|
| `ST_Level_101` | `MUS_Level101_Sw` — the 1-1 playlist (intro/A/B1/C/B2) plus the Mario alive/dead pair |
| `ST_Level_102` | `MUS_Level102_Sw_01` |
| `ST_Level_103` | `MUS_Lvl103` |
| `ST_Level_104` | `MUS_Lvl104` |

So a level picks its music by **setting a State**, not by posting a different
Event. `LevelManager.ST_CurrentLevel` is set in the Inspector per level scene,
and `Start()` calls `SetValue()` on it before the music starts:

| Scene | `ST_CurrentLevel` |
|---|---|
| `World 1-1`, `World 1-1 - Underground`, `Test Scene` | `ST_Level_101` |
| `World 1-2`, `World 1-2 - Underground`, `World 1-2 - Castle Cut` | `ST_Level_102` |
| `World 1-3` | `ST_Level_103` |
| `World 1-4` | `ST_Level_104` |

`Template.unity` is deliberately left on `None` — pick a State when you copy it
into a new level.

**Why `levelMusic` is empty.** `MUS_Level101` and `MUS_Level102` are *SetState*
Events, not play Events — the only two that exist, which is why 1-3 and 1-4 had
nothing to point at and every level ended up playing the 1-1 music. They were
wired into `LevelManager.levelMusic`, which posts a frame later than
`ST_CurrentLevel.SetValue()` and so would overwrite the State. `levelMusic` is
now empty everywhere and `ST_CurrentLevel` is the single source of truth. The
slot is still there: pick an Event if a level should post its own cue on top,
and `ChangeLevelMusicEvent()` will use it. An empty slot is a normal setup here,
so it doesn't warn.

**Who starts the music.** The `Game State Manager` prefab has a `MusicManager`
child whose `AkEvent` posts `MUS_PlayMainPlaylist` on Start. That only ever runs
in the `Main Menu`: **every level scene ships its `Game State Manager` instance
deactivated** (`m_IsActive: 0`), because the live one is meant to come from the
menu via `DontDestroyOnLoad`. `Test Scene` is the odd one out — no override, so
its copy is active.

That means pressing Play on `World 1-3` had nothing posting the music at all.
`LevelManager.Start()` now posts `musicSource` (`MUS_PlayMainPlaylist`) itself,
but only when `GameStateManager.musicStarted` is false — the flag is set in
`Awake()` for any GameStateManager that has a `MusicManager` child, so arriving
from the Main Menu does not stack a second copy of `MUS_MainSwitch` on top of the
one already playing.

It is posted on `GameStateManager.MusicGameObject`, not on the Level Manager's own
game object. A Wwise Event is scoped to the game object it was posted on: it stops
when that object is destroyed, so posting it on a per-scene object would kill the
music at the next scene load, and `PauseMusic()` / `StopMusic()` only reach it if
they target the same object. `MusicGameObject` resolves to the `MusicManager`
child when there is one (the Main Menu flow, where that child's `AkEvent` did the
posting) and to the manager itself otherwise (direct play). Posted on the
persistent object the music survives scene loads, keeps playing across levels, and
Wwise transitions between them when the State changes — which is the point of an
interactive music setup.

### Things Wwise should own, not code

Two behaviours are still done in C# because that's what the original game
did, but both are more idiomatic in Wwise:

- **Pausing music for a stinger** (hurry-up warning, flagpole) is a
  `PauseMusicPlaySoundEvent()` coroutine. A ducking bus or a State would be
  the Wwise-native way.
- **SFX no longer pause with the game.** The old code called
  `soundSource.Pause()`; the equivalent belongs in Wwise as a pause State on
  the SFX bus, not in the pause coroutine.
