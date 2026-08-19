# CLAUDE.md — DADIU 2026 / Wwise Game Audio Course

Guidance for Claude Code working in this repository.

## What this repository is

This is a **teaching repository for a game audio course** built on Audiokinetic's
**Wwise Adventure Game (WAG)**, version `2025.1.3.421`. It is not a product
codebase — the C# game code is upstream sample code from Audiokinetic and is
mostly *scaffolding for audio lessons*. The thing students actually author is
the **Wwise project**, and the thing they learn is how Wwise and Unity talk to
each other.

Assume the audience is **sound designers and audio students**, not professional
Unity programmers. Many will be confident in a DAW and in Wwise Authoring, and
much less confident in Unity, C#, and git.

## Repository layout

Everything lives one level down, inside `WwiseAdventureGame2025.1.3.421/`:

| Path | What it is |
|---|---|
| `Wwise Project/` | The **Wwise Authoring project** (`Wwise Adventure Game.wproj`). This is the heart of the course. |
| `Wwise Project/Originals/` | Source audio — `SFX/`, `Voices/`, `Plugins/`. ~740 MB, 455 WAVs. |
| `WwiseAdventureGameSource/` | The **Unity project** (Unity `6000.2.12f1`). Open *this* folder in Unity Hub. |
| `WwiseAdventureGame/` | A **prebuilt Windows player** of the finished game. Intentionally committed so students can hear the reference mix without building. |
| `bundle.json`, `install-entry.json` | Wwise Launcher install metadata. Read-only; do not edit. |

### Key fact about paths

**The Unity project is NOT at the repo root.** It is at
`WwiseAdventureGame2025.1.3.421/WwiseAdventureGameSource/`.

This matters constantly. Any tooling, script, or ignore rule that assumes a
root-level Unity project will silently fail here. The original `.gitignore` used
root-anchored patterns (`/[Ll]ibrary/`) and as a result **Unity's `Library/` and
`UserSettings/` folders were committed to git**. See "Known repo issues" below.

## Versions (do not drift from these)

| Component | Version |
|---|---|
| Unity | `6000.2.12f1` (Unity 6) |
| Wwise SDK | `2025.1.3` build 9039 |
| Wwise Unity Integration | bundle `2025.1.3.3970`, integration version 21 |

Students must be on matching Unity **and** Wwise versions. A Wwise version
mismatch between Authoring and the Unity integration is the single most common
cause of "no sound at all", and it usually presents as a confusing error rather
than an obvious version warning.

## The course-relevant structure

### Certificate lesson scenes

`WwiseAdventureGameSource/Assets/Scenes/Game Scenes/Certificate Scenes/` holds
one Unity scene per Audiokinetic certification lesson:

- **`Cert-301/`** — *Wwise Unity Integration*. ~35 scenes covering SoundBank
  loading, posting Events, `AkAmbient` position types, attenuation spheres,
  Event position confining, decompressing/decoding banks, Audio Input, States,
  Game Parameters (RTPCs), Switches, global vs. game-object scope,
  `AkEnvironment` / aux sends, callbacks, and music States/regions.
- **`Cert-251/`** — *Wwise Performance & Optimization*. The Profiler, Voice
  Profiler, virtual voices, slow motion, region SoundBank loading, quest banks.

Scenes are named by lesson number (`L2_2 - Attenuation Spheres.unity`,
`Lesson 3.1 - Understanding Virtual Voices.unity`). **When a student asks about
a specific lesson, find the matching scene first** — the answer is usually in
what that scene's GameObjects have attached, not in a script.

There is an in-editor menu for navigating these: **`Audiokinetic`** in the Unity
menu bar (implemented in
`Assets/Scripts/WAG Editor Tools/Editor/CertificateMenu.cs`). It has entries for
opening certificate scenes, loading the Main Scene, and additively loading the
environment scenes.

### Teaching scenes

