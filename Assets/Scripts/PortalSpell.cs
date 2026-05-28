using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class PortalSpell : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform firePoint;
    [SerializeField] private GameObject portalPrefab;
    [SerializeField] private LayerMask portalWallLayer;
    [SerializeField] private LineRenderer lineRenderer;

    [Header("Rango del Hechizo")]
    [SerializeField] private float rangoHechizo = 15f; 

    [Header("Colores del Rayo (Portales)")]
    [SerializeField] private Color colorRayoA = Color.orange;
    [SerializeField] private Color colorRayoB = Color.blue;

    // --- NUEVO: CONFIGURACIÓN DE AUDIO DISPAROS ---
    [Header("Audio Settings")]
    [SerializeField] private AudioSource audioSource;       // El componente que reproduce el sonido
    [SerializeField] private AudioClip sonidoPortalIzquierdo; // SFX para Portal A (Clic Izquierdo)
    [SerializeField] private AudioClip sonidoPortalDerecho;   // SFX para Portal B (Clic Derecho)

    private PlayerMagicInventory magicInventory; 
    private GameObject portalA;
    private GameObject portalB;
    private Camera cam;

    void Start()
    {
        cam = Camera.main;
        magicInventory = GetComponentInParent<PlayerMagicInventory>();

        // Si olvidaste asignar el AudioSource en el Inspector, intentamos buscar uno en el objeto
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        SpellCarouselUI carousel = Object.FindAnyObjectByType<SpellCarouselUI>();
        if (carousel != null && carousel.IsAnimating) return;

        if (Mouse.current.leftButton.wasPressedThisFrame) IntentarDisparar(true);
        if (Mouse.current.rightButton.wasPressedThisFrame) IntentarDisparar(false);
    }

    void IntentarDisparar(bool esPortalA)
    {
        if (magicInventory == null) return;

        if (!magicInventory.PuedeDisparar())
        {
            Debug.LogWarning("¡Sin LapiloShards para el Portal!");
            return;
        }

        FirePortalRay(esPortalA);
        
        // --- NUEVO: REPRODUCIR SONIDO SEGÚN EL PORTAL ---
        ReproducirSonidoDisparo(esPortalA);

        magicInventory.ConsumirUso();
    }

    // Función auxiliar para activar el clip correcto
    void ReproducirSonidoDisparo(bool esPortalA)
    {
        if (audioSource == null) return;

        // El segundo parámetro (0.3f) regula el volumen del clip. ¡Ajústalo a tu gusto!
        if (esPortalA && sonidoPortalIzquierdo != null)
        {
            audioSource.PlayOneShot(sonidoPortalIzquierdo, 0.15f); 
        }
        else if (!esPortalA && sonidoPortalDerecho != null)
        {
            audioSource.PlayOneShot(sonidoPortalDerecho, 0.15f);
        }
    }

    void FirePortalRay(bool esPortalA)
    {
        Vector2 mousePos = cam.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        Vector2 direction = (mousePos - (Vector2)firePoint.position).normalized;

        Color colorActual = esPortalA ? colorRayoA : colorRayoB;
        ConfigurarColorRayo(colorActual);

        RaycastHit2D hit = Physics2D.Raycast(firePoint.position, direction, rangoHechizo, portalWallLayer);

        if (hit.collider != null)
        {
            float unPoquitoAfuera = 0.1f; 
            Vector2 posicionCorregida = hit.point + (hit.normal * unPoquitoAfuera);
            SpawnPortal(posicionCorregida, hit.normal, esPortalA);
            StartCoroutine(ShowTrail(firePoint.position, hit.point));
        }
        else
        {
            Vector2 puntoMaximoRango = (Vector2)firePoint.position + (direction * rangoHechizo);
            StartCoroutine(ShowTrail(firePoint.position, puntoMaximoRango));
        }
    }

    void SpawnPortal(Vector2 position, Vector2 normal, bool esPortalA)
    {
        Quaternion rotation = Quaternion.FromToRotation(Vector2.right, normal);
        if (esPortalA)
        {
            if (portalA == null) portalA = Instantiate(portalPrefab, position, rotation);
            else { portalA.transform.position = position; portalA.transform.rotation = rotation; }
            portalA.GetComponent<PortalLogic>().SetupPortal(true);
        }
        else
        {
            if (portalB == null) portalB = Instantiate(portalPrefab, position, rotation);
            else { portalB.transform.position = position; portalB.transform.rotation = rotation; }
            portalB.GetComponent<PortalLogic>().SetupPortal(false);
        }

        if (portalA != null && portalB != null)
        {
            portalA.GetComponent<PortalLogic>().receiver = portalB.transform;
            portalB.GetComponent<PortalLogic>().receiver = portalA.transform;
        }
    }

    void ConfigurarColorRayo(Color colorTarget)
    {
        if (lineRenderer == null) return;
        lineRenderer.startColor = colorTarget;
        lineRenderer.endColor = colorTarget;
    }

    IEnumerator ShowTrail(Vector3 start, Vector3 end)
    {
        if (lineRenderer == null) yield break;
        lineRenderer.positionCount = 2;
        lineRenderer.SetPosition(0, start);
        lineRenderer.SetPosition(1, end);
        yield return new WaitForSeconds(0.05f);
        lineRenderer.positionCount = 0;
        lineRenderer.startColor = Color.white;
        lineRenderer.endColor = Color.white;
    }
}