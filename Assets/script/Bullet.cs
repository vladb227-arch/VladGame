using UnityEngine;

public class Bullet : MonoBehaviour
{
    [Header("Настройки")]
    public float speed = 50f;
    public float damage = 25f;
    public float lifeTime = 3f;
    public LayerMask hitMask; // Выберите только Default

    void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        // Сколько метров пуля пролетает за 1 кадр
        float moveDistance = speed * Time.deltaTime;
        RaycastHit hit;

        // Пускаем луч
        if (Physics.Raycast(transform.position, transform.forward, out hit, moveDistance, hitMask))
        {
            // Если это игрок (или его камера/контроллер) — игнорируем
            if (hit.collider.CompareTag("Player") || hit.collider.GetComponent<CharacterController>() != null)
            {
                transform.Translate(Vector3.forward * moveDistance);
                return;
            }

            // Если попали во врага
            Enemy enemy = hit.collider.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
            }

            // Если попали в стену/пол/врага — уничтожаем пулю
            Destroy(gameObject);
            return;
        }

        // Если луч ничего не задел — двигаем пулю дальше
        transform.Translate(Vector3.forward * moveDistance);
    }
}