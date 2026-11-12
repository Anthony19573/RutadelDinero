using System;
using UnityEngine;
using UnityEngine.UI;

public class ChallengeManager : MonoBehaviour
{
    public static ChallengeManager I { get; private set; }

    [System.Serializable]
    public struct DatosParada
    {
        public string tituloParada;
        [TextArea(3, 6)]
        public string narracion;
        [TextArea(2, 5)]
        public string analisisTecnico;
        [TextArea(2, 4)]
        public string preguntaReto;
        public string[] opciones;
        public int opcionCorrecta;
        public string fragmentoCodigo;
        [TextArea(2, 4)]
        public string exitoYPista;
        [TextArea(2, 5)]
        public string recomendacionDefensa;
    }

    [Header("Base de Datos Narrativa de las 5 Paradas")]
    public DatosParada[] paradas;

    [Header("Referencias a UI")]
    public Text textoPregunta;
    public Button[] botonesOpciones;
    public GameObject panelReto;

    [Header("Grafo / Mapa de Ruta")]
    public GraphMap graphMap;

    private int stopIndexActual = -1;

    public int StopIndexActual => stopIndexActual;

    private void Awake()
    {
        if (I != null && I != this)
        {
            Destroy(gameObject);
            return;
        }

        I = this;

        InicializarDatosSiEstanVacios();
    }

