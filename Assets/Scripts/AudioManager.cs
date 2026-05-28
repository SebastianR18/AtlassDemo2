using UnityEngine;

public class AudioManager : MonoBehaviour
{
    // Instancia estática para que cualquier script pueda acceder al Audio Manager
    public static AudioManager instancia;

    [Header("Configuración de Audio")]
    [SerializeField] private AudioClip musicaFondo;
    [Range(0f, 1f)] [SerializeField] private float volumenDefault = 0.5f;

    private AudioSource audioSource;

    private void Awake()
    {
        // Aplicamos el patrón Singleton: si ya existe un AudioManager, destruimos el duplicado
        if (instancia == null)
        {
            instancia = this;
            // ¡El secreto! Este objeto sobrevivirá a la recarga de escenas
            DontDestroyOnLoad(gameObject); 
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Configuramos e iniciamos el componente de Audio
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        ConfigurarYReproducirMusica();
    }

    private void ConfigurarYReproducirMusica()
    {
        if (musicaFondo != null)
        {
            audioSource.clip = musicaFondo;
            audioSource.loop = true;          // Música infinita
            audioSource.volume = volumenDefault;
            audioSource.playOnAwake = true;
            audioSource.Play();
        }
        else
        {
            Debug.LogWarning("¡Falta asignar el archivo de música en el AudioManager!");
        }
    }

    // Función útil por si más adelante quieres cambiar el volumen desde opciones
    public void CambiarVolumen(float nuevoVolumen)
    {
        if (audioSource != null)
        {
            audioSource.volume = Mathf.Clamp01(nuevoVolumen);
        }
    }
}