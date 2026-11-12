using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class GraphMap : MonoBehaviour
{
    public static GraphMap I { get; private set; }

    [Header("Nodos del Grafo (Canvas / UI)")]
    [Tooltip("Posiciones de las paradas en el Canvas o mundo para trazar la ruta")]
    public Transform[] nodePositionsUI;

    [Header("Colores de la Ruta")]
    public Color attackColor = Color.red;
    public Color defenseColor = Color.green;

    private LineRenderer lineRenderer;

    private void Awake()
    {
        if (I != null && I != this)
        {
            Destroy(gameObject);
            return;
        }

        I = this;

        lineRenderer = GetComponent<LineRenderer>();
        ConfigureLineRenderer();
    }

    private void ConfigureLineRenderer()
    {
        if (lineRenderer == null) return;

        lineRenderer.useWorldSpace = true;
        SetLineColor(attackColor);
    }

    public void UpdateGraph(int currentStopIndex)
    {
        if (lineRenderer == null)
            lineRenderer = GetComponent<LineRenderer>();

        if (nodePositionsUI == null || nodePositionsUI.Length == 0)
        {
            Debug.LogWarning("[GraphMap] nodePositionsUI no está configurado.");
            return;
        }

        if (currentStopIndex < 0)
        {
            lineRenderer.positionCount = 0;
            return;
        }

        int targetCount = Mathf.Min(currentStopIndex + 1, nodePositionsUI.Length);
        lineRenderer.positionCount = targetCount;

        for (int i = 0; i < targetCount; i++)
        {
            if (nodePositionsUI[i] != null)
            {
                lineRenderer.SetPosition(i, nodePositionsUI[i].position);
            }
        }

        Debug.Log($"[GraphMap] Grafo actualizado hasta la parada {currentStopIndex}. Puntos en línea: {targetCount}");
    }

    public void ApplyDefenseTrace()
    {
        if (lineRenderer == null)
            lineRenderer = GetComponent<LineRenderer>();

        SetLineColor(defenseColor);

        // Acortar la línea trazada solo hasta el nodo 1 (Banco Corresponsal)
        if (nodePositionsUI != null && nodePositionsUI.Length >= 2 &&
            nodePositionsUI[0] != null && nodePositionsUI[1] != null)
        {
            lineRenderer.positionCount = 2;
            lineRenderer.SetPosition(0, nodePositionsUI[0].position);
            lineRenderer.SetPosition(1, nodePositionsUI[1].position);
            Debug.Log("[GraphMap] Trazabilidad defensiva aplicada: Ruta detenida en la 2.ª parada (nodo 1: Banco Corresponsal).");
        }
        else
        {
            Debug.LogWarning("[GraphMap] No hay suficientes nodos configurados para trazar hasta el nodo 1.");
        }
    }

    private void SetLineColor(Color color)
    {
        if (lineRenderer == null) return;

        lineRenderer.startColor = color;
        lineRenderer.endColor = color;

        if (lineRenderer.material != null)
        {
            lineRenderer.material.color = color;
        }
    }
}
