using UnityEngine;
using UnityEngine.AI; // Для передвижения ИИ

public class Enemy : MonoBehaviour
{
    [Header("Характеристики")]
    public float maxHealth = 100f;
    private float currentHealth;
    public float damageToPlayer = 10f;
    public float attackRange = 2f;
    public float attackCooldown = 1f; // раз в секунду бьет
    
    [Header("Ссылки")]
    public Transform player;
    private NavMeshAgent agent;
    private float nextAttackTime;

    void Start()
    {
        currentHealth = maxHealth;
        agent = GetComponent<NavMeshAgent>();
        
        // Автоматически находим игрока по тегу
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }
    }

    void Update()
    {
        if (player == null) return;

        float distance = Vector3.Distance(transform.position, player.position);

        // Если игрок в радиусе атаки — стоим и бьем
        if (distance <= attackRange)
        {
            agent.isStopped = true;
            
            if (Time.time >= nextAttackTime)
            {
                Attack();
                nextAttackTime = Time.time + attackCooldown;
            }
        }
        else
        {
            // Иначе бежим к игроку
            agent.isStopped = false;
            agent.SetDestination(player.position);
        }
    }

    void Attack()
    {
        // Наносим урон игроку
        PlayerStats stats = player.GetComponent<PlayerStats>();
        if (stats != null)
        {
            stats.TakeDamage(damageToPlayer);
            Debug.Log("Враг ударил игрока!");
        }
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        Debug.Log("Враг получил урон. HP: " + currentHealth);
        
        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    void Die()
    {
        Debug.Log("Враг уничтожен!");
        Destroy(gameObject);
    }
}