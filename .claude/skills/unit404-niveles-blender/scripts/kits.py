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
import numpy as np

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

BAKE_SIZE = 2048  # T29 (ODD S30): was 1024
BAKE_SAMPLES = 16
ONLY = []  # piece kinds to rebuild (CLI --only); empty = the whole kit


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


# ---------------- day materials (ODD S23 a, T16): lit culture landscape on the route ----------------
# Per culture: stone = main surfaces, trim = bases, capitals, pilasters, undersides.
#   kind brick: (colour 1, colour 2, mortar, brick width m, row height m), also used for ashlar courses   kind ashlar: (colour 1, colour 2, joint, block scale)
#   bands: (band colour, bands per metre, band share) painted horizontal stripes; veins: marble veining colour
DAY = {
    "mesopotamia": dict(  # sun-dried mud brick + glazed blue tiles with gold joints (Ishtar-gate style)
        stone=dict(kind="brick", c=("#C9A46C", "#B48A55", "#8C6C45", 0.5, 0.18), rough=0.95, bump=0.6),
        trim=dict(kind="brick", c=("#1F4E9A", "#2D66BE", "#C99A2E", 0.32, 0.32), rough=0.35, bump=0.3)),
    "egipto": dict(  # warm sandstone ashlar + painted limestone with teal and ochre bands
        stone=dict(kind="brick", c=("#DDBB82", "#C9A46C", "#A8875A", 1.0, 0.5), rough=0.9, bump=0.5),
        trim=dict(kind="ashlar", c=("#EAE0C8", "#DCD0B4", "#B8AA8A", 0.8), rough=0.75, bump=0.3, bands=("#2E9C8F", 1.6, 0.14))),
    "grecia": dict(  # white marble ashlar with grey veins + terracotta tiles
        stone=dict(kind="brick", c=("#EFEDE7", "#E2DED5", "#C4BEB2", 1.2, 0.6), rough=0.45, bump=0.25, veins="#9C978F"),
        trim=dict(kind="brick", c=("#B65C33", "#A04B27", "#7A3A1E", 0.4, 0.2), rough=0.8, bump=0.4)),
    "china": dict(  # grey fired brick + red lacquer with jade bands
        stone=dict(kind="brick", c=("#93918B", "#7F7D78", "#5E5C58", 0.4, 0.12), rough=0.9, bump=0.5),
        trim=dict(kind="ashlar", c=("#A32019", "#8E1A14", "#8E1A14", 0.3), rough=0.4, bump=0.1, bands=("#3E9E73", 1.2, 0.12))),
}


def _rgba(h):
    return (*hex_lin(h), 1.0)


def _triplanar(nt, coord, make_tex):
    """Project a 2D texture along X, Y and Z of object space and blend by the face normal, so bricks keep their
    size on every face. make_tex(vector_socket) -> (color_socket, fac_socket). Returns blended (color, fac)."""
    sep = nt.nodes.new("ShaderNodeSeparateXYZ")
    nt.links.new(coord.outputs["Object"], sep.inputs[0])
    planes = []
    for a, b in (("Y", "Z"), ("X", "Z"), ("X", "Y")):
        comb = nt.nodes.new("ShaderNodeCombineXYZ")
        nt.links.new(sep.outputs[a], comb.inputs["X"])
        nt.links.new(sep.outputs[b], comb.inputs["Y"])
        planes.append(make_tex(comb.outputs[0]))
    nsep = nt.nodes.new("ShaderNodeSeparateXYZ")
    nt.links.new(coord.outputs["Normal"], nsep.inputs[0])
    w = []
    for axis in "XYZ":
        ab = nt.nodes.new("ShaderNodeMath"); ab.operation = "ABSOLUTE"
        nt.links.new(nsep.outputs[axis], ab.inputs[0])
        pw = nt.nodes.new("ShaderNodeMath"); pw.operation = "POWER"; pw.inputs[1].default_value = 8.0
        nt.links.new(ab.outputs[0], pw.inputs[0])
        w.append(pw.outputs[0])
    # sequential blend: X vs Y by wy/(wx+wy), then that vs Z by wz/(wx+wy+wz)
    sxy = nt.nodes.new("ShaderNodeMath"); sxy.operation = "ADD"
    nt.links.new(w[0], sxy.inputs[0]); nt.links.new(w[1], sxy.inputs[1])
    fxy = nt.nodes.new("ShaderNodeMath"); fxy.operation = "DIVIDE"
    nt.links.new(w[1], fxy.inputs[0]); nt.links.new(sxy.outputs[0], fxy.inputs[1])
    sall = nt.nodes.new("ShaderNodeMath"); sall.operation = "ADD"
    nt.links.new(sxy.outputs[0], sall.inputs[0]); nt.links.new(w[2], sall.inputs[1])
    fz = nt.nodes.new("ShaderNodeMath"); fz.operation = "DIVIDE"
    nt.links.new(w[2], fz.inputs[0]); nt.links.new(sall.outputs[0], fz.inputs[1])
    out = []
    for idx, dtype in ((0, "RGBA"), (1, "FLOAT")):
        m1 = nt.nodes.new("ShaderNodeMix"); m1.data_type = dtype
        m2 = nt.nodes.new("ShaderNodeMix"); m2.data_type = dtype
        ins = (lambda m: (m.inputs[6], m.inputs[7], m.outputs[2])) if dtype == "RGBA" else (lambda m: (m.inputs[2], m.inputs[3], m.outputs[0]))
        a1, b1, o1 = ins(m1); a2, b2, o2 = ins(m2)
        nt.links.new(fxy.outputs[0], m1.inputs[0])
        nt.links.new(planes[0][idx], a1); nt.links.new(planes[1][idx], b1)
        nt.links.new(fz.outputs[0], m2.inputs[0])
        nt.links.new(o1, a2); nt.links.new(planes[2][idx], b2)
        out.append(o2)
    return out[0], out[1]


