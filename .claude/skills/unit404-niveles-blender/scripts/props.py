"""UNIT-404 culture props (ODD S28-S29, T30-T31): set dressing that makes each level read as its culture.
blender -b --python props.py -- --culture grecia --out Assets/Integrante4/Art/Grecia --blend-dir "Assets/Integrante4/Art/BlenderSource~"

Every prop is base-centred (origin at the centre of its footprint, on the ground) and faces -Y in Blender, which is
-Z in Unity: towards the 2.5D camera. Its size is NOMINAL (Unity axes x = width, y = height, z = depth); Unity scales a
prop to the box it is given (LevelSceneBuilder.PropSize must match this table). In the 2.5D levels the props stand on
a decorative ground behind the path, without colliders; in the 3D worlds they stand on walkable floors as solid blocks
(the grey block keeps the collider, the prop renders) and frame the corridor gates.
Maps: <culture>_<prop>_albedo (smoothness in alpha) | normal | occlusion, plus emission for the props that glow."""
import bpy, sys, os, math, random

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import kits  # noqa: E402
from kits import box, cone, join, rocky, day_material, DAY, PALETTE  # noqa: E402
from elements import flat, cyl, prism, WOOD  # noqa: E402
from backdrops import frond  # noqa: E402

PROP_BAKE = 1024

# Unity size (width x, height y, depth z) of every prop: keep in sync with LevelSceneBuilder.PropSize.
NOMINAL = {
    # Mesopotamia
    "hut": (4.0, 3.2, 3.0), "jars": (1.6, 1.3, 1.4), "bricks": (1.8, 1.1, 1.2), "brazier": (1.0, 1.4, 1.0),
    "lamassu": (1.4, 2.6, 3.0), "palm": (3.5, 6.5, 3.5),
    # Egipto
    "obelisk": (1.2, 6.0, 1.2), "sphinx": (1.6, 2.0, 3.6), "pharaoh": (1.4, 3.2, 1.8), "papyrus": (1.8, 2.2, 1.8),
    "stele": (3.0, 2.8, 0.6),
    # Grecia
    "amphorae": (1.4, 1.3, 1.2), "kouros": (1.1, 3.4, 1.1), "olive": (3.0, 3.8, 3.0), "altar": (2.0, 1.3, 1.3),
    "lintel_gr": (8.4, 1.4, 1.4),
    # China
    "lantern": (1.1, 2.4, 1.25), "lion": (1.3, 2.2, 1.9), "bamboo": (1.8, 4.8, 1.8), "banner": (1.3, 5.1, 0.4),
    "lintel_cn": (9.0, 1.6, 1.6),
}
SETS = {
    "mesopotamia": ["hut", "jars", "bricks", "brazier", "lamassu", "palm"],
    "egipto": ["obelisk", "sphinx", "pharaoh", "papyrus", "stele", "palm", "brazier"],
    "grecia": ["amphorae", "kouros", "olive", "altar", "lintel_gr", "brazier"],
    "china": ["lantern", "lion", "bamboo", "banner", "lintel_cn"],
}
GLOWS = {"brazier", "lantern"}  # these carry the culture accent (embers, lamp light)
REED = dict(kind="brick", c=("#C9B06A", "#B39A55", "#8C7640", 0.08, 1.6), rough=0.95, bump=0.6)  # vertical reed bundles


def size(kind):
    w, h, d = NOMINAL[kind]
    return w, d, h  # Blender x, y, z


def plinth(mat, w, d, h=0.3):
    return box(mat, w, d, h)


