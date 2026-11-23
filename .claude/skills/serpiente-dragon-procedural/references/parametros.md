# Parámetros de los scripts (valores validados con el usuario en Apophis)

Leer este archivo cuando haya que cambiar la forma del cuerpo, del cuello o de la cabeza.
Todos los valores están en metros (unidades de Blender) salvo que se indique.

## cuerpo_serpiente.py

### `PTS` — curva guía del cuerpo `((x, y, z), radio)`, del cuello a la cola
Tres tramos: **cuello/tramo erguido en S** (8 puntos), **anillo en el suelo** (`_g(x, y, r)` apoya el punto
en el suelo a `z = r + 0.07`) y **cola** que se levanta (6 puntos, por FUERA del anillo).

Perfil de radios que el usuario aprobó ("que inicie con el grosor de la cabeza y se vaya adelgazando"):

| Tramo | Radio |
|---|---|
| P0 (cuello, dentro de la nuca) | 0.70 ≈ medio ancho de la cabeza |
| tramo en S | 0.70 → 0.58, sin subir nunca |
| anillo del suelo | 0.56 → 0.20 |
| cola | 0.17 → 0.01 |

Reglas aprendidas:
- **Monótono decreciente** desde el cuello. Un cuello más fino que la cabeza o un cuerpo casi uniforme
  (p. ej. 0.25 → 0.34 en 31 m) se lee "flaco" y "uniforme".
- **El cuello sale recto hacia atrás desde la nuca** ≈1.6 m (P1 ≈ P0 − HEAD_FWD·1.6 + 0.5 en z) y luego
  baja en un arco amplio (P2 ≈ P0 − HEAD_FWD·2.6 − 0.25 z). Un arco cerrado (P0→P1 ≈ 1 m) deja el cuello
  pegado al costado del cráneo y se cruza con la corona/gola.
- Nunca un cuello vertical que termina bajo el centro del cráneo: se ve "como una persona".
- Comprobar holguras: distancia entre ejes de tramos vecinos > suma de radios (+0.1). La cola va por fuera
  del anillo para no atravesarlo.
- Cambiar el número de puntos cambia el número de controles `CTRL_xx` (uno por punto); está bien.

### Otros
- `HEAD_FWD` — eje del cráneo en reposo (hacia delante y algo abajo, ej. `(-0.88, -0.40, -0.22)`).
  El script orienta el hueso `Head` con él; la cabeza se construye sobre ese hueso.
- `NB = 48` huesos Spline IK, `F = 96` fotogramas del ciclo.
- Espinas dorsales: `dist = 1.10` inicial (no empezar dentro del cráneo), tamaño ∝ radio.

## cabeza_dragon.py

| Parámetro | Valor | Efecto |
|---|---|---|
| `S` | 1.50 | escala global; la cabeza debe tener ≈ el ancho del cuello |
| `OPEN` | 22° | apertura de boca. 38° se veía "muy separada" |
| `X0, X1` | -0.60, 1.02 | largo del cráneo. 1.35 (largo y plano) = "cocodrilo" |
| `W_K / HT_K / HB_K` | ver script | ancho / alto superior / labio inferior del cráneo por tramo `u∈[0,1]`; hocico alto y romo (`HB` ≈ -0.15) |
| `EYE_X, EYE_TH` | 0.16, 28° | ojo bajo la ceja |
| ceja `0.21*brow`, `bt` 46° | — | ceja pesada que hunde el ojo (mirada fiera) |
| `NECK_OFF` | (0.10, 0, 0.05)·S | dónde queda el extremo del cuello dentro de la nuca (algo bajo = garganta) |
| `J0, J1` | -0.12, 1.30 | mandíbula. `J0` muy negativo hace que el talón asome por la garganta y brille |
| `WJ / BJ` | estrechos al inicio | talón de la mandíbula fino para que quede oculto |

Cuernos, corona (3 filas), gola de mejilla, cresta nasal, dientes y "barba" están en bucles al final del
cráneo/mandíbula; sus posiciones X dependen de `X1`/`J1`, así que si se acorta el cráneo hay que moverlas
(colmillo superior ≈ `X1 − 0.16`, colmillo inferior ≈ `J1 − 0.20`).

La cabeza toma su marco del hueso (`HW = rig @ pose_bone.matrix @ R_OBJ @ T(NECK_OFF)`), así que
reejecutar el script NO desplaza la cabeza. Ejecutar siempre **cuerpo antes que cabeza**.

## Materiales y escamas (en la plantilla)
- Grupo GN `GN_Scales_Body_v01`, entrada **Height** (`Socket_5`) = 0.05 → relieve de placas visible.
  0.02 quedaba plano. Se cambia con `md.properties.inputs.Socket_5.value` (Blender 5.x).
- Materiales `Apophis_Body_Scales`, `Apophis_Head_Scales`, `Apophis_Mouth`, `Apophis_Teeth`,
  `Apophis_Horn`, `Apophis_Eye`. Boca y ojos tienen un nodo `glow_boost` (multiplica la emisión).
- Propiedad de objeto `crack_scale` = tamaño de las grietas de lava.
- `LGT_Accent_Mouth` 140 W naranja dentro de la boca; `LGT_Rim_Back` contraluz (girarla con la cámara).
