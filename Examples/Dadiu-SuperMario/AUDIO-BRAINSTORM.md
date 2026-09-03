# Audio Brainstorm — DADIU 2026 Super Mario Project

Audio brainstorm for the DADIU 2026 Super Mario project. Organised by Wwise
structure: what plays (Events), what it plays on (Music, Ambience, UI), and what
shapes it (States, Switches, RTPCs).

Source: the "DADIU 2026 Super Mario Project" Notion page. Keep the two in sync.

## Design principles

These are the overall ideas that cut across everything else.

- Sound design can sit **in the same key as the music**.
- Sound design can be **locked to the beat/rhythm of the music** — e.g. footsteps
  quantized to 16th notes.
- **Combo feel:** collecting several pickups within a time window pitches each one up.
- Enemies are always moving, so give them a **simple looping sound** rather than
  one-shots.
- Use **Rooms and Portals** for the game's areas; reverb in caves scaled to the
  cave size.

---

## Events

### Mario — locomotion
- Jumping
- Footsteps
- Surface detection: metal / grass / treetops
- Surface impact — for Mario *and* for shells

### Mario — state and damage
- Damage sounds that vary with how much damage was taken
- Different death sounds depending on *how* he dies
- Vocal sounds / voice lines, changing with his "sobriety" status
- Fade out

### Creatures
- **Mushroom** — death, idle, voice
- **Ordinary Turtle** — death, idle, hit sounds, voice
- **Flying Turtle** — death, idle, hit sounds, voice
- **Bowser** — idle, jump, attack, death, voices
  - Doppler effect on Bowser
- Idle and walking sounds on enemies generally

### World and objects
- Flag
- Question mark boxes — resonate, then pop when hit from below
- Invisible metal boxes you can hear but not see
- Mushrooms (same distance-based treatment as the boxes)

---

## Ambience
- Environment sounds / general ambience beds
- Lava level, with attenuation or RTPC control
- Looping fire wheel
- Cave reverb, sized to the space

## Music
- Intro music
- Level music
- Stingers for stars, mushrooms, etc.
- Could be built in layers
- Stinger for hitting the flag perfectly

## UI
- Hover / select
- Main menu
- Pause screen
- Game over

---

## States
- Inside / Outside
- Mario size and power: Big / Small / Shoot Star

## Switches
- Surface materials
- Coin system
- Height ambience
- Landing hardness — driven by a fall parameter, also feeds the damage sounds

## RTPCs
- Distance-based attenuation on question mark boxes and mushrooms
- Altitude → affects reverb
- Fall speed
- Flag
- Point counter
- Distance to the end of the level / game
