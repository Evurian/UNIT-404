"""Serpiente-dragón — reconstrucción procedural del cuerpo, espinas, rig y animación.
(skill serpiente-dragon-procedural; parámetros explicados en references/parametros.md)

Ejecutar dentro de Blender (Text Editor o MCP):
    exec(open(r"...\\Apophis\\scripts\\apophis_body.py", encoding="utf-8").read())

Conserva: CTRL_Master, Apophis_Rig (huesos Head/Jaw), cabeza, materiales y
GN_Scales_Body_v01. Reconstruye: curva, controles CTRL_xx, malla del cuerpo,
espinas, huesos Spine_xx, pesos, atributos rest_pos / scale_uv y animación.
"""
import bpy, bmesh, math
from mathutils import Vector, Matrix
from mathutils.geometry import interpolate_bezier, intersect_point_line
from mathutils.kdtree import KDTree

# ---------------------------------------------------------------- parámetros
def _g(x, y, r):  # punto apoyado en el suelo
    return ((x, y, r + 0.07), r)
PTS = [  # (x, y, z), radio  — del cuello a la punta de la cola
    # el cuello nace con el grosor de la cabeza (P0 queda dentro de la nuca),
    # sale recto hacia atrás y luego baja en un arco amplio hacia la S;
    # el radio decrece de forma continua hasta la punta de la cola
    ((0.30, 0.10, 5.50), 0.70), ((1.72, 0.74, 6.05), 0.70), ((2.65, 1.15, 5.25), 0.68),
    ((2.35, 0.75, 4.00), 0.66), ((1.00, 0.30, 3.10), 0.64), ((-0.60, 0.15, 2.50), 0.62),
    ((-1.60, 0.00, 1.60), 0.60), ((-1.10, -0.20, 0.80), 0.58),
    # gran anillo enroscado en el suelo
    _g(0.00, -0.45, 0.56), _g(1.40, -0.95, 0.54), _g(2.80, -0.65, 0.52), _g(3.60, 0.40, 0.50),
    _g(3.40, 1.80, 0.47), _g(2.20, 2.75, 0.44), _g(0.60, 2.95, 0.41), _g(-0.90, 2.45, 0.38),
    _g(-2.05, 1.30, 0.35), _g(-2.45, 0.00, 0.32), _g(-2.10, -1.35, 0.29), _g(-1.00, -2.10, 0.26),
    _g(0.50, -2.35, 0.23), _g(1.95, -2.10, 0.20),
    # cola que se levanta y se enrosca (por fuera del anillo)
    ((3.10, -1.95, 0.28), 0.17), ((3.85, -1.75, 0.60), 0.13), ((4.25, -1.25, 0.95), 0.10),
    ((4.10, -0.85, 1.22), 0.07), ((3.85, -0.95, 1.08), 0.04), ((3.90, -1.20, 0.92), 0.01),
]
NB = 48                       # huesos de columna
F = 96                        # fotogramas del ciclo
HEAD_FWD = Vector((-0.88, -0.40, -0.22)).normalized()   # eje del cráneo en reposo (hacia delante y algo abajo)

sc = bpy.context.scene; vl = bpy.context.view_layer
col = bpy.data.collections["Apophis"]
master = bpy.data.objects["CTRL_Master"]
rig = bpy.data.objects["Apophis_Rig"]
win = bpy.context.window_manager.windows[0]
area = next(a for a in win.screen.areas if a.type == 'VIEW_3D')
region = next(r for r in area.regions if r.type == 'WINDOW')
def ctx(**kw): return bpy.context.temp_override(window=win, area=area, region=region, **kw)

sc.frame_set(1)
master.location = (0.35, 0, 0.13); master.rotation_euler = (0, 0, 0); vl.update()
OLD_NECK = rig.matrix_world @ rig.data.bones["Head"].head_local   # cuello actual (para mover la cabeza)

