# CLAUDE.md — DADIU 2026 / Wwise Game Audio Course

Guidance for Claude Code working in this repository.

## What this repository is

This is a **teaching repository for a game audio course**. The thing students
actually author is a **Wwise project**, and the thing they learn is how Wwise and
Unity talk to each other.

The course project is **`Examples/Dadiu-SuperMario/`** — a Unity remake of Super
Mario Bros. whose audio has been moved fully onto Wwise. The C# game code is
upstream open-source sample code; treat it as *scaffolding for audio lessons*
rather than a product codebase.

Audiokinetic's **Wwise Adventure Game** is still in the repo under
`WwiseAdventureGame2025.1.3.421/`, but it is **no longer the course project**.
Leave it alone unless asked about it specifically. Its own conventions and
version pins do not apply to Dadiu-SuperMario.

Assume the audience is **sound designers and audio students**, not professional
Unity programmers. Many will be confident in a DAW and in Wwise Authoring, and
much less confident in Unity, C#, and git.

## Repository layout

| Path | What it is |
|---|---|
| `Examples/Dadiu-SuperMario/` | **The course project.** Unity project at this level (not a subfolder), plus `Dadiu-SuperMario_WwiseProject/` beside `Assets/`. ~1.6 GB. |
| `Examples/Dadiu-SuperMario/WWISE-INTEGRATION.md` | **The detailed doc for this project.** Music architecture, per-level wiring, the local integration patch, gotchas. Read it before changing audio wiring. |
| `Examples/DADIU2025_DeathToTheUniverse/` | Last year's graduation game Wwise project. Reference only. |
| `Exercises/Exercise_01_Flintstones/` | Standalone course exercise. |
| `WwiseAdventureGame2025.1.3.421/` | Audiokinetic's WAG. Retired from the course, still committed. ~1.9 GB. |

Unlike WAG, **the Dadiu-SuperMario Unity project is at the folder root** —
`Examples/Dadiu-SuperMario/Assets`, `.../ProjectSettings`, `.../Packages`. Open
`Examples/Dadiu-SuperMario/` itself in Unity Hub.

## Versions (do not drift from these)

| Component | Version |
|---|---|
| Unity | `6000.6.0f1` |
| Wwise | `2025.1.10` build `9233` |
| Wwise Unity Integration | bundle `2025.1.10.4304`, integration version 21 |

A Wwise version mismatch between Authoring and the Unity integration is the
single most common cause of "no sound at all", and it usually presents as a
confusing error rather than an obvious version warning.

### Two things that will silently break the build

1. **`AkCallbackManager.cs` carries a local patch.** Integration 2025.1.10 is
   missing a `UNITY_6000_4_OR_NEWER` branch that `AkWwiseSetupWizard.cs` already
   has, so on Unity 6000.4+ it hits `error CS0619: 'EntityId.implicit operator
   EntityId(int)' is obsolete`. The branch was added by hand and marked
   `LOCAL PATCH (DADIU course)`. **Reinstalling or updating the Wwise Unity
   Integration overwrites it.** Restore it from `AkWwiseSetupWizard.cs`. Details
   in `WWISE-INTEGRATION.md`.
2. **Do not install the Wwise Addressables package.** The Wwise setup offers it,
   this project does not use Addressables, and it drags in a
   `com.unity.addressables` version that may not match the editor. Also note the
   `AkSoundEngineInitialization` compatibility alias only exists *with* that
   package — project code uses the current `AkUnitySoundEngine*` names directly.

## The course project

### Scene flow

`Main Menu` → `Level Start Screen` → one of `World 1-1`, `World 1-1 -
Underground`, `World 1-2`, `World 1-2 - Underground`, `World 1-2 - Castle Cut`,
`World 1-3`, `World 1-4` → `Game Over Screen` / `Time Up Screen`.

`Test Scene` is a sandbox level. `Template.unity` is the starting point for new
levels and is deliberately left unwired.

**Any scene can be played directly** — press Play on `World 1-3` and it works.
That is not free; it is held up by two pieces of code, both documented in
`WWISE-INTEGRATION.md`:

- `Assets/Scripts/WwiseBankLoader.cs` loads `BNK_Main` from Wwise's
  `initializationDelegate`, because the only `AkBank` component lives in the
  Main Menu scene.
- `GameStateManager.GetOrCreate()` spawns a manager with new-game defaults,
  because every level scene ships its `Game State Manager` instance
  **deactivated** (the live one is meant to come from the menu).

### Key scripts

- `Assets/Scripts/LevelManager.cs` — per-level god object: HUD, timer, scoring,
  pause, respawn, powerup/powerdown, **and the entire audio surface**.
- `Assets/Scripts/GameStateManager.cs` — `DontDestroyOnLoad` singleton carrying
  lives/coins/score/timer/spawn point across scene loads. Also owns
  `MusicGameObject` and the `musicStarted` flag.
- `Assets/Scripts/Mario.cs` — movement state machine. Lots of exposable state:
  `currentSpeedX`, `isGrounded`, `isDashing`, `isChangingDirection` (skid),
  `isCrouching`, `isJumping`, `isFalling`, `isDying`, `isClimbingFlagPole`.
- `Assets/Scripts/_common/Enemy.cs` — base class for all enemies, with one
  virtual per death cause (`StompedByMario`, `TouchedByStarmanMario`,
  `TouchedByRollingShell`, `HitBelowByBlock`, `HitByMarioFireball`).
