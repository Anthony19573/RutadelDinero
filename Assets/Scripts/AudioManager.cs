using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AudioManager : MonoBehaviour
{
    public static AudioManager I { get; private set; }

    [Header("Clips de Audio")]
    public AudioClip successClip;
    public AudioClip errorClip;
    public AudioClip defenseClip;

    private AudioSource audioSource;

    private void Awake()
    {
        if (I != null && I != this)
        {
            Destroy(gameObject);
            return;
        }

        I = this;
        audioSource = GetComponent<AudioSource>();

        if (audioSource != null)
        {
            audioSource.playOnAwake = false;
        }
    }

    public void PlaySuccess()
    {
        if (audioSource != null && successClip != null)
        {
            audioSource.PlayOneShot(successClip);
        }
        Debug.Log("[AudioManager] Sonido de Éxito (Success) reproducido.");
    }

    public void PlayError()
    {
        if (audioSource != null && errorClip != null)
        {
            audioSource.PlayOneShot(errorClip);
        }
        Debug.Log("[AudioManager] Sonido de Error reproducido.");
    }

    public void PlayDefense()
    {
        if (audioSource != null && defenseClip != null)
        {
            audioSource.PlayOneShot(defenseClip);
        }
        else if (audioSource != null && successClip != null)
        {
            audioSource.PlayOneShot(successClip);
        }
        Debug.Log("[AudioManager] Sonido de Alerta/Defensa reproducido.");
    }

    public void PlayVoiceover(AudioClip clip)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.PlayOneShot(clip);
            Debug.Log($"[AudioManager] Reproduciendo locución: {clip.name}");
        }
    }
}
