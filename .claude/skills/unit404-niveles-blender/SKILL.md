---
name: unit404-niveles-blender
description: Build UNIT-404 level art in Blender (modular per-culture kits for Mesopotamia, Egipto, Grecia, China) and export FBX to Unity. Use when asked to model, restyle or re-export level assets, platforms, columns, walls, towers or ships for UNIT-404, or to apply the level palette.
---

# UNIT-404 level kits (Blender -> Unity)

Reference pattern: `serpiente-dragon-procedural` (template -> parametric scripts -> verify -> process sheet).
Palette and rules: `odd/art-style-sheet.md` (dark matte base, ONE emissive colour per culture on pickups/rings/portals, no photorealism).

## Flow
1. Pick the culture: `mesopotamia | egipto | grecia | china` (palette lives in `scripts/kits.py` `PALETTE`).
2. Generate kit + FBX (headless, no MCP needed):
   `~/Blender/blender-5.2.1-linux-x64/blender -b --python <skill>/scripts/kits.py -- --culture grecia --out Assets/Integrante4/Art/Grecia --blend-dir "Assets/Integrante4/Art/BlenderSource~"`
   Output: one FBX per piece (`<culture>_platform|column|wall|tower|ship|pickup.fbx`) + `Assets/Integrante4/Art/BlenderSource~/<culture>_kit.blend` (the trailing `~` makes Unity ignore the folder) (kept outside `Assets/`: Unity cannot import `.blend` without Blender on PATH).
3. With the Blender MCP connected, tune parameters interactively (see `references/parametros.md`), then re-run step 2 so the export stays reproducible.
4. Verify visually with the MCP (`look` / screenshot) when available; headless runs only prove export, not looks.
5. In Unity, FBX pieces import at 1 unit = 1 m. Gameplay stays on Z = 0 for levels 1-2; levels 3-4 are semi-open 3D.

## Rules
- Pieces are exported centred at origin with transforms applied (scale 1).
- Do not change gameplay shapes/sizes of pickups; only materials.
- Never write outside `Assets/Integrante4/Art/<Cultura>/`.
- Blender 5.x: do not rely on the default `Principled BSDF` node existing (the script creates it).
- Add a new piece by writing a function in `kits.py` and registering it in `KITS`.

## Process sheet
For the teammate review: render each `<culture>_kit.blend` front view and list piece names + palette hex codes.