# ---------------------------------------------------------------- limpiar lo viejo
for o in [o for o in bpy.data.objects if o.name.startswith("CTRL_") and o != master] + \
         [bpy.data.objects.get(n) for n in ("Apophis_Path", "Apophis_Body", "Apophis_Spikes")]:
    if o: bpy.data.objects.remove(o, do_unlink=True)
for c in [c for c in bpy.data.curves if c.users == 0]: bpy.data.curves.remove(c)
for m in [m for m in bpy.data.meshes if m.users == 0]: bpy.data.meshes.remove(m)

def parent_keep(o, p):
    o.parent = p; o.matrix_parent_inverse = p.matrix_world.inverted()

# ---------------------------------------------------------------- curva guía
cu = bpy.data.curves.new("Apophis_Path", 'CURVE'); cu.dimensions = '3D'
cu.twist_mode = 'MINIMUM'; cu.resolution_u = 20
sp = cu.splines.new('BEZIER'); sp.bezier_points.add(len(PTS) - 1)
for bp, (co, r) in zip(sp.bezier_points, PTS):
    bp.co = co; bp.handle_left_type = bp.handle_right_type = 'AUTO'; bp.radius = r
path = bpy.data.objects.new("Apophis_Path", cu); col.objects.link(path)
parent_keep(path, master); path.hide_render = True; vl.update()

# ---------------------------------------------------------------- malla del cuerpo
bc = cu.copy(); bc.bevel_mode = 'ROUND'; bc.bevel_depth = 1.0; bc.bevel_resolution = 7
bc.use_fill_caps = True
tmp = bpy.data.objects.new("tmp", bc); col.objects.link(tmp)
tmp.matrix_world = path.matrix_world.copy(); vl.update()
me = bpy.data.meshes.new_from_object(tmp.evaluated_get(bpy.context.evaluated_depsgraph_get()))
me.transform(tmp.matrix_world)
bpy.data.objects.remove(tmp, do_unlink=True); bpy.data.curves.remove(bc)
me.name = "Apophis_Body"
for p in me.polygons: p.use_smooth = True
body = bpy.data.objects.new("Apophis_Body", me); col.objects.link(body)

# ---------------------------------------------------------------- muestreo de la curva (mundo)
PM = path.matrix_world
S = []
for i in range(len(PTS) - 1):
    a, b = sp.bezier_points[i], sp.bezier_points[i + 1]
    seg = interpolate_bezier(a.co, a.handle_right, b.handle_left, b.co, 49)
    for j in range(48):
        t = j / 48; S.append((PM @ seg[j], a.radius * (1 - t) + b.radius * t))
