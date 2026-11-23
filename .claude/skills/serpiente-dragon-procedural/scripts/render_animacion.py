"""Renderiza los fotogramas del ciclo de animación en Blender SIN interfaz.

Uso (no bloquea el Blender abierto con el MCP; guardar la escena antes):
    blender.exe -b <proyecto>/<Nombre>.blend -P render_animacion.py -- <carpeta_frames>

Después, montar el MP4 (el ciclo de 96 fotogramas repetido 2 veces = 8 s):
    <ffmpeg> -y -stream_loop 1 -framerate 24 -i <carpeta_frames>/f_%04d.png \
             -c:v libx264 -pix_fmt yuv420p -crf 18 <salida>.mp4
Si no hay ffmpeg en el PATH, suele estar el de imageio_ffmpeg:
    python -c "import imageio_ffmpeg; print(imageio_ffmpeg.get_ffmpeg_exe())"
"""
import bpy, sys, os

argv = sys.argv[sys.argv.index("--") + 1:] if "--" in sys.argv else []
frames_dir = argv[0] if argv else os.path.join(os.path.dirname(bpy.data.filepath), "animacion", "frames")
os.makedirs(frames_dir, exist_ok=True)

sc = bpy.context.scene; r = sc.render
try:
    r.engine = 'BLENDER_EEVEE'
except TypeError as e:          # el mensaje lista los motores válidos
    print(e)
sc.camera = bpy.data.objects["Apophis_Cam"]
r.resolution_x, r.resolution_y, r.resolution_percentage = 1280, 720, 100
sc.frame_start, sc.frame_end = 1, 96
r.image_settings.file_format = 'PNG'
r.filepath = os.path.join(frames_dir, "f_")
bpy.ops.render.render(animation=True)
