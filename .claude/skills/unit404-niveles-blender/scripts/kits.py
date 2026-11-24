"""UNIT-404 level kit generator (Blender 4.x/5.x, bpy). Headless:
blender -b --python kits.py -- --culture grecia --out Assets/Integrante4/Art/Grecia
Blender units = meters = Unity units. Unity import: FBX export uses -Z forward, Y up (apply transform).
Every piece has its origin at the CENTRE OF ITS BASE (z = 0 is the floor) so Unity can fit it to a block by mesh bounds."""
import bpy, sys, os, math

PALETTE = {  # from odd/art-style-sheet.md: (matte base, emissive accent)
    "mesopotamia": ("#B08A57", "#FF9A2E"),
    "egipto": ("#7A6A4F", "#2EE6C8"),
    "grecia": ("#C9CFD6", "#3DA5FF"),
    "china": ("#7B4B3A", "#4BE38A"),
}

# Per-culture silhouette parameters (tune live with the Blender MCP, then re-run the export).
STYLE = {
    "mesopotamia": dict(tiers=3, shaft_sides=4, capital="none", merlons=True, tower_roof=False, sail=False),
    "egipto": dict(tiers=1, shaft_sides=16, capital="flare", merlons=False, tower_roof=False, sail=True),
    "grecia": dict(tiers=2, shaft_sides=20, capital="slab", merlons=False, tower_roof=False, sail=True),
    "china": dict(tiers=2, shaft_sides=16, capital="slab", merlons=True, tower_roof=True, sail=True),
}


def hex_rgba(h, a=1.0):
    h = h.lstrip("#")
    lin = lambda c: (c / 255) ** 2.2
    return (lin(int(h[0:2], 16)), lin(int(h[2:4], 16)), lin(int(h[4:6], 16)), a)


def material(name, base, emit=None, strength=0.0, darken=1.0):
    m = bpy.data.materials.new(name)
    m.use_nodes = True
    nodes = m.node_tree.nodes
    b = next((n for n in nodes if n.type == "BSDF_PRINCIPLED"), None) or nodes.new("ShaderNodeBsdfPrincipled")
    out = next((n for n in nodes if n.type == "OUTPUT_MATERIAL"), None) or nodes.new("ShaderNodeOutputMaterial")
    if not b.outputs["BSDF"].links:
        m.node_tree.links.new(b.outputs["BSDF"], out.inputs["Surface"])
    r, g, bl, a = hex_rgba(base)
    b.inputs["Base Color"].default_value = (r * darken, g * darken, bl * darken, a)
    b.inputs["Roughness"].default_value = 0.85  # dark matte base
    if emit:
        b.inputs["Emission Color"].default_value = hex_rgba(emit)
        b.inputs["Emission Strength"].default_value = strength
    return m


def clear():
    bpy.ops.object.select_all(action="SELECT")
    bpy.ops.object.delete()


# ---- primitives: all take a base-centred position and return the new object ----
def box(mat, sx, sy, sz, cx=0.0, cy=0.0, base_z=0.0):
    bpy.ops.mesh.primitive_cube_add(size=1)
    o = bpy.context.active_object
    o.scale = (sx, sy, sz)
    o.location = (cx, cy, base_z + sz / 2)
    o.data.materials.append(mat)
    return o


def cone(mat, r1, r2, h, sides=16, cx=0.0, cy=0.0, base_z=0.0, twist=0.0):
    bpy.ops.mesh.primitive_cone_add(vertices=sides, radius1=r1, radius2=r2, depth=h)
    o = bpy.context.active_object
    o.location = (cx, cy, base_z + h / 2)
    o.rotation_euler = (0.0, 0.0, twist)
    o.data.materials.append(mat)
    return o


def join(parts, name, bevel=0.04):
    bpy.ops.object.select_all(action="DESELECT")
    for p in parts:
        p.select_set(True)
    bpy.context.view_layer.objects.active = parts[0]
    if len(parts) > 1:
        bpy.ops.object.join()
    o = bpy.context.active_object
    bpy.ops.object.transform_apply(location=False, rotation=True, scale=True)
    if bevel:
        mod = o.modifiers.new("soft", "BEVEL")
        mod.width = bevel
        mod.segments = 1
        bpy.ops.object.modifier_apply(modifier=mod.name)
    bpy.ops.object.shade_flat()
    o.name = name
    o.data.name = name
    return o


# ---- pieces ----
def platform(name, st, matte, trim, w=8.0, d=4.0, h=1.0):
    n = st["tiers"]
    parts = []
    for i in range(n):
        k = 1.0 - 0.14 * i
        parts.append(box(trim if i == n - 1 and n > 1 else matte, w * k, d * k, h / n, 0, 0, i * h / n))
    return join(parts, name)


