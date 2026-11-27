# Integración del trabajo del Integrante 4

Qué necesita el equipo para juntar la rama `integrante-4` con las demás: escenas, orden del flujo, dependencias y lo que queda por conectar.

## 1. Escenas en Build Settings (en este orden)

El orden sale de `SceneFlow.BuildOrder` (`Assets/Integrante4/Scripts/Core/SceneFlow.cs`). El menú **UNIT-404 → Integrante 4 → Conectar escenas (Build Settings)** las agrega solo; las escenas de otros integrantes que ya estén en la lista se conservan, pero desactivadas.

| # | Escena | Qué es |
|---|--------|--------|
| 0 | `Inicio` | Menú principal (abre el juego) |
| 1 | `Carga` | Pantalla de carga entre escenas |
| 2 | `Introduccion` | Historia inicial |
| 3 | `Transicion1_Mesopotamia` | Cinemática de la cultura |
| 4 | `Nivel1_Mesopotamia` | Nivel 1 |
| 5 | `Transicion2_Egipto` | Cinemática |
| 6 | `Nivel2_Egipto` | Nivel 2 |
| 7 | `Transicion3_Grecia` | Cinemática |
| 8 | `Nivel3_Grecia` | Nivel 3 (semiabierto 3D) |
| 9 | `Transicion4_China` | Cinemática |
| 10 | `Nivel4_China` | Nivel 4 (semiabierto 3D) |
| 11 | `Despedida` | Epílogo |
| 12 | `Creditos` | Créditos |
| 13 | `Cierre` | Pantalla final (vuelve a `Inicio`) |

Todas están en `Assets/Integrante4/Scenes/`.

**Escenas nuevas que no figuran en `docs/ACUERDOS.md` §4:** `Introduccion`, `Transicion1-4_<Cultura>`, `Despedida` y `Cierre`. Propuesta: agregarlas a §4 cuando se apruebe este aviso.

## 2. Dependencias que tienen que estar en el proyecto

- **TextMesh Pro Essentials** importado en `Assets/TextMesh Pro/` (ya está en la rama). Sin esto, las pantallas y los menús se ven sin texto.
- **Fuentes** en `Assets/Integrante4/Art/Fonts/`: Metamorphous (títulos, SIL OFL) y Kenney Future (texto, CC0), con sus licencias. Si faltan los `.asset` SDF, usar **UNIT-404 → Integrante 4 → Crear fuentes SDF**.
- Unity **6000.3.23f1** con URP (ver `docs/ACUERDOS.md` §1).

## 3. Contrato con los otros integrantes

- **Integrante 1 (jugador):** cada nivel tiene `PlayerStart` y `Main Camera`. Hoy hay un cubo `JugadorTemporal` (tag `Player`) para poder probar; al integrar se reemplaza con `InsertarJugador`. Las vidas, el combustible y el puntaje tras reaparecer son del Integrante 1; la pantalla "Sistema caído" solo mueve al jugador al último punto de control.
- **Integrante 2 (jefes):** cada nivel tiene un marcador con tag `Jefe` en la arena (`Tiamat` en el nivel 1, etc.) y una puerta de arena; el reinicio del jefe tras una caída queda a cargo del Integrante 2.
- **Integrante 3 (niveles 5-8):** por ahora el nivel 4 pasa directo a `Despedida`. Cuando lleguen los niveles 5-8, hay que insertarlos en `SceneFlow.Story` (con sus transiciones) y volver a correr "Conectar escenas". Para construirlos igual que los niveles 1-4, ver la skill `.claude/skills/unit404-generar-niveles/`.
- Eventos usados: `GameEvents` (`docs/ACUERDOS.md` §5). El HUD escucha vidas, puntaje y combustible; el narrador escucha `OnLevelCompleted`.

## 4. Solo para pruebas (quitar o ignorar al integrar)

- `HudEventSimulator (solo pruebas)` en cada nivel: teclas de prueba para el HUD; la tecla **L** dispara `OnLevelCompleted` para recorrer todo el flujo sin jugar.
- `JugadorTemporal`: lo reemplaza el jugador real.

## 5. Cómo regenerar las escenas

Las escenas del Integrante 4 se generan por código; no conviene editarlas a mano porque se pisan al regenerar.

1. **UNIT-404 → Integrante 4 → Crear niveles 1 y 2 (bloques grises)** (`LevelSceneBuilder.BuildAll`): niveles 1-4, cinemáticas y pantallas del flujo. También funciona en batchmode con `-nographics`.
2. **UNIT-404 → Integrante 4 → Renderizar fondos de pantallas** (`ScreenArtBuilder.RenderAndRebuildScreens`): los fondos del menú y de la carga. Necesita GPU (no usar `-nographics`).
3. Tests: Test Runner → EditMode → `Unit404.Integrante4.Tests.EditMode`.

## 6. No subir

- `ProjectSettings/SceneTemplateSettings.json` (lo crea el editor y es local).
- Los cambios de los Volumes (`Assets/Integrante4/Art/Volumes/*.asset`) que aparecen al abrir el editor sin haberlos tocado.
