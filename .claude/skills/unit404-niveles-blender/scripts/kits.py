"""UNIT-404 level kit generator v3 (Blender 4.x/5.x, bpy). Headless:
blender -b --python kits.py -- --culture grecia --out Assets/Integrante4/Art/Grecia --blend-dir "Assets/Integrante4/Art/BlenderSource~"
blender -b --python kits.py -- --culture arena --out Assets/Integrante4/Art/Arena   (boss-arena ground, teammate's MAT_Ground_Basalt)

Art direction (ODD S23): night like Apophis. Dark stone per culture with glowing cracks in the culture accent,
same node recipe as the template's MAT_Ground_Basalt (noise colour + voronoi distance-to-edge cracks masked by a
low-frequency noise + bump). Unity cannot read Blender shaders, so every piece is UV-unwrapped and baked to
<piece>_albedo.png, <piece>_normal.png, <piece>_emission.png next to its FBX.

Blender units = meters = Unity units. FBX: -Z forward, Y up, bake_space_transform (node rotation 0 in Unity).
Every piece has its origin at the CENTRE OF ITS BASE. Platforms are 4 x 4 m modules that Unity tiles (no stretching):
the top slab covers the full footprint, so the visible top equals the collider top (ODD S18)."""
import bpy, sys, os, math

HERE = os.path.dirname(os.path.abspath(__file__))
TEMPLATE = os.path.normpath(os.path.join(HERE, "..", "..", "serpiente-dragon-procedural", "assets", "plantilla_serpiente_dragon.blend"))

PALETTE = {  # odd/art-style-sheet.md: (base hex, emissive accent hex); base is darkened for the night look
    "mesopotamia": ("#B08A57", "#FF9A2E"),
    "egipto": ("#7A6A4F", "#2EE6C8"),
    "grecia": ("#C9CFD6", "#3DA5FF"),
    "china": ("#7B4B3A", "#4BE38A"),
}

# Per-culture parameters (tune live with the Blender MCP, then re-run the export).
#   dark: base multiplier (night look), cracks: voronoi scale (higher = smaller cells), mask: share of glowing cracks
STYLE = {
    "mesopotamia": dict(dark=0.22, cracks=2.4, blocks=0.55, mask=(0.48, 0.68), shaft_sides=4, capital="block", merlons=True, roof=False, sail=False, flutes=0),
    "egipto": dict(dark=0.25, cracks=2.0, blocks=0.6, mask=(0.5, 0.7), shaft_sides=24, capital="papyrus", merlons=False, roof=False, sail=True, flutes=0),
    "grecia": dict(dark=0.18, cracks=2.2, blocks=0.5, mask=(0.5, 0.7), shaft_sides=40, capital="doric", merlons=False, roof=False, sail=True, flutes=20),
    "china": dict(dark=0.24, cracks=2.2, blocks=0.65, mask=(0.5, 0.7), shaft_sides=16, capital="bracket", merlons=True, roof=True, sail=True, flutes=0),
}

BAKE_SIZE = 1024
BAKE_SAMPLES = 16


def hex_lin(h):
    h = h.lstrip("#")
    return tuple((int(h[i:i + 2], 16) / 255) ** 2.2 for i in (0, 2, 4))