def column(name, st, matte, trim, r=0.5, h=6.0):
    sides = st["shaft_sides"]
    parts = [box(trim, r * 2.8, r * 2.8, 0.4),
             cone(matte, r, r * 0.82, h - 0.8, sides, base_z=0.4, twist=math.pi / sides)]
    cap = st["capital"]
    if cap == "flare":
        parts.append(cone(trim, r * 0.82, r * 1.6, 0.6, sides, base_z=h - 0.4))
    elif cap == "slab":
        parts.append(box(trim, r * 2.8, r * 2.8, 0.4, base_z=h - 0.4))
    else:
        parts.append(box(trim, r * 2.2, r * 2.2, 0.4, base_z=h - 0.4))
    return join(parts, name)


def wall(name, st, matte, trim, w=10.0, h=5.0, t=1.0):
    parts = [box(matte, w, t, h)]
    if st["merlons"]:
        n = 5
        for i in range(n):
            parts.append(box(trim, w / (n * 2 - 1), t * 0.9, 0.8, -w / 2 + (w / (n * 2 - 1)) * (0.5 + 2 * i), 0, h))
    else:
        parts.append(box(trim, w * 1.04, t * 1.3, 0.4, 0, 0, h))
    return join(parts, name)


def tower(name, st, matte, trim, w=4.0, h=8.0):
    parts = [cone(matte, w * 0.75, w * 0.6, h - 0.6, 4, twist=math.pi / 4),
             box(trim, w * 1.4, w * 1.4, 0.6, base_z=h - 0.6)]
    if st["tower_roof"]:
        parts.append(cone(trim, w * 1.1, w * 0.1, 1.6, 4, base_z=h, twist=math.pi / 4))
    return join(parts, name)


def ship(name, st, matte, trim, l=6.0, w=2.0, h=1.0):
    parts = [box(matte, l, w, h * 0.8),
             box(trim, l * 0.14, w * 0.8, h * 1.6, l / 2 - l * 0.07, 0, 0),
             box(trim, l * 0.14, w * 0.8, h * 1.3, -l / 2 + l * 0.07, 0, 0),
             cone(trim, 0.09, 0.07, 4.0, 8, base_z=h * 0.8)]
    if st["sail"]:
        parts.append(box(trim, 0.08, w * 0.9, 2.4, 0, 0, h * 0.8 + 1.4))
    return join(parts, name)


def emissive_pickup(name, mat, loc, r=0.3):
    bpy.ops.mesh.primitive_ico_sphere_add(radius=r, subdivisions=2)
    o = bpy.context.active_object
    o.name = name
    o.location = loc
    o.data.materials.append(mat)
    bpy.ops.object.transform_apply(location=False, rotation=True, scale=True)
    return o


KITS = {"platform": platform, "column": column, "wall": wall, "tower": tower, "ship": ship}


def build(culture, out, blend_dir, save_blend=True):
    base, accent = PALETTE[culture]
    st = STYLE[culture]
    clear()
    matte = material(culture + "_matte", base)
    trim = material(culture + "_trim", base, darken=0.55)  # darker matte for capitals, merlons, roofs (NOT emissive)
    glow = material(culture + "_emissive", accent, accent, 4.0)
    objs = []
    for i, (kind, fn) in enumerate(KITS.items()):
        o = fn(f"{culture}_{kind}", st, matte, trim)
        o.location = (i * 12.0, 0.0, 0.0)
        objs.append(o)
    objs.append(emissive_pickup(f"{culture}_pickup", glow, (len(KITS) * 12.0, 0.0, 1.0)))
    os.makedirs(out, exist_ok=True)
    for o in objs:  # one FBX per piece: Unity prefabs per kit item
        bpy.ops.object.select_all(action="DESELECT")
        o.select_set(True)
        keep = o.location.copy()
        o.location = (0, 0, 0)
        bpy.ops.export_scene.fbx(filepath=os.path.join(out, o.name + ".fbx"), use_selection=True,
                                 apply_scale_options="FBX_SCALE_ALL", axis_forward="-Z", axis_up="Y",
                                 bake_space_transform=True)  # bakes Z-up -> Y-up into the mesh: node rotation 0, scale 1 in Unity
        o.location = keep
    if save_blend:
        # .blend must live OUTSIDE Assets/: Unity would try to import it through Blender and fail without it on PATH
        os.makedirs(blend_dir, exist_ok=True)
        bpy.ops.wm.save_as_mainfile(filepath=os.path.join(blend_dir, culture + "_kit.blend"))
    return [o.name for o in objs]


if __name__ == "__main__":
    a = sys.argv[sys.argv.index("--") + 1:] if "--" in sys.argv else []
    culture = a[a.index("--culture") + 1]
    out = a[a.index("--out") + 1]
    blend_dir = a[a.index("--blend-dir") + 1] if "--blend-dir" in a else os.path.join(os.path.dirname(out.rstrip("/")), "_blend")
    print("KIT_OK", build(culture, out, blend_dir))
