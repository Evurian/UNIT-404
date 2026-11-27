---
name: unit404-generar-niveles
description: Generate a UNIT-404 level scene (levels 5-8 Mayas, Aztecas, Incas, Archivo, or any new level) the same way Integrante 4 built levels 1-4 - code-generated Unity scene from grey gameplay blocks, culture kit art from Blender, HUD/pause/fall/narrator overlays, contracts and EditMode tests. Use when asked to create, lay out, rebuild or test a UNIT-404 level, or to follow Integrante 4's level pipeline.
---

# UNIT-404 level generation (Unity, code-built scenes)

Reference implementation: `Assets/Integrante4/Editor/LevelSceneBuilder.cs` (levels 1-4) and its tests
`Assets/Integrante4/Tests/EditMode/LevelSceneTests.cs`. Art comes from the sibling skill
`.claude/skills/unit404-niveles-blender/`. Team rules: `docs/ACUERDOS.md`; integration notes: `docs/INTEGRACION-INTEGRANTE4.md`.

## Golden rules
- Work only in your own folder (`Assets/IntegranteN/`) and branch (`integrante-N`); never edit `Assets/Integrante4/` or `TagManager.asset`. Copy the pattern; do not patch Integrante 4's builder.
- Scenes are GENERATED: never hand-edit a level scene, change the builder and rebuild. Hand edits are lost on the next build.
- Gameplay is greybox first: primitives with colliders, tags and layers from `docs/ACUERDOS.md` §2-3. Kit art only skins those blocks.
- Test first: write the failing EditMode test for each rule, rebuild, then make it pass.

## Setup for a new member (once)
1. Copy `LevelSceneBuilder.cs` to `Assets/IntegranteN/Editor/` and rename the class (for example `LevelSceneBuilderN`). Its helpers are `private static`, so they cannot be called from another class.
2. Point the constants at your folder: `Folder` (scenes), `ArtFolder`, `VolumeFolder`, and the `MenuItem` paths (`UNIT-404/Integrante N/...`).
3. Replace the per-culture tables (indexed by `level - 1`, so start your arrays at level 5 with an offset helper): `Cultures`, `Days` (sky, sun, fog), `TerrainHex`, `DecoKinds` (2.5D props), `PropSizes` (must mirror `NOMINAL` in `props.py`), `LevelBuilders`.
4. Runtime scripts are shared and reusable as they are: `PlaceholderPlayer`, `SideCameraFollow` (2.5D), `ThirdPersonCamera` (3D), `FallZone`, `FallScreen`, `PauseMenu`, `LevelNarrator`, `AchievementToast`, `CultureTransition`, the HUD prefab `Assets/Integrante4/Prefabs/HUD.prefab`.
5. Overlays: `FlowSceneBuilder.AddPauseMenu()`, `AddFallScreen(level)` and `AddNarrator(level, next)` are public. `AddPauseMenu` works for any level, BUT `AddFallScreen` (build time) and `CultureTransition` (runtime, at the end of the level) read `CulturePalette`, and the transition scenes use `CinematicScript`; both only accept levels 1-4 and throw `ArgumentOutOfRangeException`. Ask Integrante 4 to extend `CulturePalette` (`Names`, `Accents`, `Falls`) and `CinematicScript` to levels 5-8 before using them, or build the same overlays in your own builder. Ada's lines (`Narrative`) fall back to a generic line for levels without their own.

## Level recipe (what levels 1-4 follow)
A level is a 2.5D or semi-open 3D path along +X, about 190 m long, built from five zones:

| Zone | x (m) | Content |
|------|-------|---------|
| 1 | 0-40 | Tutorial: floor, first rings (`Propulsor`), fragments |
| 2 | 40-80 | First guards (`Enemigo`), obstacles, `Checkpoint_1` near x 78 |
| 3 | 80-120 | Hardest climb: ramps/terraces, wind or moving hazards |
| 4 | 120-160 | Boss arena (`ArenaStartX/EndX`): `Checkpoint_2` at the door, boss marker with tag `Jefe`, dark accent look |
| 5 | 160-190 | Arrival temple and `Portal` (solid landing pad) |