- **`Main Scene.unity`** — the complete game with all audio wired up.
- **`Main Scene (calls removed).unity`** — the same scene with the Wwise calls
  **stripped out**. This is the blank-canvas exercise: students re-implement the
  audio themselves. If a student says "there's no sound in the main scene",
  **check which of these two scenes they have open** before debugging anything.
- **`Audio Environment Scenes/`** — six additive scenes (Cave, Desert, Dungeon,
  Pine Forest, Village, Woodlands) carrying the reverb zones and ambience for
  each region. They are loaded *additively* on top of the Main Scene; audio
  "missing" in a region is often just an unloaded environment scene.
- **`Other/LilleWorld.unity`** — a small custom sandbox scene (Danish:
  "little world"), not part of upstream WAG. Course-local.
- **`Other/WwiseCertificateCompleteScene.unity`** — the finished reference.

### Wwise project organisation

Work units are split by domain, which mirrors how the lessons are taught:

- `Events/` — `Ambient`, `Destruction`, `Enemies`, `Magic`, `Music`, `NPCs`,
  `Objects`, `Player`, `Quest`, `UI`
- `Game Parameters/` — including `SideChain` (ducking) and per-domain RTPCs
- `States/` — `MusicStates`, `Player`
- `Switches/` — `General`, `Player` (surface/material switches)
- `SoundBanks/` — `Context`, `DLC`, `General`, `Region`
- Plus `Attenuations/`, `Effects/`, `Dynamic Dialogue/`, `Virtual Acoustics/`,
  `Conversion Settings/`, `Queries/`, `Mixing Sessions/`, `Soundcaster Sessions/`

### Unity to Wwise integration points

