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
from mathutils import Matrix

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


def frond(mat, length, x, y, z, tilt, heading, r=0.35):
    """Leaf cone pivoted at its BASE (ODD S32, T28): rotating around the cone centre swung the base ~1.2 m away from
    the trunk top, so the crown floated beside the trunk."""
    o = cone(mat, r, 0.0, length, 4, 0, 0, 0)
    o.data.transform(Matrix.Translation((0, 0, length / 2)))  # mesh starts at the origin
    o.location = (x, y, z)
    o.rotation_euler = (tilt, 0, heading)
    return o


def palm(trunk, leaf, x, y, h=6.0):
    top = h - 0.15
    parts = [cone(trunk, 0.22, 0.14, h, 8, x, y, 0), cone(leaf, 0.24, 0.05, 0.5, 6, x, y, top - 0.2)]  # crown knob
    for i in range(7):
        parts.append(frond(leaf, 2.6, x, y, top, math.radians(68 + 10 * (i % 2)), i * 2 * math.pi / 7))
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
    """Peristyle temple (ODD S37, T38): columns on all four sides under the roof, not only a front row."""
    parts = [box(trim, cols * 2.4, 6.0, 0.8 + z, x, y, 0)]
    x0 = x - (cols - 1) * 1.2
    for row, dy in enumerate((-2.0, 2.0)):  # front and back rows
        for i in range(cols):
            ch = h * (0.45 if i in broken and row == 0 else 1.0)
            parts.append(cone(stone, 0.5, 0.42, ch, 16, x0 + i * 2.4, y + dy, 0.8 + z))
    for sx in (x0, x0 + (cols - 1) * 2.4):  # side columns between the corner ones
        parts.append(cone(stone, 0.5, 0.42, h * (0.45 if broken else 1.0), 16, sx, y, 0.8 + z))
    if not broken:
        parts.append(box(trim, cols * 2.4, 6.0, 0.9, x, y, h + 0.8 + z))
    return parts


# ---------------- T32 (ODD S32): working people and more culture landmarks ----------------
def _xf(o, m):
    """Rebase the primitive so its mesh starts at z = 0 on its own axis, then bake matrix m into the mesh."""
    o.location = (0, 0, 0)
    z0 = min(v.co.z for v in o.data.vertices)
    o.data.transform(m @ Matrix.Translation((0, 0, -z0)))
    return o