def day_material(name, spec):
    """Lit, matte culture surface without glowing cracks: colour variation noise + bricks or ashlar joints + bump."""
    m = bpy.data.materials.new(name)
    m.use_nodes = True
    nt = m.node_tree
    nt.nodes.clear()
    out = nt.nodes.new("ShaderNodeOutputMaterial")
    bsdf = nt.nodes.new("ShaderNodeBsdfPrincipled")
    nt.links.new(bsdf.outputs["BSDF"], out.inputs["Surface"])
    bsdf.inputs["Roughness"].default_value = spec["rough"]
    coord = nt.nodes.new("ShaderNodeTexCoord")
    c1, c2, joint_hex, *size = spec["c"]

    grain = nt.nodes.new("ShaderNodeTexNoise")  # weathering / sun-faded variation
    grain.inputs["Scale"].default_value = 2.5
    grain.inputs["Detail"].default_value = 8.0
    nt.links.new(coord.outputs["Object"], grain.inputs["Vector"])
    ramp = nt.nodes.new("ShaderNodeValToRGB")
    ramp.color_ramp.elements[0].color = _rgba(c2)
    ramp.color_ramp.elements[1].color = _rgba(c1)
    nt.links.new(grain.outputs["Fac"], ramp.inputs["Fac"])
    surface = ramp.outputs["Color"]

    if spec["kind"] == "brick":
        bw, rh = size

        def make(vec):
            b = nt.nodes.new("ShaderNodeTexBrick")
            b.inputs["Color1"].default_value = _rgba(c1)
            b.inputs["Color2"].default_value = _rgba(c2)
            b.inputs["Mortar"].default_value = _rgba(joint_hex)
            b.inputs["Scale"].default_value = 1.0
            b.inputs["Mortar Size"].default_value = 0.012
            b.inputs["Brick Width"].default_value = bw
            b.inputs["Row Height"].default_value = rh
            nt.links.new(vec, b.inputs["Vector"])
            return b.outputs["Color"], b.outputs["Fac"]
        bricks, joint = _triplanar(nt, coord, make)
        # bricks carry their own colours; the grain noise only shades them
        shade = nt.nodes.new("ShaderNodeMix"); shade.data_type = "RGBA"; shade.blend_type = "MULTIPLY"
        shade.inputs[0].default_value = 0.35
        nt.links.new(bricks, shade.inputs[6]); nt.links.new(surface, shade.inputs[7])
        surface = shade.outputs[2]
        inside = nt.nodes.new("ShaderNodeMath"); inside.operation = "SUBTRACT"; inside.inputs[0].default_value = 1.0
        nt.links.new(joint, inside.inputs[1])
        height_in = inside.outputs[0]
    else:
        blocks = nt.nodes.new("ShaderNodeTexVoronoi")
        blocks.feature = "DISTANCE_TO_EDGE"
        blocks.inputs["Scale"].default_value = size[0]
        nt.links.new(coord.outputs["Object"], blocks.inputs["Vector"])
        jr = nt.nodes.new("ShaderNodeMapRange")
        jr.inputs["From Min"].default_value, jr.inputs["From Max"].default_value = 0.0, 0.03
        nt.links.new(blocks.outputs["Distance"], jr.inputs["Value"])  # 0 at the joint, 1 inside
        mix = nt.nodes.new("ShaderNodeMix"); mix.data_type = "RGBA"
        nt.links.new(jr.outputs["Result"], mix.inputs[0])
        mix.inputs[6].default_value = _rgba(joint_hex)
        nt.links.new(surface, mix.inputs[7])
        surface = mix.outputs[2]
        height_in = jr.outputs["Result"]

    if spec.get("veins"):  # marble: thin distorted wave lines
        wave = nt.nodes.new("ShaderNodeTexWave")
        wave.inputs["Scale"].default_value = 1.2
        wave.inputs["Distortion"].default_value = 12.0
        wave.inputs["Detail"].default_value = 6.0
        nt.links.new(coord.outputs["Object"], wave.inputs["Vector"])
        vr = nt.nodes.new("ShaderNodeMapRange")
        vr.inputs["From Min"].default_value, vr.inputs["From Max"].default_value = 0.0, 0.08
        vr.inputs["To Min"].default_value, vr.inputs["To Max"].default_value = 0.55, 0.0
        nt.links.new(wave.outputs["Fac"], vr.inputs["Value"])
        vm = nt.nodes.new("ShaderNodeMix"); vm.data_type = "RGBA"
        nt.links.new(vr.outputs["Result"], vm.inputs[0])
        nt.links.new(surface, vm.inputs[6])
        vm.inputs[7].default_value = _rgba(spec["veins"])
        surface = vm.outputs[2]

    if spec.get("bands"):  # painted horizontal stripes (object Z)
        band_hex, per_m, share = spec["bands"]
        sep = nt.nodes.new("ShaderNodeSeparateXYZ")
        nt.links.new(coord.outputs["Object"], sep.inputs[0])
        sc = nt.nodes.new("ShaderNodeMath"); sc.operation = "MULTIPLY"; sc.inputs[1].default_value = per_m
        nt.links.new(sep.outputs["Z"], sc.inputs[0])
        fr = nt.nodes.new("ShaderNodeMath"); fr.operation = "FRACT"
        nt.links.new(sc.outputs[0], fr.inputs[0])
        lt = nt.nodes.new("ShaderNodeMath"); lt.operation = "LESS_THAN"; lt.inputs[1].default_value = share
        nt.links.new(fr.outputs[0], lt.inputs[0])
        # stripes only on vertical faces: a horizontal face has one Z and would fall entirely inside a band
        nz = nt.nodes.new("ShaderNodeSeparateXYZ")
        nt.links.new(coord.outputs["Normal"], nz.inputs[0])
        az = nt.nodes.new("ShaderNodeMath"); az.operation = "ABSOLUTE"
        nt.links.new(nz.outputs["Z"], az.inputs[0])
        side = nt.nodes.new("ShaderNodeMath"); side.operation = "LESS_THAN"; side.inputs[1].default_value = 0.5
        nt.links.new(az.outputs[0], side.inputs[0])
        both = nt.nodes.new("ShaderNodeMath"); both.operation = "MULTIPLY"
        nt.links.new(lt.outputs[0], both.inputs[0]); nt.links.new(side.outputs[0], both.inputs[1])
        bm = nt.nodes.new("ShaderNodeMix"); bm.data_type = "RGBA"
        nt.links.new(both.outputs[0], bm.inputs[0])
        nt.links.new(surface, bm.inputs[6])
        bm.inputs[7].default_value = _rgba(band_hex)
        surface = bm.outputs[2]

    # T29 (ODD S30): rounded, worn edges. The Bevel shader normal rounds every hard edge in the baked normal map; where
    # it departs from the true normal (an edge) the stone is chipped: lighter and rougher.
    bev = nt.nodes.new("ShaderNodeBevel")
    bev.samples = 8
    bev.inputs["Radius"].default_value = 0.035
    geo = nt.nodes.new("ShaderNodeNewGeometry")
    dot = nt.nodes.new("ShaderNodeVectorMath"); dot.operation = "DOT_PRODUCT"
    nt.links.new(bev.outputs["Normal"], dot.inputs[0]); nt.links.new(geo.outputs["Normal"], dot.inputs[1])
    edge = nt.nodes.new("ShaderNodeMapRange")  # dot 1 = flat face, ~0.9 = on the rounded edge
    edge.inputs["From Min"].default_value, edge.inputs["From Max"].default_value = 0.995, 0.93
    nt.links.new(dot.outputs["Value"], edge.inputs["Value"])
    chip = nt.nodes.new("ShaderNodeTexNoise")  # break the wear up so it reads as chips, not an outline
    chip.inputs["Scale"].default_value = 9.0
    chip.inputs["Detail"].default_value = 4.0
    nt.links.new(coord.outputs["Object"], chip.inputs["Vector"])
    wear = nt.nodes.new("ShaderNodeMath"); wear.operation = "MULTIPLY"
    nt.links.new(edge.outputs["Result"], wear.inputs[0]); nt.links.new(chip.outputs["Fac"], wear.inputs[1])
    wear_c = nt.nodes.new("ShaderNodeMath"); wear_c.operation = "MULTIPLY"; wear_c.inputs[1].default_value = 1.4
    wear_c.use_clamp = True
    nt.links.new(wear.outputs[0], wear_c.inputs[0])
    worn = nt.nodes.new("ShaderNodeMix"); worn.data_type = "RGBA"; worn.blend_type = "SCREEN"
    nt.links.new(wear_c.outputs[0], worn.inputs[0])
    nt.links.new(surface, worn.inputs[6])
    worn.inputs[7].default_value = (0.45, 0.42, 0.38, 1.0)
    surface = worn.outputs[2]

    # per-pixel roughness: joints/mortar rougher than the faces, worn edges rougher (baked into the albedo alpha as
    # Unity smoothness)
    rough = nt.nodes.new("ShaderNodeMapRange")
    rough.inputs["To Min"].default_value = min(1.0, spec["rough"] + 0.25)
    rough.inputs["To Max"].default_value = spec["rough"]
    nt.links.new(height_in, rough.inputs["Value"])
    rough_w = nt.nodes.new("ShaderNodeMath"); rough_w.operation = "MAXIMUM"
    nt.links.new(rough.outputs["Result"], rough_w.inputs[0]); nt.links.new(wear_c.outputs[0], rough_w.inputs[1])
    nt.links.new(rough_w.outputs[0], bsdf.inputs["Roughness"])

    nt.links.new(surface, bsdf.inputs["Base Color"])
    micro = nt.nodes.new("ShaderNodeTexNoise")  # fine pitting: crisp detail at close range
    micro.inputs["Scale"].default_value = 38.0
    micro.inputs["Detail"].default_value = 6.0
    nt.links.new(coord.outputs["Object"], micro.inputs["Vector"])
    height = nt.nodes.new("ShaderNodeMath"); height.operation = "MULTIPLY_ADD"  # grain * 0.25 + inside
    nt.links.new(grain.outputs["Fac"], height.inputs[0])
    height.inputs[1].default_value = 0.25
    nt.links.new(height_in, height.inputs[2])
    height2 = nt.nodes.new("ShaderNodeMath"); height2.operation = "MULTIPLY_ADD"  # + micro * 0.12
    nt.links.new(micro.outputs["Fac"], height2.inputs[0])
    height2.inputs[1].default_value = 0.12
    nt.links.new(height.outputs["Value"], height2.inputs[2])
    bump = nt.nodes.new("ShaderNodeBump")
    bump.inputs["Strength"].default_value = min(1.0, spec["bump"] * 1.5)
    bump.inputs["Distance"].default_value = 0.04
    nt.links.new(height2.outputs["Value"], bump.inputs["Height"])
    nt.links.new(bev.outputs["Normal"], bump.inputs["Normal"])
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


