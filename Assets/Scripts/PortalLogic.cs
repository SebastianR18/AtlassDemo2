using UnityEngine;

public class PortalLogic : MonoBehaviour
{
    public Transform receiver; // El otro portal
    public bool isPortalA;
    
    [Header("Visual Debug")]
    [SerializeField] private Color colorA = Color.orange;
    [SerializeField] private Color colorB = Color.blue;

    [Header("Referencias Visuales")]
    [SerializeField] private SpriteRenderer portalFrameRenderer; 
    [SerializeField] private ParticleSystem portalParticles; // Referencia al sistema de partículas

    [Header("Portal Calibration")]
    [SerializeField] private float compensacionPies = 0.25f; 
    [SerializeField] private float empujeSalida = 0.8f;

    // --- NUEVO: CONFIGURACIÓN DE AUDIO AJUSTABLE ---
    [Header("Audio Teletransporte")]
    [SerializeField] private AudioClip sonidoAtravesar; // Arrastra aquí el SFX de viaje en el Inspector
    [Range(0f, 2f)] 
    [SerializeField] private float volumenTeletransporte = 1.0f; // Slider en el inspector. Súbelo a 1.5 o 2.0 si suena bajo.

    private float cooldownTimer = 0f;

    public void SetupPortal(bool identity)
    {
        isPortalA = identity;
        Color colorAsignar = isPortalA ? colorA : colorB;

        // 1. Cambiar color del GIF animado
        if (portalFrameRenderer != null)
        {
            portalFrameRenderer.color = colorAsignar;
        }

        // 2. Cambiar color del Sistema de Partículas
        if (portalParticles != null)
        {
            var mainModule = portalParticles.main;
            mainModule.startColor = colorAsignar;
        }
        else
        {
            Debug.LogWarning($"¡Falta asignar PortalParticles en el Inspector de {gameObject.name}!");
        }
    }

    void Update()
    {
        if (cooldownTimer > 0f) cooldownTimer -= Time.deltaTime;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (receiver == null || cooldownTimer > 0f) return;

            Rigidbody2D playerRb = collision.GetComponent<Rigidbody2D>();
            if (playerRb != null)
            {
                // --- REPRODUCIR SONIDO AL ATRAVESAR (VOLUMEN CONTROLADO) ---
                if (sonidoAtravesar != null)
                {
                    // Ahora utiliza el valor dinámico del slider asignado en el Inspector
                    AudioSource.PlayClipAtPoint(sonidoAtravesar, transform.position, volumenTeletransporte); 
                }

                // Lógica de teletransportación física
                float currentSpeed = playerRb.linearVelocity.magnitude;
                Vector2 direccionSalida = receiver.right; 
                Vector2 ejeAlturaPortal = receiver.up;

                Vector2 posicionFinal = (Vector2)receiver.position + (direccionSalida * empujeSalida) - (ejeAlturaPortal * compensacionPies);
                collision.transform.position = posicionFinal;
                playerRb.linearVelocity = direccionSalida * currentSpeed;

                receiver.GetComponent<PortalLogic>().cooldownTimer = 0.2f;
            }
        }
    }
}