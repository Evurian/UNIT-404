"""UNIT-404 level elements (ODD T18): art for the gameplay objects that keep their primitive collider.
blender -b --python elements.py -- --culture grecia --out Assets/Integrante4/Art/Grecia --blend-dir "Assets/Integrante4/Art/BlenderSource~"

Every element is modelled at its gameplay size (NOMINAL, Unity axes x, y, z) with its origin at the CENTRE of that box,
the way Unity primitives are centred. Unity adds it as a child of the primitive with localScale = primitive unit /
nominal, so it fills exactly the shape the collider has (see LevelSceneBuilder.ApplyElements). Accent parts glow
in the ONE culture accent (art style sheet); bodies use the culture's lit DAY materials. Maps baked per element:
<culture>_<element>_albedo|normal|emission.png. The portal arch is the exception: base-centred decoration placed
next to the arrival pad, no collider."""
import bpy, sys, os, math

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import kits  # noqa: E402
from kits import box, cone, join, rocky, day_material, DAY, PALETTE  # noqa: E402

ELEMENT_BAKE = 512
WOOD = dict(kind="brick", c=("#7A5233", "#66432A", "#3E2A1A", 2.4, 0.09), rough=0.85, bump=0.5)

# Unity size (x, y, z) of the gameplay primitive each element dresses; Blender builds x, depth = y, height = z.
NOMINAL = {
    "crystal": (0.6, 0.6, 0.6),      # Fragmento: sphere scaled 0.6
    "ring": (2.5, 0.3, 2.5),         # AnilloPropulsor: cylinder (2.5, 0.15, 2.5), unit height 2, rotated 90 deg X
    "tablet": (0.7, 1.0, 0.2),       # Glifo / Inscripcion / TiraBambu
    "checkpoint": (1.0, 4.0, 1.0),
    "torch": (0.6, 1.2, 0.6),        # Antorcha (level 2)
    "stake": (0.6, 2.0, 0.6),        # Estaca (level 1)
    "log": (1.0, 2.0, 1.0),          # Tronco: cylinder unit, lying across the path
    "drum": (1.0, 2.0, 1.0),         # ColumnaCaida: cylinder unit
    "cart": (2.0, 1.5, 8.0 / 3.0),   # Carreta (level 4): cube
}
COMMON = ["crystal", "ring", "tablet", "checkpoint", "arch"]
EXTRA = {"mesopotamia": ["stake"], "egipto": ["torch"], "grecia": ["log", "drum"], "china": ["cart"]}


def flat(name, hexc, rough=0.7, emit=0.0):
    m = bpy.data.materials.new(name)
    m.use_nodes = True
    nt = m.node_tree
    nt.nodes.clear()
    out = nt.nodes.new("ShaderNodeOutputMaterial")
    b = nt.nodes.new("ShaderNodeBsdfPrincipled")
    nt.links.new(b.outputs["BSDF"], out.inputs["Surface"])
    col = (*kits.hex_lin(hexc), 1.0)
    b.inputs["Base Color"].default_value = col
    b.inputs["Roughness"].default_value = rough
    if emit:
        b.inputs["Emission Color"].default_value = col
        b.inputs["Emission Strength"].default_value = emit
    return m


def cyl(mat, r, h, sides=16, cx=0.0, cy=0.0, base_z=0.0, axis="Z"):
    bpy.ops.mesh.primitive_cylinder_add(vertices=sides, radius=r, depth=h)
    o = bpy.context.active_object
    if axis == "Y":
        o.rotation_euler = (math.pi / 2, 0.0, 0.0)
        o.location = (cx, cy, base_z)
    else:
        o.location = (cx, cy, base_z + h / 2)
    o.data.materials.append(mat)
    return o