def stairs(name, st, stone, trim, w=6.0, h=2.0, d=2.0):
    """Ziggurat terrace (T17): battered core, plinth, full-footprint cap (flat walkable top = collider top) and a stair
    flight in relief on both long faces (the 2.5D camera sees one, the other side is free)."""
    parts = [box(trim, w, d, 0.25), box(stone, w * 0.96, d * 0.84, h - 0.5, base_z=0.25), box(trim, w, d, 0.25, base_z=h - 0.25)]
    n, flight, recess = 5, w * 0.3, d * 0.08
    rise = (h - 0.5) / n
    for side in (-1, 1):
        for i in range(n):  # lowest step projects most, every step stays inside the footprint
            proj = recess * (n - i) / n
            parts.append(box(trim if i % 2 else stone, flight, proj, rise, 0, side * (d * 0.42 + proj / 2), 0.25 + i * rise))
        for sx in (-1, 1):  # side balustrades of the flight
            parts.append(box(trim, 0.18, recess, h - 0.5, sx * (flight / 2 + 0.09), side * (d * 0.46), 0.25))
    return join(parts, name)


def block(name, st, stone, trim, w=3.0, h=3.0, d=2.0):
    """Dressed stone block (T17): three ashlar courses with shadow gaps, flat cornice on top (= collider top) and a
    carved relief panel on both faces."""
    course = (h - 0.5) / 3
    parts = [box(trim, w, d, 0.2)]
    for i in range(3):
        parts.append(box(stone, w * (0.97 - 0.01 * i), d * 0.95, course - 0.04, base_z=0.2 + i * course))
    parts.append(box(trim, w, d, 0.3, base_z=h - 0.3))
    for side in (-1, 1):
        parts.append(box(trim, w * 0.5, 0.04, course * 1.6, 0, side * d * 0.485, 0.2 + course * 0.7))
    return join(parts, name)


