using UnityEngine;

public class LapiloCollectible : MonoBehaviour
{
    [Header("Screen Shake Settings")]
    [SerializeField] private float shakeDuration = 0.5f;
    [SerializeField] private float shakeMagnitude = 0.1f;

    // --- NUEVO: CONFIGURACIÓN DE AUDIO ---
    [Header("Audio Settings")]
    [SerializeField] private AudioClip sonidoRecoleccion; // Arrastra aquí el SFX del Lapilo

    private bool yaRecogido = false;

    void OnTriggerEnter2D(Collider2D other)
    {
        // Verificamos si es Adira quien tocó el Lapilo y si no se ha recogido ya
        if (other.CompareTag("Player") && !yaRecogido)
        {
            yaRecogido = true;

            // --- NUEVO: REPRODUCIR SONIDO DE RECOLECCIÓN ---
            // Usamos PlayClipAtPoint para que el sonido no se corte al destruir el objeto
            if (sonidoRecoleccion != null)
            {
                AudioSource.PlayClipAtPoint(sonidoRecoleccion, transform.position, 1.5f); // 1.0f es el volumen máximo
            }

            // 1. Buscamos el script de movimiento/control de Adira
            PlayerMovement player = other.GetComponent<PlayerMovement>();
            if (player != null)
            {
                player.IniciarAscensionLapilo();
            }

            // 2. Efecto de temblor de pantalla lento (Opcional si usas Cinemachine o tu propia cámara)
            // Puedes llamar aquí a tu sistema de CameraShake habitual

            // 3. El Lapilo desaparece visualmente
            Destroy(gameObject);
        }
    }
}