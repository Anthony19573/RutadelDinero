using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager I { get; private set; }

    public enum SimMode
    {
        Ataque,
        Defensa
    }

    [Header("Referencias")]
    [SerializeField] private ScenarioData scenarioData;
    public GameObject botonDefensa;

    [Header("Estado de la Simulación")]
    [SerializeField] private SimMode modoActual = SimMode.Ataque;

    [Header("Control de Flujo Lineal")]
    [Tooltip("Índice de la parada que debe investigarse actualmente (0 a 4)")]
    public int paradaActualEsperada = 0;

    // HashSet para garantizar que ninguna parada se resuelva o incremente más de una vez
    private HashSet<int> paradasResueltas = new HashSet<int>();
    private List<string> evidenciasEncontradas = new List<string>();
    private List<string> defensasAplicadas = new List<string>();

    private const string PREFS_PARADA_ESPERADA = "ParadaActualEsperada";
    private const string PREFS_SIM_MODE = "SimMode_RutaAR";

    public ScenarioData ScenarioData => scenarioData;
    public ScenarioData data => scenarioData;
    public SimMode ModoActual => modoActual;
    public IReadOnlyList<string> EvidenciasEncontradas => evidenciasEncontradas;
    public IReadOnlyList<string> DefensasAplicadas => defensasAplicadas;
    public int ParadasResueltasCount => paradasResueltas.Count;
    public int ParadasResueltas => paradasResueltas.Count;

    private void Awake()
    {
        if (I != null && I != this)
        {
            Destroy(gameObject);
            return;
        }

        I = this;

#if UNITY_EDITOR
        // Temporalmente limpiar PlayerPrefs en el Editor para iniciar siempre limpio en Parada 0
        PlayerPrefs.DeleteAll();
        ResetearProgreso();
#else
        CargarProgreso();
#endif
    }

    private void Start()
    {
        ActualizarEstadoBotonDefensa();

        if (UIManager.I != null)
        {
            UIManager.I.ActualizarTopHUD(modoActual, paradaActualEsperada, paradasResueltas.Count);
        }
    }

    public void CargarProgreso()
    {
        paradasResueltas.Clear();
        paradaActualEsperada = PlayerPrefs.GetInt(PREFS_PARADA_ESPERADA, 0);
        int modoGuardado = PlayerPrefs.GetInt(PREFS_SIM_MODE, 0);
        modoActual = (SimMode)modoGuardado;

        for (int i = 0; i < paradaActualEsperada; i++)
        {
            paradasResueltas.Add(i);
        }

        Debug.Log($"[GameManager] Progreso cargado de PlayerPrefs: ParadaEsperada={paradaActualEsperada}, Modo={modoActual}");
    }

    public void GuardarProgreso()
    {
        PlayerPrefs.SetInt(PREFS_PARADA_ESPERADA, paradaActualEsperada);
        PlayerPrefs.SetInt(PREFS_SIM_MODE, (int)modoActual);
        PlayerPrefs.Save();
        Debug.Log($"[GameManager] Progreso guardado: ParadaEsperada={paradaActualEsperada}, Modo={modoActual}");
    }

    public void ResetProgress()
    {
        ResetearProgreso();
    }

    public void ResetearProgreso()
    {
        PlayerPrefs.DeleteKey(PREFS_PARADA_ESPERADA);
        PlayerPrefs.DeleteKey(PREFS_SIM_MODE);
        PlayerPrefs.Save();

        paradaActualEsperada = 0;
        paradasResueltas.Clear();
        evidenciasEncontradas.Clear();
        defensasAplicadas.Clear();
        modoActual = SimMode.Ataque;

        ActualizarEstadoBotonDefensa();

        if (UIManager.I != null)
        {
            UIManager.I.ActualizarTopHUD(modoActual, 0, 0);
        }

        Debug.Log("[GameManager] Progreso reseteado a valores iniciales (Parada 0).");
    }

    public void ActualizarEstadoBotonDefensa()
    {
        if (botonDefensa != null)
        {
            // Solo se activa cuando el jugador completa las 5 paradas o ya está en modo defensa
            bool habilitar = (paradasResueltas.Count >= 5 || paradaActualEsperada >= 5 || modoActual == SimMode.Defensa);
            botonDefensa.SetActive(habilitar);
        }
    }

    // Registra la evidencia y garantiza el incremento ÚNICO (+1) de la parada
    public void RegisterEvidence(int paradaIndex, string id)
    {
        if (!paradasResueltas.Contains(paradaIndex))
        {
            paradasResueltas.Add(paradaIndex);

            if (!evidenciasEncontradas.Contains(id))
            {
                evidenciasEncontradas.Add(id);
            }

            // Incrementa paradaActualEsperada ÚNICAMENTE UNA VEZ aquí
            paradaActualEsperada = Mathf.Clamp(paradasResueltas.Count, 0, 5);
            GuardarProgreso();
            ActualizarEstadoBotonDefensa();

            if (UIManager.I != null)
            {
                UIManager.I.ActualizarTopHUD(modoActual, paradaActualEsperada, paradasResueltas.Count);
            }

            Debug.Log($"[GameManager] Parada superada: {id} (Índice {paradaIndex}). Progreso único: {paradaActualEsperada}/5");
        }
        else
        {
            Debug.Log($"[GameManager] Parada {paradaIndex} ya estaba en paradasResueltas. Se ignora re-incremento.");
        }
    }

    public void RegisterEvidence(string id)
    {
        RegisterEvidence(paradaActualEsperada, id);
    }

    public void ActivarDefensaTrazabilidad()
    {
        ApplyDefense("Trazabilidad");
    }

    public void ApplyDefense(string id)
    {
        if (!defensasAplicadas.Contains(id))
        {
            defensasAplicadas.Add(id);
        }

        modoActual = SimMode.Defensa;
        GuardarProgreso();

        Debug.Log("Trazabilidad activada. Analizando transacciones como grafo");

        ActualizarEstadoBotonDefensa();

        if (GraphMap.I != null)
        {
            GraphMap.I.ApplyDefenseTrace();
        }

        if (AudioManager.I != null)
        {
            AudioManager.I.PlayDefense();
        }

        if (UIManager.I != null)
        {
            UIManager.I.ActualizarTopHUD(modoActual, paradaActualEsperada, paradasResueltas.Count);
            UIManager.I.MostrarInfografiaDefensa();
        }
    }

    public void EndScenario()
    {
        if (modoActual == SimMode.Ataque)
        {
            Debug.Log("El dinero recorre las 5 paradas y se pierde");
        }
        else if (modoActual == SimMode.Defensa)
        {
            Debug.Log("El rastreo detiene el flujo en la 2.ª parada");
        }
    }
}
