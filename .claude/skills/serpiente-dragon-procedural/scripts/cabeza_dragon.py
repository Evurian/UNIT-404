"""Serpiente-dragón — cabeza procedural (cráneo, mandíbula, cuernos, espinas, dientes, ojos).
(skill serpiente-dragon-procedural; ejecutar DESPUÉS de cuerpo_serpiente.py)

Ejecutar dentro de Blender:
    exec(open(r"...\\Apophis\\scripts\\apophis_head.py", encoding="utf-8").read())

Reemplaza las mallas de Apophis_Head / Apophis_Jaw / Apophis_Eyelids y recoloca
Eye_L / Eye_R, el hueso Jaw y la luz de la boca. Mantiene la posición de la
cabeza (hueso Head) y los materiales.
Índices de material: 0 escamas, 1 boca, 2 dientes, 3 cuerno/obsidiana.
"""
import bpy, bmesh, math, random
from mathutils import Vector, Matrix
from mathutils.bvhtree import BVHTree

S = 1.50                       # escala global de la cabeza
OPEN = math.radians(22)        # apertura de la boca (moderada, como la referencia)
HINGE = Vector((-0.34, 0, -0.14))
random.seed(11)

# ----------------------------------------------------------- utilidades
def lerp_keys(keys, u):
    for i in range(len(keys) - 1):
        u0, v0 = keys[i]; u1, v1 = keys[i + 1]
        if u <= u1:
            t = (u - u0) / (u1 - u0); t = t * t * (3 - 2 * t); return v0 + (v1 - v0) * t
    return keys[-1][1]
def spow(a, p): return math.copysign(abs(a) ** p, a)
def angd(a, b): return math.atan2(math.sin(a - b), math.cos(a - b))

def tube_path(bm, pts, radii, n_rad=8, mat=0):
    rings = []; prev = None
    for i, p in enumerate(pts):
        tan = (pts[min(i + 1, len(pts) - 1)] - pts[max(i - 1, 0)]).normalized()
        if prev is None:
            up = Vector((0, 0, 1)) if abs(tan.z) < 0.9 else Vector((1, 0, 0)); b1 = tan.cross(up).normalized()
        else:
            b1 = (prev - tan * prev.dot(tan)).normalized()
        b2 = tan.cross(b1); prev = b1; r = radii[i]
        rings.append([bm.verts.new(p)] if r <= 1e-5 else
                     [bm.verts.new(p + (b1 * math.cos(a) + b2 * math.sin(a)) * r)
                      for a in [2 * math.pi * k / n_rad for k in range(n_rad)]])
    fs = []
    if len(rings[0]) > 1: fs.append(bm.faces.new(list(reversed(rings[0]))))
    for i in range(len(rings) - 1):
        A, B = rings[i], rings[i + 1]
        for k in range(n_rad):
            fs.append(bm.faces.new((A[k], A[(k + 1) % n_rad], B[0])) if len(B) == 1 else
                      bm.faces.new((A[k], A[(k + 1) % n_rad], B[(k + 1) % n_rad], B[k])))
    for f in fs: f.material_index = mat; f.smooth = True

def horn(bm, base, d, bend, L, r0, n=12, n_rad=8, mat=3, power=1.1):
    d = Vector(d).normalized(); bend = Vector(bend)
    tube_path(bm, [base + d * L * t + bend * L * t * t for t in [i / (n - 1) for i in range(n)]],
              [r0 * (1 - i / (n - 1)) ** power for i in range(n)], n_rad, mat)

def loft(bm, stations, n, shape, mat_fn):
    rings = [[bm.verts.new(shape(X, 2 * math.pi * k / n)) for k in range(n)] for X in stations]
    for i in range(len(rings) - 1):
        A, B = rings[i], rings[i + 1]
        for k in range(n):
            f = bm.faces.new((A[k], B[k], B[(k + 1) % n], A[(k + 1) % n])); f.material_index = mat_fn(f); f.smooth = True
    bm.faces.new(list(reversed(rings[0]))); bm.faces.new(rings[-1])