def figure(M, x, y, z=0.0, pose="stand", face=0.0, hat=False, load=None):
    """Low-poly worker, 1.7 m. pose: stand | bend (moulding bricks, planting) | pull (hauling a rope, leaning forward)
    | carry (basket on the head or a pole on the shoulder: load = 'basket' | 'pole') | hoe. Faces -Y rotated by face."""
    tilt = {"stand": 0, "bend": 65, "pull": 35, "carry": 0, "hoe": 25}[pose]
    world = Matrix.Translation((x, y, z)) @ Matrix.Rotation(face, 4, "Z")
    hip = Matrix.Translation((0, 0, 0.85)) @ Matrix.Rotation(math.radians(-tilt), 4, "X")  # lean towards -Y
    parts = []
    for sx in (-1, 1):  # legs (pull: one leg back)
        leg = cone(M["cloth"], 0.08, 0.06, 0.85, 6)
        lean = Matrix.Rotation(math.radians(18 if (pose == "pull" and sx > 0) else 0), 4, "X")
        parts.append(_xf(leg, world @ Matrix.Translation((sx * 0.1, 0, 0)) @ lean @ Matrix.Translation((0, 0, 0))))
    parts.append(_xf(cone(M["cloth"], 0.2, 0.15, 0.55, 6), world @ hip))                      # torso / tunic
    head = cone(M["skin"], 0.12, 0.1, 0.24, 6)
    parts.append(_xf(head, world @ hip @ Matrix.Translation((0, 0, 0.6))))
    if hat:  # conical straw hat
        parts.append(_xf(cone(M["straw"], 0.3, 0.0, 0.16, 8), world @ hip @ Matrix.Translation((0, 0, 0.8))))
    for sx in (-1, 1):
        arm = cone(M["skin"], 0.05, 0.04, 0.55, 5)
        if pose in ("pull", "bend", "hoe"):
            # world direction of the arm: bend hangs straight down, pull reaches forward, hoe forward-down
            a = Matrix.Rotation(math.radians({"bend": 0, "pull": -70, "hoe": -45}[pose] + tilt), 4, "X")
        elif pose == "carry" and load == "basket":
            a = Matrix.Rotation(math.radians(sx * 160), 4, "Y")  # both hands up to the basket
        else:
            a = Matrix.Rotation(math.radians(sx * 8), 4, "Y")
        parts.append(_xf(arm, world @ hip @ Matrix.Translation((sx * 0.22, 0, 0.5)) @ a @ Matrix.Translation((0, 0, -0.5))))
    if load == "basket":
        parts.append(_xf(cone(M["straw"], 0.14, 0.24, 0.22, 8), world @ hip @ Matrix.Translation((0, 0, 0.74))))
    elif load == "pole":
        pole = cyl(M["trunk"], 0.025, 1.6, 5)
        parts.append(_xf(pole, world @ hip @ Matrix.Translation((0, 0, 0.55)) @ Matrix.Rotation(math.pi / 2, 4, "Y") @ Matrix.Translation((0, 0, -0.8))))
        for sx in (-1, 1):
            parts.append(_xf(cone(M["straw"], 0.16, 0.2, 0.25, 6), world @ hip @ Matrix.Translation((sx * 0.75, 0, 0.12))))
    if pose == "hoe":
        hoe = cyl(M["trunk"], 0.02, 1.3, 5)
        parts.append(_xf(hoe, world @ hip @ Matrix.Translation((0, -0.35, 0.3)) @ Matrix.Rotation(math.radians(140), 4, "X")))
    return parts


def ground(hill, x, y):
    """Height of the hills strip at (x, y): nearest grid vertex (strips are built at the origin)."""
    best = min(hill.data.vertices, key=lambda v: (v.co.x - x) ** 2 + (v.co.y - y) ** 2)
    return best.co.z


def wall_line(hill, x):
    """Great Wall course at x: (y, foundation base, walkway height). The walkway rides 1.6 m over the ground at the
    centre and the body reaches 0.3 m under the lowest ground of its 2.1 x 1.4 footprint, so it never floats."""
    y = 9.0 + 2.0 * math.sin(x * 0.07)
    lowest = min(ground(hill, x + dx, y + dy) for dx in (-1.05, 1.05) for dy in (-0.7, 0.7))
    return y, lowest - 0.3, ground(hill, x, y) + 1.6


def boat(M, x, y, z, l=4.0, crew=1, rnd=None):
    parts = [box(M["trunk"], l, 1.0, 0.35, x, y, z), cone(M["trunk"], 0.5, 0.05, 0.6, 4, x - l / 2 - 0.25, y, z)]
    parts[-1].rotation_euler = (0, math.radians(-70), math.pi / 4)
    for i in range(crew):
        parts += figure(M, x - l * 0.25 + i * l * 0.4, y, z + 0.3, "bend" if i == 0 else "stand", face=math.pi / 2)
    return parts


