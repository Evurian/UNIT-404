"""UNIT-404 level backdrops (ODD S17, T19): three depth layers per culture, replacing the grey Fondo_* boxes.
blender -b --python backdrops.py -- --culture egipto --out Assets/Integrante4/Art/Egipto --blend-dir "Assets/Integrante4/Art/BlenderSource~"

Each layer is a 60 m strip along X (Blender X = Unity X), base-centred, its front edge at Y = 0 and its scenery behind
(+Y in Blender = away from the 2.5D camera). Unity tiles the strips along the level at three depths and scales the far
ones up (LevelSceneBuilder.Backdrop); levels 3-4 mirror them on both sides of the 3D path. Only an albedo map is baked
(<culture>_bg_<layer>_albedo.png): they are seen far away through the day haze, normal maps would not show.
  mesopotamia: near salt fields + river + palms, mid mud-brick city wall, far ziggurats
  egipto:      near Nile + reeds + palms,       mid temple + nilometer,   far pyramids + dunes
  grecia:      near rocky shore + sea,          mid felled hills + ruined temple, far islands on the sea
  china:       near pines + rocks,              mid Great Wall on hills,  far mountains"""
import bpy, sys, os, math, random

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import kits  # noqa: E402
from kits import box, cone, join, rocky, day_material, DAY  # noqa: E402
from elements import flat, cyl  # noqa: E402

SEG = 60.0
LAYERS = ("near", "mid", "far")