def mast(name, st, stone, trim, w=0.5, h=8.0, d=2.0):
    """Banner mast (T17): stone footing, pole, cross-arm and a banner hanging across the path depth (faces the 3D
    camera, which looks along +X)."""
    parts = [box(stone, w, w, 0.6), cone(trim, 0.16, 0.12, h - 0.6, 12, base_z=0.6),
             box(trim, 0.14, d * 0.95, 0.14, 0, 0, h - 0.9),
             box(stone, 0.05, d * 0.85, 2.8, 0, 0, h - 3.8)]
    for i in range(3):  # weighted fringe
        parts.append(box(trim, 0.07, d * 0.85, 0.12, 0, 0, h - 3.8 + i * 1.1))
    parts.append(cone(trim, 0.2, 0.0, 0.5, 8, base_z=h - 0.05))
    return join(parts, name)


def keep(name, st, stone, trim, w=8.0, h=12.0, d=8.0):
    """Banner keep (T17), level 4 goal tower: walkable full-footprint roof deck flush with the collider top (the arrival
    pad stands on it; nothing rises above it), corner pilasters, banners on every face."""
    parts = [box(trim, w, d, 0.6), box(stone, w * 0.9, d * 0.9, h - 1.4, base_z=0.6), box(trim, w, d, 0.8, base_z=h - 0.8)]
    for sx in (-1, 1):
        for sy in (-1, 1):
            parts.append(box(trim, 0.9, 0.9, h - 1.4, sx * w * 0.43, sy * d * 0.43, 0.6))
    for axis in (0, 1):
        for side in (-1, 1):
            if axis == 0:
                parts.append(box(trim, 0.05, d * 0.3, h * 0.45, side * w * 0.455, 0, h * 0.35))
            else:
                parts.append(box(trim, w * 0.3, 0.05, h * 0.45, 0, side * d * 0.455, h * 0.35))
    return join(parts, name)


