using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Vuforia;

public class StopTarget : MonoBehaviour
{
    [Header("Configuración de Parada")]
    [Tooltip("Índice de la parada (0 a 4)")]
    public int stopIndex;

    [Tooltip("Diorama o Modelo 3D del Modo Ataque")]
    public GameObject modelo3D;

    [Tooltip("Diorama 3D específico del Modo Defensa (control preventivo)")]
    public GameObject dioramaDefensa;

    [Tooltip("Segundos que permanece visible el diorama/holograma en Modo Defensa antes de auto-ocultarse")]
    public float duracionDefensaSegundos = 8f;

    [Header("Holograma 3D Flotante (World Space)")]
    [Tooltip("Canvas World Space que flota sobre el marcador")]
    public GameObject holograma3D;
    public Text textoTitulo3D;
    public Text textoCuerpo3D;
    public Button botonResolver3D;
    public UnityEngine.UI.Image panelFondoHolograma;
    public UnityEngine.UI.Image barraCabeceraHolograma;

    private ObserverBehaviour mObserverBehaviour;
    private Renderer mRenderer;
    private Material mMaterialInstanciado;
    private ARVisualController mVisualController;
    private ARVisualController mDefensaVisualController;
    private bool vozParadaReproducida = false;
    private Coroutine mAutoHideDefensaCoroutine;

    private void Start()
    {
        mObserverBehaviour = GetComponent<ObserverBehaviour>();

        if (modelo3D != null)
        {
            mVisualController = modelo3D.GetComponent<ARVisualController>();
            mRenderer = modelo3D.GetComponent<Renderer>();
            if (mRenderer != null)
            {
                mMaterialInstanciado = mRenderer.material;
            }
        }

        if (dioramaDefensa != null)
        {
            mDefensaVisualController = dioramaDefensa.GetComponent<ARVisualController>();
            dioramaDefensa.SetActive(false);
        }

        if (botonResolver3D != null)
        {
            botonResolver3D.onClick.RemoveAllListeners();
            botonResolver3D.onClick.AddListener(OnBotonResolverClick);
        }

        if (mObserverBehaviour != null)
        {
            mObserverBehaviour.OnTargetStatusChanged += OnTargetStatusChanged;
            OnTargetStatusChanged(mObserverBehaviour, mObserverBehaviour.TargetStatus);
        }
        else
        {
            Debug.LogWarning($"[StopTarget] No se encontró ObserverBehaviour en {gameObject.name}");
        }
    }

    private void OnDestroy()
    {
        DetenerAutoOcultadoDefensa();

        if (mObserverBehaviour != null)
        {
            mObserverBehaviour.OnTargetStatusChanged -= OnTargetStatusChanged;
        }

        if (mMaterialInstanciado != null)
        {
            Destroy(mMaterialInstanciado);
        }
    }

    public void ResetVozParada()
    {
        vozParadaReproducida = false;
    }

    public void OnBotonResolverClick()
    {
        if (holograma3D != null)
        {
            holograma3D.SetActive(false);
        }

        if (ChallengeManager.I != null)
        {
            ChallengeManager.I.AbrirRetoDirecto(stopIndex);
        }
    }

