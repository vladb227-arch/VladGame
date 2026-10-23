using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement; // Нужно для перезапуска сцены

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
    public float staminaDrain = 20f;
    public float staminaRegen = 15f;
    public float regenDelay = 1f;

    [Header("Прыжок")]
    public float jumpStaminaCost = 10f;

    [Header("UI")]
    public Image healthFill;
    public Image staminaFill;

    [Header("UI Смерти")]
    public GameObject deathScreen; // Сюда перетащим панель смерти
    private bool isDead = false;   // Флаг, чтобы игрок не умер дважды

    [HideInInspector] public bool isRunning = false;
    [HideInInspector] public bool canRun = true;

    private float regenTimer = 0f;

    void Start()
    {
        currentHealth = maxHealth;
        currentStamina = maxStamina;
        UpdateUI();
        
        if (deathScreen != null) deathScreen.SetActive(false); // Скрываем экран смерти при старте
    }

    void Update()
    {
        if (isDead) return; // Если мертвы, ничего не обновляем

        HandleStamina();
        UpdateUI();
    }

    void HandleStamina()
    {
        bool wantsToRun = (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) && currentStamina > 0f;
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        bool isMoving = Mathf.Abs(h) > 0.01f || Mathf.Abs(v) > 0.01f;

        isRunning = wantsToRun && isMoving && canRun;

        if (isRunning)
        {
            currentStamina -= staminaDrain * Time.deltaTime;
            currentStamina = Mathf.Max(currentStamina, 0f);
            regenTimer = regenDelay;

            if (currentStamina <= 0f) canRun = false;
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
                if (currentStamina >= maxStamina * 0.2f) canRun = true;
            }
        }
    }

    void UpdateUI()
    {
        if (healthFill != null) healthFill.fillAmount = currentHealth / maxHealth;
        if (staminaFill != null) staminaFill.fillAmount = currentStamina / maxStamina;
    }

    public void TakeDamage(float amount)
    {
        if (isDead) return; // Если уже мертв, урон не получаем

        currentHealth -= amount;
        currentHealth = Mathf.Max(currentHealth, 0f);
        UpdateUI();

        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    public void Heal(float amount)
    {
        if (isDead) return;
        currentHealth += amount;
        currentHealth = Mathf.Min(currentHealth, maxHealth);
        UpdateUI();
    }

    public bool TrySpendStamina(float amount)
    {
        if (isDead) return false; // Мертвый не тратит стамину

        if (currentStamina >= amount)
        {
            currentStamina -= amount;
            regenTimer = regenDelay;
            UpdateUI();
            return true;
        }
        return false;
    }

    // ===== СМЕРТЬ И ПЕРЕЗАПУСК =====
    void Die()
    {
        isDead = true;
        Debug.Log("Игрок погиб!");

        // Показываем экран смерти
        if (deathScreen != null) deathScreen.SetActive(true);

        // Отключаем управление (чтобы камера не крутилась и игрок не ходил)
        MonoBehaviour[] scripts = GetComponents<MonoBehaviour>();
        foreach (MonoBehaviour script in scripts)
        {
            if (script != this) script.enabled = false; // Отключаем всё, кроме PlayerStats
        }

        // Освобождаем курсор
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Останавливаем время (опционально, если хотите заморозить игру)
        // Time.timeScale = 0f;
    }

    // Этот метод мы вызовем из кнопки на экране смерти
    public void RestartGame()
    {
        // Time.timeScale = 1f; // Включаем время обратно, если останавливали
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}