# ----------------------------------------------------------- cráneo
X0, X1 = -0.60, 1.02            # cráneo corto: hocico de dragón, no de cocodrilo
U = lambda X: (X - X0) / (X1 - X0)
W_K = [(0, .44), (.15, .53), (.30, .56), (.45, .49), (.60, .42), (.78, .37), (.90, .34), (.96, .28), (.99, .19), (1, .06)]
HT_K = [(0, .42), (.14, .60), (.30, .64), (.42, .57), (.55, .47), (.72, .41), (.85, .41), (.94, .35), (.985, .25), (1, .08)]
HB_K = [(0, -.36), (.20, -.26), (.32, -.17), (.45, -.15), (.85, -.15), (1, -.10)]
sw = lambda X: lerp_keys(W_K, U(X)); sht = lambda X: lerp_keys(HT_K, U(X)); shb = lambda X: lerp_keys(HB_K, U(X))
EYE_X, EYE_TH = 0.16, math.radians(28)

def skull(X, th):
    w, ht, hb = sw(X), sht(X), shb(X); c, s = math.cos(th), math.sin(th)
    y = w * spow(c, 0.72)
    z = ht * s ** 0.8 if s > 0 else hb * abs(s) ** 0.55
    for sg in (1, -1):
        bt = math.radians(46) if sg > 0 else math.radians(134)
        brow = math.exp(-((X - (EYE_X - 0.01)) ** 2) / 0.012) * math.exp(-(angd(th, bt) ** 2) / 0.035)
        z += 0.21 * brow; y += 0.14 * brow * sg            # ceja pesada que sobresale
        et = EYE_TH if sg > 0 else math.pi - EYE_TH
        sock = math.exp(-((X - EYE_X) ** 2) / 0.009) * math.exp(-(angd(th, et) ** 2) / 0.035)
        y -= 0.075 * sock * sg; z -= 0.015 * sock        # cuenca profunda
        cheek = math.exp(-((X + 0.05) ** 2) / 0.05) * math.exp(-(angd(th, 0 if sg > 0 else math.pi) ** 2) / 0.12)
        y += 0.05 * cheek * sg                            # pómulo
    z += 0.04 * math.exp(-((X - 0.92) ** 2) / 0.004) * max(0, s) * (abs(c) ** 0.5)   # fosas nasales
    z += 0.025 * math.exp(-(angd(th, math.pi / 2) ** 2) / 0.02) * (1 if 0.40 < X < 0.90 else 0)  # cresta nasal
    return Vector((X, y, z))

def skull_mat(f):
    cn = f.calc_center_median()
    return 1 if (cn.z < shb(cn.x) * 0.35 and cn.x > -0.05 and abs(cn.y) < sw(cn.x) * 0.82) else 0

bmH = bmesh.new()
loft(bmH, [X0 + (X1 - X0) * i / 40 for i in range(41)], 40, skull, skull_mat)
bvh = BVHTree.FromBMesh(bmH)
def surf(X, th, out=2.0):
    radial = Vector((0, math.cos(th), math.sin(th)))
    hit = bvh.ray_cast(Vector((X, 0, 0.12)) + radial * out, -radial)[0]
    return hit, radial