    private void OnTargetStatusChanged(ObserverBehaviour behaviour, TargetStatus targetStatus)
    {
        // Estricto: considerar visible ÚNICAMENTE cuando Status == TRACKED (sin Extended Tracking ni Limited)
        bool isTracked = (targetStatus.Status == Status.TRACKED);

        if (isTracked)
        {
            // Mientras algún panel 2D de reto/feedback esté abierto en pantalla, ignorar
            if (UIManager.I != null && UIManager.I.HayPanelAbierto())
            {
                return;
            }

            if (GameManager.I == null) return;

            if (GameManager.I.ModoActual == GameManager.SimMode.Ataque)
            {
                DetenerAutoOcultadoDefensa();
                if (dioramaDefensa != null) dioramaDefensa.SetActive(false);

                int esperada = GameManager.I.paradaActualEsperada;

                if (stopIndex > esperada)
                {
                    if (modelo3D != null) modelo3D.SetActive(false);
                    if (holograma3D != null) holograma3D.SetActive(false);

                    string nombreEsperada = (GameManager.I.data != null && GameManager.I.data.nombresParadas != null && esperada < GameManager.I.data.nombresParadas.Length)
                        ? GameManager.I.data.nombresParadas[esperada]
                        : $"Parada {esperada + 1}";

                    if (UIManager.I != null)
                    {
                        UIManager.I.MostrarAvisoOrden("ORDEN INCORRECTO: Primero debes investigar y resolver la Parada " + (esperada + 1) + " (" + nombreEsperada + ").");
                    }
                }
                else if (stopIndex < esperada)
                {
                    if (modelo3D != null)
                    {
                        modelo3D.SetActive(true);
                        if (mVisualController != null) mVisualController.SetDefenseMode(true);
                        SetCubeColor(new Color(0.22f, 0.95f, 0.54f));
                    }

                    if (holograma3D != null) holograma3D.SetActive(false);

                    string nombreParada = (GameManager.I.data != null && GameManager.I.data.nombresParadas != null && stopIndex < GameManager.I.data.nombresParadas.Length)
                        ? GameManager.I.data.nombresParadas[stopIndex]
                        : $"Parada {stopIndex + 1}";

                    string nombreEsperada = (GameManager.I.data != null && GameManager.I.data.nombresParadas != null && esperada < GameManager.I.data.nombresParadas.Length)
                        ? GameManager.I.data.nombresParadas[esperada]
                        : $"Parada {esperada + 1}";

                    if (UIManager.I != null)
                    {
                        UIManager.I.MostrarAvisoOrden($"PARADA YA INVESTIGADA: La Parada {stopIndex + 1} ({nombreParada}) ya fue superada. Dirígete a la Parada {esperada + 1} ({nombreEsperada}).");
                    }
                }
                else // stopIndex == esperada: Parada correcta en Modo Ataque
                {
                    if (modelo3D != null)
                    {
                        modelo3D.SetActive(true);
                        if (mVisualController != null) mVisualController.SetDefenseMode(false);
                        SetCubeColor(new Color(1f, 0.35f, 0.31f));
                    }

                    if (!vozParadaReproducida)
                    {
                        vozParadaReproducida = true;
                        if (AudioManager.I != null)
                        {
                            AudioManager.I.PlayParadaNarrator(stopIndex);
                        }
                    }

                    ConfigurarHologramaAtaque();
                }
            }
            else // Modo Defensa
            {
                // Ocultar diorama de ataque, activar dioramaDefensa y mostrar holograma con recomendación preventiva
                if (modelo3D != null) modelo3D.SetActive(false);

                if (dioramaDefensa != null)
                {
                    dioramaDefensa.SetActive(true);
                    if (mDefensaVisualController != null) mDefensaVisualController.SetDefenseMode(true);
                }

                ConfigurarHologramaDefensa();

                // Reproducir narración defensiva de la parada y usar duracion + 1.5s para auto-ocultar
                float duracion = (AudioManager.I != null) ? AudioManager.I.PlayDefensaParadaNarrator(stopIndex) : 0f;
                float tiempoEspera = (duracion > 0f) ? (duracion + 1.5f) : duracionDefensaSegundos;

                DetenerAutoOcultadoDefensa();
                mAutoHideDefensaCoroutine = StartCoroutine(AutoOcultarDefensaCoroutine(tiempoEspera));
            }
        }
        else
        {
            // Si el usuario retira el marcador antes en Modo Defensa, detener también el audio del narrador
            if (mAutoHideDefensaCoroutine != null && AudioManager.I != null)
            {
                AudioManager.I.StopNarrator();
            }

            // En cualquier estado distinto a TRACKED (EXTENDED_TRACKED, LIMITED, NO_POSE), ocultar todo de inmediato
            DetenerAutoOcultadoDefensa();
            OcultarElementosVisuales();
        }
    }

