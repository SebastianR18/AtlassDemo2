using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement; // Requerido para recargar escenas

public class LevelReset : MonoBehaviour
{
    [Header("Configuración de Teclado")]
    // Tecla por defecto asignada ("r"). Puedes cambiarla en el Inspector por "escape", "space", etc.
    [SerializeField] private string teclaReinicio = "r"; 

    [Header("Configuración de Tiempo")]
    // Tiempo en segundos que el jugador debe mantener presionada la tecla
    [SerializeField] private float tiempoRequerido = 1.5f; 

    private float temporizadorHold = 0f;

    void Update()
    {
        // Usamos Keyboard.current de forma directa con la propiedad de la tecla.
        // Al usar 'wasPressedThisFrame' o evaluar el estado actual del botón,
        // necesitamos que Unity verifique si la tecla está presionada en este frame.
        var controlTecla = Keyboard.current?[teclaReinicio];

        if (controlTecla == null) return;

        // CORRECCIÓN: Para leer si un InputControl genérico está presionado, 
        // usamos '.IsPressed()' como un método, o leemos su valor flotante.
        if (controlTecla.IsPressed()) 
        {
            // Acumulamos el tiempo transcurrido
            temporizadorHold += Time.deltaTime;

            // Si el jugador superó el tiempo requerido manteniendo la tecla...
            if (temporizadorHold >= tiempoRequerido)
            {
                ReiniciarNivelActual();
            }
        }
        else
        {
            // Si el jugador suelta la tecla, el temporizador se restablece
            temporizadorHold = 0f;
        }
    }

    void ReiniciarNivelActual()
    {
        Debug.Log("¡Tecla mantenida el tiempo suficiente! Reiniciando nivel...");
        
        // Obtenemos el índice de la escena que está activa actualmente
        int escenaActual = SceneManager.GetActiveScene().buildIndex;
        
        // Solicitamos al gestor de escenas que la cargue de nuevo desde cero
        SceneManager.LoadScene(escenaActual);
    }
}