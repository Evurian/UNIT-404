# kits.py parameters (metres)

| Piece | Function | Defaults |
|---|---|---|
| platform | `platform(name, mat, loc, w, d, h)` | 8 x 4 x 1 |
| column | `column(name, mat, loc, r, h)` | r 0.5, h 6 |
| wall | `wall(name, mat, loc, w, h, t)` | 10 x 5, thickness 1 |
| tower | `tower(name, mat, loc, w, h)` | 4 wide, 8 high |
| ship | `ship(name, mat, loc, l, w, h)` | 6 x 2 x 1 |
| pickup | `emissive_pickup(name, mat, loc, r)` | r 0.3, emission strength 4 |

Palette: matte base roughness 0.85; emissive strength 4.0 for the accent colour (see `PALETTE`).
