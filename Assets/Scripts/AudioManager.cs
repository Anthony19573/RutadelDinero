using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AudioManager : MonoBehaviour
{
    public static AudioManager I { get; private set; }

    [Header("Clips de Efectos de Sonido (SFX)")]
    public AudioClip successClip;
    public AudioClip errorClip;
    public AudioClip defenseClip;

    [Header("Fuente de Voz del Narrador")]
    public AudioSource voiceSource;

    [Header("Clips de Narración (Voz)")]
    public AudioClip introClip;
    public AudioClip aciertoClip;
    public AudioClip defensaClip;
    public AudioClip[] clipsParadas = new AudioClip[5];
    public AudioClip[] clipsDefensaParadas = new AudioClip[5];

    private AudioSource sfxSource;

    private void Awake()
    {
        if (I != null && I != this)
        {
            Destroy(gameObject);
            return;
        }

        I = this;

        AudioSource[] sources = GetComponents<AudioSource>();
        sfxSource = sources.Length > 0 ? sources[0] : gameObject.AddComponent<AudioSource>();
        sfxSource.playOnAwake = false;

        if (voiceSource == null)
        {
            if (sources.Length > 1)
            {
                voiceSource = sources[1];
            }
            else
            {
                voiceSource = gameObject.AddComponent<AudioSource>();
            }
        }

        voiceSource.playOnAwake = false;
        voiceSource.loop = false;
    }

    public void PlaySuccess()
    {
        if (sfxSource != null && successClip != null)
        {
            sfxSource.PlayOneShot(successClip);
        }
        Debug.Log("[AudioManager] Sonido de Éxito (Success) reproducido.");
    }

    public void PlayError()
    {
        if (sfxSource != null && errorClip != null)
        {
            sfxSource.PlayOneShot(errorClip);
        }
        Debug.Log("[AudioManager] Sonido de Error reproducido.");
    }

    public void PlayDefense()
    {
        if (sfxSource != null && defenseClip != null)
        {
            sfxSource.PlayOneShot(defenseClip);
        }
        else if (sfxSource != null && successClip != null)
        {
            sfxSource.PlayOneShot(successClip);
        }
        Debug.Log("[AudioManager] Sonido de Alerta/Defensa reproducido.");
    }

    public void PlayNarrator(AudioClip clip)
    {
        if (clip == null || voiceSource == null) return;

        // Detener cualquier narración previa para evitar superposición de voces
        voiceSource.Stop();
        voiceSource.clip = clip;
        voiceSource.Play();

        Debug.Log($"[AudioManager][NARRADOR] Reproduciendo archivo: {clip.name}");
    }

    public void PlayParadaNarrator(int stopIndex)
    {
        if (clipsParadas != null && stopIndex >= 0 && stopIndex < clipsParadas.Length && clipsParadas[stopIndex] != null)
        {
            Debug.Log($"[AudioManager][PARADA {stopIndex}] Reproduciendo clip de parada: {clipsParadas[stopIndex].name}");
            PlayNarrator(clipsParadas[stopIndex]);
        }
        else
        {
            Debug.LogWarning($"[AudioManager] No se encontró clip de narración para parada {stopIndex}.");
        }
    }

    public float PlayDefensaParadaNarrator(int stopIndex)
    {
        if (clipsDefensaParadas != null && stopIndex >= 0 && stopIndex < clipsDefensaParadas.Length && clipsDefensaParadas[stopIndex] != null)
        {
            AudioClip clip = clipsDefensaParadas[stopIndex];
            Debug.Log($"[AudioManager][DEFENSA PARADA {stopIndex}] Reproduciendo clip: {clip.name} ({clip.length:F2}s)");
            PlayNarrator(clip);
            return clip.length;
        }

        Debug.LogWarning($"[AudioManager] No se encontró clip de defensa para parada {stopIndex}.");
        return 0f;
    }

    public void StopNarrator()
    {
        if (voiceSource != null && voiceSource.isPlaying)
        {
            voiceSource.Stop();
            Debug.Log("[AudioManager] Narrador detenido.");
        }
    }

    public void PlayVoiceover(AudioClip clip)
    {
        PlayNarrator(clip);
    }
}