    private IEnumerator AutoOcultarDefensaCoroutine(float segundos)
    {
        yield return new WaitForSeconds(segundos);

        if (dioramaDefensa != null) dioramaDefensa.SetActive(false);
        if (holograma3D != null) holograma3D.SetActive(false);

        mAutoHideDefensaCoroutine = null;
        Debug.Log($"[StopTarget] DioramaDefensa y Holograma de Parada {stopIndex} auto-ocultados tras {segundos:F2}s.");
    }

    private void DetenerAutoOcultadoDefensa()
    {
        if (mAutoHideDefensaCoroutine != null)
        {
            StopCoroutine(mAutoHideDefensaCoroutine);
            mAutoHideDefensaCoroutine = null;
        }
    }

    private void OcultarElementosVisuales()
    {
        if (modelo3D != null) modelo3D.SetActive(false);
        if (dioramaDefensa != null) dioramaDefensa.SetActive(false);
        if (holograma3D != null) holograma3D.SetActive(false);
    }

    private void ConfigurarHologramaAtaque()
    {
        if (holograma3D == null) return;

        holograma3D.SetActive(true);

        if (ChallengeManager.I != null && ChallengeManager.I.paradas != null && stopIndex >= 0 && stopIndex < ChallengeManager.I.paradas.Length)
        {
            var p = ChallengeManager.I.paradas[stopIndex];

            if (barraCabeceraHolograma != null)
            {
                barraCabeceraHolograma.color = new Color(255f / 255f, 90f / 255f, 78f / 255f, 1f); // Rojo Ataque #FF5A4E
            }

            if (textoTitulo3D != null)
            {
                textoTitulo3D.text = p.tituloParada;
                textoTitulo3D.color = Color.white;
            }

            if (textoCuerpo3D != null)
            {
                textoCuerpo3D.color = Color.white;
                textoCuerpo3D.text = $"<b><color=#FFC24A>[NARRACIÓN DE LOS HECHOS REALES]</color></b>\n{p.narracion}\n\n<b><color=#FF5A4E>[ANÁLISIS DE VULNERABILIDAD]</color></b>\n{p.analisisTecnico}";
            }

            if (botonResolver3D != null)
            {
                botonResolver3D.gameObject.SetActive(true);
            }
        }
    }

    private void ConfigurarHologramaDefensa()
    {
        if (holograma3D == null) return;

        holograma3D.SetActive(true);

        if (ChallengeManager.I != null && ChallengeManager.I.paradas != null && stopIndex >= 0 && stopIndex < ChallengeManager.I.paradas.Length)
        {
            var p = ChallengeManager.I.paradas[stopIndex];

            if (barraCabeceraHolograma != null)
            {
                barraCabeceraHolograma.color = new Color(56f / 255f, 242f / 255f, 138f / 255f, 1f); // Verde Neón #38F28A
            }

            if (textoTitulo3D != null)
            {
                textoTitulo3D.text = $"DEFENSA ACTIVA - {p.tituloParada}";
                textoTitulo3D.color = new Color(2f / 255f, 10f / 255f, 6f / 255f, 1f);
            }

            if (textoCuerpo3D != null)
            {
                textoCuerpo3D.color = Color.white;
                string estado = (stopIndex <= 1)
                    ? "<b><color=#38F28A>[ESTADO: NODO OPERATIVO Y SUPERVISADO 24/7]</color></b>"
                    : "<b><color=#FFC24A>[ESTADO: NODO AISLADO POR CORTE DE GRAFO]</color></b>";

                textoCuerpo3D.text = $"{estado}\n\n<b><color=#38F28A>[RECOMENDACIÓN TÉCNICA APLICADA]</color></b>\n{p.recomendacionDefensa}";
            }

            if (botonResolver3D != null)
            {
                botonResolver3D.gameObject.SetActive(false);
            }
        }
    }

    private void SetCubeColor(Color color)
    {
        if (mMaterialInstanciado != null)
        {
            mMaterialInstanciado.color = color;
        }
        else if (mRenderer != null && mRenderer.material != null)
        {
            mRenderer.material.color = color;
        }
    }
}
