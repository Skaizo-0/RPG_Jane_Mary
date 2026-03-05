using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public CharacterController controller;
    public Animator animator;

    public float walkSpeed = 2f;
    public float runSpeed = 5f;
    public float gravity = -9.81f;
    public float turnSpeed = 10f;

    private Vector3 velocity;
    private float currentSpeed;

    void Update()
    {
        // 1. Получаем ввод от клавиатуры (WASD)
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        Vector3 direction = new Vector3(horizontal, 0f, vertical).normalized;

        // 2. Логика движения
        if (direction.magnitude >= 0.1f)
        {
            // Определяем скорость (бег или ходьба)
            bool isRunning = Input.GetKey(KeyCode.LeftShift);
            float targetSpeed = isRunning ? runSpeed : walkSpeed;

            // Плавный поворот в сторону движения
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);

            // Двигаем персонажа
            controller.Move(direction * targetSpeed * Time.deltaTime);

            // Текущая скорость для аниматора (0.5 - шаг, 1.0 - бег)
            currentSpeed = isRunning ? 1f : 0.5f;
        }
        else
        {
            currentSpeed = 0f; // Стоим
        }

        // 3. Передаем значение в Animator (тот самый параметр Speed из Blend Tree)
        animator.SetFloat("Speed", currentSpeed, 0.1f, Time.deltaTime);

        // 4. Гравитация (чтобы не улетал вверх)
        if (controller.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }
}