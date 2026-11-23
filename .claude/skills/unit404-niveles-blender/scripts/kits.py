"""UNIT-404 level kit generator (Blender 4.x/5.x, bpy). Headless:
blender -b --python kits.py -- --culture grecia --out Assets/Integrante4/Art/Grecia
Blender units = meters = Unity units. Unity import: FBX export uses -Z forward, Y up (apply transform)."""
import bpy, sys, os, math

PALETTE = {  # from odd/art-style-sheet.md
    "mesopotamia": ("#B08A57", "#FF9A2E"),
    "egipto": ("#7A6A4F", "#2EE6C8"),
    "grecia": ("#C9CFD6", "#3DA5FF"),
    "china": ("#7B4B3A", "#4BE38A"),
}


def hex_rgba(h, a=1.0):
    h = h.lstrip("#")
    lin = lambda c: (c / 255) ** 2.2
    return (lin(int(h[0:2], 16)), lin(int(h[2:4], 16)), lin(int(h[4:6], 16)), a)


def material(name, base, emit=None, strength=0.0):
    m = bpy.data.materials.new(name)
    m.use_nodes = True
    nodes = m.node_tree.nodes
    b = next((n for n in nodes if n.type == "BSDF_PRINCIPLED"), None) or nodes.new("ShaderNodeBsdfPrincipled")
    out = next((n for n in nodes if n.type == "OUTPUT_MATERIAL"), None) or nodes.new("ShaderNodeOutputMaterial")
    if not b.outputs["BSDF"].links:
        m.node_tree.links.new(b.outputs["BSDF"], out.inputs["Surface"])
    b.inputs["Base Color"].default_value = hex_rgba(base)
    b.inputs["Roughness"].default_value = 0.85  # dark matte base
    if emit:
        b.inputs["Emission Color"].default_value = hex_rgba(emit)
        b.inputs["Emission Strength"].default_value = strength
    return m


def clear():
    bpy.ops.object.select_all(action="SELECT")
    bpy.ops.object.delete()


def _finish(obj, name, mat, location):
    obj.name = name
    obj.location = location
    obj.data.materials.append(mat)
    bpy.ops.object.transform_apply(location=False, rotation=True, scale=True)
    return obj


def platform(name, mat, loc, w=8.0, d=4.0, h=1.0):
    bpy.ops.mesh.primitive_cube_add(size=1)
    o = bpy.context.active_object
    o.scale = (w, d, h)
    return _finish(o, name, mat, loc)


def column(name, mat, loc, r=0.5, h=6.0):
    bpy.ops.mesh.primitive_cylinder_add(radius=r, depth=h, vertices=16)
    return _finish(bpy.context.active_object, name, mat, (loc[0], loc[1], loc[2] + h / 2))


def wall(name, mat, loc, w=10.0, h=5.0, t=1.0):
    bpy.ops.mesh.primitive_cube_add(size=1)
    o = bpy.context.active_object
    o.scale = (w, t, h)
    return _finish(o, name, mat, (loc[0], loc[1], loc[2] + h / 2))


def tower(name, mat, loc, w=4.0, h=8.0):
    return wall(name, mat, loc, w=w, h=h, t=w)


def ship(name, mat, loc, l=6.0, w=2.0, h=1.0):
    bpy.ops.mesh.primitive_cube_add(size=1)
    o = bpy.context.active_object
    o.scale = (l, w, h)
    return _finish(o, name, mat, loc)


def emissive_pickup(name, mat, loc, r=0.3):
    bpy.ops.mesh.primitive_ico_sphere_add(radius=r, subdivisions=2)
    return _finish(bpy.context.active_object, name, mat, loc)


KITS = {"platform": platform, "column": column, "wall": wall, "tower": tower, "ship": ship}


def build(culture, out):
    base, accent = PALETTE[culture]
    clear()
    matte = material(culture + "_matte", base)
    glow = material(culture + "_emissive", accent, accent, 4.0)
    objs = []
    for i, (kind, fn) in enumerate(KITS.items()):
        objs.append(fn(f"{culture}_{kind}", matte, (i * 12.0, 0.0, 0.0)))
    objs.append(emissive_pickup(f"{culture}_pickup", glow, (len(KITS) * 12.0, 0.0, 1.0)))
    os.makedirs(out, exist_ok=True)
    for o in objs:  # one FBX per piece: Unity prefabs per kit item
        bpy.ops.object.select_all(action="DESELECT")
        o.select_set(True)
        keep = o.location.copy()
        o.location = (0, 0, 0)
        bpy.ops.export_scene.fbx(filepath=os.path.join(out, o.name + ".fbx"), use_selection=True,
                                 apply_scale_options="FBX_SCALE_ALL", axis_forward="-Z", axis_up="Y")
        o.location = keep
    bpy.ops.wm.save_as_mainfile(filepath=os.path.join(out, culture + "_kit.blend"))
    return [o.name for o in objs]


if __name__ == "__main__":
    a = sys.argv[sys.argv.index("--") + 1:] if "--" in sys.argv else []
    culture = a[a.index("--culture") + 1]
    out = a[a.index("--out") + 1]
    print("KIT_OK", build(culture, out))