# ---------------- Mesopotamia ----------------
def hut(c, M):
    """Mudhif reed house: barrel vault of reed arches over a rectangular plan, entrance with lattice on -Y."""
    w, d, h = size("hut")
    reed = day_material(c + "_reed", REED)
    parts = [box(M["trim"], w, d, 0.15)]
    bpy.ops.mesh.primitive_cylinder_add(vertices=24, radius=1.0, depth=d * 0.95)
    vault = bpy.context.active_object
    vault.rotation_euler = (math.pi / 2, 0, 0)
    vault.scale = (w / 2 * 0.92, h - 0.4, 1.0)
    vault.location = (0, 0, 0.15)
    vault.data.materials.append(reed)
    bpy.context.view_layer.update()
    bpy.ops.object.transform_apply(location=True, rotation=True, scale=True)
    for v in vault.data.vertices:  # keep the upper half (the vault), flatten the lower one onto the floor
        v.co.z = max(v.co.z, 0.15)
    parts.append(vault)
    for i in range(7):  # reed arch ribs
        y = -d * 0.45 + i * d * 0.15
        bpy.ops.mesh.primitive_torus_add(major_radius=1.0, minor_radius=0.06, major_segments=24, minor_segments=6)
        rib = bpy.context.active_object
        rib.rotation_euler = (math.pi / 2, 0, 0)
        rib.scale = (w / 2 * 0.95, h - 0.25, 1.0)
        rib.location = (0, y, 0.15)
        rib.data.materials.append(reed)
        bpy.context.view_layer.update()
        bpy.ops.object.transform_apply(location=True, rotation=True, scale=True)
        for v in rib.data.vertices:
            v.co.z = max(v.co.z, 0.15)
        parts.append(rib)
    parts.append(box(M["wood"], 1.0, 0.1, 1.7, 0, -d * 0.49, 0.15))  # door
    for k in range(4):  # lattice bands around the door
        parts.append(box(reed, 2.2, 0.08, 0.05, 0, -d * 0.5, 0.5 + k * 0.45))
    return parts


def jars(c, M):
    clay = M["clay"]
    parts = []
    for x, y, s in ((-0.4, 0.1, 1.0), (0.35, -0.2, 0.85), (0.2, 0.4, 0.7), (-0.15, -0.4, 0.6)):
        h = 1.25 * s
        body = cone(clay, 0.12 * s, 0.32 * s, h * 0.55, 16, x, y, 0)
        shoulder = cone(clay, 0.32 * s, 0.14 * s, h * 0.35, 16, x, y, h * 0.55)
        lip = cyl(M["trim"], 0.15 * s, h * 0.1, 16, x, y, h * 0.9)
        parts += [body, shoulder, lip]
    return parts


def bricks(c, M):
    parts = [box(M["wood"], 1.8, 1.2, 0.1)]  # pallet
    for layer in range(5):
        for i in range(4):
            for j in range(3):
                if layer == 4 and (i + j) % 2:
                    continue  # uneven top course
                rot = layer % 2
                parts.append(box(M["stone"], 0.4, 0.36, 0.18, -0.66 + i * 0.44 + (0.04 if rot else 0), -0.4 + j * 0.4, 0.1 + layer * 0.2))
    return parts


def brazier(c, M):
    glow = M["glow"]
    return [box(M["trim"], 0.8, 0.8, 0.2), cone(M["stone"], 0.18, 0.12, 0.8, 12, base_z=0.2),
            cone(M["trim"], 0.18, 0.48, 0.3, 16, base_z=1.0),
            cone(glow, 0.38, 0.0, 0.18, 12, base_z=1.22), cone(glow, 0.12, 0.0, 0.14, 6, 0.15, 0.05, 1.25)]


def lamassu(c, M):
    """Winged human-headed bull (Assyrian gate guardian), stylised: plinth, bull body, folded wing, bearded head with
    a horned tiara. Faces -Y."""
    w, d, h = size("lamassu")
    s = M["stone"]
    parts = [plinth(M["trim"], w, d, 0.35)]
    parts.append(box(s, w * 0.6, d * 0.62, 0.95, 0, 0.1, 1.0))           # body
    for x in (-0.3, 0.3):
        for y in (-0.75, 0.9):
            parts.append(box(s, 0.28, 0.32, 0.7, x, y, 0.35))           # legs
    wing = box(s, 0.12, d * 0.62, 0.9, w * 0.32, 0.25, 1.4)             # folded wing (relief), both sides
    wing.rotation_euler = (math.radians(-12), 0, 0)
    wing2 = box(s, 0.12, d * 0.62, 0.9, -w * 0.32, 0.25, 1.4)
    wing2.rotation_euler = (math.radians(-12), 0, 0)
    for k in range(4):  # feather rows
        for sx in (-1, 1):
            parts.append(box(M["trim"], 0.04, d * 0.5, 0.05, sx * w * 0.39, 0.3, 1.55 + k * 0.18))
    parts += [wing, wing2]
    parts.append(box(s, 0.55, 0.5, 0.6, 0, -d * 0.36, 1.75))              # head
    parts.append(box(M["trim"], 0.5, 0.18, 0.45, 0, -d * 0.45, 1.5))      # beard
    parts.append(cyl(M["trim"], 0.3, 0.32, 16, 0, -d * 0.36, 2.28))       # tiara
    return parts


