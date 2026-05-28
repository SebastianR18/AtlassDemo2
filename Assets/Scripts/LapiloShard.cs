using UnityEngine;

public class LapiloShard : MonoBehaviour
{
    // --- NUEVO: CONFIGURACIÓN DE AUDIO ---
    [Header("Audio Settings")]
    [SerializeField] private AudioClip sonidoRecoleccion; // Arrastra aquí el SFX del Shard (Cristal)

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // Buscamos el inventario mágico global en Adira
            PlayerMagicInventory inventory = collision.GetComponent<PlayerMagicInventory>();

            if (inventory != null)
            {
                // --- NUEVO: REPRODUCIR SONIDO DE RECOLECCIÓN ---
                // Se genera un emisor temporal para que el sonido termine de sonar tras el Destroy
                if (sonidoRecoleccion != null)
                {
                    AudioSource.PlayClipAtPoint(sonidoRecoleccion, transform.position, 1.5f); // 1.0f es el volumen máximo
                }

                inventory.RecargarUso();
                Destroy(gameObject); // Destruye el cristal recolectado
            }
        }
    }
}