def prism(mat, w, h, d, base_z=0.0):
    """Triangular prism (pediment / gable): base w along X, apex h up, depth d along Y."""
    v = [(-w / 2, -d / 2, base_z), (w / 2, -d / 2, base_z), (0, -d / 2, base_z + h),
         (-w / 2, d / 2, base_z), (w / 2, d / 2, base_z), (0, d / 2, base_z + h)]
    f = [(0, 1, 2), (5, 4, 3), (0, 3, 4, 1), (1, 4, 5, 2), (2, 5, 3, 0)]
    me = bpy.data.meshes.new("prism")
    me.from_pydata(v, [], f)
    me.materials.append(mat)
    o = bpy.data.objects.new("prism", me)
    bpy.context.collection.objects.link(o)
    return o


# ---------------- elements (centred: z from -height/2 to +height/2) ----------------
def crystal(c, body, trim, glow):
    h = NOMINAL["crystal"][1]
    top = cone(glow, 0.2, 0.0, h * 0.55, 6, base_z=-h * 0.05)
    bottom = cone(glow, 0.0, 0.2, h * 0.45, 6, base_z=-h / 2)
    shards = [cone(glow, 0.07, 0.0, 0.22, 5, cx=0.17 * math.cos(a), cy=0.17 * math.sin(a), base_z=-0.12)
              for a in (0.4, 2.5, 4.4)]
    return [top, bottom, *shards]


def ring(c, body, trim, glow):
    w, t = NOMINAL["ring"][0], NOMINAL["ring"][1]
    bpy.ops.mesh.primitive_torus_add(major_radius=w / 2 - 0.16, minor_radius=0.12, major_segments=48, minor_segments=10)
    o = bpy.context.active_object
    o.data.materials.append(glow)
    parts = [o]
    for i in range(4):  # stone clamps hold the ring
        a = i * math.pi / 2 + math.pi / 4
        r = w / 2 - 0.16
        clamp = box(trim, 0.36, 0.36, t, r * math.cos(a), r * math.sin(a), -t / 2)
        clamp.rotation_euler = (0, 0, a)
        parts.append(clamp)
    return parts


def tablet(c, body, trim, glow):
    w, h, d = NOMINAL["tablet"][0], NOMINAL["tablet"][1], NOMINAL["tablet"][2]
    z0 = -h / 2
    parts = []
    if c == "mesopotamia":  # baked clay tablet, cuneiform rows
        slab = box(body, w * 0.9, d * 0.85, h * 0.9, base_z=z0 + h * 0.05)
        rocky(slab, 0.015, 2)
        parts.append(slab)
        marks = [(x, z) for z in (0.25, 0.1, -0.05, -0.2) for x in (-0.18, -0.06, 0.06, 0.18)]
        size = (0.07, 0.04)
    elif c == "egipto":  # round-topped stela, cartouche column
        parts += [box(body, w * 0.86, d * 0.8, h * 0.68, base_z=z0), cyl(body, w * 0.43, d * 0.8, 24, base_z=z0 + h * 0.68, axis="Y")]
        marks = [(0.0, z) for z in (0.22, 0.08, -0.06, -0.2, -0.34)]
        size = (0.16, 0.08)
    elif c == "grecia":  # stele with a pediment, inscription lines
        parts += [box(body, w * 0.8, d * 0.75, h * 0.78, base_z=z0), box(trim, w * 0.9, d * 0.85, 0.06, base_z=z0 + h * 0.78),
                  prism(trim, w * 0.9, h * 0.16, d * 0.85, base_z=z0 + h * 0.84)]
        marks = [(0.0, z) for z in (0.12, 0.02, -0.08, -0.18, -0.28)]
        size = (0.4, 0.03)
    else:  # china: bamboo strips tied with two cords
        for i in range(6):
            parts.append(cyl(body, 0.05, h * 0.94, 8, cx=-0.25 + i * 0.1, base_z=z0 + h * 0.03))
        for zc in (0.3, -0.3):
            parts.append(box(trim, w * 0.92, d * 0.7, 0.04, base_z=zc))
        marks = [(-0.25 + i * 0.1, z) for i in range(6) for z in (0.15, 0.0, -0.15)]
        size = (0.04, 0.06)
    for side in (-1, 1):  # glowing glyphs on both faces
        for x, z in marks:
            parts.append(box(glow, size[0], 0.02, size[1], x, side * d * 0.42, z))
    return parts