    public void InicializarDatosSiEstanVacios()
    {
        if (paradas != null && paradas.Length == 5) return;

        paradas = new DatosParada[5];

        // Parada 0: Banco Andino
        paradas[0] = new DatosParada
        {
            tituloParada = "PARADA 1: BANCO ANDINO (4 feb 2016)",
            narracion = "29 de enero de 2015: 16 empleados reciben un correo de Rasel Ahlam con un falso currículum. Tras un año de espionaje en una red plana con switches de $10, el 4 de febrero de 2016 a las 20:36 los atacantes usan credenciales robadas en la terminal SWIFT, alteran 2 bytes del software para anular validaciones y apagan la impresora.",
            analisisTecnico = "Vulnerabilidad: Infiltración por Spear Phishing, falta de segmentación de red, ausencia de monitoreo de integridad de archivos (FIM) y falta de autenticación multifactor (MFA) en terminales críticas.",
            preguntaReto = "¿Qué vector de ataque inicial usó Lazarus con el correo de Rasel Ahlam?",
            opciones = new string[] { "Inyección SQL", "Phishing con troyano en archivo .zip", "Ataque DDoS a SWIFT" },
            opcionCorrecta = 1,
            fragmentoCodigo = "[SWIFT-01]",
            exitoYPista = "¡Fragmento [SWIFT-01] obtenido! Las órdenes salieron hacia Nueva York. DIRÍGETE AL MARCADOR 2: BANCO CORRESPONSAL.",
            recomendacionDefensa = "CONTROL PREVENTIVO (ORIGEN): Implementar Sandbox de correo, segmentación de red con Firewalls internos aislando SWIFT, monitoreo FIM sobre ejecutables y MFA obligatorio."
        };

        // Parada 1: Banco Corresponsal
        paradas[1] = new DatosParada
        {
            tituloParada = "PARADA 2: BANCO CORRESPONSAL (4-5 feb 2016)",
            narracion = "Las 36 órdenes por $951M llegan a la Reserva Federal de NY. 30 órdenes ($850M) se congelan automáticamente porque la dirección de la sucursal tenía la palabra JUPITER (nombre de un buque sancionado). Sin embargo, como en el Banco Andino era fin de semana y no había canal de emergencia 24/7, 4 órdenes por $81M hacia Manila fueron aprobadas.",
            analisisTecnico = "Vulnerabilidad: Ausencia de trazabilidad de grafo transaccional en tiempo real y falta de protocolo de verificación out-of-band 24/7 ante ráfagas de transferencias atípicas en días no laborables.",
            preguntaReto = "¿Qué detuvo las 30 órdenes por $850 millones en el Banco Corresponsal?",
            opciones = new string[] { "Coincidencia de la palabra JUPITER en lista de sanciones", "Límite de monto diario", "Bloqueo manual del operador" },
            opcionCorrecta = 0,
            fragmentoCodigo = "[FED-02]",
            exitoYPista = "¡Fragmento [FED-02] obtenido! $81 millones cruzaron sin ser detenidos. DIRÍGETE AL MARCADOR 3: CUENTAS FALSAS.",
            recomendacionDefensa = "PUNTO DE CORTE DEL GRAFO: Analizar las transacciones como un grafo en tiempo real habría detectado la anomalía (beneficiarios nuevos + horario nocturno + ráfaga) y retenido los $81M aquí mismo mediante confirmación 24/7."
        };

        // Parada 2: Cuentas Falsas
        paradas[2] = new DatosParada
        {
            tituloParada = "PARADA 3: CUENTAS FALSAS - AGENCIA LAS PALMERAS (5-9 feb 2016)",
            narracion = "En mayo de 2015 se abrieron 5 cuentas en dólares con identidades falsas y apenas $500 cada una. Estuvieron inactivas 9 meses. El viernes 5 de febrero reciben los $81 millones sin disparar alertas. Las cámaras de seguridad de la agencia dejan de grabar del 4 al 9 de febrero y se retiran $20M en una caja de cartón y luego $58M.",
            analisisTecnico = "Vulnerabilidad: Debilidad crítica en el proceso KYC (Conoce a tu Cliente), ausencia de reglas automáticas de bloqueo para cuentas dormidas que reciben montos millonarios y sabotaje de controles físicos (CCTV).",
            preguntaReto = "¿Qué falla de cumplimiento permitió usar cuentas creadas con nombres inexistentes?",
            opciones = new string[] { "Falla en el cifrado TLS", "Verificación KYC débil y sin alerta de cuenta dormida", "Clonación de tarjetas" },
            opcionCorrecta = 1,
            fragmentoCodigo = "[KYC-03]",
            exitoYPista = "¡Fragmento [KYC-03] obtenido! Los fondos fueron retirados de las cuentas puente. DIRÍGETE AL MARCADOR 4: CASA DE CAMBIO.",
            recomendacionDefensa = "CONTROL PREVENTIVO (DESTINO): Validación biométrica y documental KYC estricta, y retención automática (Hold) ante ingresos superiores al perfil histórico en cuentas inactivas."
        };

        // Parada 3: Casa de Cambio
        paradas[3] = new DatosParada
        {
            tituloParada = "PARADA 4: CASA DE CAMBIO (5-13 feb 2016)",
            narracion = "Para romper el rastro bancario antes de que lleguen las órdenes de congelamiento desde Bangladesh (retrasadas por el feriado de Año Nuevo Chino), los $81 millones se transfieren a una casa de cambio local, donde se convierten rápidamente de dólares a pesos filipinos en operaciones fraccionadas.",
            analisisTecnico = "Vulnerabilidad: Falta de detección de fraccionamiento (structuring/smurfing) y aislamiento de información entre entidades financieras locales e internacionales.",
            preguntaReto = "¿Con qué objetivo los atacantes pasaron los dólares por una casa de cambio antes del casino?",
            opciones = new string[] { "Pagar impuestos locales", "Romper la trazabilidad convirtiendo divisas y fraccionando montos", "Invertir en bolsa" },
            opcionCorrecta = 1,
            fragmentoCodigo = "[FX-04]",
            exitoYPista = "¡Fragmento [FX-04] obtenido! El dinero convertido va rumbo a su etapa final de lavado. DIRÍGETE AL MARCADOR 5: EL CASINO.",
            recomendacionDefensa = "CONTROL PREVENTIVO (CONVERSIÓN): Sistema automático de Reporte de Operaciones Sospechosas (ROS) ante conversiones masivas o fraccionadas del mismo origen y bloqueo preventivo."
        };

        // Parada 4: El Casino
        paradas[4] = new DatosParada
        {
            tituloParada = "PARADA 5: EL CASINO (Febrero-Marzo 2016)",
            narracion = "Los fondos entran a salas VIP de casinos en Manila (que en 2016 estaban exentos de la ley antilavado). Allí los operadores cambian el dinero por fichas y juegan al bacará apostando simultáneamente a la Banca y al Jugador: casi no ganan ni pierden, pero al retirar el dinero sale como ganancia legítima de casino. $66 millones se pierden para siempre.",
            analisisTecnico = "Vulnerabilidad: Vacío regulatorio AML (antilavado) en casinos y ausencia de trazabilidad de punta a punta entre la banca tradicional y sectores no financieros.",
            preguntaReto = "¿Qué técnica usaron en las mesas de bacará para lavar el dinero sin riesgo de perderlo?",
            opciones = new string[] { "Contar cartas", "Apuesta doble simultánea (Banca y Jugador)", "Hackear máquinas tragamonedas" },
            opcionCorrecta = 1,
            fragmentoCodigo = "[AML-05]",
            exitoYPista = "¡Fragmento [AML-05] obtenido! Has reconstruido las 5 paradas de la Ruta del Dinero.",
            recomendacionDefensa = "CONTROL PREVENTIVO (LAVADO): Inclusión obligatoria de casinos en la normativa antilavado (Ley RA 10927), prohibición de apuestas espejo y trazabilidad de origen de fondos."
        };
    }