# ---------------- materials ----------------
def stone_material(name, base_hex, accent_hex, st, trim=False):
    """Node recipe of the template's MAT_Ground_Basalt, recoloured per culture. Emission Strength stays 0-1 (mask);
    Unity multiplies it for bloom."""
    m = bpy.data.materials.new(name)
    m.use_nodes = True
    nt = m.node_tree
    nt.nodes.clear()
    out = nt.nodes.new("ShaderNodeOutputMaterial")
    bsdf = nt.nodes.new("ShaderNodeBsdfPrincipled")
    nt.links.new(bsdf.outputs["BSDF"], out.inputs["Surface"])
    coord = nt.nodes.new("ShaderNodeTexCoord")

    noise = nt.nodes.new("ShaderNodeTexNoise")
    noise.inputs["Scale"].default_value = 3.0
    noise.inputs["Detail"].default_value = 10.0
    noise.inputs["Roughness"].default_value = 0.65
    nt.links.new(coord.outputs["Object"], noise.inputs["Vector"])
    ramp = nt.nodes.new("ShaderNodeValToRGB")
    base = hex_lin(base_hex)
    k = st["dark"] * (0.6 if trim else 1.0)
    lo, hi = tuple(c * k * 0.35 for c in base), tuple(c * k for c in base)
    ramp.color_ramp.elements[0].color = (*lo, 1.0)
    ramp.color_ramp.elements[1].color = (*hi, 1.0)
    nt.links.new(noise.outputs["Fac"], ramp.inputs["Fac"])
    nt.links.new(ramp.outputs["Color"], bsdf.inputs["Base Color"])
    bsdf.inputs["Roughness"].default_value = 0.88

    vor = nt.nodes.new("ShaderNodeTexVoronoi")
    vor.feature = "DISTANCE_TO_EDGE"
    vor.inputs["Scale"].default_value = st["cracks"]
    nt.links.new(coord.outputs["Object"], vor.inputs["Vector"])
    thin = nt.nodes.new("ShaderNodeMapRange")
    thin.inputs["From Min"].default_value, thin.inputs["From Max"].default_value = 0.0, 0.025
    thin.inputs["To Min"].default_value, thin.inputs["To Max"].default_value = 1.0, 0.0
    nt.links.new(vor.outputs["Distance"], thin.inputs["Value"])
    patch = nt.nodes.new("ShaderNodeTexNoise")
    patch.inputs["Scale"].default_value = 0.35
    patch.inputs["Detail"].default_value = 2.0
    nt.links.new(coord.outputs["Object"], patch.inputs["Vector"])
    mask = nt.nodes.new("ShaderNodeMapRange")
    mask.inputs["From Min"].default_value, mask.inputs["From Max"].default_value = st["mask"]
    nt.links.new(patch.outputs["Fac"], mask.inputs["Value"])
    mul = nt.nodes.new("ShaderNodeMath")
    mul.operation = "MULTIPLY"
    nt.links.new(thin.outputs["Result"], mul.inputs[0])
    nt.links.new(mask.outputs["Result"], mul.inputs[1])
    bsdf.inputs["Emission Color"].default_value = (*hex_lin(accent_hex), 1.0)
    nt.links.new(mul.outputs["Value"], bsdf.inputs["Emission Strength"])

    # Hewn stone blocks: big voronoi cells whose edges become dark sunken joints (colour + bump)
    blocks = nt.nodes.new("ShaderNodeTexVoronoi")
    blocks.feature = "DISTANCE_TO_EDGE"
    blocks.inputs["Scale"].default_value = st.get("blocks", 0.9)
    nt.links.new(coord.outputs["Object"], blocks.inputs["Vector"])
    joint = nt.nodes.new("ShaderNodeMapRange")
    joint.inputs["From Min"].default_value, joint.inputs["From Max"].default_value = 0.0, 0.05
    nt.links.new(blocks.outputs["Distance"], joint.inputs["Value"])  # 0 at the joint, 1 inside the block
    shade = nt.nodes.new("ShaderNodeMix")
    shade.data_type = "RGBA"
    shade.inputs["B"].default_value = (*tuple(c * k * 0.15 for c in base), 1.0)
    inv = nt.nodes.new("ShaderNodeMath")  # 1 - joint: the joint takes the dark colour B
    inv.operation = "SUBTRACT"
    inv.inputs[0].default_value = 1.0
    nt.links.new(joint.outputs["Result"], inv.inputs[1])
    nt.links.new(inv.outputs["Value"], shade.inputs["Factor"])
    nt.links.new(ramp.outputs["Color"], shade.inputs["A"])
    nt.links.new(shade.outputs["Result"], bsdf.inputs["Base Color"])

    height = nt.nodes.new("ShaderNodeMath")
    height.operation = "MULTIPLY_ADD"  # noise * 0.35 + joint
    nt.links.new(noise.outputs["Fac"], height.inputs[0])
    height.inputs[1].default_value = 0.35
    nt.links.new(joint.outputs["Result"], height.inputs[2])
    bump = nt.nodes.new("ShaderNodeBump")
    bump.inputs["Strength"].default_value = 0.8
    bump.inputs["Distance"].default_value = 0.08
    nt.links.new(height.outputs["Value"], bump.inputs["Height"])
    nt.links.new(bump.outputs["Normal"], bsdf.inputs["Normal"])
    return m