def palm(c, M):
    w, d, h = size("palm")
    h -= 0.75  # trunk height: the fronds rise ~0.75 m above the crown
    parts = [cone(M["trunk"], 0.24, 0.15, h, 10, 0, 0, 0), cone(M["leaf"], 0.26, 0.05, 0.5, 6, 0, 0, h - 0.35)]
    for k in range(int(h / 0.45)):  # trunk scars
        parts.append(cyl(M["trunk"], 0.25 - 0.08 * k / (h / 0.45), 0.08, 10, 0, 0, k * 0.45))
    for i in range(8):
        parts.append(frond(M["leaf"], 2.0, 0, 0, h - 0.15, math.radians(64 + 12 * (i % 2)), i * 2 * math.pi / 8, 0.3))
    return parts


# ---------------- Egipto ----------------
def obelisk(c, M):
    w, d, h = size("obelisk")
    parts = [box(M["trim"], w, d, 0.4), cone(M["stone"], 0.5, 0.32, h - 0.9, 4, base_z=0.4, twist=math.pi / 4),
             cone(M["gold"], 0.32, 0.0, 0.5, 4, base_z=h - 0.5, twist=math.pi / 4)]
    for k in range(6):  # carved cartouche bands on the front face
        parts.append(box(M["trim"], 0.22, 0.04, 0.32, 0, -0.33 + k * 0.012, 1.0 + k * 0.7))
    return parts


def sphinx(c, M):
    w, d, h = size("sphinx")
    s = M["stone"]
    parts = [plinth(M["trim"], w, d, 0.3), box(s, w * 0.62, d * 0.62, 0.7, 0, 0.35, 0.3)]  # lying lion body
    for x in (-0.25, 0.25):
        parts.append(box(s, 0.28, d * 0.38, 0.25, x, -d * 0.3, 0.3))   # forepaws
    parts.append(box(s, 0.62, 0.7, 0.7, 0, -d * 0.18, 0.95))            # chest
    parts.append(box(s, 0.5, 0.45, 0.55, 0, -d * 0.24, 1.45))           # head
    nemes = cone(M["stripe"], 0.42, 0.25, 0.55, 4, 0, -d * 0.2, 1.3, twist=math.pi / 4)  # striped headdress
    parts += [nemes, box(M["stripe"], 0.8, 0.16, 0.55, 0, -d * 0.12, 1.1)]
    return parts


def pharaoh(c, M):
    w, d, h = size("pharaoh")
    s = M["stone"]
    parts = [plinth(M["trim"], w, d, 0.4), box(s, w * 0.7, d * 0.8, 0.9, 0, 0.1, 0.4)]  # throne
    parts += [box(s, 0.7, 0.5, 1.0, 0, 0.0, 1.3), box(s, 0.6, 0.5, 0.5, 0, -0.35, 0.95)]  # torso, lap
    for x in (-0.18, 0.18):
        parts.append(box(s, 0.2, 0.3, 0.55, x, -0.7, 0.4))                 # shins
    parts.append(box(s, 0.38, 0.38, 0.42, 0, 0.0, 2.3))                     # head
    parts.append(cone(M["stripe"], 0.34, 0.2, 0.55, 4, 0, 0.05, 2.15, twist=math.pi / 4))  # nemes
    parts.append(cyl(M["gold"], 0.1, 0.18, 8, 0, -0.18, 2.72))              # uraeus
    return parts


def papyrus(c, M):
    parts = []
    rnd = random.Random(3)
    for i in range(9):
        a, r = rnd.uniform(0, 6.28), rnd.uniform(0, 0.6)
        x, y, ht = r * math.cos(a), r * math.sin(a), rnd.uniform(1.4, 2.0)
        stem = cone(M["leaf"], 0.03, 0.02, ht, 5, x, y, 0)
        stem.rotation_euler = (rnd.uniform(-0.15, 0.15), rnd.uniform(-0.15, 0.15), 0)
        parts += [stem, cone(M["leaf"], 0.02, 0.22, 0.25, 8, x, y, ht - 0.05)]  # umbel
    return parts


def stele(c, M):
    """Hieroglyph wall: painted limestone slab with rows of carved signs and a cavetto cornice."""
    w, d, h = size("stele")
    parts = [box(M["trim"], w, d, 0.3), box(M["stone"], w * 0.94, d * 0.7, h - 0.6, 0, 0, 0.3),
             box(M["trim"], w, d, 0.3, 0, 0, h - 0.3)]
    rnd = random.Random(5)
    for row in range(4):
        for col in range(9):
            sw, sh = rnd.uniform(0.08, 0.18), rnd.uniform(0.12, 0.26)
            m = M["stripe"] if (row + col) % 3 else M["gold"]
            parts.append(box(m, sw, 0.04, sh, -1.2 + col * 0.3, -d * 0.36, 0.55 + row * 0.48))
    return parts


