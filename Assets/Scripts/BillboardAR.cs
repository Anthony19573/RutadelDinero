using UnityEngine;

public class BillboardAR : MonoBehaviour
{
    [Tooltip("Cámara objetivo hacia la cual mirar. Si es nula, usa Camera.main")]
    public Camera targetCamera;

    [Tooltip("Velocidad de interpolación suave del giro")]
    public float smoothSpeed = 12f;

    [Tooltip("Mantener el eje Y completamente vertical")]
    public bool keepVertical = true;

    private void Start()
    {
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }
    }

    private void LateUpdate()
    {
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
            if (targetCamera == null) return;
        }

        Vector3 dirToCam = targetCamera.transform.position - transform.position;

        if (keepVertical)
        {
            dirToCam.y = 0f;
        }

        if (dirToCam.sqrMagnitude > 0.0001f)
        {
            // Para Canvas World Space, el frente del UI apunta en dirección opuesta a la cámara
            Quaternion targetRot = Quaternion.LookRotation(-dirToCam, Vector3.up);

            if (smoothSpeed > 0f)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * smoothSpeed);
            }
            else
            {
                transform.rotation = targetRot;
            }
        }
    }
}
