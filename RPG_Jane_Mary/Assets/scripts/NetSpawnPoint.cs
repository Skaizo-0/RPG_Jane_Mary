using UnityEngine;
using FishNet;
using FishNet.Component.Spawning;
using System.Collections;

public class NetSpawnPoint : MonoBehaviour
{
    private void Start()
    {
        // Используем Start, чтобы дождаться инициализации всего остального
        Register();
    }

    private void Register()
    {
        PlayerSpawner spawner = InstanceFinder.NetworkManager.GetComponent<PlayerSpawner>();
        if (spawner != null)
        {
            // Устанавливаем точку спавна
            spawner.Spawns = new Transform[] { this.transform };
            Debug.Log($"[SPAWN] Точка {gameObject.name} зарегистрирована в спавнере.");
        }
    }
}