for sg in (1, -1):
    s3 = Vector((1, sg, 1))
    # cuernos principales: hacia atrás, curvándose hacia arriba
    horn(bmH, Vector((-0.12, 0.27 * sg, 0.56)), (-1, 0.40 * sg, 0.55), (-0.15, 0.10 * sg, -0.55), 1.45, 0.14, n=16, n_rad=10)
    horn(bmH, Vector((-0.34, 0.42 * sg, 0.30)), (-1, 0.55 * sg, 0.05), (0, 0.05 * sg, 0.35), 0.95, 0.09, n=12)
    # espinas de ceja (sobre el ojo)
    for q in range(3):
        horn(bmH, Vector((EYE_X - 0.04 - 0.11 * q, (0.46 - 0.02 * q) * sg, 0.62 - 0.03 * q)),
             (-0.9, 0.45 * sg, 0.55), (-0.25, 0, 0), 0.24 + 0.06 * q, 0.04, n=7, n_rad=6)
    # gola de mejilla: espinas largas hacia atrás y afuera
    for q in range(6):
        X = 0.05 - 0.13 * q
        hit, radial = surf(X, math.radians(-8) if sg > 0 else math.radians(188))
        if hit is None: continue
        L = 0.45 + 0.08 * q + random.uniform(-0.05, 0.05)
        horn(bmH, hit - radial * 0.02, (-1, 0.85 * sg, -0.25), (-0.1, 0.05 * sg, -0.12), L, 0.05, n=8, n_rad=6)
    # protuberancias nasales
    horn(bmH, Vector((0.92, 0.11 * sg, sht(0.92) - 0.01)), (0.3, 0.3 * sg, 1), (-0.3, 0, 0), 0.10, 0.035, n=5, n_rad=6)
# cresta nasal de pequeñas espinas
for q in range(6):
    X = 0.82 - 0.11 * q
    horn(bmH, Vector((X, 0, sht(X) + 0.01)), (-0.6, 0, 1), (-0.4, 0, 0), 0.08 + 0.025 * q, 0.03, n=6, n_rad=6)
# corona de espinas (tres filas sobre la nuca)
for X, cnt, Lb in ((-0.20, 13, 0.55), (-0.42, 15, 0.80), (-0.58, 13, 0.62)):
    for q in range(cnt):
        th = math.radians(15 + 150 * q / (cnt - 1))
        hit, radial = surf(X, th)
        if hit is None: continue
        top = math.sin(th)
        L = Lb * (0.7 + 0.5 * top) * random.uniform(0.85, 1.15)
        horn(bmH, hit - radial * 0.02, radial * 0.45 + Vector((-1, 0, 0.05)), Vector((-0.2, 0, -0.18)),
             L, 0.045 + 0.03 * top, n=9, n_rad=7)
# dientes superiores (colmillos grandes delante)
X = 0.96; q = 0
while X > 0.02:
    w = sw(X) * 0.80; hb = shb(X)
    fang = abs(X - 0.86) < 0.05; mid = abs(X - 0.48) < 0.05
    L = 0.42 if fang else (0.20 if mid else 0.09 + 0.03 * (q % 2)); r = 0.055 if fang else (0.032 if mid else 0.019)
    for sg in (1, -1):
        horn(bmH, Vector((X, w * sg, hb + 0.03)), (0.15, -0.12 * sg, -1), (-0.28 if fang else 0, 0, 0), L, r, n=7, n_rad=7, mat=2, power=1.3)
    X -= 0.095; q += 1
bmH.transform(Matrix.Diagonal((S, S, S, 1)))
head_me = bpy.data.meshes.new("Apophis_Head_v4"); bmH.to_mesh(head_me); bmH.free()

# ----------------------------------------------------------- mandíbula (coords locales: origen en bisagra)
J0, J1 = -0.12, 1.30            # talón corto: queda oculto en cráneo/garganta
UJ = lambda X: (X - J0) / (J1 - J0)
WJ = [(0, .30), (.10, .42), (.18, .47), (.45, .40), (.80, .32), (.93, .26), (1, .10)]
TJ = [(0, .04), (1, .03)]
BJ = [(0, -.26), (.10, -.38), (.25, -.36), (.65, -.29), (.88, -.23), (.95, -.17), (1, -.06)]
jw = lambda X: lerp_keys(WJ, UJ(X)); jt = lambda X: lerp_keys(TJ, UJ(X)); jb = lambda X: lerp_keys(BJ, UJ(X))
def jaw(X, th):
    c, s = math.cos(th), math.sin(th)
    return Vector((X, jw(X) * spow(c, 0.7), jt(X) * abs(s) ** 0.5 if s > 0 else jb(X) * abs(s) ** 0.8))
