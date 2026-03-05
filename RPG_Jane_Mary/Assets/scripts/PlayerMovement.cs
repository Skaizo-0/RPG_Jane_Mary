using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public CharacterController controller;
    public Animator animator;

    public float walkSpeed = 3f;
    public float runSpeed = 6f;
    public float gravity = -9.81f;
    public float turnSpeed = 10f;

    private Vector3 velocity;
    private Transform cam; // Ссылка на основную камеру

    void Start()
    {
        // Находим главную камеру в начале игры
        cam = Camera.main.transform;
    }

    void Update()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        Vector3 inputDirection = new Vector3(horizontal, 0f, vertical).normalized;

        if (inputDirection.magnitude >= 0.1f)
        {
            // 1. ПОЛУЧАЕМ НАПРАВЛЕНИЕ КАМЕРЫ
            Vector3 camForward = cam.forward;
            Vector3 camRight = cam.right;

            // 2. ОБНУЛЯЕМ Y (чтобы персонаж не пытался взлететь, если камера смотрит вверх)
            camForward.y = 0;
            camRight.y = 0;
            camForward.Normalize();
            camRight.Normalize();

            // 3. СЧИТАЕМ НАПРАВЛЕНИЕ ОТНОСИТЕЛЬНО КАМЕРЫ
            // Это магия: складываем "вперед камеры" * нажатие W и "право камеры" * нажатие D
            Vector3 moveDirection = (camForward * vertical + camRight * horizontal).normalized;

            // Определяем скорость
            bool isRunning = Input.GetKey(KeyCode.LeftShift);
            float targetSpeed = isRunning ? runSpeed : walkSpeed;

            // Поворачиваем персонажа лицом туда, куда он идет
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);

            // Двигаем персонажа
            controller.Move(moveDirection * targetSpeed * Time.deltaTime);

            // Анимация
            float animSpeed = isRunning ? 1f : 0.5f;
            animator.SetFloat("Speed", animSpeed, 0.1f, Time.deltaTime);
        }
        else
        {
            animator.SetFloat("Speed", 0f, 0.1f, Time.deltaTime);
        }

        // Гравитация
        if (controller.isGrounded && velocity.y < 0) velocity.y = -2f;
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    // Тот самый метод для остановки при получении урона
    public void StopMoving()
    {
        velocity = Vector3.zero;
        if (animator != null) animator.SetFloat("Speed", 0);
    }
}