"""Compone la lámina de proceso (5 paneles) de una criatura.

Uso (Python normal, fuera de Blender; requiere Pillow):
    python componer_lamina.py <carpeta_proyecto> <NOMBRE>

Espera en <carpeta_proyecto>:
    render_final.png                      (1920x1080, Cycles)
    laminas/01_topologia.png              (1920x1080, wireframe sin GN ni suelo)
    laminas/02_rig.png                    (1920x1080, sólido + huesos de color)
    laminas/03_cabeza_detalle.png         (VERTICAL 1000x1150 -> no se recorta)
    laminas/04_pose_rig.png               (1920x1080, fotograma de pose)
Escribe laminas/lamina_<nombre>.png (3000 px de ancho).
Edita los textos de PANELES para cada diseño.
"""
import sys, os
from PIL import Image, ImageDraw, ImageFont

base = sys.argv[1]
NOMBRE = sys.argv[2] if len(sys.argv) > 2 else "CRIATURA"
SUBTITULO = "Serpiente-dragón · modelado procedural en Blender con Python (bpy)"
PANELES = {   # (etiqueta, pie de foto) — actualizar con los datos reales del diseño
    1: ("TOPOLOGÍA BASE", "Malla de quads; escamas añadidas con Geometry Nodes"),
    2: ("ESCAMAS + RIGGING", "Cuerpo afinado desde el grosor de la cabeza · Spline IK"),
    3: ("POSE DE ANIMACIÓN", "Fotograma del ciclo con el rig"),
    4: ("DETALLE DE CABEZA", "Cráneo corto de dragón · ceja pesada · colmillos"),
    5: ("RENDER FINAL", "Cycles · lava entre escamas · brasas GN · niebla volumétrica"),
}
PIE = "Blender · Python / bpy · Geometry Nodes · Cycles · Spline IK"

lam = os.path.join(base, "laminas")
def _font(names, size):
    # Windows fonts first, then common Linux fonts, then PIL default
    dirs = ["C:/Windows/Fonts/", "/usr/share/fonts/truetype/dejavu/", "/usr/share/fonts/truetype/liberation/"]
    for d in dirs:
        for n in names:
            if os.path.exists(d + n):
                return ImageFont.truetype(d + n, size)
    return ImageFont.load_default()
title_f = _font(["georgiab.ttf", "DejaVuSerif-Bold.ttf"], 92)
sub_f = _font(["segoeui.ttf", "DejaVuSans.ttf"], 38)
lab_f = _font(["segoeuib.ttf", "DejaVuSans-Bold.ttf"], 40)
cap_f = _font(["segoeui.ttf", "DejaVuSans.ttf"], 30)

W, M = 3000, 40
colw = (W - 4 * M) // 3
BG = (24, 24, 27)
ACC = (255, 120, 30)

def crop_fit(img, w, h, cx=0.5, cy=0.5):
    iw, ih = img.size
    s = max(w / iw, h / ih)
    img = img.resize((round(iw * s), round(ih * s)), Image.LANCZOS)
    iw, ih = img.size
    x = int((iw - w) * cx); y = int((ih - h) * cy)
    return img.crop((x, y, x + w, y + h))

def load(n):
    folder = base if n == "render_final.png" else lam
    return Image.open(os.path.join(folder, n)).convert("RGB")

TITLE_H = 230
r1h = 1000
r2h = 1087
CAP = 120
H = TITLE_H + r1h + CAP + r2h + CAP + 70
canvas = Image.new("RGB", (W, H), BG)
d = ImageDraw.Draw(canvas)

d.text((M, 40), f"GUÍA DE MODELADO 3D: {NOMBRE.upper()}", font=title_f, fill=(240, 236, 230))
d.text((M + 4, 150), SUBTITULO, font=sub_f, fill=(170, 170, 175))
d.rectangle((M, TITLE_H - 18, W - M, TITLE_H - 14), fill=ACC)

def panel(img, x, y, w, h, num):
    label, cap = PANELES[num]
    canvas.paste(img, (x, y))
    d.rectangle((x, y, x + w - 1, y + h - 1), outline=(70, 70, 75), width=2)
    d.rounded_rectangle((x + 20, y + 20, x + 84, y + 84), radius=14, fill=ACC)
    tw = d.textlength(str(num), font=lab_f)
    d.text((x + 52 - tw / 2, y + 26), str(num), font=lab_f, fill=(20, 20, 20))
    d.text((x, y + h + 18), label, font=lab_f, fill=(240, 236, 230))
    d.text((x, y + h + 68), cap, font=cap_f, fill=(160, 160, 165))

y1 = TITLE_H
panel(crop_fit(load("01_topologia.png"), colw, r1h), M, y1, colw, r1h, 1)
panel(crop_fit(load("02_rig.png"), colw, r1h), M * 2 + colw, y1, colw, r1h, 2)
panel(crop_fit(load("04_pose_rig.png"), colw, r1h, cx=0.48), M * 3 + colw * 2, y1, colw, r1h, 3)
y2 = y1 + r1h + CAP
wide = colw * 2 + M
# el detalle de cabeza se renderiza ya en vertical: crop centrado, nada se corta
panel(crop_fit(load("03_cabeza_detalle.png"), colw, r2h), M, y2, colw, r2h, 4)
panel(crop_fit(load("render_final.png"), wide, r2h), M * 2 + colw, y2, wide, r2h, 5)

d.text((M, H - 55), PIE, font=cap_f, fill=(110, 110, 115))
out = os.path.join(lam, f"lamina_{NOMBRE.lower()}.png")
canvas.save(out)
print(out, canvas.size)