def jaw_mat(f):
    cn = f.calc_center_median()
    return 1 if (cn.z > jt(cn.x) * 0.4 and cn.x > 0.12 and abs(cn.y) < jw(cn.x) * 0.82) else 0
bmJ = bmesh.new()
loft(bmJ, [J0 + (J1 - J0) * i / 32 for i in range(33)], 32, jaw, jaw_mat)
X = 1.24; q = 0
while X > 0.36:
    w = jw(X) * 0.80; fang = abs(X - 1.10) < 0.05
    L = 0.32 if fang else 0.08 + 0.025 * (q % 2); r = 0.05 if fang else 0.018
    for sg in (1, -1):
        horn(bmJ, Vector((X, w * sg, jt(X) - 0.02)), (0.1, -0.1 * sg, 1), (-0.22 if fang else 0, 0, 0), L, r, n=7, n_rad=7, mat=2, power=1.3)
    X -= 0.095; q += 1
for sg in (1, -1):        # espinas bajo la mandíbula ("barba")
    for q in range(5):
        X = 0.95 - 0.24 * q
        horn(bmJ, Vector((X, jw(X) * 0.55 * sg, jb(X) * 0.85)), (-0.7, 0.45 * sg, -0.75), (-0.3, 0, 0),
             0.22 + 0.07 * q, 0.04, n=7, n_rad=6)
tp = [Vector((0.25 + 0.08 * i, 0, 0.06 + 0.02 * math.sin(i * 0.7))) for i in range(10)]
tube_path(bmJ, tp, [0.075 * (1 - 0.3 * i / 9) for i in range(10)], 8, 1)
for sg in (1, -1):
    tube_path(bmJ, [tp[-1] + Vector((0.06 * i, 0.022 * i * sg, 0.012 * i)) for i in range(5)],
              [0.035 * (1 - i / 4) + 0.002 for i in range(5)], 6, 1)
bmJ.transform(Matrix.Diagonal((S, S, S, 1)))
jaw_me = bpy.data.meshes.new("Apophis_Jaw_v4"); bmJ.to_mesh(jaw_me); bmJ.free()

# ----------------------------------------------------------- aplicar en la escena
vl = bpy.context.view_layer; sc = bpy.context.scene; sc.frame_set(1)
rig = bpy.data.objects["Apophis_Rig"]
head = bpy.data.objects["Apophis_Head"]; jawo = bpy.data.objects["Apophis_Jaw"]
for pb in (rig.pose.bones["Head"], rig.pose.bones["Jaw"]):
    pb.rotation_mode = 'XYZ'
vl.update()
# marco de la cabeza a partir del hueso (no se acumula al reejecutar): el
# extremo del cuello queda dentro de la nuca, NECK_OFF por detrás del origen del cráneo
NECK_OFF = Vector((0.10, 0, 0.05)) * S      # cuello grueso: su extremo entra hondo en la nuca, algo bajo (garganta)
R_OBJ = Matrix(((0, -1, 0, 0), (1, 0, 0, 0), (0, 0, 1, 0), (0, 0, 0, 1)))   # ejes del hueso -> objeto
HW = rig.matrix_world @ rig.pose.bones["Head"].matrix @ R_OBJ @ Matrix.Translation(NECK_OFF)
def swap(obj, newme):
    old = obj.data; mats = list(old.materials)
    obj.data = newme; bpy.data.meshes.remove(old); newme.name = obj.name
    for m in mats: newme.materials.append(m)
swap(head, head_me)
swap(jawo, jaw_me)
JW = HW @ Matrix.Translation(HINGE * S) @ Matrix.Rotation(OPEN, 4, 'Y')
win = bpy.context.window_manager.windows[0]
area = next(a for a in win.screen.areas if a.type == 'VIEW_3D'); region = next(r for r in area.regions if r.type == 'WINDOW')
RI = rig.matrix_world.inverted(); hid = rig.hide_viewport; rig.hide_viewport = False
vl.objects.active = rig
with bpy.context.temp_override(window=win, area=area, region=region, active_object=rig):
    bpy.ops.object.mode_set(mode='EDIT')
    jb_ = rig.data.edit_bones["Jaw"]
    jb_.head = RI @ (JW @ Vector((0, 0, 0))); jb_.tail = RI @ (JW @ Vector((1.2, 0, 0)))
    jb_.align_roll(RI.to_3x3() @ (JW.to_3x3() @ Vector((0, 0, 1))))
    bpy.ops.object.mode_set(mode='OBJECT')
