---
name: unit404-niveles-blender
description: Build UNIT-404 level art in Blender (modular per-culture kits for Mesopotamia, Egipto, Grecia, China) and export FBX to Unity. Use when asked to model, restyle or re-export level assets, platforms, columns, walls, towers or ships for UNIT-404, or to apply the level palette.
---

# UNIT-404 level kits (Blender -> Unity)

Reference pattern: `serpiente-dragon-procedural` (template -> parametric scripts -> verify -> process sheet).
Palette and rules: `odd/art-style-sheet.md` (ONE emissive colour per culture on pickups/rings/portals, no photorealism).
Look (ODD S23): the route is a lit landscape of the culture (`DAY` materials in `kits.py`: mud brick + glazed tiles, sandstone + painted limestone, marble + terracotta, grey brick + red lacquer); only the boss arena is dark (night stone with glowing cracks in the culture accent, Mesopotamia uses the teammate's basalt via `--culture arena`).

## Flow
1. Pick the culture: `mesopotamia | egipto | grecia | china` (palette lives in `scripts/kits.py` `PALETTE`).
2. Generate kit + FBX (headless, no MCP needed):
   `~/Blender/blender-5.2.1-linux-x64/blender -b --python <skill>/scripts/kits.py -- --culture grecia --out Assets/Integrante4/Art/Grecia --blend-dir "Assets/Integrante4/Art/BlenderSource~"`
   Output: one FBX per piece (`<culture>_platform|column|wall|tower|ship|pickup.fbx`), its 2048 day maps `<piece>_albedo.png` (smoothness = 1 - roughness in the alpha) + `<piece>_normal.png` (Bevel-shader rounded edges) + `<piece>_occlusion.png` (also multiplied 55 % into the albedo) (no emission), for Egipto/Grecia/China also `<culture>_arena_platform_albedo|normal|emission.png` baked on the same platform UVs, + `Assets/Integrante4/Art/BlenderSource~/<culture>_kit.blend` (the trailing `~` makes Unity ignore the folder) (kept outside `Assets/`: Unity cannot import `.blend` without Blender on PATH).
   Rebuild only some pieces with `--only stairs,block` (other FBX/maps untouched, the `.blend` is not saved).
   Level elements (ODD T18) and backdrops (ODD T19) have their own scripts, same arguments:
   `<blender> -b --python <skill>/scripts/elements.py -- --culture grecia --out Assets/Integrante4/Art/Grecia --blend-dir "Assets/Integrante4/Art/BlenderSource~"`
   `<blender> -b --python <skill>/scripts/backdrops.py -- --culture grecia --out Assets/Integrante4/Art/Grecia --blend-dir "Assets/Integrante4/Art/BlenderSource~"`
   Culture props (ODD T30-T31) the same way: `<blender> -b --python <skill>/scripts/props.py -- --culture grecia --out ... [--only olive,altar] [--no-bake]` (the run fails with `PROPS_BAD_FIT` if a prop leaves its `NOMINAL` box).
   Run ONE Blender at a time: several headless bakes in parallel exhaust the laptop's RAM.
3. With the Blender MCP connected, tune parameters interactively (see `references/parametros.md`), then re-run step 2 so the export stays reproducible.
4. Verify visually with the MCP (`look` / screenshot) when available; headless runs only prove export, not looks.
5. In Unity, FBX pieces import at 1 unit = 1 m. Gameplay stays on Z = 0 for levels 1-2; levels 3-4 are semi-open 3D.

## Unity fit (verified in batchmode)
- Export uses `bake_space_transform=True`: without it the FBX root carries a 270 deg X rotation and Unity pieces lie on their side.
- Every piece has its origin at the centre of its base. Nominal sizes (width, height, depth): platform 4x1x4 (tiled, never stretched), column 1.4x6x1.4, wall 10x5x1, tower 5.6x8x5.6, ship 6x1.6x2. `LevelSceneBuilder.ApplyKit` scales a piece to its grey block (the block stays as collider, its renderer is disabled).
- Detailed pieces (T17): stairs 6x2x2 (`Terraza*`), block 3x3x2 (`Bloque*`), mast 0.5x8x2 (`Mastil*`, banner faces +X for the 3D camera), keep 8x12x8 (`Torre_Estandarte`); walkable tops stay flush with the collider top. Long walls (`Ladera_*`) are tiled 10 m pieces.
- Elements (T18, `elements.py`): crystal, ring, tablet, checkpoint, arch + stake (L1), torch (L2), log/drum (L3), cart (L4). Modelled at the gameplay primitive's size (`NOMINAL`) with the origin at the box CENTRE; `LevelSceneBuilder.ApplyElements` parents them to the primitive with `localScale = primitive unit / nominal`, so they fill the collider, move with obstacles and hide with collected pickups. Maps: albedo, normal, emission (the accent glow). The arch is base-centred decoration next to the arrival pad.
- Backdrops (T19, `backdrops.py`): `<culture>_bg_near|mid|far`, 60 m strips with the front edge at the origin; `LevelSceneBuilder.Backdrop` tiles them outside `Limites_*` (2.5D behind the path, 3D on both sides). Albedo only.
- Props (T30-T31, `props.py`): base-centred, facing -Y in Blender (= -Z in Unity, towards the 2.5D camera), sized by `NOMINAL` (mirror it in `LevelSceneBuilder.PropSizes`). 2.5D: `Dress25D` puts them on a decorative ground strip behind the path (`Deco_*`, no collider). 3D: `Prop(...)` / `Gate(...)` blocks (`Prop_<id>_<kind>`, `Puerta<N>_<I|D|Dintel>_<kind>`) keep the collider and `ApplyKit` skins them; leaves and fronds pivot at their base (`backdrops.frond`). Backdrops v2 (T32) add working figures (`backdrops.figure`, poses stand/bend/pull/carry/hoe) and landmarks per layer.
- `LevelSceneBuilder.KitMaterial` builds a URP Lit material per map prefix (`Art/<Culture>/Materials/<prefix>.mat`) from the baked PNGs; a missing emission map turns emission off. Arena platforms reuse `<culture>_platform.fbx` with the `<culture>_arena_platform` maps. Silhouettes per culture live in `STYLE` in `kits.py`.

## Rules
- Pieces are exported with their origin at the base centre and transforms applied (scale 1).
- Do not change gameplay shapes/sizes of pickups; only materials.
- Never write outside `Assets/Integrante4/Art/<Cultura>/`.
- Blender 5.x: do not rely on the default `Principled BSDF` node existing (the script creates it).
- Add a new piece by writing a function in `kits.py` and registering it in `KITS`.

## Process sheet
For the teammate review: render each `<culture>_kit.blend` front view and list piece names + palette hex codes.
