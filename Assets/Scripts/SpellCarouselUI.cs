using UnityEngine;
using System.Collections;

public class SpellCarouselUI : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private RectTransform spellA;
    [SerializeField] private RectTransform spellB;

    [Header("Positions")]
    [SerializeField] private Vector2 activePosition = new Vector2(0, 0);   
    [SerializeField] private Vector2 inactivePosition = new Vector2(0, 0); 

    [Header("Animation Settings")]
    [SerializeField] private float duration = 0.4f; 
    [SerializeField] private AnimationCurve sideMovementCurve = AnimationCurve.EaseInOut(0, 0, 1, 0);
    [SerializeField] private float maxLeftOffset = -60f; 
    [SerializeField] private AnimationCurve scaleCurve = AnimationCurve.Linear(0, 1, 1, 1);

    // --- NUEVO: CONFIGURACIÓN DE AUDIO DEL INTERFAZ ---
    [Header("Audio Settings")]
    [SerializeField] private AudioSource audioSource;         // Componente de audio en la UI
    [SerializeField] private AudioClip sonidoIntercambio;     // El SFX de cambio/deslizamiento
    [Range(0f, 1f)]
    [SerializeField] private float volumenIntercambio = 0.5f; // Volumen controlado del menú

    private bool isAActive = true;
    private bool isAnimating = false;

    // Propiedad pública para que el mánager revise el estado
    public bool IsAnimating => isAnimating;

    void Start()
    {
        isAnimating = false; // Forzar desbloqueo al arrancar el juego
        
        spellA.anchoredPosition = activePosition;
        spellB.anchoredPosition = inactivePosition;
        spellA.localScale = Vector3.one;
        spellB.localScale = Vector3.one;
        spellA.SetAsLastSibling(); 

        // Control de seguridad: Si no se asignó en el Inspector, busca el AudioSource en este objeto
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
    }

    // El SpellManager llamará a esto para iniciar el viaje visual
    public void CambiarHechizoVisual(int index)
    {
        // Si el índice es 0, el hechizo A debe estar al frente. Si es 1, el B debe estar al frente.
        bool colocarAAlFrente = (index == 0);

        // Si el estado visual ya coincide con lo que pide el mánager, no hacemos nada
        if (isAActive == colocarAAlFrente) return;

        isAActive = colocarAAlFrente;

        // Detenemos cualquier animación previa por si acaso y empezamos la nueva
        StopAllCoroutines();
        StartCoroutine(AnimateCarousel());
    }

    IEnumerator AnimateCarousel()
    {
        isAnimating = true;
        float elapsed = 0f;

        // --- NUEVO: REPRODUCIR AUDIO JUSTO AL INICIAR LA ANIMACIÓN ---
        if (audioSource != null && sonidoIntercambio != null)
        {
            audioSource.PlayOneShot(sonidoIntercambio, volumenIntercambio);
        }

        RectTransform goingBack = isAActive ? spellB : spellA;
        RectTransform comingFront = isAActive ? spellA : spellB;

        bool capasIntercambiadas = false;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            if (!capasIntercambiadas && t > 0.5f)
            {
                comingFront.SetAsLastSibling();
                capasIntercambiadas = true;
            }

            Vector2 linearPosBack = Vector2.Lerp(activePosition, inactivePosition, t);
            Vector2 linearPosFront = Vector2.Lerp(inactivePosition, activePosition, t);

            float leftOffset = sideMovementCurve.Evaluate(t) * maxLeftOffset;
            goingBack.anchoredPosition = new Vector2(linearPosBack.x + leftOffset, linearPosBack.y);
            comingFront.anchoredPosition = linearPosFront;

            float currentScale = scaleCurve.Evaluate(t);
            goingBack.localScale = new Vector3(currentScale, currentScale, 1f);
            comingFront.localScale = Vector3.one;

            yield return null;
        }

        // Ajustes finales exactos
        spellA.anchoredPosition = isAActive ? activePosition : inactivePosition;
        spellB.anchoredPosition = isAActive ? inactivePosition : activePosition;
        spellA.localScale = Vector3.one;
        spellB.localScale = Vector3.one;
        comingFront.SetAsLastSibling(); 

        // Liberamos el candado de forma segura
        isAnimating = false;
    }
}