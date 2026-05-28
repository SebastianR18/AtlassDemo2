using UnityEngine;
using TMPro;

public class PlayerMagicInventory : MonoBehaviour
{
    [Header("UI Canvas")]
    [SerializeField] private TextMeshProUGUI textoContadorUI; // El texto de TextMeshPro del Canvas

    [Header("Sistema de LapiloShards Global")]
    [SerializeField] private int usosMaximos = 5;
    private int usosActuales;

    void Start()
    {
        usosActuales = usosMaximos;
        ActualizarInterfaz();
    }

    // Función para que los hechizos comprueben si Adira tiene munición
    public bool PuedeDisparar()
    {
        return usosActuales > 0;
    }

    // Función para restar una carga tras un disparo exitoso
    public void ConsumirUso()
    {
        if (usosActuales > 0)
        {
            usosActuales--;
            ActualizarInterfaz();
        }
    }

    // Función pública para que los LapiloShards sumen +1
    public void RecargarUso()
    {
        if (usosActuales < usosMaximos)
        {
            usosActuales++;
            ActualizarInterfaz();
            Debug.Log($"¡LapiloShard obtenido! Cargas globales: {usosActuales}");
        }
    }

    void ActualizarInterfaz()
    {
        if (textoContadorUI != null)
        {
            textoContadorUI.text = usosActuales.ToString();
        }
    }
}