KITS = {"platform": platform, "column": column, "wall": wall, "tower": tower, "ship": ship,
        "stairs": stairs, "block": block, "mast": mast, "keep": keep}


# ---------------- bake ----------------
MAPS = (("albedo", "DIFFUSE", "sRGB"), ("normal", "NORMAL", "Non-Color"), ("emission", "EMIT", "sRGB"),
        ("occlusion", "AO", "Non-Color"), ("roughness", "ROUGHNESS", "Non-Color"))
AO_IN_ALBEDO = 0.55  # share of the baked AO multiplied into the albedo
SHARP = ("albedo", "normal", "occlusion", "roughness")  # T29 route maps


def bake_maps(o, out, size=BAKE_SIZE, prefix=None, kinds=("albedo", "normal", "emission"), unwrap=True):
    """Smart-UV the piece (unless unwrap=False: reuse its UVs) and bake the maps in `kinds` to
    <prefix>_<kind>.png next to its FBX (prefix defaults to the object name)."""
    prefix = prefix or o.name
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
    if unwrap:
        bpy.ops.object.mode_set(mode="EDIT")
        bpy.ops.mesh.select_all(action="SELECT")
        bpy.ops.uv.smart_project(angle_limit=math.radians(66), island_margin=0.02)
        bpy.ops.object.mode_set(mode="OBJECT")
    baked = {}
    smooth = {"occlusion", "roughness"} <= set(kinds)  # only then the albedo carries an alpha (smoothness)
    for kind, btype, colorspace in (m for m in MAPS if m[0] in kinds):
        img = bpy.data.images.new(f"{prefix}_{kind}", size, size, alpha=(kind == "albedo" and smooth))
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
        baked[kind] = img
    if "albedo" in baked and "occlusion" in baked and "roughness" in baked:
        # T29 (ODD S30): AO darkens crevices in the albedo (direct light too, URP's occlusion map only touches
        # ambient) and the alpha carries smoothness = 1 - roughness (URP Lit "smoothness from albedo alpha")
        n = size * size * 4
        alb, ao, ro = (np.empty(n, dtype=np.float32) for _ in range(3))
        baked["albedo"].pixels.foreach_get(alb)
        baked["occlusion"].pixels.foreach_get(ao)
        baked["roughness"].pixels.foreach_get(ro)
        alb, ao, ro = alb.reshape(-1, 4), ao.reshape(-1, 4), ro.reshape(-1, 4)
        alb[:, :3] *= (1.0 - AO_IN_ALBEDO + AO_IN_ALBEDO * ao[:, :1])
        alb[:, 3] = 1.0 - ro[:, 0]
        baked["albedo"].pixels.foreach_set(alb.ravel())
    for kind, img in baked.items():
        if kind == "roughness":
            continue  # lives in the albedo alpha
        img.filepath_raw = os.path.join(out, f"{prefix}_{kind}.png")
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


