using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager I { get; private set; }

    [Header("Top HUD (Barra Superior Fija - Estilo SOC)")]
    public GameObject topHUD;
    public Text textoModo;
    public Text textoProgreso;

    [Header("Panel 1: Introducción")]
    public GameObject panelIntro;

    [Header("Panel 2: Historia y Contexto (Paso A)")]
    public GameObject panelHistoria;
    public Text textoTituloHistoria;
    public Text textoNarracion;
    public Text textoAnalisisTecnico;
    public Button botonResolverReto;

    [Header("Panel 3: Reto Técnico (Paso B)")]
    public GameObject panelReto;

    [Header("Panel 4: Feedback Limpio (Paso C)")]
    public GameObject panelFeedback;
    public Text textoTituloFeedback;
    public Text textoCajaFragmento;
    public Text textoCajaPista;
    public Button botonContinuar;

    [Header("Panel 5: Cierre del Modo Ataque")]
    public GameObject panelFinalAtaque;
    public Text textoFinalAtaque;
    public Button botonActivarDefensaFinal;

    [Header("Panel 6: Infografía Comparativa de Defensa")]
    public GameObject panelInfografiaDefensa;
    public Button botonCerrarInfografia;

    [Header("Panel 7: Recomendación Defensiva por Parada")]
    public GameObject panelDefensaDetalle;
    public Text textoTituloDefensa;
    public Text textoRecomendacionDefensa;
    public Text textoEstadoNodo;
    public Button botonCerrarDefensaDetalle;

    [Header("Panel 8: Aviso de Orden Estricto")]
    public GameObject panelAvisoOrden;
    public Text textoAvisoOrden;
    public Button botonCerrarAviso;

    // Paleta SOC
    public static readonly Color ColorFondoSOC = new Color(2f / 255f, 10f / 255f, 6f / 255f, 235f / 255f);
    public static readonly Color ColorVerdeNeon = new Color(56f / 255f, 242f / 255f, 138f / 255f, 1f);
    public static readonly Color ColorRojoAtaque = new Color(255f / 255f, 90f / 255f, 78f / 255f, 1f);
    public static readonly Color ColorAmbarPista = new Color(255f / 255f, 194f / 255f, 74f / 255f, 1f);

    private void Awake()
    {
        if (I != null && I != this)
        {
            Destroy(gameObject);
            return;
        }

        I = this;
    }

    private void Start()
    {
        if (botonResolverReto != null)
        {
            botonResolverReto.onClick.RemoveAllListeners();
            botonResolverReto.onClick.AddListener(() =>
            {
                OcultarHistoria();
                if (ChallengeManager.I != null) ChallengeManager.I.AbrirRetoActual();
            });
        }

        if (botonContinuar != null)
        {
            botonContinuar.onClick.RemoveAllListeners();
            botonContinuar.onClick.AddListener(() =>
            {
                if (ChallengeManager.I != null) ChallengeManager.I.ContinuarDesdeFeedback();
            });
        }

        if (botonActivarDefensaFinal != null)
        {
            botonActivarDefensaFinal.onClick.RemoveAllListeners();
            botonActivarDefensaFinal.onClick.AddListener(() =>
            {
                OcultarFinalAtaque();
                if (GameManager.I != null) GameManager.I.ActivarDefensaTrazabilidad();
            });
        }

        if (botonCerrarInfografia != null)
        {
            botonCerrarInfografia.onClick.RemoveAllListeners();
            botonCerrarInfografia.onClick.AddListener(OcultarInfografiaDefensa);
        }

        if (botonCerrarDefensaDetalle != null)
        {
            botonCerrarDefensaDetalle.onClick.RemoveAllListeners();
            botonCerrarDefensaDetalle.onClick.AddListener(OcultarDefensaDetalle);
        }

        if (botonCerrarAviso != null)
        {
            botonCerrarAviso.onClick.RemoveAllListeners();
            botonCerrarAviso.onClick.AddListener(OcultarAvisoOrden);
        }
    }

    // Consulta de exclusión para evitar que el tracking re-dispare eventos cuando hay un panel abierto
    public bool HayPanelAbierto()
    {
        return (panelHistoria != null && panelHistoria.activeSelf) ||
               (panelReto != null && panelReto.activeSelf) ||
               (panelFeedback != null && panelFeedback.activeSelf) ||
               (panelAvisoOrden != null && panelAvisoOrden.activeSelf) ||
               (panelFinalAtaque != null && panelFinalAtaque.activeSelf) ||
               (panelInfografiaDefensa != null && panelInfografiaDefensa.activeSelf) ||
               (panelDefensaDetalle != null && panelDefensaDetalle.activeSelf);
    }

    public void ActualizarTopHUD(GameManager.SimMode modo, int paradas, int fragmentos)
    {
        if (textoModo != null)
        {
            if (modo == GameManager.SimMode.Ataque)
            {
                textoModo.text = "● MODO: ATAQUE 2016";
                textoModo.color = ColorRojoAtaque;
            }
            else
            {
                textoModo.text = "🛡️ MODO: DEFENSA ACTIVA";
                textoModo.color = ColorVerdeNeon;
            }
        }

        if (textoProgreso != null)
        {
            textoProgreso.text = $"PARADAS: {Mathf.Clamp(paradas, 0, 5)}/5 | FRAGMENTOS: {Mathf.Clamp(fragmentos, 0, 5)}/5";
            textoProgreso.color = ColorAmbarPista;
        }

        if (topHUD != null) topHUD.SetActive(true);
    }

    // --- INTRO ---
    public void MostrarIntro()
    {
        if (panelIntro != null) panelIntro.SetActive(true);
    }

    public void OcultarIntro()
    {
        if (panelIntro != null) panelIntro.SetActive(false);
    }

    // --- HISTORIA (Paso A) ---
    public void MostrarHistoria(string titulo, string narracion, string analisis)
    {
        OcultarTodosLosModalesContextuales();

        if (textoTituloHistoria != null) textoTituloHistoria.text = titulo;
        if (textoNarracion != null) textoNarracion.text = narracion;
        if (textoAnalisisTecnico != null) textoAnalisisTecnico.text = analisis;

        if (panelHistoria != null) panelHistoria.SetActive(true);
    }

    public void OcultarHistoria()
    {
        if (panelHistoria != null) panelHistoria.SetActive(false);
    }

    // --- RETO (Paso B) ---
    public void MostrarReto()
    {
        OcultarTodosLosModalesContextuales();
        if (panelReto != null) panelReto.SetActive(true);
    }

    public void OcultarReto()
    {
        if (panelReto != null) panelReto.SetActive(false);
    }

    // --- FEEDBACK LIMPIO (Paso C) ---
    public void MostrarFeedbackExito(string fragmento, string analisis, string pista)
    {
        OcultarTodosLosModalesContextuales();

        if (textoTituloFeedback != null)
        {
            textoTituloFeedback.text = "¡ANÁLISIS FORENSE VALIDADO CON ÉXITO!";
            textoTituloFeedback.color = ColorVerdeNeon;
        }

        if (textoCajaFragmento != null)
        {
            textoCajaFragmento.text = $"<b>Fragmento Criptográfico Obtenido:</b> <color=#38F28A>{fragmento}</color>\n<size=19>{analisis}</size>";
        }

        if (textoCajaPista != null)
        {
            textoCajaPista.text = $"<b>PISTA TÁCTICA:</b> {pista}";
            textoCajaPista.color = ColorAmbarPista;
        }

        if (panelFeedback != null) panelFeedback.SetActive(true);
    }

    public void MostrarFeedbackExito(string fragmento, string pista)
    {
        MostrarFeedbackExito(fragmento, "Vulnerabilidad forense identificada y neutralizada en el terminal.", pista);
    }

    public void OcultarFeedback()
    {
        if (panelFeedback != null) panelFeedback.SetActive(false);
    }

    // --- CIERRE DE ATAQUE ---
    public void MostrarFinalAtaque()
    {
        OcultarTodosLosModalesContextuales();

        if (textoFinalAtaque != null)
        {
            textoFinalAtaque.text = "<b>SIMULACIÓN DE ATAQUE COMPLETADA:</b>\nEl dinero recorrió las 5 paradas sin trazabilidad integrada.\n\n<color=#FF5A4E>Resultado real de 2016: $81M desviados, $66M perdidos definitivamente.</color>";
        }

        if (panelFinalAtaque != null) panelFinalAtaque.SetActive(true);
    }

    public void OcultarFinalAtaque()
    {
        if (panelFinalAtaque != null) panelFinalAtaque.SetActive(false);
    }

    // --- INFOGRAFÍA DEFENSIVA ---
    public void MostrarInfografiaDefensa()
    {
        OcultarTodosLosModalesContextuales();
        if (panelInfografiaDefensa != null) panelInfografiaDefensa.SetActive(true);
    }

    public void OcultarInfografiaDefensa()
    {
        if (panelInfografiaDefensa != null) panelInfografiaDefensa.SetActive(false);
    }

    // --- DETALLE DEFENSIVO POR PARADA ---
    public void MostrarDefensaDetalle(int stopIndex)
    {
        OcultarTodosLosModalesContextuales();

        if (ChallengeManager.I != null && ChallengeManager.I.paradas != null && stopIndex >= 0 && stopIndex < ChallengeManager.I.paradas.Length)
        {
            var p = ChallengeManager.I.paradas[stopIndex];

            if (textoTituloDefensa != null)
            {
                textoTituloDefensa.text = $"DEFENSA ACTIVA - {p.tituloParada}";
            }

            if (textoRecomendacionDefensa != null)
            {
                textoRecomendacionDefensa.text = p.recomendacionDefensa;
            }

            if (textoEstadoNodo != null)
            {
                if (stopIndex <= 1)
                {
                    textoEstadoNodo.text = "ESTADO: [NODO OPERATIVO Y MONITOREADO 24/7]";
                    textoEstadoNodo.color = ColorVerdeNeon;
                }
                else
                {
                    textoEstadoNodo.text = "ESTADO: [NODO AISLADO POR CORTE DE FLUJO EN GRAFO]";
                    textoEstadoNodo.color = ColorAmbarPista;
                }
            }
        }

        if (panelDefensaDetalle != null) panelDefensaDetalle.SetActive(true);
    }

    public void OcultarDefensaDetalle()
    {
        if (panelDefensaDetalle != null) panelDefensaDetalle.SetActive(false);
    }

    // --- AVISO DE ORDEN ESTRICTO ---
    public void MostrarAvisoOrden(string mensaje)
    {
        if (textoAvisoOrden != null)
        {
            textoAvisoOrden.text = mensaje;
            textoAvisoOrden.color = ColorAmbarPista;
        }

        if (panelAvisoOrden != null) panelAvisoOrden.SetActive(true);
    }

    public void OcultarAvisoOrden()
    {
        if (panelAvisoOrden != null) panelAvisoOrden.SetActive(false);
    }

    public void OcultarTodosLosModalesContextuales()
    {
        if (panelHistoria != null) panelHistoria.SetActive(false);
        if (panelReto != null) panelReto.SetActive(false);
        if (panelFeedback != null) panelFeedback.SetActive(false);
        if (panelFinalAtaque != null) panelFinalAtaque.SetActive(false);
        if (panelInfografiaDefensa != null) panelInfografiaDefensa.SetActive(false);
        if (panelDefensaDetalle != null) panelDefensaDetalle.SetActive(false);
        if (panelAvisoOrden != null) panelAvisoOrden.SetActive(false);
    }

    public void OcultarContextualesPorPerdidaTracking()
    {
        if (panelHistoria != null && panelHistoria.activeSelf) panelHistoria.SetActive(false);
        if (panelReto != null && panelReto.activeSelf) panelReto.SetActive(false);
        if (panelDefensaDetalle != null && panelDefensaDetalle.activeSelf) panelDefensaDetalle.SetActive(false);
    }

    public void MostrarFeedback(string mensaje)
    {
        MostrarAvisoOrden(mensaje);
    }
}
