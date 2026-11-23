---
name: serpiente-dragon-procedural
description: Flujo completo, validado en el proyecto Apophis, para crear de forma procedural (Python/bpy vía Blender MCP) criaturas serpiente-dragón o wyrm. Incluye cuerpo largo enroscado con grosor que se afina, cabeza de dragón con cuernos/corona/colmillos, escamas con Geometry Nodes, rig Spline IK animado, materiales de lava/roca, renders en Cycles, lámina de proceso de 5 paneles y video en bucle. Úsala siempre que el usuario pida modelar, rediseñar o continuar una serpiente, dragón serpentino, naga, basilisco, wyrm, Apophis, Jörmungandr, Quetzalcóatl o criatura similar en Blender, o pida "otro diseño como Apophis", la lámina de proceso o el video de una criatura así, aunque no mencione la skill.
license: MIT
metadata:
  author: proyecto Apophis (UNSA)
  version: "1.0.0"
  domain: blender
  role: specialist
  triggers: serpiente, dragón, wyrm, naga, basilisco, Apophis, criatura serpentina, lámina de proceso, cuerpo enroscado, escamas, lava
  related-skills: creature-artist, rigging, geometry-nodes, materials, rendering, animation
---

# Serpiente-dragón procedural

Esta skill reutiliza lo que funcionó en Apophis: una escena plantilla con el rig, los materiales, las luces,
las cámaras y las escamas ya hechas, más scripts paramétricos que regeneran el cuerpo y la cabeza. Un diseño
nuevo consiste en **copiar la plantilla, cambiar parámetros, reejecutar y verificar mirando**, no en modelar
a mano. Esto importa porque el usuario lo entrega en un curso donde debe **mostrar el proceso** (topología,
rig, texturizado): todo debe ser código reproducible, nunca un modelo generado por IA (`generate_3d`).

## Recursos

| Archivo | Para qué |
|---|---|
| `assets/plantilla_serpiente_dragon.blend` | escena base: rig `Apophis_Rig` (huesos Head/Jaw), `CTRL_Master`, materiales, GN de escamas, luces, suelo, brasas, cámaras `Apophis_Cam`/`Cam_Head` |
| `scripts/cuerpo_serpiente.py` | curva `PTS` → cuerpo, espinas, 48 huesos Spline IK, controles, pesos, atributos de escamas y animación de 96 fotogramas |
| `scripts/cabeza_dragon.py` | cráneo por loft, mandíbula, cuernos, corona, gola, dientes, ojos y párpados sobre el hueso Head |
| `scripts/paneles_proceso.py` | renders Cycles de la lámina, preparación de vistas de topología/rig, diagnóstico por píxel |
| `scripts/componer_lamina.py` | compone la lámina de 5 paneles (Pillow, fuera de Blender) |
| `scripts/render_animacion.py` | fotogramas del video con Blender en segundo plano (`-b`) |
| `references/parametros.md` | **leer antes de tocar la forma**: qué hace cada parámetro y qué valores aprobó el usuario |

Los objetos de la plantilla se llaman `Apophis_*`. Está bien conservarlos así: los scripts los buscan por
nombre. El nombre de la criatura nueva va en el archivo `.blend`, las carpetas y la lámina.

## Flujo de trabajo

### 0. Conexión
`get_addon_status` y luego `get_scene_info`. Si el MCP no conecta, pide al usuario que abra Blender y pulse
**Connect** en la pestaña BlenderMCP. Si Blender abre con la escena por defecto, carga el `.blend` del proyecto.
Antes de instalar cualquier cosa externa (addons, paquetes de pip), di qué se va a instalar y espera el sí.

### 1. Crear el proyecto (diseño nuevo)
```
<Proyecto>/<Nombre>/
  <Nombre>.blend        ← copia de assets/plantilla_serpiente_dragon.blend
  scripts/              ← copia de cuerpo_serpiente.py y cabeza_dragon.py (se editan ahí)
  laminas/   animacion/frames/
```
Abre el `.blend` con `bpy.ops.wm.open_mainfile`. Si el usuario da una imagen de referencia, descríbele lo que
vas a imitar (silueta del cuerpo, forma del cráneo, apertura de boca, cómo entra el cuello) antes de cambiar nada.