def template_basalt():
    """The teammate's exact boss-arena ground material, appended from the Apophis template (ODD S23)."""
    with bpy.data.libraries.load(TEMPLATE, link=False) as (src, dst):
        dst.materials = ["MAT_Ground_Basalt"]
    m = dst.materials[0]
    # its emission strength is mask * 6: keep the mask in 0-1 for the bake, Unity applies the glow
    for n in m.node_tree.nodes:
        if n.type == "MATH" and n.operation == "MULTIPLY" and not n.inputs[1].is_linked and abs(n.inputs[1].default_value - 6.0) < 1e-3:
            n.inputs[1].default_value = 1.0
    return m


def clear():
    # current scene only: safe to run inside a live Blender session through the MCP
    for o in list(bpy.context.scene.objects):
        bpy.data.objects.remove(o, do_unlink=True)


# ---------------- primitives (base-centred) ----------------
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


def rocky(o, strength=0.18, cuts=4):
    """Subdivide and displace with noise: hand-hewn rock look for undersides and ruins."""
    bpy.context.view_layer.objects.active = o
    sub = o.modifiers.new("sub", "SUBSURF")
    sub.levels, sub.subdivision_type = cuts // 2, "SIMPLE"
    tex = bpy.data.textures.new(o.name + "_rock", "CLOUDS")
    tex.noise_scale = 0.6
    d = o.modifiers.new("rock", "DISPLACE")
    d.texture, d.strength, d.mid_level = tex, strength, 0.5
    for mod in list(o.modifiers):
        bpy.ops.object.modifier_apply(modifier=mod.name)
    return o


def join(parts, name, bevel=0.03):
    bpy.ops.object.select_all(action="DESELECT")
    for p in parts:
        p.select_set(True)
    bpy.context.view_layer.objects.active = parts[0]
    if len(parts) > 1:
        bpy.ops.object.join()
    o = bpy.context.active_object
    # location too: parts are built around the world origin, so the joined origin lands on the centre of the base
    # (with location=False the origin stayed at parts[0], e.g. the platform slab centre 0.81 m up: ODD S18 bug)
    bpy.ops.object.transform_apply(location=True, rotation=True, scale=True)
    if bevel:
        mod = o.modifiers.new("soft", "BEVEL")
        mod.width, mod.segments, mod.limit_method = bevel, 2, "ANGLE"
        bpy.ops.object.modifier_apply(modifier=mod.name)
    bpy.ops.object.shade_flat()
    o.name = o.data.name = name
    return o


# ---------------- pieces ----------------
def platform(name, st, stone, trim, w=4.0, d=4.0, h=1.0):
    """Floating stone platform module: full-footprint top slab (= collider) + tapered rocky underside inside the block."""
    slab = box(stone, w, d, h * 0.38, base_z=h * 0.62)
    under = cone(trim, w * 0.62, w * 0.36, h * 0.66, 4, base_z=0.0, twist=math.pi / 4)
    under.scale = (1.0, d / w, 1.0)
    rocky(under, 0.12)
    return join([slab, under], name)


