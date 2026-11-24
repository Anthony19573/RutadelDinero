using UnityEngine;

public class ARVisualController : MonoBehaviour
{
    [Header("Elementos Animados")]
    [Tooltip("Partes del diorama que rotan suavemente (radares, monedas, fichas, etc.)")]
    public Transform[] elementosRotatorios;
    public float velocidadRotacion = 45f;

    [Tooltip("Partes que flotan suavemente arriba y abajo")]
    public Transform[] elementosFlotantes;
    public float amplitudFlotacion = 0.03f;
    public float velocidadFlotacion = 2.5f;

    [Header("Iluminación de Estado")]
    public Light luzEstado;
    public Color colorAtaque = new Color(1f, 0.18f, 0.12f, 1f);
    public Color colorDefensa = new Color(0.22f, 0.95f, 0.54f, 1f);
    public float velocidadParpadeo = 4.5f;
    public float intensidadMinAtaque = 0.35f;
    public float intensidadMaxAtaque = 3.2f;
    public float intensidadDefensa = 2.6f;

    [Header("Escudo Defensivo")]
    [Tooltip("Esfera/escudo translúcido verde activado en Modo Defensa")]
    public GameObject escudoDefensa;

    private bool enModoDefensa = false;
    private Vector3[] posicionesBaseFlotantes;

    private void Awake()
    {
        if (elementosFlotantes != null && elementosFlotantes.Length > 0)
        {
            posicionesBaseFlotantes = new Vector3[elementosFlotantes.Length];
            for (int i = 0; i < elementosFlotantes.Length; i++)
            {
                if (elementosFlotantes[i] != null)
                {
                    posicionesBaseFlotantes[i] = elementosFlotantes[i].localPosition;
                }
            }
        }
    }

    private void OnEnable()
    {
        if (GameManager.I != null)
        {
            SetDefenseMode(GameManager.I.ModoActual == GameManager.SimMode.Defensa);
        }
        else
        {
            AplicarEstadoVisual();
        }
    }

    private void Update()
    {
        // Rotación suave de elementos animados
        if (elementosRotatorios != null)
        {
            for (int i = 0; i < elementosRotatorios.Length; i++)
            {
                if (elementosRotatorios[i] != null)
                {
                    elementosRotatorios[i].Rotate(Vector3.up, velocidadRotacion * Time.deltaTime, Space.Self);
                }
            }
        }

        // Flotación suave (ej. monedas o módulos)
        if (elementosFlotantes != null && posicionesBaseFlotantes != null)
        {
            for (int i = 0; i < elementosFlotantes.Length && i < posicionesBaseFlotantes.Length; i++)
            {
                if (elementosFlotantes[i] != null)
                {
                    float offset = Mathf.Sin(Time.time * velocidadFlotacion + i * 1.2f) * amplitudFlotacion;
                    elementosFlotantes[i].localPosition = posicionesBaseFlotantes[i] + new Vector3(0f, offset, 0f);
                }
            }
        }

        // Animación de luz de alerta en Modo Ataque vs luz fija verde en Modo Defensa
        if (luzEstado != null)
        {
            if (!enModoDefensa)
            {
                float t = (Mathf.Sin(Time.time * velocidadParpadeo) + 1f) * 0.5f;
                luzEstado.color = colorAtaque;
                luzEstado.intensity = Mathf.Lerp(intensidadMinAtaque, intensidadMaxAtaque, t);
            }
            else
            {
                luzEstado.color = colorDefensa;
                luzEstado.intensity = intensidadDefensa;
            }
        }

        // Rotación sutil del escudo en Modo Defensa
        if (enModoDefensa && escudoDefensa != null && escudoDefensa.activeSelf)
        {
            escudoDefensa.transform.Rotate(Vector3.up, 18f * Time.deltaTime, Space.Self);
        }
    }

    public void SetDefenseMode(bool activo)
    {
        enModoDefensa = activo;
        AplicarEstadoVisual();
    }

    public void ActivarModoAtaque()
    {
        SetDefenseMode(false);
    }

    public void ActivarModoDefensa()
    {
        SetDefenseMode(true);
    }

    private void AplicarEstadoVisual()
    {
        if (luzEstado != null)
        {
            luzEstado.color = enModoDefensa ? colorDefensa : colorAtaque;
            luzEstado.intensity = enModoDefensa ? intensidadDefensa : intensidadMaxAtaque;
        }

        if (escudoDefensa != null)
        {
            escudoDefensa.SetActive(enModoDefensa);
        }
    }
}