def build(culture, out, blend_dir, save_blend=True, bake=True, size=BAKE_SIZE):
    """Culture kits (ODD S23, T16): every piece is baked with the lit DAY material (albedo + normal, no glow) for the
    route. The platform is also baked, on the same UVs, with the dark night stone + glowing cracks as
    <culture>_arena_platform_* for the boss arena (Egipto, Grecia, China; Mesopotamia's arena uses the `arena` kit)."""
    clear()
    os.makedirs(out, exist_ok=True)
    objs = []
    night = {}
    if culture == "arena":
        basalt = template_basalt()
        o = platform("arena_platform", STYLE["grecia"], basalt, basalt)
        objs.append(o)
    else:
        base, accent = PALETTE[culture]
        st = STYLE[culture]
        stone = day_material(culture + "_stone", DAY[culture]["stone"])
        trim = day_material(culture + "_trim", DAY[culture]["trim"])
        if culture != "mesopotamia":
            night = {stone.name: stone_material(culture + "_arena_stone", base, accent, st),
                     trim.name: stone_material(culture + "_arena_trim", base, accent, st, trim=True)}
        only = set(ONLY) if ONLY else None
        for i, (kind, fn) in enumerate(KITS.items()):
            if only and kind not in only:
                continue
            o = fn(f"{culture}_{kind}", st, stone, trim)
            o.location = (i * 12.0, 0.0, 0.0)
            objs.append(o)
    for o in objs:
        if bake:
            if culture == "arena":
                bake_maps(o, out, size)
            else:
                bake_maps(o, out, size, kinds=SHARP)
                if night and o.name.endswith("_platform"):
                    day = [s.material for s in o.material_slots]
                    for s in o.material_slots:
                        s.material = night[s.material.name]
                    bake_maps(o, out, size, prefix=f"{culture}_arena_platform", unwrap=False)
                    for s, m in zip(o.material_slots, day):
                        s.material = m
        export_fbx(o, out)
    if culture != "arena" and not ONLY:
        p = emissive_pickup(f"{culture}_pickup", PALETTE[culture][1], (len(KITS) * 12.0, 0.0, 1.0))
        export_fbx(p, out)
        objs.append(p)
    if save_blend and blend_dir and not ONLY:
        # .blend must live OUTSIDE Unity's import path (folder ending in ~): Unity would call Blender to import it
        os.makedirs(blend_dir, exist_ok=True)
        bpy.ops.wm.save_as_mainfile(filepath=os.path.join(blend_dir, culture + "_kit.blend"))
    return [o.name for o in objs]


if __name__ == "__main__":
    a = sys.argv[sys.argv.index("--") + 1:] if "--" in sys.argv else []
    culture = a[a.index("--culture") + 1]
    out = a[a.index("--out") + 1]
    blend_dir = a[a.index("--blend-dir") + 1] if "--blend-dir" in a else ""
    size = int(a[a.index("--size") + 1]) if "--size" in a else BAKE_SIZE
    # --only stairs,block : rebuild just those pieces (the other FBX/maps stay untouched; the .blend is not saved)
    ONLY = a[a.index("--only") + 1].split(",") if "--only" in a else []
    print("KIT_OK", build(culture, out, blend_dir, bake="--no-bake" not in a, size=size))