def checkpoint(c, body, trim, glow):
    h = NOMINAL["checkpoint"][1]
    z0 = -h / 2
    parts = [box(trim, 1.0, 1.0, 0.4, base_z=z0), box(body, 0.8, 0.8, 0.2, base_z=z0 + 0.4),
             cone(body, 0.5, 0.33, 2.9, 4, base_z=z0 + 0.6, twist=math.pi / 4),
             cone(glow, 0.33, 0.0, 0.4, 4, base_z=z0 + 3.5, twist=math.pi / 4)]
    for zc in (1.2, 2.3):  # glowing glyph bands around the shaft
        r = 0.5 - (0.17 * (zc - 0.6) / 2.9)
        parts.append(box(glow, r * 1.42 + 0.02, r * 1.42 + 0.02, 0.08, base_z=z0 + zc))
    parts.append(cone(glow, 0.0, 0.1, 0.05, 6, base_z=z0 + 3.9))
    return parts


def torch(c, body, trim, glow):
    h = NOMINAL["torch"][1]
    z0 = -h / 2
    return [box(trim, 0.5, 0.5, 0.15, base_z=z0), cone(body, 0.09, 0.07, 0.5, 8, base_z=z0 + 0.15),
            cone(trim, 0.13, 0.28, 0.2, 10, base_z=z0 + 0.65),
            cone(glow, 0.22, 0.0, 0.35, 7, base_z=z0 + 0.85), cone(glow, 0.1, 0.0, 0.25, 5, cx=0.1, base_z=z0 + 0.85)]


def stake(c, body, trim, glow):
    h = NOMINAL["stake"][1]
    z0 = -h / 2
    wood = day_material(c + "_wood", WOOD)
    parts = []
    for i in range(3):
        a = i * 2 * math.pi / 3
        x, y = 0.14 * math.cos(a), 0.14 * math.sin(a)
        parts.append(cyl(wood, 0.1, 1.6 - i * 0.12, 8, x, y, z0))
        parts.append(cone(wood, 0.1, 0.0, 0.4, 8, x, y, z0 + 1.6 - i * 0.12))
    parts.append(cyl(trim, 0.27, 0.12, 12, base_z=z0 + 0.7))
    return parts


def log(c, body, trim, glow):
    h = NOMINAL["log"][1]
    wood = day_material(c + "_wood", WOOD)
    bark = cyl(wood, 0.46, h * 0.94, 14, base_z=-h * 0.47)  # margin: the rock displacement must stay inside the collider
    rocky(bark, 0.03, 2)
    rings = flat(c + "_heartwood", "#C79A63", 0.8)
    return [bark, cyl(rings, 0.4, 0.04, 14, base_z=h * 0.47), cyl(rings, 0.4, 0.04, 14, base_z=-h * 0.47 - 0.04)]


def drum(c, body, trim, glow):
    h = NOMINAL["drum"][1]
    shaft = cone(body, 0.48, 0.46, h * 0.9, 40, base_z=-h * 0.45)
    for i, v in enumerate(shaft.data.vertices):  # Doric flutes
        if i % 2:
            v.co.x *= 0.92
            v.co.y *= 0.92
    return [shaft, cyl(trim, 0.5, h * 0.05, 40, base_z=-h / 2), cyl(trim, 0.5, h * 0.05, 40, base_z=h * 0.45)]


def cart(c, body, trim, glow):
    w, h, d = NOMINAL["cart"][0], NOMINAL["cart"][1], NOMINAL["cart"][2]
    z0 = -h / 2
    wood = day_material(c + "_wood", WOOD)
    parts = [box(wood, w * 0.9, d * 0.82, 0.12, base_z=z0 + 0.5)]
    for side in (-1, 1):
        parts.append(box(trim, w * 0.9, 0.08, 0.45, 0, side * d * 0.4, z0 + 0.62))  # lacquered side boards
        for x in (-0.55, 0.55):
            parts.append(cyl(wood, 0.4, 0.12, 16, x, side * d * 0.45, z0 + 0.4, axis="Y"))
            parts.append(cyl(trim, 0.1, 0.16, 8, x, side * d * 0.45, z0 + 0.4, axis="Y"))
    parts += [box(body, 0.7, d * 0.3, 0.55, -0.35, -d * 0.18, z0 + 0.62), box(body, 0.6, d * 0.3, 0.45, 0.4, d * 0.15, z0 + 0.62),
              box(trim, 0.08, 0.08, h - 0.7, w * 0.42, d * 0.36, z0 + 0.62)]
    return parts