rig.hide_viewport = hid
vl.update(); jawo.matrix_world = JW; head.matrix_world = HW; vl.update()

# ojos en la nueva cuenca
for sg, nm in ((1, "Eye_L"), (-1, "Eye_R")):
    sock = skull(EYE_X, EYE_TH if sg > 0 else math.pi - EYE_TH) * S
    nrm = Vector((0.10, 0.92 * sg, 0.30)).normalized()
    look = Vector((0.65, 0.74 * sg, -0.05)).normalized()
    Rr = 0.098 * S; cpos = sock - nrm * 0.065 * S
    e = bpy.data.objects[nm]
    e.matrix_world = HW @ Matrix.Translation(cpos) @ look.to_track_quat('X', 'Z').to_matrix().to_4x4() @ Matrix.Diagonal((Rr, Rr, Rr, 1))
# párpado inferior + borde (anillo más grueso abajo; la ceja hace de párpado superior)
lids = bpy.data.objects["Apophis_Eyelids"]; bmL = bmesh.new()
for sg in (1, -1):
    e = bpy.data.objects["Eye_L" if sg > 0 else "Eye_R"]
    EL = HW.inverted() @ e.matrix_world
    cpos = EL.translation; R3 = EL.to_3x3().normalized(); look = R3 @ Vector((1, 0, 0))
    b1 = R3 @ Vector((0, 1, 0)); b2 = R3 @ Vector((0, 0, 1)); Rr = 0.098 * S
    ring = [cpos + look * 0.04 * S + (b1 * math.cos(a) + b2 * math.sin(a) * 0.9) * Rr * 1.03 for a in [2 * math.pi * k / 32 for k in range(32)]]
    rad = [(0.016 + 0.02 * max(0, -math.sin(2 * math.pi * k / 32)) ** 1.5) * S for k in range(32)]
    rings = []
    for i, p in enumerate(ring):
        tan = (ring[(i + 1) % 32] - ring[(i - 1) % 32]).normalized(); outw = (p - cpos - look * 0.04 * S).normalized()
        ax2 = tan.cross(outw).normalized()
        rings.append([bmL.verts.new(p + (outw * math.cos(t) + ax2 * math.sin(t)) * rad[i]) for t in [2 * math.pi * j / 8 for j in range(8)]])
    for i in range(32):
        A, B = rings[i], rings[(i + 1) % 32]
        for j in range(8):
            f = bmL.faces.new((A[j], A[(j + 1) % 8], B[(j + 1) % 8], B[j])); f.smooth = True
oldl = lids.data; lm = bpy.data.meshes.new("Apophis_Eyelids_v4"); bmL.to_mesh(lm); bmL.free()
mats = list(oldl.materials); lids.data = lm; bpy.data.meshes.remove(oldl); lm.name = "Apophis_Eyelids"
for m in mats: lm.materials.append(m)
lids.matrix_world = HW
# luz de garganta
ml = bpy.data.objects.get("LGT_Accent_Mouth")
if ml: ml.location = Vector((0.40, 0, -0.10)) * S
vl.update()
for o in (head, jawo, lids):
    a = o.data.attributes.get("rest_pos") or o.data.attributes.new("rest_pos", 'FLOAT_VECTOR', 'POINT')
    M = o.matrix_world; a.data.foreach_set("vector", [c for v in o.data.vertices for c in (M @ v.co)])
    o.update_tag()
print("cabeza v4: %d caras cráneo+anexos, %d caras mandíbula" % (len(head.data.polygons), len(jawo.data.polygons)))
