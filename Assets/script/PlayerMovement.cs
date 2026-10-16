using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Гравитация и прыжок")]
    public float gravity = -9.81f;
    public float jumpHeight = 1.5f;
    public float groundedRememberTime = 0.15f;

    [Header("Кулдаун прыжка")]
    public float jumpCooldown = 0.5f;

    [Header("Проверка земли (рейкаст)")]
    public float groundCheckDistance = 0.2f; // на сколько вниз пускать луч
    public LayerMask groundMask = ~0;        // какие слои считаются землёй

    [Header("Ссылка на камеру")]
    public Transform cameraTransform;

    private PlayerStats stats;
    private CharacterController controller;
    private Vector3 velocity;

    private float groundedRemember;
    private bool jumpRequested;
    private float jumpCooldownTimer;
    private bool isGrounded;

    void Awake()
    {
        stats = GetComponent<PlayerStats>();
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        // КУЛДАУН
        if (jumpCooldownTimer > 0f)
            jumpCooldownTimer -= Time.deltaTime;

        // ДВИЖЕНИЕ
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        Vector3 forward = cameraTransform.forward;
        Vector3 right = cameraTransform.right;
        forward.y = 0f;
        right.y = 0f;
        forward.Normalize();
        right.Normalize();

        Vector3 move = right * horizontal + forward * vertical;

        float currentSpeed = stats.isRunning ? stats.runSpeed : stats.walkSpeed;
        controller.Move(move * currentSpeed * Time.deltaTime);

        Vector3 rayOrigin = transform.position - Vector3.up * (controller.height * 0.5f - controller.radius);
        isGrounded = Physics.Raycast(rayOrigin, Vector3.down,
                                     controller.radius + groundCheckDistance,
                                     groundMask, QueryTriggerInteraction.Ignore)
                     || controller.isGrounded; // на всякий случай — комбинируем

        // ГРАВИТАЦИЯ И СОСТОЯНИЕ ЗЕМЛИ
        if (isGrounded)
        {
            groundedRemember = groundedRememberTime;

            if (velocity.y < 0)
                velocity.y = -2f;
        }
        else
        {
            groundedRemember -= Time.deltaTime;
        }

        // ЗАПОМИНАЕМ ПРЫЖОК
        if (Input.GetButtonDown("Jump") && groundedRemember > 0f && jumpCooldownTimer <= 0f)
        {
            jumpRequested = true;
        }

        // ВЫПОЛНЕНИЕ ПРЫЖКА
        if (jumpRequested && groundedRemember > 0f)
        {
            if (stats.TrySpendStamina(stats.jumpStaminaCost))
            {
                velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
                groundedRemember = 0f;
                jumpCooldownTimer = jumpCooldown;
            }

            jumpRequested = false;
        }

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }
}