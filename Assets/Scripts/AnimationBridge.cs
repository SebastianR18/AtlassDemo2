using UnityEngine;

public class AnimationBridge : MonoBehaviour
{
    private PlayerMovement playerMovement;

    void Start()
    {
        // Buscamos el script de movimiento que está en nuestro objeto padre
        playerMovement = GetComponentInParent<PlayerMovement>();
    }

    // Esta función SÍ aparecerá en la lista del Animator de Adira
    public void EventoAscenderBolaDeLuz()
    {
        if (playerMovement != null)
        {
            // Le avisamos al padre que ya es hora de subir
            playerMovement.EventoAscenderBolaDeLuz();
        }
        else
        {
            Debug.LogError("AnimationBridge: ¡No se encontró el script PlayerMovement en el objeto Padre!");
        }
    }
}