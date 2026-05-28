using UnityEngine;
using System.Collections;

public class TelonStartBehavior : MonoBehaviour
{
    private void Start()
    {
        StartCoroutine(ActivarAperturaPorTriggerCo());
    }

    private IEnumerator ActivarAperturaPorTriggerCo()
    {
        Animator anim = GetComponent<Animator>();
        if (anim != null)
        {
            // 1. Nos aseguramos de limpiar el trigger de cierre por si acaso
            anim.ResetTrigger("Cerrar");

            // 2. Esperamos un frame final para que Unity asiente la UI en la nueva escena
            yield return new WaitForEndOfFrame();

            // 3. ¡Fuego! Disparamos el trigger para que pase de Pantalla Negra a Abrirse
            anim.SetTrigger("Abrir");
        }
    }
}