# ---------------- Grecia ----------------
def amphorae(c, M):
    clay = M["clay"]
    parts = []
    for x, y, s in ((-0.35, 0.0, 1.0), (0.32, 0.18, 0.9), (0.05, -0.32, 0.75)):
        h = 1.25 * s
        parts += [cone(clay, 0.04 * s, 0.26 * s, h * 0.5, 16, x, y, 0), cone(clay, 0.26 * s, 0.1 * s, h * 0.32, 16, x, y, h * 0.5),
                  cyl(clay, 0.08 * s, h * 0.18, 12, x, y, h * 0.82)]
        for sx in (-1, 1):  # handles
            parts.append(box(clay, 0.04 * s, 0.05 * s, h * 0.25, x + sx * 0.14 * s, y, h * 0.66))
        parts.append(cyl(M["paint"], 0.255 * s, 0.07 * s, 16, x, y, h * 0.42))  # black-figure band
    return parts


def kouros(c, M):
    w, d, h = size("kouros")
    s = M["stone"]
    parts = [box(M["trim"], w, d, 0.6), box(s, 0.9, 0.9, 0.1, 0, 0, 0.6)]
    for x, y in ((-0.12, 0.1), (0.12, -0.15)):  # left foot forward
        parts.append(cone(s, 0.09, 0.12, 1.2, 10, x, y, 0.7))
    parts += [box(s, 0.5, 0.3, 0.9, 0, 0, 1.9), cone(s, 0.17, 0.2, 0.3, 10, 0, 0, 1.6)]  # torso, hips
    for x in (-0.31, 0.31):
        parts.append(cone(s, 0.07, 0.06, 0.95, 8, x, 0, 1.75))                       # arms at the sides
    parts += [cyl(s, 0.08, 0.15, 8, 0, 0, 2.8), box(s, 0.24, 0.27, 0.32, 0, 0, 2.95)]  # neck, head
    return parts


def olive(c, M):
    """Gnarled olive tree: twisted split trunk and lumpy silver-green canopy."""
    trunk = []
    for i, (x, y, tilt) in enumerate(((-0.12, 0.0, 0.2), (0.15, 0.08, -0.25))):
        t = cone(M["trunk"], 0.18, 0.1, 2.0, 8, x, y, 0)
        t.rotation_euler = (0, tilt, i * 0.6)
        trunk.append(t)
    rocky(trunk[0], 0.05, 2)
    rocky(trunk[1], 0.05, 2)
    parts = trunk
    rnd = random.Random(11)
    for k in range(6):
        bpy.ops.mesh.primitive_ico_sphere_add(subdivisions=2, radius=rnd.uniform(0.6, 0.85))
        b = bpy.context.active_object
        b.location = (rnd.uniform(-0.8, 0.8), rnd.uniform(-0.8, 0.8), rnd.uniform(2.3, 3.0))
        b.scale = (1.0, 1.0, 0.75)
        b.data.materials.append(M["olive"])
        rocky(b, 0.25, 2)
        parts.append(b)
    return parts


def altar(c, M):
    w, d, h = size("altar")
    parts = [box(M["trim"], w, d, 0.2), box(M["stone"], w * 0.85, d * 0.8, 0.75, 0, 0, 0.2),
             box(M["trim"], w, d, 0.15, 0, 0, 0.95)]
    for sx in (-1, 1):  # volutes
        parts.append(cyl(M["stone"], 0.12, d * 0.95, 12, sx * w * 0.42, 0, 1.15, axis="Y"))
    for k in range(5):  # garland triglyphs on the front
        parts.append(box(M["paint"], 0.08, 0.04, 0.4, -0.6 + k * 0.3, -d * 0.41, 0.35))
    return parts


def lintel_gr(c, M):
    """Architrave + frieze + pediment spanning a corridor gate (sits on two columns)."""
    w, d, h = size("lintel_gr")
    parts = [box(M["stone"], w, d, 0.45), box(M["trim"], w, d * 0.9, 0.35, 0, 0, 0.45)]
    for k in range(int(w / 0.7)):  # triglyphs
        parts.append(box(M["stone"], 0.18, 0.06, 0.3, -w / 2 + 0.35 + k * 0.7, -d * 0.47, 0.47))
    parts.append(prism(M["stone"], w, 0.6, d, base_z=0.8))
    return parts