    // Paso A: Inicia la secuencia abriendo el Panel de Contexto (PanelHistoria)
    public void IniciarSecuenciaParada(int stopIndex)
    {
        if (paradas == null || stopIndex < 0 || stopIndex >= paradas.Length)
        {
            Debug.LogWarning($"[ChallengeManager] Parada {stopIndex} fuera de rango.");
            return;
        }

        stopIndexActual = stopIndex;
        DatosParada datos = paradas[stopIndex];

        if (UIManager.I != null)
        {
            UIManager.I.MostrarHistoria(datos.tituloParada, datos.narracion, datos.analisisTecnico);
        }
    }

    // Paso B: Abrir PanelReto tras presionar 'Analizar y Resolver Reto' en PanelHistoria o botón del Holograma 3D
    public void AbrirRetoDirecto(int stopIndex)
    {
        stopIndexActual = stopIndex;
        AbrirRetoActual();
    }

    public void AbrirRetoActual()
    {
        if (stopIndexActual < 0 || stopIndexActual >= paradas.Length) return;

        DatosParada datos = paradas[stopIndexActual];

        if (textoPregunta != null)
        {
            textoPregunta.text = datos.preguntaReto;
        }

        if (botonesOpciones != null)
        {
            for (int i = 0; i < botonesOpciones.Length; i++)
            {
                if (botonesOpciones[i] != null)
                {
                    if (datos.opciones != null && i < datos.opciones.Length)
                    {
                        botonesOpciones[i].gameObject.SetActive(true);

                        Text txt = botonesOpciones[i].GetComponentInChildren<Text>();
                        if (txt != null) txt.text = datos.opciones[i];

                        int opcIndex = i;
                        botonesOpciones[i].onClick.RemoveAllListeners();
                        botonesOpciones[i].onClick.AddListener(() => ValidarRespuesta(opcIndex));
                    }
                    else
                    {
                        botonesOpciones[i].gameObject.SetActive(false);
                    }
                }
            }
        }

        if (UIManager.I != null)
        {
            UIManager.I.MostrarReto();
        }
        else if (panelReto != null)
        {
            panelReto.SetActive(true);
        }
    }

    // Paso C: Validar respuesta técnica (SOLO si coincide con la paradaActualEsperada)
    public void ValidarRespuesta(int seleccion)
    {
        if (stopIndexActual < 0 || stopIndexActual >= paradas.Length) return;

        // SOLO verificar que la parada actual sea igual a GameManager.I.paradaActualEsperada
        if (GameManager.I != null && stopIndexActual != GameManager.I.paradaActualEsperada)
        {
            Debug.LogWarning($"[ChallengeManager] Intento de validar parada {stopIndexActual} pero se esperaba {GameManager.I.paradaActualEsperada}");
            return;
        }

        DatosParada datos = paradas[stopIndexActual];

        if (seleccion == datos.opcionCorrecta)
        {
            if (AudioManager.I != null) AudioManager.I.PlaySuccess();

            if (UIManager.I != null)
            {
                UIManager.I.OcultarReto();
            }
            else if (panelReto != null)
            {
                panelReto.SetActive(false);
            }

            // Incrementa paradaActualEsperada ÚNICAMENTE UNA VEZ (+1) dentro de RegisterEvidence
            if (GameManager.I != null)
            {
                GameManager.I.RegisterEvidence(stopIndexActual, datos.tituloParada);
            }

            // Actualizar trazado en el grafo
            if (GraphMap.I != null)
            {
                GraphMap.I.UpdateGraph(stopIndexActual);
            }
            else if (graphMap != null)
            {
                graphMap.UpdateGraph(stopIndexActual);
            }

            // Mostrar el feedback limpio
            if (UIManager.I != null)
            {
                UIManager.I.MostrarFeedbackExito(datos.fragmentoCodigo, datos.analisisTecnico, datos.exitoYPista);
            }
        }
        else
        {
            if (AudioManager.I != null) AudioManager.I.PlayError();

            if (textoPregunta != null)
            {
                textoPregunta.text = $"<color=#FF5A4E><b>[ERROR DE PROTOCOLO: RESPUESTA INCORRECTA]</b></color>\n{datos.preguntaReto}";
            }
            Debug.Log($"[ChallengeManager] Opción {seleccion} incorrecta para la parada {stopIndexActual}.");
        }
    }

    // Al presionar 'CONTINUAR INVESTIGACIÓN' en PanelFeedback:
    // SOLO oculta PanelFeedback (y si paradaActualEsperada >= 5, abre PanelFinalAtaque).
    // NUNCA incrementa paradaActualEsperada ni vuelve a llamar a RegisterEvidence.
    public void ContinuarDesdeFeedback()
    {
        if (UIManager.I != null)
        {
            UIManager.I.OcultarFeedback();
        }

        if (GameManager.I != null && GameManager.I.paradaActualEsperada >= 5)
        {
            if (UIManager.I != null)
            {
                UIManager.I.MostrarFinalAtaque();
            }
        }

        stopIndexActual = -1;
    }

    public void OcultarPanel()
    {
        if (panelReto != null) panelReto.SetActive(false);
    }
}