def hills(mat, w, depth, height, y0=0.0, seed=0, cuts=48):
    """Rolling terrain strip: a subdivided grid displaced with low-frequency clouds, edges pulled down to the base."""
    bpy.ops.mesh.primitive_grid_add(x_subdivisions=cuts, y_subdivisions=max(4, cuts // 4), size=1)
    o = bpy.context.active_object
    o.scale = (w, depth, 1.0)
    o.location = (0, y0 + depth / 2, 0)
    bpy.ops.object.transform_apply(location=True, rotation=False, scale=True)
    rnd = random.Random(seed)
    ph = [rnd.uniform(0, 6.28) for _ in range(3)]
    for v in o.data.vertices:
        x, y = v.co.x, v.co.y
        edge = min(1.0, (y - y0) / (depth * 0.35))  # front edge stays at 0 so the strip sits on the ground
        n = 0.5 + 0.25 * math.sin(x * 0.11 + ph[0]) + 0.15 * math.sin(x * 0.27 + ph[1]) + 0.1 * math.sin(x * 0.61 + y * 0.3 + ph[2])
        v.co.z = max(0.0, edge) * n * height
    o.data.materials.append(mat)
    return o


def palm(trunk, leaf, x, y, h=6.0):
    parts = [cone(trunk, 0.22, 0.14, h, 8, x, y, 0)]
    for i in range(7):
        a = i * 2 * math.pi / 7
        f = cone(leaf, 0.35, 0.0, 2.6, 4, x, y, h - 0.2)
        f.rotation_euler = (math.radians(70), 0, a)
        parts.append(f)
    return parts


def pine(leaf, trunk, x, y, h=7.0):
    parts = [cyl(trunk, 0.2, h * 0.3, 6, x, y, 0)]
    for i in range(3):
        parts.append(cone(leaf, h * (0.28 - i * 0.06), 0.0, h * 0.45, 8, x, y, h * (0.2 + i * 0.22)))
    return parts


def ziggurat(stone, trim, x, y, w=16.0, tiers=4):
    parts = []
    for i in range(tiers):
        s = w * (1 - i * 0.2)
        parts.append(box(stone if i % 2 == 0 else trim, s, s * 0.8, 2.6, x, y, i * 2.6))
    parts.append(box(trim, w * 0.12, w * 0.5, tiers * 2.6, x, y - w * 0.15, 0))  # front stairway
    return parts


def temple(stone, trim, x, y, cols=6, h=6.0, broken=(), z=0.0):
    parts = [box(trim, cols * 2.4, 6.0, 0.8 + z, x, y, 0)]
    for i in range(cols):
        ch = h * (0.45 if i in broken else 1.0)
        parts.append(cone(stone, 0.5, 0.42, ch, 16, x - (cols - 1) * 1.2 + i * 2.4, y - 2.0, 0.8 + z))
    if not broken:
        parts.append(box(trim, cols * 2.4, 6.0, 0.9, x, y, h + 0.8 + z))
    return parts


def build_layer(culture, layer, M):
    rnd = random.Random(sum(map(ord, culture + layer)))  # stable across runs (str hash is salted)
    P = []
    if culture == "mesopotamia":
        if layer == "near":
            P.append(hills(M["sand"], SEG, 10, 0.6, 0, 1, 24))
            P.append(box(M["water"], SEG, 3.0, 0.08, 0, 4.0, 0.05))
            for i in range(9):
                P.append(box(M["salt"], rnd.uniform(2, 5), rnd.uniform(1, 2), 0.06, -SEG / 2 + 3 + i * 6.5, rnd.uniform(0.6, 2.0), 0.02))
            for i in range(5):
                P += palm(M["trunk"], M["leaf"], -SEG / 2 + 6 + i * 12 + rnd.uniform(-2, 2), 7.0, rnd.uniform(5, 7))
        elif layer == "mid":
            P.append(hills(M["sand"], SEG, 14, 1.5, 0, 2, 24))
            P.append(box(M["stone"], SEG, 1.6, 3.5, 0, 9.0, 0))  # low city wall: the far ziggurats rise above it
            for i in range(20):
                P.append(box(M["trim"], 1.2, 1.8, 0.9, -SEG / 2 + 1.5 + i * 3.0, 9.0, 3.5))
            for i in range(10):  # flat-roofed mud-brick houses in front of the wall
                P.append(box(M["stone"], rnd.uniform(3, 5), 3, rnd.uniform(1.8, 3.2), -SEG / 2 + 3 + i * 6, 4.0, 0))
        else:
            P.append(hills(M["sand"], SEG, 20, 3.0, 0, 3, 32))
            P += ziggurat(M["stone"], M["trim"], -12, 10, 18, 4)
            P += ziggurat(M["stone"], M["trim"], 16, 14, 12, 3)
    elif culture == "egipto":
        if layer == "near":
            P.append(hills(M["sand"], SEG, 10, 0.5, 0, 4, 24))
            P.append(box(M["water"], SEG, 4.0, 0.08, 0, 4.5, 0.05))
            for i in range(24):
                P.append(cone(M["leaf"], 0.08, 0.0, rnd.uniform(1.2, 2.2), 4, -SEG / 2 + i * 2.5 + rnd.uniform(-0.5, 0.5), 2.2, 0))
            for i in range(5):
                P += palm(M["trunk"], M["leaf"], -SEG / 2 + 5 + i * 12 + rnd.uniform(-2, 2), 8.0, rnd.uniform(5, 7.5))
        elif layer == "mid":
            P.append(hills(M["sand"], SEG, 14, 1.2, 0, 5, 24))
            P += temple(M["stone"], M["trim"], -10, 8, 6, 7.0)
            P.append(box(M["trim"], 6, 6, 1.0, 14, 7, 0))  # nilometer well with its stepped stairs and column
            for i in range(4):
                P.append(box(M["stone"], 6 - i * 1.2, 6 - i * 1.2, 0.5, 14, 7, 1.0 + i * 0.5))
            P.append(cone(M["trim"], 0.4, 0.35, 6.0, 12, 14, 7, 3.0))
        else:
            P.append(hills(M["sand"], SEG, 22, 4.0, 0, 6, 32))
            for x, s in ((-14, 22), (6, 30), (24, 16)):
                P.append(cone(M["stone"], s * 0.7, 0.2, s * 0.62, 4, x, 12, 0, twist=math.pi / 4))
    elif culture == "grecia":
        if layer == "near":
            P.append(box(M["water"], SEG, 20, 0.1, 0, 10, -0.6))
            for i in range(12):
                r = box(M["rock"], rnd.uniform(1.5, 4), rnd.uniform(1.5, 3), rnd.uniform(0.8, 2.5), -SEG / 2 + 2 + i * 5, rnd.uniform(0.5, 3), -0.6)
                rocky(r, 0.4, 4)
                P.append(r)
        elif layer == "mid":
            P.append(hills(M["grass"], SEG, 16, 6.0, 0, 7, 40))
            for i in range(14):  # felled hills: stumps
                P.append(cyl(M["trunk"], 0.3, 0.6, 6, -SEG / 2 + 3 + i * 4 + rnd.uniform(-1, 1), rnd.uniform(1.5, 4), 0.8))
            P += temple(M["stone"], M["trim"], 6, 5, 6, 6.0, broken=(1, 4), z=3.0)  # ruined temple on its podium
        else:
            P.append(box(M["water"], SEG, 40, 0.1, 0, 20, -0.6))
            for x, s in ((-18, 10), (4, 16), (22, 8)):
                isl = cone(M["rock"], s, s * 0.3, s * 0.45, 12, x, 24, -0.6)
                rocky(isl, 0.8, 4)
                P.append(isl)
    else:  # china
        if layer == "near":
            P.append(hills(M["grass"], SEG, 10, 0.8, 0, 8, 24))
            for i in range(8):
                P += pine(M["leaf"], M["trunk"], -SEG / 2 + 4 + i * 7.5 + rnd.uniform(-1.5, 1.5), rnd.uniform(3, 7), rnd.uniform(5, 8))
            for i in range(5):
                r = box(M["rock"], 2.5, 2, 1.8, -SEG / 2 + 8 + i * 12, 1.5, 0)
                rocky(r, 0.4, 4)
                P.append(r)
        elif layer == "mid":
            P.append(hills(M["grass"], SEG, 16, 7.0, 0, 9, 40))
            for i in range(30):  # the Great Wall riding the ridge
                x = -SEG / 2 + 1 + i * 2.0
                z = 4.5 + 2.5 * math.sin(x * 0.11)
                P.append(box(M["stone"], 2.1, 1.4, 1.6, x, 9.0 + 2.0 * math.sin(x * 0.07), z))
                if i % 2 == 0:
                    P.append(box(M["stone"], 0.6, 1.5, 0.5, x, 9.0 + 2.0 * math.sin(x * 0.07), z + 1.6))
                if i % 10 == 5:
                    P.append(box(M["trim"], 3.0, 3.0, 3.6, x, 9.0 + 2.0 * math.sin(x * 0.07), z))
        else:
            for x, s in ((-20, 14), (-6, 22), (8, 16), (22, 26)):
                m = cone(M["rock"], s * 0.45, s * 0.08, s * 1.2, 10, x, 18 + rnd.uniform(-3, 3), 0)
                rocky(m, 1.2, 4)
                P.append(m)
            P.append(hills(M["grass"], SEG, 12, 2.0, 0, 10, 24))
    return join(P, f"{culture}_bg_{layer}", bevel=0)


COLOURS = {  # sand/grass, water, rock, leaf, trunk, salt
    "mesopotamia": dict(sand="#C9A46C", water="#3D7FA6", rock="#9C8664", leaf="#4E7A3A", trunk="#6B4E32", salt="#EEE8DA", grass="#8E9A4E"),
    "egipto": dict(sand="#E2C38A", water="#2F8DA3", rock="#B89A6A", leaf="#5C8C3E", trunk="#6B4E32", salt="#F2EAD6", grass="#9AA65A"),
    "grecia": dict(sand="#D8C9A8", water="#2E6FB0", rock="#A39E94", leaf="#5E7D45", trunk="#6E5236", salt="#FFFFFF", grass="#8C9A55"),
    "china": dict(sand="#B9A684", water="#4A7FA0", rock="#7D8A86", leaf="#2F5E3A", trunk="#5A4030", salt="#E8E8E0", grass="#6E8C4E"),
}


def build(culture, out, blend_dir, bake=True, size=kits.BAKE_SIZE):
    kits.clear()
    os.makedirs(out, exist_ok=True)
    col = COLOURS[culture]
    M = {k: flat(f"{culture}_bg_{k}", v, 0.9) for k, v in col.items()}
    M["stone"] = day_material(culture + "_bg_stone", DAY[culture]["stone"])
    M["trim"] = day_material(culture + "_bg_trim", DAY[culture]["trim"])
    names = []
    for i, layer in enumerate(LAYERS):
        o = build_layer(culture, layer, M)
        o.location = (0, i * 50.0, 0)
        if bake:
            kits.bake_maps(o, out, size, kinds=("albedo",))
        kits.export_fbx(o, out)
        names.append(o.name)
    if blend_dir:
        os.makedirs(blend_dir, exist_ok=True)
        bpy.ops.wm.save_as_mainfile(filepath=os.path.join(blend_dir, culture + "_backdrop.blend"))
    return names


if __name__ == "__main__":
    a = sys.argv[sys.argv.index("--") + 1:] if "--" in sys.argv else []
    culture = a[a.index("--culture") + 1]
    out = a[a.index("--out") + 1]
    blend_dir = a[a.index("--blend-dir") + 1] if "--blend-dir" in a else ""
    size = int(a[a.index("--size") + 1]) if "--size" in a else kits.BAKE_SIZE
    print("BACKDROP_OK", build(culture, out, blend_dir, bake="--no-bake" not in a, size=size))