`Assets/Scripts/` (~274 project C# files, excluding the Wwise API itself). The
scripts worth knowing, because they are what students inspect in the Inspector:

- `Object Utility/SoundMaterial.cs` — exposes an `AK.Wwise.Switch` for footstep
  and impact material.
- `Object Utility/SoundOnAnimationEvent.cs` — posting Events from animation.
- `Object Utility/EventPositionConfiner.cs` — the Cert-301 L2_4 subject.
- `UI/SliderControlledRTPC.cs`, `UI/SetRTPCtoValueOnEnable.cs`,
  `UI/ToggleControlledRTPCSetter.cs` — RTPC driving from UI.
- `OnToggleSetState.cs`, `AkOnDropdownSetState.cs` — State setting from UI.
- `LoadSoundBankByName.cs` — runtime bank loading.
- `Managers/` — `GameManager`, `PlayerManager`, `DialogueManager`,
  `QuestManager`, `InputManager`, `LanguageManager`, `PlatformManager`.

These use **Wwise-Types** (`AK.Wwise.Event`, `AK.Wwise.RTPC`, `AK.Wwise.Switch`,
`AK.Wwise.State`) rather than raw string-based `AkUnitySoundEngine` calls.
Prefer Wwise-Types in any new example code — that is what the certification
teaches, and it gives students a Wwise Picker dropdown in the Inspector instead
of a name they can typo.

## Build and run

There is no CI, no test suite, and no command-line build. The workflow is
entirely GUI:

1. Open `WwiseAdventureGame2025.1.3.421/WwiseAdventureGameSource/` in Unity Hub
   with Unity `6000.2.12f1`.
2. Open `Wwise Project/Wwise Adventure Game.wproj` in Wwise `2025.1.3`.
3. In Wwise: **Generate SoundBanks**. Nothing will make sound until this is done
   — generated banks are not in git.
4. Press Play in Unity.

### SoundBank paths

From `Assets/WwiseSettings.xml`:

- `RootOutputPath` = `../../WwiseAdventureGameSource/Assets/WwiseData/Banks`
- `WwiseStreamingAssetsPath` = `Audio\GeneratedSoundBanks`
- `GenerateSoundBanksAsPreBuildStep` = **false** — Unity will *not* generate
  banks for you. Students must generate manually after any Wwise change.
- `CopySoundBanksAsPreBuildStep` = **true**
- WAAPI enabled on `127.0.0.1:8080`, but `AutoSyncWaapi` = false.

`Assets/StreamingAssets/Audio/GeneratedSoundBanks/` is empty in a fresh clone by
design. `Assets/WwiseData/` does not exist until the first generation.

## Working in this repo

### Debugging checklist for "no sound"

Work through this order — it resolves the large majority of student reports:

1. Have SoundBanks been generated since the last Wwise change?
2. Is the open scene `Main Scene` or `Main Scene (calls removed)`?
3. Do the Wwise Authoring version and the Unity integration version match?
4. Are the required Audio Environment scenes loaded additively?
5. Is the Event actually referenced in the Inspector, or is the Wwise-Type empty?
6. Is the relevant SoundBank loaded at runtime (`Context` / `Region` / `DLC` are
   loaded on demand, not all at startup)?
7. Only then reach for the Wwise Profiler.

### Editing conventions

- The upstream Audiokinetic code carries a `Copyright (c) 2018 Audiokinetic Inc.`
  header. **Do not reformat, refactor, or "modernise" upstream files.** Keeping
  them identical to the shipped WAG is what lets students follow along with
  Audiokinetic's own lesson videos and documentation.
- Course-specific additions should be clearly separated from upstream WAG
  content, so the project can be re-based onto a newer WAG release later.
- Never edit `.meta` files by hand. Never commit a Unity asset without its
  `.meta` file, and never commit a `.meta` without its asset — a missing `.meta`
  reassigns GUIDs and silently breaks every prefab and scene reference to it.
- `.wwu` work units are XML and merge badly. Avoid concurrent edits to the same
  work unit; that is why the Wwise project is split into many small ones.

### Git conventions

- Binary-heavy repo: ~740 MB of source audio, ~819 MB `.git`. Be conscious of
  what you add.
- **Never commit** `Library/`, `Temp/`, `UserSettings/`, `.cache/`,
  `*.wsettings`, `*.validationcache`, or generated `.bnk`/`.wem` files. The
  `.gitignore` covers all of these — its patterns are deliberately **unanchored**
  because the Unity project is nested. Do not rewrite them as `/Library/`.
- `Originals/` **is** tracked and should stay tracked — it is the source audio.

## Known repo issues

These are real, currently-unresolved problems. Mention them if relevant; do not
silently "fix" them, since untracking files affects everyone who has cloned.

1. **`Library/` and `UserSettings/` are committed** (~78 files, including
   `ScriptAssemblies/*.dll`). Caused by the original root-anchored `.gitignore`
   patterns not matching the nested Unity project. The `.gitignore` now excludes
   them, but they remain in the index and must be untracked explicitly with
   `git rm -r --cached` on those two folders.

2. **Per-user Wwise files are committed**: `Wwise Adventure Game.danne.wsettings`
   and `.danne.validationcache`. These are stamped with a Windows username and
   will conflict for every student.

3. **No Git LFS**, despite 455 WAVs and 740 MB of audio. `git-lfs 3.7.0` is
   installed locally. Converting now requires a history rewrite, so it is a
   decision for the whole course, not a drive-by change.

4. **`.gitattributes` is minimal** — only `* text=auto`. It has no `-text`
   markers for binary assets and no `merge=unityyamlmerge` for `.unity` /
   `.prefab` files, so scene merges are hand-conflict-prone. Changing it on an
   existing repo triggers a renormalisation diff, so coordinate before doing it.

## Tone when helping students

- Lead with the audio concept, then the Unity mechanic. "This is a Switch so the
  same footstep Event can pick a surface" lands better than "this field is an
  `AK.Wwise.Switch`".
- Point at the lesson scene when one exists — seeing it wired up beats reading
  about it.
- Wwise Authoring changes are made in Wwise, not in Unity. When a student asks
  Claude to "make the sword louder", the answer is usually a Wwise Authoring
  action, not a code edit.
