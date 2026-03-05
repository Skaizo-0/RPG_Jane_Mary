using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float speed = 15f;
    public float damage = 30f;
    public float lifetime = 3f; // Чтобы сфера не летела вечно в пустоту

    void Start()
    {
        // Удалить сферу через 3 секунды, если она ни в кого не попала
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        // Летим вперед
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        // Если попали во врага
        if (other.CompareTag("Enemy"))
        {
            Health enemyHealth = other.GetComponent<Health>();
            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(0, damage);
            }
            Destroy(gameObject); // Исчезнуть после попадания
        }
    }
}