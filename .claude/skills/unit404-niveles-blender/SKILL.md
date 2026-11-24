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
   Output: one FBX per piece (`<culture>_platform|column|wall|tower|ship|pickup.fbx`), its day maps `<piece>_albedo.png` + `<piece>_normal.png` (no emission), for Egipto/Grecia/China also `<culture>_arena_platform_albedo|normal|emission.png` baked on the same platform UVs, + `Assets/Integrante4/Art/BlenderSource~/<culture>_kit.blend` (the trailing `~` makes Unity ignore the folder) (kept outside `Assets/`: Unity cannot import `.blend` without Blender on PATH).
3. With the Blender MCP connected, tune parameters interactively (see `references/parametros.md`), then re-run step 2 so the export stays reproducible.
4. Verify visually with the MCP (`look` / screenshot) when available; headless runs only prove export, not looks.
5. In Unity, FBX pieces import at 1 unit = 1 m. Gameplay stays on Z = 0 for levels 1-2; levels 3-4 are semi-open 3D.

## Unity fit (verified in batchmode)
- Export uses `bake_space_transform=True`: without it the FBX root carries a 270 deg X rotation and Unity pieces lie on their side.
- Every piece has its origin at the centre of its base. Nominal sizes (width, height, depth): platform 4x1x4 (tiled, never stretched), column 1.4x6x1.4, wall 10x5x1, tower 5.6x8x5.6, ship 6x1.6x2. `LevelSceneBuilder.ApplyKit` scales a piece to its grey block (the block stays as collider, its renderer is disabled).
- `LevelSceneBuilder.KitMaterial` builds a URP Lit material per map prefix (`Art/<Culture>/Materials/<prefix>.mat`) from the baked PNGs; a missing emission map turns emission off. Arena platforms reuse `<culture>_platform.fbx` with the `<culture>_arena_platform` maps. Silhouettes per culture live in `STYLE` in `kits.py`.

## Rules
- Pieces are exported with their origin at the base centre and transforms applied (scale 1).
- Do not change gameplay shapes/sizes of pickups; only materials.
- Never write outside `Assets/Integrante4/Art/<Cultura>/`.
- Blender 5.x: do not rely on the default `Principled BSDF` node existing (the script creates it).
- Add a new piece by writing a function in `kits.py` and registering it in `KITS`.

## Process sheet
For the teammate review: render each `<culture>_kit.blend` front view and list piece names + palette hex codes.