def column(name, st, stone, trim, r=0.5, h=6.0):
    sides = st["shaft_sides"]
    shaft = cone(stone, r, r * 0.84, h - 0.9, sides, base_z=0.45, twist=math.pi / sides)
    if st["flutes"]:  # Doric flutes: pull every other vertex inwards
        for i, v in enumerate(shaft.data.vertices):
            if i % 2:
                v.co.x *= 0.9
                v.co.y *= 0.9
    parts = [box(trim, r * 2.8, r * 2.8, 0.45), shaft]
    cap = st["capital"]
    if cap == "papyrus":
        parts.append(cone(trim, r * 0.84, r * 1.7, 0.9, sides, base_z=h - 0.45 - 0.45))
        parts.append(box(trim, r * 3.2, r * 3.2, 0.25, base_z=h - 0.25))
    elif cap == "doric":
        parts.append(cone(trim, r * 0.84, r * 1.3, 0.3, sides, base_z=h - 0.75))
        parts.append(box(trim, r * 2.9, r * 2.9, 0.45, base_z=h - 0.45))
    elif cap == "bracket":
        parts.append(box(trim, r * 4.0, r * 1.2, 0.35, base_z=h - 0.8))
        parts.append(box(trim, r * 1.2, r * 4.0, 0.35, base_z=h - 0.45))
    else:
        parts.append(box(trim, r * 2.4, r * 2.4, 0.45, base_z=h - 0.45))
    return join(parts, name)


def wall(name, st, stone, trim, w=10.0, h=5.0, t=1.0):
    parts = [box(stone, w, t, h)]
    for i in range(3):  # pilasters / buttresses break the flat face
        parts.append(box(trim, 0.6, t * 1.3, h * 0.92, -w / 2 + w * (i + 0.5) / 3, 0, 0))
    if st["merlons"]:
        n = 7
        mw = w / (n * 2 - 1)
        for i in range(n):
            parts.append(box(trim, mw, t * 1.05, 0.8, -w / 2 + mw * (0.5 + 2 * i), 0, h))
    else:
        parts.append(box(trim, w * 1.04, t * 1.4, 0.45, 0, 0, h))
    return join(parts, name)


def tower(name, st, stone, trim, w=4.0, h=8.0):
    parts = [cone(stone, w * 0.75, w * 0.6, h - 0.6, 4, twist=math.pi / 4),
             box(trim, w * 1.4, w * 1.4, 0.6, base_z=h - 0.6)]
    if st["roof"]:  # Chinese hip roof with an upturned eave
        parts.append(cone(trim, w * 1.25, w * 0.95, 0.3, 4, base_z=h, twist=math.pi / 4))
        parts.append(cone(trim, w * 0.95, w * 0.1, 1.8, 4, base_z=h + 0.3, twist=math.pi / 4))
    elif st["merlons"]:
        for sx in (-1, 1):
            for sy in (-1, 1):
                parts.append(box(trim, 0.7, 0.7, 0.8, sx * w * 0.55, sy * w * 0.55, h))
    return join(parts, name)


def ship(name, st, stone, trim, l=6.0, w=2.0, h=1.0):
    hull = cone(stone, w * 0.75, w * 0.5, h * 0.8, 6, twist=0.0)
    hull.scale = (l / (w * 1.5), 1.0, 1.0)
    parts = [hull,
             box(trim, l * 0.12, w * 0.5, h * 1.7, l / 2 - l * 0.1, 0, 0),
             box(trim, l * 0.12, w * 0.5, h * 1.3, -l / 2 + l * 0.1, 0, 0),
             cone(trim, 0.09, 0.07, 4.0, 8, base_z=h * 0.8)]
    if st["sail"]:
        parts.append(box(trim, 0.08, w * 1.0, 2.4, 0, 0, h * 0.8 + 1.4))
    return join(parts, name)


def emissive_pickup(name, accent_hex, loc, r=0.3):
    m = bpy.data.materials.new(name + "_emissive")
    m.use_nodes = True
    b = next(n for n in m.node_tree.nodes if n.type == "BSDF_PRINCIPLED")
    b.inputs["Base Color"].default_value = (*hex_lin(accent_hex), 1.0)
    b.inputs["Emission Color"].default_value = (*hex_lin(accent_hex), 1.0)
    b.inputs["Emission Strength"].default_value = 4.0
    bpy.ops.mesh.primitive_ico_sphere_add(radius=r, subdivisions=2)
    o = bpy.context.active_object
    o.name = name
    o.location = loc
    o.data.materials.append(m)
    return o


KITS = {"platform": platform, "column": column, "wall": wall, "tower": tower, "ship": ship}


