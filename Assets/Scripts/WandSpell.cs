using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class WandSpell : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private Transform firePoint;
    [SerializeField] private float maxDistance = 50f;
    [SerializeField] private LayerMask collisionLayers;
    [SerializeField] private float trailDuration = 0.1f;

    [Header("Cooldown Settings")]
    [SerializeField] private float fireRate = 0.5f; 
    private float nextFireTime = 0f; 

    [Header("Recoil Settings")]
    [SerializeField] private float recoilForce = 5f;
    private Rigidbody2D playerRb; 

    [Header("Visuals")]
    [SerializeField] private LineRenderer lineRenderer;

    [Header("Glow Config (HDR Override)")]
    [ColorUsage(true, true)] 
    [SerializeField] private Color rayColor = Color.cyan; 
    [SerializeField] private float glowIntensity = 3f; 

    [Header("Juice Effects")]
    [SerializeField] private GameObject impactParticlesPrefab; // Chispas en la pared
    [SerializeField] private ParticleSystem floorDustParticles; // ¡Polvo de los pies!
    [SerializeField] private float groundCheckDistance = 0.2f;  // Qué tan cerca debe estar el piso

    // --- NUEVO: CONFIGURACIÓN DE AUDIO ---
    [Header("Audio Settings")]
    [SerializeField] private AudioSource audioSource;   // El componente emisor en la varita
    [SerializeField] private AudioClip sonidoDisparo;   // Arrastra aquí el SFX de la varita

    // --- REFERENCIA AL SISTEMA DE LAPILOSHARDS ---
    private PlayerMagicInventory magicInventory;
    private Camera cam;

    void Start()
    {
        cam = Camera.main;
        playerRb = GetComponentInParent<Rigidbody2D>();

        // Buscamos el inventario mágico global en el objeto principal de Adira
        magicInventory = GetComponentInParent<PlayerMagicInventory>();

        if (lineRenderer == null) lineRenderer = GetComponent<LineRenderer>();
        
        if (lineRenderer != null)
        {
            lineRenderer.positionCount = 0; 
            ApplyHdrColorToLine();
        }

        // CONTROL DE AUDIO DE SEGURIDAD: Intentamos buscar un componente si se olvidó asignar
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        // 1. Buscamos el SpellManager en el objeto para revisar si la UI está cambiando de hechizo
        SpellManager manager = GetComponentInParent<SpellManager>();
        
        if (manager != null)
        {
            // Buscamos el carrusel a través del manager
            SpellCarouselUI carousel = Object.FindAnyObjectByType<SpellCarouselUI>();
            
            // ¡CANDADO DE DISPARO!: Si la UI está animando, salimos del Update inmediatamente
            if (carousel != null && carousel.IsAnimating) return;
        }

        // 2. Si no está animando, procesamos el intento de disparo con Click Izquierdo
        if (Mouse.current.leftButton.wasPressedThisFrame && Time.time >= nextFireTime)
        {
            IntentarDispararRayo();
        }
    }

    // --- CONTROLADOR DE MUNICIÓN GLOBAL ---
    void IntentarDispararRayo()
    {
        if (magicInventory != null)
        {
            // Verificamos si Adira cuenta con LapiloShards en su inventario
            if (!magicInventory.PuedeDisparar())
            {
                Debug.LogWarning("¡Sin LapiloShards! No puedes disparar el rayo de la varita.");
                return;
            }

            // Si hay munición, ejecuta la lógica del disparo
            ShootHitscan();

            // --- NUEVO: REPRODUCIR AUDIO DE DISPARO ---
            ReproducirAudioVarita();

            // Descontamos una carga del contador global
            magicInventory.ConsumirUso();

            // Aplicamos el cooldown del disparo de forma normal
            nextFireTime = Time.time + fireRate;
        }
        else
        {
            // Respaldo de seguridad: Si no se encuentra el inventario, dispara normalmente para no romper el juego
            ShootHitscan();
            ReproducirAudioVarita();
            nextFireTime = Time.time + fireRate;
        }
    }

    // Función auxiliar para activar de manera limpia el sonido
    void ReproducirAudioVarita()
    {
        if (audioSource != null && sonidoDisparo != null)
        {
            // PlayOneShot evita cortar el sonido anterior si el fireRate es muy rápido
            audioSource.PlayOneShot(sonidoDisparo, 0.2f);
        }
    }

    void ShootHitscan()
    {
        Vector2 mousePos = cam.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        Vector2 direction = (mousePos - (Vector2)firePoint.position).normalized;

        // --- LÓGICA DE RETROCESO ---
        if (playerRb != null)
        {
            playerRb.AddForce(-direction * recoilForce, ForceMode2D.Impulse);

            // Activar polvo si está en el suelo
            if (IsGrounded())
            {
                TriggerFloorDust();
            }
        }

        // --- RE-APLICACIÓN DE COLOR (Glow Override) ---
        if (lineRenderer != null)
        {
            ApplyHdrColorToLine();
        }

        // --- LÓGICA DEL RAYO Y COLISIÓN ---
        RaycastHit2D hit = Physics2D.Raycast(firePoint.position, direction, maxDistance, collisionLayers);
        Vector2 endPoint = hit.collider != null ? hit.point : (Vector2)firePoint.position + (direction * maxDistance);

        if (hit.collider != null && impactParticlesPrefab != null)
        {
            GameObject particles = Instantiate(impactParticlesPrefab, hit.point, Quaternion.LookRotation(hit.normal));
            Destroy(particles, 1f);
        }

        StartCoroutine(ShowTrail(firePoint.position, endPoint));
    }

    // Función que lanza un pequeño rayo hacia abajo desde el jugador para ver si pisa suelo
    bool IsGrounded()
    {
        if (playerRb == null) return false;
        
        // Lanzamos el rayo desde la posición del Rigidbody (centro del jugador) hacia abajo
        RaycastHit2D hit = Physics2D.Raycast(playerRb.position, Vector2.down, groundCheckDistance, collisionLayers);
        
        return hit.collider != null;
    }

    // Activa la ráfaga de polvo
    void TriggerFloorDust()
    {
        if (floorDustParticles != null)
        {
            floorDustParticles.Play();
        }
    }

    IEnumerator ShowTrail(Vector3 start, Vector3 end)
    {
        if (lineRenderer == null) yield break;

        lineRenderer.positionCount = 2;
        lineRenderer.SetPosition(0, start);
        lineRenderer.SetPosition(1, end);
        yield return new WaitForSeconds(trailDuration);
        lineRenderer.positionCount = 0;
    }

    void ApplyHdrColorToLine()
    {
        Color hdrColor = rayColor * Mathf.Pow(2f, glowIntensity);
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] { new GradientColorKey(hdrColor, 0.0f), new GradientColorKey(hdrColor, 1.0f) },
            new GradientAlphaKey[] { new GradientAlphaKey(1.0f, 0.0f), new GradientAlphaKey(1.0f, 1.0f) }
        );
        lineRenderer.colorGradient = gradient;
    }
}