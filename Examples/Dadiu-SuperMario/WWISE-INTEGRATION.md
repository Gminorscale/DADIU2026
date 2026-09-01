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
  **This is why level scenes don't work when played directly**: if you press
  Play on `World 1-1` without having gone through `Main Menu` first, there is
  no `GameStateManager` instance yet, and `LevelManager`/`Mario` both call
  `FindObjectOfType<GameStateManager>()` on `Start()` with no null check —
  instant `NullReferenceException`. Always enter play mode from `Main Menu`,
  or add a bootstrap `GameStateManager` to each level scene as a fallback.
- `Assets/Scripts/LevelManager.cs` — per-level god object: HUD, timer,
  scoring, pause/unpause, respawn, powerup/powerdown state, **and both the
  legacy Unity audio system and the Wwise integration side by side**.
- `Assets/Sounds/` — legacy audio source of truth: 18 music `.mp3`s
  (`01-main-theme-overworld.mp3` etc.) and 17 SFX `.wav`s (`smb_*.wav`),
  played via `AudioSource`/`AudioClip`.
- `Assets/Wwise/` — Wwise Unity Integration (SDK 2023.1.6), installed but
  only wired into two scripts (see below).

## Wwise project structure (`Dadiu-SuperMario_WwiseProject/`)

| Work unit / folder | Status |
|---|---|
| `Interactive Music Hierarchy/Default Work Unit.wwu` | **Built out.** Per-level `MusicSwitchContainer`s driven by a `Levels` state group (`MUS_Levels_Sw` → `MUS_Level101_Sw`, `MUS_Level102_Sw_01`, ...), a playlist container for World 1-1 with intro/loop segments (`MUS_101_Intro`, `MUS_101_A`, `MUS_101_B1`, `MUS_101_C`, `MUS_101_B2`), and Mario alive/dead segment pairs per level. This is a genuine interactive-music setup, not a stub. |
| `Events/Music.wwu` | `MUS_PlayMainPlaylist` (top-level "start the music state machine" event — posted once from `LevelManager.Start()` via `WwMusicSource`), `MUS_Level101`, `MUS_Level102`. |
| `Events/MarioStates.wwu` | `EVT_MarioAlive`, `EVT_MarioDead` — **authored but never posted from any script.** |
| `Events/Debug.wwu` + `Actor-Mixer Hierarchy/Debug` | Placeholder synth test tones (`DB_Synth_*`) used to sanity-check the signal chain. Not game content. |
| `Actor-Mixer Hierarchy` (everything else) | **Empty.** None of the 17 gameplay SFX `.wav`s in `Assets/Sounds/` have been imported as Wwise Originals or turned into Sound objects/Events. |
| `States/` | `MarioState` (None/Small/Large/Star/Dead), `MarioLives` (00–03), `Levels` (None/101/102/103/104), `DayNight` (None/Day/Night — unused by any script; likely copied in as scaffolding, not yet load-bearing). |
| `Game Parameters/` | `RTPC_TimeLeft`, `RTPC_MarioSpeed` (both driven from code), `TimeOfDay` (unused). |
| `SoundBanks/` | Single bank, `BNK_Main`. |

## C# ↔ Wwise wiring, as it stands today

Only `LevelManager.cs` (30 Wwise-type fields) and `Mario.cs` (`MarioSpeed`
RTPC, `InAirSound` event) touch Wwise at all.

**Actually posted / driving something real:**
`WwMusicSource` (starts music state machine), `WwLevelMusic` /
`WwLevelMusicHurry`, `WwdeadSound`, `WwpowerupSound`, `WwpipePowerdownSound`,
`WwstompSound`, `WwkickSound`, `WwcoinSound`, `WwoneUpSound`,
`ST_MarioSmall`/`ST_MarioLarge`, `RTPC_TimeLeft`, `Mario.MarioSpeed`,
`Mario.InAirSound`, `WwjumpSmallSound`.

**Declared on `LevelManager` but never posted** (dead fields — and most have
no matching Wwise Event yet either): `WwflagpoleSound`, `WwwarningSound`,
`WwbowserFallSound`, `WwbowserFireSound`, `WwbreakBlockSound`, `WwbumpSound`,
`WwfireballSound`, `WwjumpSuperSound`, `WwcastleCompleteMusic`,
`WwlevelCompleteMusic`, `WwStarmanMusic`, `WwStarmanMusicHurry`.

**Still entirely on the legacy `AudioSource`/`AudioClip` path** — these call
`t_LevelManager.soundSource.PlayOneShot(t_LevelManager.xSound)` and have no
Wwise equivalent posted anywhere: `Bowser.cs`, `Starman.cs`,
`RegularBrickBlock.cs`, `BridgeAxe.cs`, `MarioFireball.cs`, `StaticBlock.cs`,
`OneupMushroom.cs`, `PipeWarpDown.cs`, `PipeWarpSide.cs`,
`_common/PowerupObject.cs`, `_common/CollectibleBlock.cs`. `GameOverScreen.cs`
is 100% legacy (`AudioSource gameOverMusicSource`).

**Orphaned on the Wwise side** (authored, never called from any script):
`EVT_MarioAlive`, `EVT_MarioDead`, and `MusicManagerWwise.cs`
(`SetLevel101State`/`SetLevel102State`, driving the `Levels` state group) —
this component exists as a script but nothing instantiates or calls it.

See the accompanying plans in the project conversation for: (1) rewriting
`LevelManager` to drop the legacy audio system entirely, (2) fixing the
issues above, and (3) the full sound/music structure for finishing the Wwise
integration.