S.append((PM @ sp.bezier_points[-1].co, sp.bezier_points[-1].radius))
P = [s[0] for s in S]; R = [s[1] for s in S]; n = len(P)
acc = [0.0]
for i in range(1, n): acc.append(acc[-1] + (P[i] - P[i - 1]).length)
total = acc[-1]
T = [(P[min(i + 1, n - 1)] - P[max(i - 1, 0)]).normalized() for i in range(n)]
Y = Vector((0, 1, 0))
# dirección del lomo: por fuera de cada curva, hacia arriba en el suelo, giro suave
k = [0.0] * n
for i in range(1, n - 1): k[i] = T[i - 1].cross(T[i + 1]).y
ref = 1 if sum(k[:n // 8]) > 0 else -1
phi_t = [0.0 if (k[i] * ref >= 0 or P[i].z < 0.9) else -math.pi for i in range(n)]
phi = []
for i in range(n):
    ws = s = 0.0
    for j in range(max(0, i - 150), min(n, i + 150)):
        d = acc[j] - acc[i]; w = math.exp(-(d * d) / (2 * 0.45 ** 2)); ws += w; s += w * phi_t[j]
    phi.append(s / ws)
DORS = []
Z = Vector((0, 0, 1))
for i in range(n):
    t = T[i]; up = (Z - t * t.z)
    up = up.normalized() if up.length > 1e-4 else Vector((1, 0, 0))
    cy = t.cross(Y)
    if cy.length > 0.2:            # tramo erguido (en el plano XZ): lomo por fuera de la curva
        n0 = cy.normalized(); b = t.cross(n0).normalized()
        plane = (n0 * math.cos(phi[i]) + b * math.sin(phi[i])).normalized()
    else:
        plane = up
    g = max(0.0, min(1.0, (P[i].z - 0.9) / 0.8))   # en el suelo el lomo mira hacia arriba
    d = up * (1 - g) + plane * g
    d = d - t * d.dot(t)
    DORS.append(d.normalized() if d.length > 1e-4 else up)
def idx_at(dist):
    for i in range(1, n):
        if acc[i] >= dist: return i
    return n - 1

# ---------------------------------------------------------------- huesos de columna
RW = rig.matrix_world; RI = RW.inverted()
joints = []
for q in range(NB + 1):
    i = idx_at(total * q / NB); joints.append(P[i])
neck_new = P[0]
delta = neck_new - OLD_NECK
for o in bpy.context.selected_objects: o.select_set(False)
rig.hide_viewport = False; vl.objects.active = rig
with ctx(active_object=rig):
    bpy.ops.object.mode_set(mode='EDIT')
    eb = rig.data.edit_bones
    for b in [b for b in eb if b.name.startswith("Spine_")]: eb.remove(b)
    prev = None
    for q in range(NB):
        b = eb.new(f"Spine_{q:02d}"); b.head = RI @ joints[q]; b.tail = RI @ joints[q + 1]
        b.align_roll(RI.to_3x3() @ Vector((0, -1, 0)))
        if prev: b.parent = prev; b.use_connect = True
        prev = b
    # mover cabeza y mandíbula al nuevo cuello
    dl = RI.to_3x3() @ delta
    eb["Jaw"].head += dl; eb["Jaw"].tail += dl
    # hueso Head alineado con el eje del cráneo, con raíz en el extremo del cuello
    hl = eb["Head"].length
    up = (Vector((0, 0, 1)) - HEAD_FWD * HEAD_FWD.z).normalized()
    eb["Head"].use_connect = False
    eb["Head"].head = RI @ neck_new; eb["Head"].tail = RI @ (neck_new + HEAD_FWD * hl)
    eb["Head"].align_roll(RI.to_3x3() @ up)
    eb["Head"].parent = eb["Spine_00"]
    bpy.ops.object.mode_set(mode='OBJECT')
for pb in rig.pose.bones:
    if pb.name.startswith("Spine_"):
        pb.color.palette = 'THEME09'
last = rig.pose.bones[f"Spine_{NB - 1:02d}"]
c = last.constraints.new('SPLINE_IK'); c.target = path; c.chain_count = NB
c.use_curve_radius = False; c.y_scale_mode = 'FIT_CURVE'; c.xz_scale_mode = 'NONE'
c.use_even_divisions = True

# ---------------------------------------------------------------- controles (hooks)
CTRLS = []
for i, bp in enumerate(sp.bezier_points):
    e = bpy.data.objects.new(f"CTRL_{i:02d}", None); col.objects.link(e)
    e.empty_display_type = 'CIRCLE'; e.empty_display_size = max(0.22, bp.radius * 1.4)
    e.location = PM @ bp.co; e.rotation_euler = (math.pi / 2, 0, 0); e.hide_render = True
    vl.update(); parent_keep(e, master); vl.update()
    h = path.modifiers.new(f"Hook_{i:02d}", 'HOOK'); h.object = e
    h.vertex_indices_set([3 * i, 3 * i + 1, 3 * i + 2]); h.center = bp.co
    h.matrix_inverse = (path.matrix_world.inverted() @ e.matrix_world).inverted()
    CTRLS.append(e)
vl.update()

# reposo = pose evaluada (divisiones uniformes => punto fijo)
for it in range(3):
    vl.update()
    data = {pb.name: (RI @ (RW @ pb.head), RI @ (RW @ pb.tail), pb.z_axis.copy())
            for pb in rig.pose.bones if pb.name.startswith("Spine_")}
    data = {k2: (pb.head.copy(), pb.tail.copy(), pb.z_axis.copy())
            for k2, pb in ((pb.name, pb) for pb in rig.pose.bones if pb.name.startswith("Spine_"))}
    with ctx(active_object=rig):
        bpy.ops.object.mode_set(mode='EDIT')
        eb = rig.data.edit_bones
        for k2 in data: eb[k2].use_connect = False
        for k2, (h, tl, z) in data.items(): eb[k2].head = h; eb[k2].tail = tl; eb[k2].align_roll(z)
        for k2 in data:
            if eb[k2].parent and eb[k2].parent.name.startswith("Spine_"): eb[k2].use_connect = True
        bpy.ops.object.mode_set(mode='OBJECT')
vl.update()
err = max(max((pb.head - pb.bone.head_local).length, (pb.tail - pb.bone.tail_local).length)
          for pb in rig.pose.bones if pb.name.startswith("Spine_"))
print("rig reposo error:", round(err, 5))

# ---------------------------------------------------------------- pesos (máx 3 influencias)
bones = [b for b in rig.data.bones if b.name.startswith("Spine_")]
SEG = [(RW @ b.head_local, RW @ b.tail_local) for b in bones]
def weigh(obj, pts):
    obj.vertex_groups.clear(); vgs = [obj.vertex_groups.new(name=b.name) for b in bones]
    for vi, p in enumerate(pts):
        ds = []
        for A, B in SEG:
            q, tt = intersect_point_line(p, A, B); tt = max(0, min(1, tt)); ds.append((A.lerp(B, tt) - p).length)
        o3 = sorted(range(len(ds)), key=lambda x: ds[x])[:3]
        w = [(x, math.exp(-(ds[x] - ds[o3[0]]) * 8)) for x in o3]; s = sum(v for _, v in w)
        for x, v in w:
            if v / s > 0.03: vgs[x].add([vi], v / s, 'REPLACE')
world_pts = [v.co.copy() for v in me.vertices]
weigh(body, world_pts)
a = me.attributes.new("rest_pos", 'FLOAT_VECTOR', 'POINT')
a.data.foreach_set("vector", [x for p in world_pts for x in p])

# ---------------------------------------------------------------- coordenadas de escamas
rowc = [0.0]
for i in range(1, n): rowc.append(rowc[-1] + (P[i] - P[i - 1]).length * 0.55 / (0.5 * (R[i] + R[i - 1]) + 0.10))
kd = KDTree(n)
for i, p in enumerate(P): kd.insert(p, i)
kd.balance()
vals = []
for p in world_pts:
    _, i, _ = kd.find(p); best = None
    for j in (i - 1, i):
        if 0 <= j < n - 1:
            q, tt = intersect_point_line(p, P[j], P[j + 1]); tt = max(0, min(1, tt))
            d = (P[j].lerp(P[j + 1], tt) - p).length
            if best is None or d < best[0]: best = (d, j, tt)
    d, j, tt = best
    cpt = P[j].lerp(P[j + 1], tt); t = T[j].lerp(T[j + 1], tt).normalized()
    dv = DORS[j].lerp(DORS[j + 1], tt); dv = (dv - t * dv.dot(t)).normalized(); side = t.cross(dv).normalized()
    r = p - cpt; r = r - t * r.dot(t)
    ang = math.atan2(r.dot(side), r.dot(dv))
    vals.append((math.cos(ang), math.sin(ang), rowc[j] * (1 - tt) + rowc[j + 1] * tt))
a = me.attributes.new("scale_uv", 'FLOAT_VECTOR', 'POINT')
a.data.foreach_set("vector", [x for v in vals for x in v])
me.materials.append(bpy.data.materials["Apophis_Body_Scales"])
body["crack_scale"] = 1.0
parent_keep(body, rig)
am = body.modifiers.new("Armature", 'ARMATURE'); am.object = rig
gm = body.modifiers.new("GN_Scales", 'NODES'); gm.node_group = bpy.data.node_groups["GN_Scales_Body_v01"]

# ---------------------------------------------------------------- espinas dorsales
def add_spike(bm, base, dirv, back, L, r0, n_rad=7, n_len=7, bend=0.3):
    dirv = dirv.normalized(); sd = dirv.cross(back).normalized(); b2 = sd.cross(dirv).normalized()
    rings = []
    for q in range(n_len):
        t = q / (n_len - 1); p = base + dirv * L * t + b2 * bend * L * t * t; r = r0 * (1 - t) ** 1.2
        if q == n_len - 1: rings.append([bm.verts.new(p)]); break
        rings.append([bm.verts.new(p + (sd * math.cos(a) + b2 * math.sin(a)) * r)
                      for a in [2 * math.pi * z / n_rad for z in range(n_rad)]])
    bm.faces.new(list(reversed(rings[0])))
    for q in range(len(rings) - 1):
        A, B = rings[q], rings[q + 1]
        for z in range(n_rad):
            bm.faces.new((A[z], A[(z + 1) % n_rad], B[0]) if len(B) == 1 else
                         (A[z], A[(z + 1) % n_rad], B[(z + 1) % n_rad], B[z]))
bm = bmesh.new(); dist = 1.10; cnt = 0
while dist < total * 0.94:
    i = idx_at(dist); p = P[i]; r = R[i]; t = T[i]; d = DORS[i]; side = t.cross(d).normalized()
    big = 1.0 * r + 0.05
    add_spike(bm, p + d * r * 0.75, d + t * 0.7, t, big * (1.2 if cnt % 2 == 0 else 0.8), r * 0.30, bend=0.4)
    if r > 0.14:
        for s in (1, -1):
            dv = (d * 0.75 + side * s * 0.75).normalized()
            add_spike(bm, p + dv * r * 0.8, dv + t * 0.6, t, big * 0.45, r * 0.15, n_rad=5, n_len=4, bend=0.3)
    dist += max(0.15, r * 0.5); cnt += 1
sme = bpy.data.meshes.new("Apophis_Spikes"); bm.to_mesh(sme); bm.free()
for f in sme.polygons: f.use_smooth = True
sme.materials.append(bpy.data.materials["Apophis_Horn"])
spk = bpy.data.objects.new("Apophis_Spikes", sme); col.objects.link(spk)
spts = [v.co.copy() for v in sme.vertices]; weigh(spk, spts)
a = sme.attributes.new("rest_pos", 'FLOAT_VECTOR', 'POINT'); a.data.foreach_set("vector", [x for p in spts for x in p])
spk["crack_scale"] = 1.0
parent_keep(spk, rig); am2 = spk.modifiers.new("Armature", 'ARMATURE'); am2.object = rig

# ---------------------------------------------------------------- animación (onda cola -> cabeza)
NC = len(CTRLS)
rest = [e.location.copy() for e in CTRLS]
def amp(i, base, top):
    """Amplitud según la altura del control: el tramo erguido se mueve más,
    el anillo del suelo apenas se desliza y la punta de la cola se agita."""
    z = rest[i].z
    a = base + (top - base) * max(0.0, min(1.0, (z - 0.5) / 4.5))
    if i >= NC - 5: a += 0.10 * (i - (NC - 6)) / 5
    return a
for f in range(1, F + 2, 4):
    w = 2 * math.pi * (f - 1) / F
    for i, e in enumerate(CTRLS):
        ph = w - (NC - 1 - i) * 0.35
        ax = amp(i, 0.04, 0.30); ay = amp(i, 0.04, 0.28); az = amp(i, 0.0, 0.13) if i < NC - 6 else amp(i, 0.03, 0.1)
        e.location = rest[i] + Vector((ax * math.sin(ph), ay * math.sin(ph + math.pi / 2), az * math.sin(2 * ph)))
        e.keyframe_insert("location", frame=f)
for e, r0 in zip(CTRLS, rest): e.location = r0
sc.frame_set(1); vl.update()
for o in CTRLS + [rig, path]: o.hide_viewport = True
print("cuerpo reconstruido: largo %.2f m, %d huesos, %d controles, %d espinas" % (total, NB, NC, cnt))