### 2. Respaldo antes de cada cambio grande
`bpy.ops.wm.save_as_mainfile(filepath=".../<Nombre>_vN_respaldo.blend", copy=True)` y `cp` del script que vas
a editar a `*_vN_respaldo.py`. El usuario itera mucho y a veces quiere volver atrás.

### 3. Cuerpo → cabeza (en ese orden)
Edita parámetros en los scripts del proyecto (ver `references/parametros.md`) y ejecútalos con
`exec(open(r"<skill>/scripts/cuerpo_serpiente.py", encoding="utf-8").read())`, después `cabeza_dragon.py`.
La cabeza depende del hueso Head que coloca el cuerpo; al revés queda desalineada.

Principios de diseño que el usuario pidió explícitamente:
- **Cabeza de dragón, no de cocodrilo**: cráneo corto y alto, hocico romo y alto, ceja pesada que hunde los
  ojos, cuernos hacia atrás y boca abierta unos 20-25°.
- **Cuello que nace del grosor de la cabeza** y un cuerpo que se afina continuamente hasta la cola, como una
  serpiente real.
- **El cuello entra por la nuca**: sale recto hacia atrás y luego se arquea hacia abajo. Nunca vertical bajo
  la cabeza ("como una persona") ni con un arco tan cerrado que roce el cráneo.
- Cuerpo largo; el anillo en el suelo y la cola por fuera del anillo.

### 4. Verificar mirando (bucle)
Después de cada ejecución usa `look`: `angles` con vistas lateral, tres cuartos y frontal sobre cabeza y
cuerpo; de cerca la unión cabeza-cuello en `material`; `frames` [13, 37, 61, 85] para ver que la animación no
cruza geometría. Compara con la referencia y repite el paso 3. `get_scene_info` no sirve para juzgar la forma.

Si aparece un artefacto en un render (manchas, brillos raros), usa `que_hay_en_pixel()` de `paneles_proceso.py`
para saber qué objeto es, y ocúltalo temporalmente para confirmarlo, en vez de adivinar.

### 5. Cámaras, renders y lámina
```python
exec(open(r"<skill>/scripts/paneles_proceso.py", encoding="utf-8").read())
encuadrar_cam_cabeza()      # tres cuartos; mirar con look(mode="camera", shading="rendered")
renders_cycles(r"<Proyecto>/<Nombre>")
```
- La cámara general debe ver el cuello **de lado** (tres cuartos), si no el arco no se lee. Si giras la cámara,
  gira `LGT_Rim_Back` lo mismo alrededor del modelo, y deja aire sobre la corona (`lens`/`shift_y`).
- El panel 4 se renderiza **vertical (1000×1150)**; un render apaisado recortado cortaba la cabeza.
- Para los paneles 1 y 2: `preparar_topologia()` + `look(mode="camera", shading="wireframe", max_size=1920)`,
  `preparar_rig()` + `look(mode="camera", shading="solid", max_size=1920)`, copiando cada imagen de
  tool-results a `laminas/01_topologia.png` / `02_rig.png`; después **`restaurar()`** y guarda el `.blend`.
- Actualiza los textos de `PANELES` en `componer_lamina.py` con los datos reales (largo del cuerpo, apertura
  de boca…) y ejecuta `python componer_lamina.py <Proyecto>/<Nombre> <NOMBRE>`. Revisa la lámina con Read.

### 6. Video
Guarda la escena y lanza en segundo plano (Bash `run_in_background`) Blender sin interfaz con
`render_animacion.py`, luego monta el MP4 con ffmpeg (instrucciones en el script). Comprueba que los
fotogramas tengan la hora actual y mira uno antes de dar el video por hecho. Envíalo con `SendUserFile`.

### 7. Cierre
Deja la escena en EEVEE, fotograma 1, cámara `Apophis_Cam`, y guardada. Actualiza la memoria del proyecto
con archivos, parámetros finales y pendientes.

## Comunicación con el usuario
- Responde en español, con lo que cambió en términos visibles (grosor, ángulo, posición).
- Muestra el resultado con imágenes de `look` o del render.
- Di qué falta o qué no verificaste, por ejemplo si el video no se re-renderizó.
- Tras cada cambio visible, actualiza el render y la lámina, porque son lo que entrega.