def extras(culture, layer, M, rnd, hill):
    P = []
    g = (lambda x, y: ground(hill, x, y)) if hill else (lambda x, y: 0.0)
    if culture == "mesopotamia":
        if layer == "near":  # brick makers: moulds, drying rows, stacks
            for r in range(3):
                for i in range(14):
                    x, y = -26 + i * 0.6, 1.0 + r * 0.55
                    P.append(box(M["clay"], 0.36, 0.22, 0.09, x, y, g(x, y)))
            for i, (x, pose, load) in enumerate(((-17.0, "bend", None), (-15.5, "bend", None), (-13.0, "carry", "basket"), (-28.5, "stand", None))):
                P += figure(M, x, 1.6 + 0.3 * (i % 2), g(x, 1.6), pose, face=rnd.uniform(-0.6, 0.6), load=load)
            for x in (-11.0, -10.0):
                for k in range(4):
                    P.append(box(M["clay"], 0.8, 0.6, 0.2, x, 1.4, g(x, 1.4) + k * 0.2))
            for i, x in enumerate((8.0, 10.0, 12.0)):  # basket line to the river
                P += figure(M, x, 2.2, g(x, 2.2), "carry", face=math.pi / 2, load="basket")
            P += boat(M, 20, 4.0, 0.12, 4.5, 2, rnd)  # reed boat on the river
        elif layer == "mid":  # city gate with glazed towers, doors and windows
            for sx in (-1, 1):
                P.append(box(M["stone"], 4, 2.4, 6.5, sx * 3.6, 9.0, 0))
                P.append(box(M["trim"], 4.1, 2.5, 1.0, sx * 3.6, 9.0, 4.2))  # glazed band
                for k in range(3):
                    P.append(box(M["stone"], 0.9, 2.5, 0.8, sx * 3.6 - 1.4 + k * 1.4, 9.0, 6.5))
            P.append(box(M["trim"], 3.2, 2.4, 1.2, 0, 9.0, 4.0))
            P.append(box(M["dark"], 2.2, 0.2, 4.0, 0, 7.75, 0))  # gate opening
            for i in range(10):
                x = -SEG / 2 + 3 + i * 6
                P.append(box(M["dark"], 0.7, 0.1, 1.2, x - 0.6, 2.45, g(x, 2.5)))  # doors on the houses' fronts
                P.append(box(M["dark"], 0.5, 0.1, 0.4, x + 0.9, 2.45, g(x, 2.5) + 1.2))
            for x in (-8.0, -6.5, 6.0, 7.5):
                P += figure(M, x, 1.6, g(x, 1.6), "carry", face=rnd.uniform(-0.4, 0.4), load="basket" if x < 0 else None)
        else:  # temple on the big ziggurat top and a palm grove
            P.append(box(M["trim"], 4.0, 3.5, 2.6, -12, 10, 10.4))
            for i in range(8):
                P += palm(M["trunk"], M["leaf"], -SEG / 2 + 3 + i * 7.5, 3.0 + rnd.uniform(-1, 1), rnd.uniform(4, 6))
    elif culture == "egipto":
        if layer == "near":  # haulers pulling a block on a sledge, a water pourer, reed boats on the Nile
            x0 = -12.0
            P.append(box(M["trunk"], 3.6, 1.6, 0.25, x0, 1.6, g(x0, 1.6)))
            P.append(box(M["stone"], 3.0, 1.4, 1.6, x0, 1.6, g(x0, 1.6) + 0.25))
            P.append(box(M["trunk"], 9.0, 0.05, 0.05, x0 - 6.3, 1.6, g(x0, 1.6) + 0.95))  # rope
            for i in range(6):
                x = x0 - 3.2 - i * 1.4
                P += figure(M, x, 1.35 + 0.5 * (i % 2), g(x, 1.6), "pull", face=-math.pi / 2)
            P += figure(M, x0 + 2.4, 1.2, g(x0 + 2.4, 1.2), "bend", face=math.pi / 2)  # pours water under the sledge
            P += boat(M, 10, 4.5, 0.12, 4.5, 2, rnd)
            P += boat(M, 22, 5.2, 0.12, 3.5, 1, rnd)
        elif layer == "mid":  # pylon gateway between two obelisks
            for sx in (-1, 1):
                P.append(cone(M["stone"], 2.6, 1.9, 7.0, 4, 3 + sx * 3.0, 8, 0, twist=math.pi / 4))
                P.append(cone(M["trim"], 0.45, 0.28, 6.0, 4, 3 + sx * 6.6, 6, 0, twist=math.pi / 4))
                P.append(cone(M["gold"], 0.28, 0.0, 0.5, 4, 3 + sx * 6.6, 6, 6.0, twist=math.pi / 4))
            P.append(box(M["trim"], 2.0, 1.5, 5.0, 3, 8, 0))
            P.append(box(M["dark"], 1.2, 0.2, 3.6, 3, 7.2, 0))
            for x in (-2.0, 0.0):
                P += figure(M, x, 2.0, g(x, 2.0), "stand", face=rnd.uniform(-0.5, 0.5))
        else:  # pyramid under construction with its ramp and a column of haulers
            P.append(cone(M["stone"], 9.0, 4.0, 6.0, 4, 26, 12, 0, twist=math.pi / 4))
            ramp = box(M["sand"], 2.5, 14, 0.6, 26, 2.5, 0)
            ramp.rotation_euler = (math.radians(24), 0, 0)
            P.append(ramp)
            for i in range(5):
                P += figure(M, 26 + rnd.uniform(-0.6, 0.6), 1.5 + i * 1.4, 0.5 + i * 0.62, "pull", face=math.pi)
    elif culture == "grecia":
        if layer == "near":  # fishermen in their boats, a pier with amphorae
            P += boat(M, -14, 7.0, -0.45, 4.0, 2, rnd)
            P += boat(M, 6, 11.0, -0.45, 3.5, 1, rnd)
            P.append(box(M["salt"], 3.0, 2.0, 0.02, -14, 8.6, -0.48))  # net on the water
            P.append(box(M["trunk"], 2.0, 7.0, 0.25, 18, 3.5, -0.3))   # pier
            for k in range(4):
                P.append(cyl(M["trunk"], 0.12, 1.0, 6, 17.2 + (k % 2) * 1.6, 1 + (k // 2) * 4, -1.0))
            for k in range(4):
                P.append(cone(M["clay"], 0.05, 0.22, 0.6, 10, 17.5 + k * 0.35, 1.6, -0.05))
            P += figure(M, 18.3, 4.5, -0.05, "carry", face=0.0, load="basket")
        elif layer == "mid":  # olive grove on the slope, potters by their kiln
            for i in range(9):
                x, y = -26 + i * 2.6 + rnd.uniform(-0.5, 0.5), 3.0 + rnd.uniform(-1, 1)
                z = g(x, y)
                P.append(cone(M["trunk"], 0.14, 0.08, 1.4, 6, x, y, z))
                bpy.ops.mesh.primitive_ico_sphere_add(subdivisions=1, radius=0.9, location=(x, y, z + 1.9))
                crown = bpy.context.active_object
                crown.data.materials.append(M["leaf"])
                P.append(crown)
            xk = 20.0
            bpy.ops.mesh.primitive_uv_sphere_add(segments=12, ring_count=6, radius=1.2, location=(xk, 2.5, g(xk, 2.5)))
            P.append(bpy.context.active_object)
            P[-1].data.materials.append(M["clay"])
            for k in range(6):
                x = xk - 3.5 + k * 0.45
                P.append(cone(M["clay"], 0.05, 0.2, 0.55, 10, x, 1.4, g(x, 1.4)))
            P += figure(M, xk - 1.8, 1.8, g(xk - 1.8, 1.8), "bend", face=0.4)
            P += figure(M, xk + 1.8, 1.6, g(xk + 1.8, 1.6), "carry", face=-0.5, load="basket")
        else:  # a trireme and a temple on the big island
            P.append(box(M["trunk"], 10, 1.6, 0.8, -6, 12, -0.6))
            P.append(cone(M["trunk"], 0.8, 0.1, 1.6, 4, -11.6, 12, -0.6))
            P[-1].rotation_euler = (0, math.radians(-75), math.pi / 4)
            P.append(box(M["salt"], 0.08, 3.0, 2.6, -6, 12, 0.2))  # sail
            for k in range(10):
                oar = box(M["trunk"], 0.08, 2.6, 0.06, -10 + k * 0.9, 12, -0.4)
                oar.rotation_euler = (math.radians(20), 0, 0)
                P.append(oar)
            P += temple(M["stone"], M["trim"], 4, 24, 4, 3.0, z=6.4)
    else:  # china
        if layer == "near":  # rice terraces with farmers in straw hats
            for t in range(4):
                P.append(box(M["grass"], 20 - t * 3, 2.2, 0.6 + t * 0.6, 17, 6.5 + t * 2.0, 0))
                P.append(box(M["water"], 19.6 - t * 3, 1.8, 0.02, 17, 6.5 + t * 2.0, 0.6 + t * 0.6))
                for k in range(3):
                    x = 10 + k * 5 + t * 1.5
                    if x < 17 + (20 - t * 3) / 2 - 1:
                        P += figure(M, x, 6.5 + t * 2.0, 0.62 + t * 0.6, "bend" if (k + t) % 2 else "hoe", face=rnd.uniform(-0.8, 0.8), hat=True)
        elif layer == "mid":  # wall builders carrying loads on shoulder poles, scaffolding at a tower
            for i, x in enumerate((-20.0, -12.0, -3.0, 8.0, 16.0)):
                y, _, walk = wall_line(hill, x)
                P += figure(M, x, y, walk, "carry", face=math.pi / 2, hat=True, load="pole")
            xs = -SEG / 2 + 1 + 15 * 2.0  # tower at i = 15
            ys, _, ws = wall_line(hill, xs)
            zs = ground(hill, xs, ys - 1.9)
            for k in range(4):
                P.append(box(M["trunk"], 0.08, 0.08, ws + 2.0 - zs, xs - 1.8 + k * 1.2, ys - 1.9, zs))
            for k in range(3):
                P.append(box(M["trunk"], 3.8, 0.4, 0.06, xs, ys - 1.9, zs + 1.2 + k * 1.3))
        else:  # pagoda on the foothills
            for t in range(5):
                w = 4.0 - t * 0.6
                P.append(box(M["stone"], w, w, 1.5, 2, 8, t * 1.8))
                P.append(cone(M["trim"], w * 0.95, w * 0.3, 0.45, 4, 2, 8, t * 1.8 + 1.5, twist=math.pi / 4))
            P.append(cone(M["gold"], 0.12, 0.0, 1.2, 6, 2, 8, 9.0))
    return P


def build_layer(culture, layer, M):
    rnd = random.Random(sum(map(ord, culture + layer)))  # stable across runs (str hash is salted)
    P = []
    hill = None
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
            hill = hills(M["grass"], SEG, 16, 7.0, 0, 9, 40)
            P.append(hill)
            for i in range(30):  # the Great Wall riding the ridge, standing on the hills (ODD S37, T38)
                x = -SEG / 2 + 1 + i * 2.0
                y, base, walk = wall_line(hill, x)
                P.append(box(M["stone"], 2.1, 1.4, walk - base, x, y, base))  # body + foundation down into the ground
                if i % 2 == 0:
                    P.append(box(M["stone"], 0.6, 1.5, 0.5, x, y, walk))
                if i % 10 == 5:
                    P.append(box(M["trim"], 3.0, 3.0, walk + 2.0 - base, x, y, base))
        else:
            for x, s in ((-20, 14), (-6, 22), (8, 16), (22, 26)):
                m = cone(M["rock"], s * 0.45, s * 0.08, s * 1.2, 10, x, 18 + rnd.uniform(-3, 3), 0)
                rocky(m, 1.2, 4)
                P.append(m)
            P.append(hills(M["grass"], SEG, 12, 2.0, 0, 10, 24))
    if hill is None:
        hill = next((o for o in P if o.name.startswith("Grid")), None)
    P += extras(culture, layer, M, rnd, hill)
    return join(P, f"{culture}_bg_{layer}", bevel=0)


FIGURE_COLOURS = {
    "mesopotamia": dict(skin="#8A5A3A", cloth="#E6D9BC", straw="#C9A75E", clay="#A8643A", dark="#2A2018", gold="#C99A2E"),
    "egipto": dict(skin="#8A5536", cloth="#F2EEE2", straw="#C9A75E", clay="#A8643A", dark="#231C14", gold="#D9A72E"),
    "grecia": dict(skin="#B07A55", cloth="#E9E4D8", straw="#C9A75E", clay="#C0683A", dark="#1E1A18", gold="#C99A2E"),
    "china": dict(skin="#B48660", cloth="#3E5A78", straw="#D2B36C", clay="#9C6A44", dark="#1E1E22", gold="#D9A72E"),
}

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
    for k, v in FIGURE_COLOURS[culture].items():  # workers, clay, dark openings (T32)
        M[k] = flat(f"{culture}_bg_{k}", v, 0.85)
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