def arch(c, body, trim, glow, w=6.6, h=6.0, d=1.0):
    """Portal gate over the arrival pad (base-centred, no collider): culture silhouette + glowing inner frame."""
    parts = []
    post = 0.8
    for sx in (-1, 1):
        x = sx * (w / 2 - post / 2)
        if c == "egipto":  # pylon towers (battered)
            parts.append(cone(body, post * 0.95, post * 0.7, h - 0.6, 4, x, 0, 0, twist=math.pi / 4))
        elif c == "grecia":
            parts.append(cone(body, post * 0.45, post * 0.38, h - 0.9, 24, x, 0, 0.3))
            parts.append(box(trim, post, post, 0.3, x, 0, 0))
        else:
            parts.append(box(body, post, d, h - 0.6, x, 0, 0))
        parts.append(box(glow, 0.08, d * 0.5, h - 1.0, sx * (w / 2 - post - 0.06), 0, 0.2))  # inner glow
    parts.append(box(trim, w + 0.4, d * 1.1, 0.6, 0, 0, h - 0.6))
    parts.append(box(glow, w - 2 * post - 0.2, d * 0.5, 0.08, 0, 0, h - 0.8))
    if c == "grecia":
        parts.append(prism(trim, w + 0.4, 1.0, d * 1.1, base_z=h))
    elif c == "china":  # paifang roof with a lower beam
        parts.append(box(trim, w * 0.9, d * 0.6, 0.3, 0, 0, h - 1.6))
        parts.append(prism(trim, w + 1.4, 1.1, d * 1.6, base_z=h))
    elif c == "mesopotamia":  # stepped crenellations
        for i in range(-3, 4):
            parts.append(box(body, 0.5, d, 0.5 + 0.2 * (3 - abs(i)), i * 0.95, 0, h))
    else:
        parts.append(box(trim, w + 0.8, d * 1.3, 0.3, 0, 0, h))  # cavetto cornice
    return parts


BUILDERS = {"crystal": crystal, "ring": ring, "tablet": tablet, "checkpoint": checkpoint, "torch": torch,
            "stake": stake, "log": log, "drum": drum, "cart": cart, "arch": arch}


def build(culture, out, blend_dir, bake=True, size=ELEMENT_BAKE):
    kits.clear()
    os.makedirs(out, exist_ok=True)
    body = day_material(culture + "_el_stone", DAY[culture]["stone"])
    trim = day_material(culture + "_el_trim", DAY[culture]["trim"])
    glow = flat(culture + "_glow", PALETTE[culture][1], 0.3, emit=1.0)
    names = []
    for i, kind in enumerate(COMMON + EXTRA[culture]):
        o = join(BUILDERS[kind](culture, body, trim, glow), f"{culture}_{kind}", bevel=0.01)
        o.location = (i * 8.0, 0.0, 0.0)
        if bake:
            kits.bake_maps(o, out, size)
        kits.export_fbx(o, out)
        names.append(o.name)
    if blend_dir:
        os.makedirs(blend_dir, exist_ok=True)
        bpy.ops.wm.save_as_mainfile(filepath=os.path.join(blend_dir, culture + "_elements.blend"))
    return names


if __name__ == "__main__":
    a = sys.argv[sys.argv.index("--") + 1:] if "--" in sys.argv else []
    culture = a[a.index("--culture") + 1]
    out = a[a.index("--out") + 1]
    blend_dir = a[a.index("--blend-dir") + 1] if "--blend-dir" in a else ""
    size = int(a[a.index("--size") + 1]) if "--size" in a else ELEMENT_BAKE
    print("ELEMENTS_OK", build(culture, out, blend_dir, bake="--no-bake" not in a, size=size))