# ---------------- bake ----------------
def bake_maps(o, out, size=BAKE_SIZE):
    """Smart-UV the piece and bake albedo / tangent normal / emission to PNGs next to its FBX."""
    scn = bpy.context.scene
    try:
        scn.render.engine = "CYCLES"
    except TypeError as e:
        raise RuntimeError("Cycles is required for baking: " + str(e))
    scn.cycles.samples = BAKE_SAMPLES
    scn.cycles.device = "CPU"
    bpy.ops.object.select_all(action="DESELECT")
    o.select_set(True)
    bpy.context.view_layer.objects.active = o
    bpy.ops.object.mode_set(mode="EDIT")
    bpy.ops.mesh.select_all(action="SELECT")
    bpy.ops.uv.smart_project(angle_limit=math.radians(66), island_margin=0.02)
    bpy.ops.object.mode_set(mode="OBJECT")
    for kind, btype, colorspace in (("albedo", "DIFFUSE", "sRGB"), ("normal", "NORMAL", "Non-Color"), ("emission", "EMIT", "sRGB")):
        img = bpy.data.images.new(f"{o.name}_{kind}", size, size, alpha=False)
        img.colorspace_settings.name = colorspace
        for slot in o.material_slots:
            nt = slot.material.node_tree
            node = nt.nodes.get("BAKE_TARGET") or nt.nodes.new("ShaderNodeTexImage")
            node.name = "BAKE_TARGET"
            node.image = img
            nt.nodes.active = node
        kw = dict(type=btype, margin=8, use_clear=True)
        if btype == "DIFFUSE":
            kw["pass_filter"] = {"COLOR"}
        bpy.ops.object.bake(**kw)
        img.filepath_raw = os.path.join(out, f"{o.name}_{kind}.png")
        img.file_format = "PNG"
        img.save()


def export_fbx(o, out):
    bpy.ops.object.select_all(action="DESELECT")
    o.select_set(True)
    keep = o.location.copy()
    o.location = (0, 0, 0)
    bpy.ops.export_scene.fbx(filepath=os.path.join(out, o.name + ".fbx"), use_selection=True,
                             apply_scale_options="FBX_SCALE_ALL", axis_forward="-Z", axis_up="Y",
                             bake_space_transform=True)  # bakes Z-up -> Y-up into the mesh: node rotation 0, scale 1 in Unity
    o.location = keep


def build(culture, out, blend_dir, save_blend=True, bake=True):
    clear()
    os.makedirs(out, exist_ok=True)
    objs = []
    if culture == "arena":
        basalt = template_basalt()
        o = platform("arena_platform", STYLE["grecia"], basalt, basalt)
        objs.append(o)
    else:
        base, accent = PALETTE[culture]
        st = STYLE[culture]
        stone = stone_material(culture + "_stone", base, accent, st)
        trim = stone_material(culture + "_trim", base, accent, st, trim=True)
        for i, (kind, fn) in enumerate(KITS.items()):
            o = fn(f"{culture}_{kind}", st, stone, trim)
            o.location = (i * 12.0, 0.0, 0.0)
            objs.append(o)
    for o in objs:
        if bake:
            bake_maps(o, out)
        export_fbx(o, out)
    if culture != "arena":
        p = emissive_pickup(f"{culture}_pickup", PALETTE[culture][1], (len(KITS) * 12.0, 0.0, 1.0))
        export_fbx(p, out)
        objs.append(p)
    if save_blend and blend_dir:
        # .blend must live OUTSIDE Unity's import path (folder ending in ~): Unity would call Blender to import it
        os.makedirs(blend_dir, exist_ok=True)
        bpy.ops.wm.save_as_mainfile(filepath=os.path.join(blend_dir, culture + "_kit.blend"))
    return [o.name for o in objs]


if __name__ == "__main__":
    a = sys.argv[sys.argv.index("--") + 1:] if "--" in sys.argv else []
    culture = a[a.index("--culture") + 1]
    out = a[a.index("--out") + 1]
    blend_dir = a[a.index("--blend-dir") + 1] if "--blend-dir" in a else ""
    print("KIT_OK", build(culture, out, blend_dir, bake="--no-bake" not in a))
