using UnityEngine;
using UnityEngine.UI;
using Vuforia;

public class StopTarget : MonoBehaviour
{
    [Header("Configuración de Parada")]
    [Tooltip("Índice de la parada (0 a 4)")]
    public int stopIndex;

    [Tooltip("Modelo 3D que aparecerá sobre el marcador")]
    public GameObject modelo3D;

    [Header("Holograma 3D Flotante (World Space)")]
    [Tooltip("Canvas World Space que flota sobre el marcador")]
    public GameObject holograma3D;
    public Text textoTitulo3D;
    public Text textoCuerpo3D;
    public Button botonResolver3D;
    public UnityEngine.UI.Image panelFondoHolograma;

    private ObserverBehaviour mObserverBehaviour;
    private Renderer mRenderer;
    private Material mMaterialInstanciado;

    private void Start()
    {
        mObserverBehaviour = GetComponent<ObserverBehaviour>();

        if (modelo3D != null)
        {
            mRenderer = modelo3D.GetComponent<Renderer>();
            if (mRenderer != null)
            {
                // Instanciar material para permitir cambios de color independientes
                mMaterialInstanciado = mRenderer.material;
            }
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
        if (mObserverBehaviour != null)
        {
            mObserverBehaviour.OnTargetStatusChanged -= OnTargetStatusChanged;
        }

        if (mMaterialInstanciado != null)
        {
            Destroy(mMaterialInstanciado);
        }
    }

    public void OnBotonResolverClick()
    {
        // Ocultar temporalmente el holograma 3D y abrir el reto 2D en pantalla
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
        bool isTracked = (targetStatus.Status == Status.TRACKED || targetStatus.Status == Status.EXTENDED_TRACKED);

        if (isTracked)
        {
            // Mientras algún panel de reto/feedback esté activo en pantalla, ignorar para evitar interferencias
            if (UIManager.I != null && UIManager.I.HayPanelAbierto())
            {
                return;
            }

            if (GameManager.I == null) return;

            if (GameManager.I.ModoActual == GameManager.SimMode.Ataque)
            {
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
                        SetCubeColor(new Color(0.22f, 0.95f, 0.54f)); // Verde resuelto
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
                        SetCubeColor(new Color(1f, 0.35f, 0.31f)); // Rojo ataque / objetivo actual
                    }

                    // Activar y configurar el Holograma 3D flotante con narración y análisis
                    ConfigurarHologramaAtaque();
                }
            }
            else // Modo Defensa
            {
                if (modelo3D != null)
                {
                    modelo3D.SetActive(true);

                    // Paradas 0 y 1: verde (#38F28A)
                    // Paradas 2, 3 y 4: gris bloqueado (#555555)
                    if (stopIndex <= 1)
                    {
                        SetCubeColor(new Color(0.22f, 0.95f, 0.54f)); // Verde protegido
                    }
                    else
                    {
                        SetCubeColor(new Color(0.33f, 0.33f, 0.33f)); // Gris bloqueado
                    }
                }

                // Activar y configurar el Holograma 3D flotante con recomendación técnica
                ConfigurarHologramaDefensa();
            }
        }
        else
        {
            if (modelo3D != null)
            {
                modelo3D.SetActive(false);
            }

            if (holograma3D != null)
            {
                holograma3D.SetActive(false);
            }
        }
    }

    private void ConfigurarHologramaAtaque()
    {
        if (holograma3D == null) return;

        holograma3D.SetActive(true);

        if (ChallengeManager.I != null && ChallengeManager.I.paradas != null && stopIndex >= 0 && stopIndex < ChallengeManager.I.paradas.Length)
        {
            var p = ChallengeManager.I.paradas[stopIndex];

            if (textoTitulo3D != null)
            {
                textoTitulo3D.text = p.tituloParada;
                textoTitulo3D.color = new Color(56f / 255f, 242f / 255f, 138f / 255f); // Verde Neón
            }

            if (textoCuerpo3D != null)
            {
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

            if (textoTitulo3D != null)
            {
                textoTitulo3D.text = $"DEFENSA ACTIVA - {p.tituloParada}";
                textoTitulo3D.color = new Color(56f / 255f, 242f / 255f, 138f / 255f);
            }

            if (textoCuerpo3D != null)
            {
                string estado = (stopIndex <= 1)
                    ? "<b><color=#38F28A>[ESTADO: NODO OPERATIVO Y SUPERVISADO 24/7]</color></b>"
                    : "<b><color=#FFC24A>[ESTADO: NODO AISLADO POR CORTE DE GRAFO]</color></b>";

                textoCuerpo3D.text = $"{estado}\n\n<b><color=#38F28A>[RECOMENDACIÓN TÉCNICA APLICADA]</color></b>\n{p.recomendacionDefensa}";
            }

            // Ocultar botón de resolver reto en modo defensa
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
