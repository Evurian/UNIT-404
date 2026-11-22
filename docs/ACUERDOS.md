# Acuerdos del equipo UNIT-404

## 1. Versión de Unity
- Versión acordada: **Unity 6000.3.23f1**. **Todos usan exactamente la misma versión.**
- Unity 6 usa `linearDamping` / `angularDamping`.
- Plantilla: URP (el proyecto se creó con URP; ya no es 3D Built-in).
- Project Settings: Version Control = Visible Meta Files, Asset Serialization = Force Text, Active Input Handling = Both.

## 2. Tags
`Player`, `Fragmento`, `Propulsor`, `Obstaculo`, `Contaminacion`, `Observador`, `Enemigo`, `Jefe`, `Chip`, `Glifo`, `Checkpoint`, `Plataforma`

## 3. Capas (Layers)
`Player`, `Enemigo`, `Jefe`, `Proyectil`, `Entorno`, `Coleccionable`

## 4. Escenas
`Inicio`, `Carga`, `Nivel1_Mesopotamia`, `Nivel2_Egipto`, `Nivel3_Grecia`, `Nivel4_China`, `Nivel5_Mayas`, `Nivel6_Aztecas`, `Nivel7_Incas`, `Nivel8_Archivo`, `Creditos`

Escenas de prueba personales: `Test_Integrante1` a `Test_Integrante4`.

## 5. Eventos comunes (`GameEvents`)
`OnLivesChanged(int)`, `OnScoreChanged(int)`, `OnFuelChanged(float)`, `OnPlayerHit`, `OnPlayerDied`, `OnBossDefeated`, `OnLevelCompleted`

Los eventos son estáticos y sobreviven al cambio de escena: todo suscriptor debe desuscribirse en `OnDisable`.

## 6. Reglas de oro
- Nadie toca `ProjectSettings/TagManager.asset` salvo el Integrante 1.
- Cada quien trabaja solo en `Assets/IntegranteN` y en sus escenas.
- Cada quien trabaja en su rama `integrante-N`; nunca push directo a `main`.

## 7. Añadidos del Integrante 1 (no están en el GDD)
- Invulnerabilidad de 0.5 s tras un golpe (evita varias vidas perdidas por frames seguidos).
