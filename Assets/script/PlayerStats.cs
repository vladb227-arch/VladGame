using UnityEngine;
using UnityEngine.UI;

public class PlayerStats : MonoBehaviour
{
    [Header("Здоровье")]
    public float maxHealth = 100f;
    private float currentHealth;

    [Header("Стамина")]
    public float maxStamina = 100f;
    private float currentStamina;

    [Header("Бег")]
    public float walkSpeed = 5f;
    public float runSpeed = 9f;
    public float staminaDrain = 20f;   // расход стамины в секунду при беге
    public float staminaRegen = 15f;   // восстановление в секунду
    public float regenDelay = 1f;      // задержка перед восстановлением после бега

    [Header("Прыжок")]
    public float jumpStaminaCost = 10f; // сколько стамины стоит один прыжок

    [Header("UI")]
    public Image healthFill;
    public Image staminaFill;

    // Для других скриптов (PlayerMovement)
    [HideInInspector] public bool isRunning = false;
    [HideInInspector] public bool canRun = true;

    private float regenTimer = 0f;

    void Start()
    {
        currentHealth = maxHealth;
        currentStamina = maxStamina;
        UpdateUI();
    }

    void Update()
    {
        HandleStamina();
        UpdateUI();
    }

    void HandleStamina()
    {
        // Хотим бежать, если зажат Shift и есть стамина
        bool wantsToRun = (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
                          && currentStamina > 0f;

        // Проверяем, двигается ли игрок
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        bool isMoving = Mathf.Abs(h) > 0.01f || Mathf.Abs(v) > 0.01f;

        isRunning = wantsToRun && isMoving && canRun;

        if (isRunning)
        {
            currentStamina -= staminaDrain * Time.deltaTime;
            currentStamina = Mathf.Max(currentStamina, 0f);
            regenTimer = regenDelay;

            if (currentStamina <= 0f)
                canRun = false;
        }
        else
        {
            if (regenTimer > 0f)
            {
                regenTimer -= Time.deltaTime;
            }
            else
            {
                currentStamina += staminaRegen * Time.deltaTime;
                currentStamina = Mathf.Min(currentStamina, maxStamina);

                if (currentStamina >= maxStamina * 0.2f)
                    canRun = true;
            }
        }
    }

    void UpdateUI()
    {
        if (healthFill != null)
            healthFill.fillAmount = currentHealth / maxHealth;

        if (staminaFill != null)
            staminaFill.fillAmount = currentStamina / maxStamina;
    }

    // ===== Публичные методы =====

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        currentHealth = Mathf.Max(currentHealth, 0f);
        UpdateUI();

        if (currentHealth <= 0f)
            Debug.Log("Игрок погиб");
    }

    public void Heal(float amount)
    {
        currentHealth += amount;
        currentHealth = Mathf.Min(currentHealth, maxHealth);
        UpdateUI();
    }

    // Тратит стамину, если её хватает. Возвращает true, если трата прошла.
    public bool TrySpendStamina(float amount)
    {
        if (currentStamina >= amount)
        {
            currentStamina -= amount;
            regenTimer = regenDelay; // после траты стамина восстанавливается с задержкой
            UpdateUI();
            return true;
        }
        return false;
    }
}