Per level: exactly **8 glyphs** (`Glyphs(...)`), **2 checkpoints**, **1 portal**, a `PlayerStart`, a `Main Camera`, fragments, rings and obstacles in every zone.
- 2.5D levels (1-2): gameplay on Z = 0, `pathDepth = 2`.
- 3D levels (3-4): `pathDepth = 8`, `Finish(..., freeDepth: true)` swaps in `ThirdPersonCamera`.
- Level number is read from the scene name: `NivelN_<Cultura>` (`name[5]`), so keep that naming (single digit).

### Builder vocabulary (call these, do not hand-place objects)
`NewLevel(name, sky)` -> layout calls -> `Finish(scene, start, name, freeDepth)`.
- Surfaces: `Floor`, `Solid`, `Walk`, `Rampa` + `RampBody` (every ramp needs its solid body), `SidePath`.
- Gameplay: `Pickups`, `Ring`, `Glyphs`, `Tagged` (checkpoints, boss, stakes), `Guard`, `Wind`, `Mover`, `Beam`, `CartRow`, `Ship`, `Portal`.
- 3D dressing with colliders: `Prop`, `Gate`. 2.5D dressing is automatic (`Dress25D`, decorative only, no collider).
- `Finish` adds the player placeholder, play bounds + fall zone, kit art (`ApplyKit`), elements, backdrops, palette, day look, HUD, pause menu, fall screen and narrator, then saves the scene.

## Art (Blender -> Unity)
Follow `.claude/skills/unit404-niveles-blender/SKILL.md`, with two changes for a new culture:
- Add the culture to `PALETTE` and `STYLE` in `scripts/kits.py` (and the per-culture tables in `elements.py`, `backdrops.py`, `props.py`).
- Write to your own folder: `--out Assets/IntegranteN/Art/<Cultura> --blend-dir "Assets/IntegranteN/Art/BlenderSource~"`.
Run one Blender at a time (headless bakes exhaust RAM). Pieces are base-centred, 1 unit = 1 m; `ApplyKit` matches a piece to its block by name prefix (`KitPiece`: `Columna_` -> column, `Terraza` -> stairs, `Bloque` -> block, `Muro`/`Ladera_` -> wall, `Torre_` -> tower, `Barco` -> ship, `Prop_<id>_<kind>` -> that prop, anything else -> platform), so the block names decide the art.

## Lessons from levels 1-4 (each one is a test there; copy the tests)
- Kit art must never rise above its collider top, or the player sinks into merlons and roofs (`FitUnderCollider`; test `KitArt_NeverRisesAboveItsCollider`).
- Visible top = collider top, no invisible ledges (`Platforms_VisibleTopMatchesColliderTop_NoInvisibleLedges`).
- No step on the main line without a ramp; ramps meet floors without a lip and have a solid body (`MainLine_HasNoStepWithoutARamp`, `Ramps_*`).
- 2.5D props are decorative only and sit a step below the path, so they never read as obstacles (`Level_IsDressedWithCultureProps`).
- Large obstacles (ships) leave a lane to go around them; masts and posts get their own colliders (`SolidCollider`).
- Six invisible bounds plus a fall zone under every surface (`Level_HasSixInvisibleBoundsEnclosingThePlayArea`, `Level_HasFallZoneBelowEverySurfaceAndTheRetryScreen`).
- Imported FBX must have transforms applied (`bake_space_transform=True`), or pieces float or lie on their side.

## Build, test, verify
1. Menu `UNIT-404/Integrante N/...` or batchmode:
   `Unity -batchmode -nographics -quit -projectPath <repo> -executeMethod <YourBuilder>.BuildAll -logFile build.log`
2. Tests: `Unity -batchmode -nographics -projectPath <repo> -runTests -testPlatform EditMode -testResults results.xml -logFile tests.log`
   (copy the relevant cases from `LevelSceneTests.cs` into your own test assembly; an asmdef test assembly cannot reference Assembly-CSharp types such as `LevelNarrator`, so assert through object names, tags and Unity/TMP components).
3. If the Unity Editor has the project open, batchmode refuses the lock: rsync `Assets Packages ProjectSettings Library` to a scratch folder, build and test there, then copy back only your rebuilt scenes.
4. Play the level in the Editor: headless tests prove structure, not feel. With `HudEventSimulator` in the scene, the **L** key completes the level to walk the flow.

## Integration
- Add your scenes, in story order with their transitions, to `SceneFlow.Story` (owned by Integrante 4; coordinate) and run `UNIT-404/Integrante 4/Conectar escenas (Build Settings)`.
- Never commit `ProjectSettings/SceneTemplateSettings.json` or Volume churn you did not cause.