# ---------------- China ----------------
def lantern(c, M):
    """Stone lantern (shidengLong): base, post, firebox with glowing windows, curved cap."""
    s = M["stone"]
    return [box(s, 0.9, 0.9, 0.25), cone(s, 0.22, 0.18, 0.9, 6, base_z=0.25), box(s, 0.7, 0.7, 0.12, 0, 0, 1.15),
            box(s, 0.5, 0.5, 0.5, 0, 0, 1.27), box(M["glow"], 0.52, 0.3, 0.3, 0, 0, 1.37), box(M["glow"], 0.3, 0.52, 0.3, 0, 0, 1.37),
            cone(M["trim"], 0.62, 0.1, 0.35, 6, base_z=1.77), cone(s, 0.08, 0.0, 0.28, 6, base_z=2.12)]


def lion(c, M):
    """Guardian lion (shishi) on a plinth, seated, one paw on a ball, curly mane."""
    w, d, h = size("lion")
    s = M["stone"]
    parts = [box(M["trim"], w, d, 0.5), box(s, 0.8, 1.2, 0.5, 0, 0.15, 0.5)]
    parts.append(cone(s, 0.42, 0.32, 0.8, 10, 0, -0.05, 0.9))  # chest
    for x in (-0.22, 0.22):
        parts.append(box(s, 0.18, 0.22, 0.75, x, -0.48, 0.5))  # front legs
    bpy.ops.mesh.primitive_uv_sphere_add(segments=16, ring_count=8, radius=0.18, location=(0.22, -0.72, 0.68))
    ball = bpy.context.active_object
    ball.data.materials.append(M["trim"])
    parts.append(ball)
    bpy.ops.mesh.primitive_ico_sphere_add(subdivisions=2, radius=0.42, location=(0, -0.2, 1.85))
    mane = bpy.context.active_object
    mane.data.materials.append(s)
    rocky(mane, 0.12, 2)
    parts += [mane, box(s, 0.42, 0.35, 0.36, 0, -0.5, 1.62)]  # muzzle
    return parts


def bamboo(c, M):
    parts = []
    rnd = random.Random(7)
    for i in range(10):
        a, r = rnd.uniform(0, 6.28), rnd.uniform(0, 0.6)
        x, y, ht = r * math.cos(a), r * math.sin(a), rnd.uniform(3.4, 4.8)
        parts.append(cyl(M["bamboo"], 0.06, ht, 8, x, y, 0))
        for k in range(1, int(ht / 0.5)):
            parts.append(cyl(M["trunk"], 0.07, 0.04, 8, x, y, k * 0.5))  # nodes
        for k in range(3):  # leaves pivot at their base on the stalk (same fix as the palm fronds, T28)
            parts.append(frond(M["leaf"], 0.7, x, y, ht - 0.3 - k * 0.4, math.radians(70), rnd.uniform(0, 6.28), 0.08))
    return parts


def banner(c, M):
    w, d, h = size("banner")
    parts = [box(M["stone"], 0.6, 0.4, 0.3), cyl(M["wood"], 0.06, h - 0.2, 8, 0, 0, 0.3),
             box(M["wood"], 1.1, 0.06, 0.06, 0.45, 0, h - 0.4)]
    flag = box(M["red"], 0.9, 0.03, 2.4, 0.5, 0, h - 2.9)
    parts += [flag, box(M["glow_cloth"], 0.5, 0.04, 0.5, 0.5, 0, h - 1.8)]  # emblem
    return parts


def lintel_cn(c, M):
    """Paifang top: red lacquer beams, jade band and a sweeping tiled roof (sits on two columns)."""
    w, d, h = size("lintel_cn")
    parts = [box(M["trim"], w * 0.92, d * 0.6, 0.4), box(M["jade"], w * 0.92, d * 0.62, 0.12, 0, 0, 0.4),
             box(M["trim"], w * 0.92, d * 0.6, 0.3, 0, 0, 0.52)]
    roof = prism(M["tile"], w * 0.94, 0.75, d, base_z=0.82)
    parts.append(roof)
    for sx in (-1, 1):  # upturned eaves
        e = cone(M["tile"], 0.12, 0.0, 0.5, 4, sx * w * 0.45, 0, 0.82)
        e.rotation_euler = (0, sx * math.radians(-50), 0)
        parts.append(e)
    return parts


