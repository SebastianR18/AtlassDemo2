using UnityEngine;
using UnityEngine.InputSystem;

public class SpellManager : MonoBehaviour
{
    [Header("Spells Config")]
    public int currentSpellIndex = 0;
    private int totalSpells = 2; 

    private WandSpell raycastSpellScript;
    private PortalSpell portalSpellScript; 
    private SpellCarouselUI spellCarouselUI; 

    void Start()
    {
        spellCarouselUI = Object.FindAnyObjectByType<SpellCarouselUI>();

        if (spellCarouselUI == null)
        {
            Debug.LogWarning("SpellManager: No se encontró el script 'SpellCarouselUI' en la escena.");
        }

        raycastSpellScript = GetComponent<WandSpell>();
        portalSpellScript = GetComponent<PortalSpell>();

        if (raycastSpellScript == null) Debug.LogError("SpellManager: ¡Falta 'WandSpell' en la Varita!");
        if (portalSpellScript == null) Debug.LogError("SpellManager: ¡Falta 'PortalSpell' en la Varita!");

        // Control de seguridad inicial para el Inspector
        if (currentSpellIndex >= totalSpells || currentSpellIndex < 0)
        {
            currentSpellIndex = 0;
        }

        SelectSpell(currentSpellIndex);
    }

    void Update()
    {
        // Si la UI está animando, bloqueamos la entrada para que no salte de hechizo descontroladamente
        if (spellCarouselUI != null && spellCarouselUI.IsAnimating) return;

        float scrollInput = Mouse.current.scroll.ReadValue().y;

        if (Mathf.Abs(scrollInput) > 0.1f)
        {
            if (scrollInput > 0f) // Scroll hacia arriba
            {
                currentSpellIndex = (currentSpellIndex + 1) % totalSpells;
            }
            else // Scroll hacia abajo
            {
                currentSpellIndex--;
                if (currentSpellIndex < 0) currentSpellIndex = totalSpells - 1;
            }

            // Ejecutamos el cambio de mecánicas y le avisamos a la UI
            SelectSpell(currentSpellIndex);
        }
    }

    void SelectSpell(int index)
    {
        // 1. Intercambio inmediato de los scripts lógicos del gameplay
        if (raycastSpellScript != null) raycastSpellScript.enabled = (index == 0);
        if (portalSpellScript != null) portalSpellScript.enabled = (index == 1);

        // 2. Le ordenamos a la UI que empiece su animación de cartas
        if (spellCarouselUI != null)
        {
            spellCarouselUI.CambiarHechizoVisual(index);
        }

        Debug.Log($"SpellManager: Hechizo seleccionado -> Índice {index}");
    }
}