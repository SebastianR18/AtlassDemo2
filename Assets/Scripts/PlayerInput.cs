using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] float speed = 5f;
    [SerializeField] float jumpForce = 12f;
    
    [Header("Detection Settings")]
    [SerializeField] LayerMask groundLayer; // Selecciona "Ground" en el Inspector
    [SerializeField] Transform groundCheck; // Crea un objeto vacío a los pies del jugador
    [SerializeField] float checkRadius = 0.2f;

    [Header("Input References")]
    [SerializeField] InputActionReference move;
    [SerializeField] InputActionReference jump;

    [Header("Visual References")]
    [SerializeField] private Transform characterVisuals; // Arrastra aquí a "AdiraMain" desde la Jerarquía
    [SerializeField] private GameObject wandObject;       // <--- NUEVO: Arrastra aquí el objeto de la Varita desde la Jerarquía

    [Header("Lapilo Ascension Config")]
    [SerializeField] private float velocidadAscenso = 15f; // Ajustado por defecto a una velocidad más rápida

    private Rigidbody2D rb;
    private Animator animator;
    private Vector2 moveDirection;
    private bool jumpRequested;
    private bool isFacingRight = true;
    private bool isGrounded;

    // Control lógico para la escena final del nivel
    private bool ascendiendo = false;
    private bool bloqueadoPorLapilo = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponentInChildren<Animator>();
    }

    void Update()
    {
        // Si ya recogimos el Lapilo, ignoramos cualquier input del teclado/mando
        if (bloqueadoPorLapilo)
        {
            moveDirection = Vector2.zero;
            return;
        }

        moveDirection = move.action.ReadValue<Vector2>();

        // Chequeo de suelo mediante un pequeño círculo invisible
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, checkRadius, groundLayer);

        if (jump.action.WasPressedThisFrame() && isGrounded) 
            jumpRequested = true;

        if (moveDirection.x > 0 && !isFacingRight) Flip();
        else if (moveDirection.x < -0.1f && isFacingRight) Flip();
    }

    private void FixedUpdate()
    {
        // CONTROL DE ASCENSO: Si el Evento de Animación ya se disparó, Adira solo sube
        if (ascendiendo)
        {
            rb.linearVelocity = new Vector2(0f, velocidadAscenso);
            return;
        }

        // Si está bloqueada esperando el frame de la animación, se queda quieta en su sitio
        if (bloqueadoPorLapilo)
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            return;
        }

        // 1. OBTENER VELOCIDAD OBJETIVO
        float targetSpeed = moveDirection.x * speed;

        // 2. CALCULAR DIFERENCIA (¿Qué tan rápido vamos vs qué tan rápido queremos ir?)
        float speedDif = targetSpeed - rb.linearVelocity.x;

        // 3. ACELERACIÓN DINÁMICA
        float accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? 10f : 5f; 

        // 4. APLICAR LA FUERZA
        float movement = speedDif * accelRate;
        rb.AddForce(Vector2.right * movement, ForceMode2D.Force);

        // 5. SALTO
        if (jumpRequested)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0); 
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            jumpRequested = false;
        }

        ActualizarAnimaciones();
    }

    private void Flip()
    {
        // Invertimos el estado de la dirección
        isFacingRight = !isFacingRight;
        
        // Rotamos el objeto PADRE 180 grados en el eje Y.
        // Esto hace que el CapsuleCollider2D del padre gire físicamente de forma simétrica.
        if (isFacingRight)
        {
            transform.rotation = Quaternion.Euler(0f, 0f, 0f); // Mirando a la derecha (Rotación normal)
        }
        else
        {
            transform.rotation = Quaternion.Euler(0f, 180f, 0f); // Mirando a la izquierda (Rotado en Y)
        }
    }

    private void ActualizarAnimaciones()
    {
        if (animator == null) return;

        bool isMoving = Mathf.Abs(moveDirection.x) > 0.1f;
        
        animator.SetBool("isWalking", isMoving);
        animator.SetBool("isIdle", !isMoving);
        animator.SetBool("isJump", !isGrounded);
    }

    // --- FUNCIONES DE EVENTO PARA EL LAPILO ---

    // Llamado de forma externa por el script del LapiloCollectible al colisionar
    public void IniciarAscensionLapilo()
    {
        bloqueadoPorLapilo = true;
        
        // Congelamos físicas iniciales para que no caiga ni se desplace por inercia
        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Kinematic; 
        
        // Desactivamos el collider para que no choque con plataformas flotantes mientras sube
        Collider2D playerCollider = GetComponent<Collider2D>();
        if (playerCollider != null) playerCollider.enabled = false;

        // --- OCULTAR LA VARITA ---
        if (wandObject != null)
        {
            wandObject.SetActive(false); // La varita se desactiva por completo al instante
        }

        // Apagamos el SpellManager si está en el mismo objeto para que no use magia
        MonoBehaviour spellManager = GetComponent<MonoBehaviour>(); 
        if (spellManager != null && spellManager.GetType().Name == "SpellManager")
        {
            spellManager.enabled = false;
        }

        // Disparamos la animación en el Animator de Adira
        if (animator != null)
        {
            animator.SetTrigger("AdiraBall");
        }
    }

    // FUNCIÓN DE EVENTO DE ANIMACIÓN: Colócala en el frame exacto de "AdiraBall" o "AdiraFlyLoop"
    public void EventoAscenderBolaDeLuz()
    {
        Debug.Log("¡Frame de ascenso alcanzado! Adira comienza a subir rápidamente.");
        ascendiendo = true;
        
        // Iniciamos la carga del próximo nivel en segundo plano
        StartCoroutine(CambiarDeNivelCo());
    }

    private IEnumerator CambiarDeNivelCo()
    {
        // Reducido a 1.2 segundos para acoplarse mejor al ritmo de la subida rápida
        yield return new WaitForSeconds(1.2f);
        Debug.Log("Cargando siguiente nivel o pantalla de victoria...");
        // UnityEngine.SceneManagement.SceneManager.LoadScene("TuSiguienteNivel");
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, checkRadius);
        }
    }
}