- `Assets/Scripts/WwiseBankLoader.cs` — course addition, not upstream.

### Wwise project

`Dadiu-SuperMario_WwiseProject/`. Single SoundBank, `BNK_Main`.

The music is a real interactive-music setup, and it is the best thing in the
project to teach from: `MUS_PlayMainPlaylist` plays `MUS_MainSwitch`, which
switches on `MarioState` into `MUS_Levels_Sw`, which switches on the `Levels`
State group to pick per-level music. **A level chooses its music by setting a
State, not by posting a different Event** — `LevelManager.ST_CurrentLevel` is
picked per level scene in the Inspector.

The gameplay SFX are largely still placeholder synth tones from
`Actor-Mixer Hierarchy/Debug`. Authoring real ones is course work.

### C# ↔ Wwise conventions

- Use **Wwise-Types** (`AK.Wwise.Event`, `AK.Wwise.RTPC`, `AK.Wwise.Switch`,
  `AK.Wwise.State`), never raw string-based `AkUnitySoundEngine` calls. Students
  get a Wwise Picker dropdown instead of a name they can typo.
- Event fields are plain camelCase (`coinSound`, `musicSource`) — the old `Ww`
  prefix was removed. `RTPC_` and `ST_` prefixes are kept, because those mirror
  the Wwise object names.
- `Post()` on an unbound (None) Event is a safe no-op, so unauthored audio makes
  the game silent rather than crashing. "No sound" therefore usually means an
  unpicked Event, not a bug.
- Wwise Events don't expose length to C#, so coroutines that used
  `AudioClip.length` use serialized float durations on `LevelManager`.

## Build and run

No CI, no test suite, no command-line build. Entirely GUI:

1. Open `Examples/Dadiu-SuperMario/` in Unity Hub with `6000.6.0f1`.
2. Open `Dadiu-SuperMario_WwiseProject/Dadiu-SuperMario_WwiseProject.wproj` in
   Wwise `2025.1.10`.
3. In Wwise: **Generate SoundBanks**.
4. Press Play in Unity — any scene.

In the Editor the integration reads banks straight from the Wwise project's
`GeneratedSoundBanks/` folder, so they do not need to be copied into
`StreamingAssets` for editor play. `GenerateSoundBanksAsPreBuildStep` is false —
students must generate manually after any Wwise change.

### Debugging checklist for "no sound"

1. Have SoundBanks been generated since the last Wwise change?
2. Is the Event actually picked in the Inspector, or is the Wwise-Type empty?
3. Do the Wwise Authoring and Unity integration versions match?
4. Is `ST_CurrentLevel` set on the Level Manager for this scene?
5. Did the compile succeed? A broken `Assets/Wwise` assembly silently stops all
   project scripts from compiling too.
6. Only then reach for the Wwise Profiler.

## Editing conventions

- The Mario game code is upstream open-source sample code. Prefer additive,
  clearly-commented changes over refactors, so the diff against upstream stays
  legible.
- **`Assets/Wwise/` is vendor code.** Do not edit it except for the documented
  `AkCallbackManager.cs` patch, and mark anything you must change with a
  `LOCAL PATCH` comment plus a note in `WWISE-INTEGRATION.md`.
- Never edit `.meta` files by hand. Never commit an asset without its `.meta`,
  and never a `.meta` without its asset — a missing `.meta` reassigns GUIDs and
  silently breaks every prefab and scene reference to it.
- `Assets/Wwise/ScriptableObjects/` holds the `WwiseObjectReference` assets that
  every Inspector-picked Event/State/RTPC points at **by Unity GUID**. Deleting
  or regenerating that folder empties every Wwise field in every scene and
  prefab. Back it up before any integration reinstall.
- Some scenes are still in Unity 5-era prefab serialization (`m_ParentPrefab`).
  Unity rewrites them on save — expect large diffs and verify prefab overrides
  survived, `ST_CurrentLevel` especially.
- `.wwu` work units are XML and merge badly. Avoid concurrent edits to the same
  work unit.

## Git conventions

- Binary-heavy repo. Be conscious of what you add.
- **Never commit** `Library/`, `Temp/`, `Logs/`, `UserSettings/`, `.cache/`,
  `*.wsettings`, `*.validationcache`, or generated `.bnk`/`.wem` files.
- `Originals/` **is** tracked — it is the source audio.

## Known repo issues

1. **No Git LFS**, despite a lot of committed audio. Converting now requires a
   history rewrite, so it is a decision for the whole course, not a drive-by.
2. **Per-user Wwise files are committed** (`*.danne.wsettings`,
   `*.danne.validationcache`). Stamped with a Windows username; they will
   conflict for every student.
3. **`.gitattributes` is minimal** — only `* text=auto`, with no `-text` markers
   for binary assets and no `merge=unityyamlmerge` for `.unity`/`.prefab`, so
   scene merges are hand-conflict-prone. Changing it triggers a renormalisation
   diff, so coordinate before doing it.
4. WAG's `Library/` and `UserSettings/` are committed from before it was
   retired. Harmless now, but they inflate the repo.

## Tone when helping students

- Lead with the audio concept, then the Unity mechanic. "This is a Switch so one
  stomp Event can pick a sound per enemy" lands better than "this field is an
  `AK.Wwise.Switch`".
- Point at a scene when one exists — seeing it wired up beats reading about it.
- Wwise Authoring changes are made in Wwise, not in Unity. When a student asks
  to "make the stomp louder", the answer is usually a Wwise Authoring action,
  not a code edit.