BUILDERS = {k: globals()[k] for k in NOMINAL}

PLANT = {  # flat colours per culture
    "mesopotamia": dict(leaf="#4E7A3A", trunk="#6B4E32", clay="#B5793F", gold="#C99A2E", stripe="#1F4E9A", paint="#2B2B2B", olive="#7D8C5A", bamboo="#8FA14E", red="#A32019", jade="#3E9E73", tile="#3B3B3B"),
    "egipto": dict(leaf="#5C8C3E", trunk="#6B4E32", clay="#B5793F", gold="#D9A72E", stripe="#2E7FA3", paint="#2B2B2B", olive="#7D8C5A", bamboo="#8FA14E", red="#A32019", jade="#2E9C8F", tile="#3B3B3B"),
    "grecia": dict(leaf="#5E7D45", trunk="#5E4A36", clay="#C0683A", gold="#C99A2E", stripe="#2E6FB0", paint="#1E1A18", olive="#8A9A6A", bamboo="#8FA14E", red="#A32019", jade="#3E9E73", tile="#8E3A22"),
    "china": dict(leaf="#3F7A3A", trunk="#5A4030", clay="#9C6A44", gold="#C99A2E", stripe="#2E6FB0", paint="#2B2B2B", olive="#7D8C5A", bamboo="#8FA14E", red="#B0221A", jade="#3E9E73", tile="#3A3F44"),
}


def check_fit(o, kind):
    """The prop must fit its NOMINAL box (5 % slack) and stand on z = 0: Unity scales it to that box."""
    w, d, h = size(kind)
    xs = [v.co.x for v in o.data.vertices]
    ys = [v.co.y for v in o.data.vertices]
    zs = [v.co.z for v in o.data.vertices]
    ok = (max(xs) - min(xs) <= w * 1.05 and max(ys) - min(ys) <= d * 1.05 and max(zs) <= h * 1.05 and abs(min(zs)) < 0.02)
    print(f"FIT {o.name} {max(xs) - min(xs):.2f}x{max(ys) - min(ys):.2f}x{max(zs):.2f} vs {w}x{d}x{h} min_z {min(zs):.3f} {'OK' if ok else 'BAD'}")
    return ok


def build(culture, out, blend_dir, bake=True, size_px=PROP_BAKE, only=()):
    kits.clear()
    os.makedirs(out, exist_ok=True)
    M = {k: flat(f"{culture}_pr_{k}", v, 0.8) for k, v in PLANT[culture].items()}
    M["stone"] = day_material(culture + "_pr_stone", DAY[culture]["stone"])
    M["trim"] = day_material(culture + "_pr_trim", DAY[culture]["trim"])
    M["wood"] = day_material(culture + "_pr_wood", WOOD)
    M["glow"] = flat(culture + "_pr_glow", PALETTE[culture][1], 0.3, emit=1.0)
    M["glow_cloth"] = flat(culture + "_pr_emblem", "#E8C25A", 0.6)
    names, bad = [], []
    for i, kind in enumerate(SETS[culture]):
        if only and kind not in only:
            continue
        o = join(BUILDERS[kind](culture, M), f"{culture}_{kind}", bevel=0.01)
        if not check_fit(o, kind):
            bad.append(kind)
        o.location = (i * 10.0, 0.0, 0.0)
        if bake:
            kinds = kits.SHARP + (("emission",) if kind in GLOWS else ())
            kits.bake_maps(o, out, size_px, kinds=kinds)
        kits.export_fbx(o, out)
        names.append(o.name)
    if blend_dir and not only:
        os.makedirs(blend_dir, exist_ok=True)
        bpy.ops.wm.save_as_mainfile(filepath=os.path.join(blend_dir, culture + "_props.blend"))
    if bad:
        raise SystemExit("PROPS_BAD_FIT " + ",".join(bad))
    return names


if __name__ == "__main__":
    a = sys.argv[sys.argv.index("--") + 1:] if "--" in sys.argv else []
    culture = a[a.index("--culture") + 1]
    out = a[a.index("--out") + 1]
    blend_dir = a[a.index("--blend-dir") + 1] if "--blend-dir" in a else ""
    px = int(a[a.index("--size") + 1]) if "--size" in a else PROP_BAKE
    only = a[a.index("--only") + 1].split(",") if "--only" in a else ()
    print("PROPS_OK", build(culture, out, blend_dir, bake="--no-bake" not in a, size_px=px, only=only))
