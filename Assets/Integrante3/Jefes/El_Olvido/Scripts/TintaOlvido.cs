using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// El Olvido: convierte un modelo (guardián, masa de tinta o forma real) en tinta viva
/// y anima la disolución para derretirlo o volver a formarlo.
/// Ponlo en la raíz del modelo y asigna M_Olvido_Tinta.
///
/// Uso desde la máquina de estados del jefe:
///   tinta.Derretir(1f, () => { /* cambiar de forma */ });
///   tinta.Formar(1f);
/// </summary>
public class TintaOlvido : MonoBehaviour
{
    [Tooltip("Material con el shader UNIT404/Olvido/Tinta (M_Olvido_Tinta).")]
    public Material materialTinta;

    [Tooltip("Al iniciar, cambia todos los materiales del modelo por la tinta. " +
             "Así sirven los guardianes de otros integrantes sin editar sus prefabs.")]
    public bool aplicarAlIniciar = true;

    [Tooltip("Empieza derretido (invisible) para luego llamar a Formar().")]
    public bool empezarDerretido = false;

    [Tooltip("Altura aproximada del modelo en metros: sirve para derretirlo de arriba abajo.")]
    public float altura = 3f;

    [Tooltip("Multiplica el tamaño de las letras del material (la masa, que es ancha, usa ~1.4).")]
    [Range(0.5f, 3f)] public float escalaLetras = 1f;

    static readonly int IdDisolver = Shader.PropertyToID("_Dissolve");
    static readonly int IdAltura = Shader.PropertyToID("_MeltHeight");
    static readonly int IdBase = Shader.PropertyToID("_MeltBase");
    static readonly int IdLetra = Shader.PropertyToID("_GlyphSize");

    Renderer[] renderers;
    MaterialPropertyBlock bloque;
    Coroutine animacion;

    /// <summary>0 = modelo completo, 1 = derretido del todo.</summary>
    public float Disolucion { get; private set; }
    public bool Animando => animacion != null;

    void Awake()
    {
        renderers = GetComponentsInChildren<Renderer>(true);
        bloque = new MaterialPropertyBlock();
        if (aplicarAlIniciar) AplicarTinta();
        Fijar(empezarDerretido ? 1f : 0f);
    }

    /// <summary>Pone la tinta en todos los renderers del modelo (menos partículas y estelas).</summary>
    public void AplicarTinta()
    {
        if (materialTinta == null) { Debug.LogWarning($"{name}: falta materialTinta", this); return; }
        foreach (var r in renderers)
        {
            if (r is ParticleSystemRenderer || r is TrailRenderer || r is LineRenderer) continue;
            if (Excluido(r)) continue;   // núcleo, ojo y páginas conservan su material
            var mats = new Material[r.sharedMaterials.Length];
            for (int i = 0; i < mats.Length; i++) mats[i] = materialTinta;
            r.sharedMaterials = mats;
        }
    }

    /// <summary>Partes que no son tinta: el núcleo de luz, el ojo y las páginas.</summary>
    static bool Excluido(Renderer r) => r.name.Contains("Nucleo") || r.name.Contains("Pagina");

    /// <summary>Fija la disolución al instante (0 entero, 1 derretido).</summary>
    public void Fijar(float valor)
    {
        Disolucion = Mathf.Clamp01(valor);
        foreach (var r in renderers)
        {
            if (r == null || Excluido(r)) continue;
            r.GetPropertyBlock(bloque);
            bloque.SetFloat(IdDisolver, Disolucion);
            bloque.SetFloat(IdAltura, altura);
            bloque.SetFloat(IdBase, transform.position.y);   // los pies: el pivote de la raíz
            if (materialTinta != null) bloque.SetFloat(IdLetra, materialTinta.GetFloat(IdLetra) * escalaLetras);
            r.SetPropertyBlock(bloque);
        }
    }

    /// <summary>Se derrite de arriba abajo; al terminar llama a alTerminar.</summary>
    public void Derretir(float duracion = 1f, Action alTerminar = null) => Animar(1f, duracion, alTerminar);

    /// <summary>Se vuelve a formar desde el charco; al terminar llama a alTerminar.</summary>
    public void Formar(float duracion = 1f, Action alTerminar = null) => Animar(0f, duracion, alTerminar);

    void Animar(float destino, float duracion, Action alTerminar)
    {
        if (animacion != null) StopCoroutine(animacion);
        animacion = StartCoroutine(Rutina(destino, duracion, alTerminar));
    }

    IEnumerator Rutina(float destino, float duracion, Action alTerminar)
    {
        float inicio = Disolucion;
        for (float t = 0f; t < duracion; t += Time.deltaTime)
        {
            Fijar(Mathf.Lerp(inicio, destino, Mathf.SmoothStep(0f, 1f, t / duracion)));
            yield return null;
        }
        Fijar(destino);
        animacion = null;
        alTerminar?.Invoke();
    }
}
