using UnityEngine;
using UnityEngine.InputSystem; // ¡IMPORTANTE! Necesitamos esta librería

public class WandRotation : MonoBehaviour
{
    private Camera cam;

    void Start()
    {
        cam = Camera.main;
    }

    void Update()
    {
        RotateWand();
    }

    void RotateWand()
    {
        // 1. Obtener la posición del mouse usando el NUEVO sistema
        Vector2 mousePixelPos = Mouse.current.position.ReadValue();

        // 2. Convertir a coordenadas del mundo
        // Usamos la distancia Z entre la cámara y el objeto para la conversión
        float distanceToCam = Mathf.Abs(cam.transform.position.z);
        Vector3 mouseWorldPos = cam.ScreenToWorldPoint(new Vector3(mousePixelPos.x, mousePixelPos.y, distanceToCam));

        // 3. Calcular dirección
        Vector3 direction = mouseWorldPos - transform.position;

        // 4. Calcular ángulo
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // 5. Aplicar la rotación
        transform.rotation = Quaternion.Euler(0, 0, angle - 90f);
    }
}