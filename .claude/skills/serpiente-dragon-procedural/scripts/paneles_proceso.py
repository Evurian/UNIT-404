"""Utilidades para los paneles de la lámina. Ejecutar dentro de Blender (MCP):

    exec(open(r"<skill>/scripts/paneles_proceso.py", encoding="utf-8").read())
    renders_cycles(r"<carpeta_proyecto>")   # render_final + 03 (vertical) + 04 (pose)
    preparar_topologia()   # luego: look(mode="camera", shading="wireframe", max_size=1920)
    preparar_rig()         # luego: look(mode="camera", shading="solid",     max_size=1920)
    restaurar()            # SIEMPRE al terminar, antes de guardar

`look` guarda la imagen en la carpeta tool-results de la sesión; copiarla a
laminas/01_topologia.png y laminas/02_rig.png.
"""
import bpy, bmesh, math, os, time
from mathutils import Vector

OCULTAR_EN_PROCESO = ("SM_Ground_Basalt", "FX_Embers_Domain")


def _view3d_shading():
    for a in bpy.context.window_manager.windows[0].screen.areas:
        if a.type == 'VIEW_3D':
            return a.spaces[0].shading


def encuadrar_cam_cabeza(dist=11.0, direccion=(0.82, -0.55, 0.12), objetivo_local=(0.1, 0, 0.1)):
    """Cam_Head en tres cuartos hacia la cabeza (como la referencia)."""
    sc = bpy.context.scene; sc.frame_set(1)
    h = bpy.data.objects["Apophis_Head"]
    tgt = h.matrix_world @ Vector(objetivo_local)
    cam = bpy.data.objects["Cam_Head"]
    d = Vector(direccion).normalized()
    cam.location = tgt + d * dist
    cam.rotation_euler = (tgt - cam.location).to_track_quat('-Z', 'Y').to_euler()


def renders_cycles(proyecto, frame_pose=68):
    sc = bpy.context.scene; r = sc.render
    r.engine = 'CYCLES'; sc.cycles.device = 'GPU'

    def rend(cam, w, h, frame, path):
        sc.camera = bpy.data.objects[cam]; r.resolution_x = w; r.resolution_y = h
        sc.frame_set(frame); r.filepath = path; t = time.time()
        bpy.ops.render.render(write_still=True)
        print(path, round(time.time() - t, 1), "s")
    rend("Apophis_Cam", 1920, 1080, 1, os.path.join(proyecto, "render_final.png"))
    # el detalle de cabeza va en VERTICAL para que el panel 4 no lo recorte
    rend("Cam_Head", 1000, 1150, 1, os.path.join(proyecto, "laminas", "03_cabeza_detalle.png"))
    rend("Apophis_Cam", 1920, 1080, frame_pose, os.path.join(proyecto, "laminas", "04_pose_rig.png"))
    r.engine = 'BLENDER_EEVEE'; sc.frame_set(1)
    sc.camera = bpy.data.objects["Apophis_Cam"]; r.resolution_x = 1920; r.resolution_y = 1080


def preparar_topologia():
    """Sin suelo, sin brasas y sin el modificador de escamas: se ve la malla base."""
    sc = bpy.context.scene; sc.frame_set(1); sc.camera = bpy.data.objects["Apophis_Cam"]
    sc.render.resolution_x = 1920; sc.render.resolution_y = 1080
    for n in OCULTAR_EN_PROCESO:
        bpy.data.objects[n].hide_viewport = True
    bpy.data.objects["Apophis_Body"].modifiers["GN_Scales"].show_viewport = False


def preparar_rig():
    """Escamas visibles + huesos dibujados como octaedros de color por delante
    (amarillo columna, rojo cabeza/mandíbula). `look` no muestra bien los huesos
    reales del armature, por eso se crea una malla temporal TMP_BoneViz."""
    bpy.data.objects["Apophis_Body"].modifiers["GN_Scales"].show_viewport = True
    for n in OCULTAR_EN_PROCESO:
        bpy.data.objects[n].hide_viewport = True
    rig = bpy.data.objects["Apophis_Rig"]; bpy.context.view_layer.update()
    bm = bmesh.new(); RW = rig.matrix_world

    def octa(h, t, r, mi):
        ax = t - h; L = ax.length; d = ax.normalized()
        up = Vector((0, 0, 1)) if abs(d.z) < 0.9 else Vector((1, 0, 0))
        a = d.cross(up).normalized(); b = d.cross(a); m = h + ax * 0.15; w = L * r
        ring = [bm.verts.new(m + (a * math.cos(k * math.pi / 2) + b * math.sin(k * math.pi / 2)) * w) for k in range(4)]
        vh = bm.verts.new(h); vt = bm.verts.new(t)
        for k in range(4):
            for f in (bm.faces.new((vh, ring[(k + 1) % 4], ring[k])), bm.faces.new((vt, ring[k], ring[(k + 1) % 4]))):
                f.material_index = mi
    for pb in rig.pose.bones:
        mi = 0 if pb.name.startswith("Spine_") else 1
        octa(RW @ pb.head, RW @ pb.tail, 0.28 if mi == 0 else 0.14, mi)
    me = bpy.data.meshes.new("TMP_BoneViz"); bm.to_mesh(me); bm.free()
    for nm, col in (("TMP_BoneYellow", (0.95, 0.72, 0.05, 1)), ("TMP_BoneRed", (0.6, 0.03, 0.02, 1))):
        m = bpy.data.materials.new(nm); m.diffuse_color = col; me.materials.append(m)
    o = bpy.data.objects.new("TMP_BoneViz", me); bpy.context.scene.collection.objects.link(o)
    o.show_in_front = True
    _view3d_shading().color_type = 'MATERIAL'   # si no, el visor pinta todo gris


def restaurar():
    o = bpy.data.objects.get("TMP_BoneViz")
    if o:
        me = o.data; bpy.data.objects.remove(o); bpy.data.meshes.remove(me)
    for nm in ("TMP_BoneYellow", "TMP_BoneRed"):
        m = bpy.data.materials.get(nm)
        if m: bpy.data.materials.remove(m)
    _view3d_shading().color_type = 'SINGLE'
    for n in OCULTAR_EN_PROCESO:
        bpy.data.objects[n].hide_viewport = False
    bpy.data.objects["Apophis_Body"].modifiers["GN_Scales"].show_viewport = True
    sc = bpy.context.scene; sc.frame_set(1); sc.camera = bpy.data.objects["Apophis_Cam"]


def que_hay_en_pixel(cam_name, px, py, W, H):
    """Diagnóstico: qué objeto/material se ve en un píxel del render
    (así se encontró que una 'mancha blanca' era el talón de la mandíbula)."""
    sc = bpy.context.scene; dg = bpy.context.evaluated_depsgraph_get()
    cam = bpy.data.objects[cam_name]; cd = cam.data; M = cam.matrix_world
    if H > W: sh = cd.sensor_width; sw = sh * W / H
    else: sw = cd.sensor_width; sh = sw * H / W
    x = (px / W - 0.5) * sw; y = (0.5 - py / H) * sh
    d = (M.to_3x3() @ Vector((x, y, -cd.lens))).normalized()
    hit, loc, nrm, idx, ob, mx = sc.ray_cast(dg, M.translation, d)
    if not hit:
        return None
    ev = ob.evaluated_get(dg); mat = None
    if ob.type == 'MESH' and ev.data.materials:
        mat = ev.data.materials[ev.data.polygons[idx].material_index].name
    return ob.name, mat, tuple(round(c, 2) for c in loc)
