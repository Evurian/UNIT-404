using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SfxPlayer : MonoBehaviour
{
    public AudioClip recarga;
    public AudioClip impulso;
    public AudioClip dano;
    public AudioClip victoria;

    AudioSource source;

    void Awake()
    {
        source = GetComponent<AudioSource>();
    }

    public void Play(AudioClip clip)
    {
        if (clip == null) return;
        if (source == null) source = GetComponent<AudioSource>();
        source.PlayOneShot(clip);
    }
}
