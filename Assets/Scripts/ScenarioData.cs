using UnityEngine;

[CreateAssetMenu(fileName = "ScenarioData", menuName = "AR/ScenarioData", order = 1)]
public class ScenarioData : ScriptableObject
{
    [Header("Configuración del Escenario")]
    public string nombreEscenario = "La ruta del dinero";

    public string[] nombresParadas = new string[]
    {
        "Banco Andino",
        "Banco Corresponsal",
        "Cuentas falsas",
        "Casa de cambio",
        "Casino"
    };

    public string[] fragmentosCodigo;
}
