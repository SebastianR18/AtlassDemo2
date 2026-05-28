using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class KillZone : MonoBehaviour
{
    [Header("Audio Settings")]
    [SerializeField] private AudioClip sonidoMuerte; // El SFX de la explosión/derrota
    [Range(0f, 1f)]
    [SerializeField] private float volumenMuerte = 0.8f;

    [Header("Visual Effects")]
    [SerializeField] private GameObject particulasExplosionPrefab; // El Prefab del sistema de partículas de la explosión

    [Header("UI Telon Animation")]
    [SerializeField] private Animator telonAnimator; // Arrastra aquí el Animator del objeto "Telon"
    [SerializeField] private float tiempoCerradoTelon = 0.5f; // Cuánto tarda el telón en cerrarse por completo antes de reiniciar

    private bool yaMurio = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !yaMurio)
        {
            yaMurio = true;
            StartCoroutine(SecuenciaMuerteCo(collision.gameObject));
        }
    }

    private IEnumerator SecuenciaMuerteCo(GameObject player)
    {
        // 1. REPRODUCIR SONIDO
        if (sonidoMuerte != null)
        {
            AudioSource.PlayClipAtPoint(sonidoMuerte, player.transform.position, volumenMuerte);
        }

        // 2. APARECER PARTÍCULAS
        if (particulasExplosionPrefab != null)
        {
            GameObject explosion = Instantiate(particulasExplosionPrefab, player.transform.position, Quaternion.identity);
            Destroy(explosion, 2f);
        }

        // 3. OCULTAR A ADIRA Y CONGELARLA
        player.transform.localScale = Vector3.zero;

        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
        if (rb != null) rb.simulated = false;

        PlayerMovement movement = player.GetComponent<PlayerMovement>();
        if (movement != null) movement.enabled = false;

        // 4. --- NUEVO: ACTIVAR EL TELÓN NEGRO ---
        if (telonAnimator != null)
        {
            telonAnimator.SetTrigger("Cerrar");
        }

        // Esperamos exactamente el tiempo que tardan las paredes en juntarse en el centro
        yield return new WaitForSeconds(tiempoCerradoTelon);

        // 5. REINICIAR (Unity recargará la escena y todo volverá a su posición inicial limpia)
        string escenaActual = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(escenaActual);
    }
}