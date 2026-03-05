using UnityEngine;

public class Bootstrapper : MonoBehaviour
{
    public PlayerMovement playerMove;
    public PlayerCombat playerCombat;

    void Awake()
    {
        // Создаем сервис ввода (Инфраструктура)
        IInputService input = new StandaloneInput();

        // Прокидываем его в логику (Внедрение зависимостей)
        playerMove.Construct(input);
        playerCombat.Construct(input);

        Debug.Log("Архитектура инициализирована: Ввод связан с Игроком.");
    }
}