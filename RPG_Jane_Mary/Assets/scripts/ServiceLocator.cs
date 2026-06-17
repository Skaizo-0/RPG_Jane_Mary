using System;
using System.Collections.Generic;

public static class ServiceLocator
{
    private static readonly Dictionary<Type, object> _services = new Dictionary<Type, object>();

    public static void Register<T>(T service) => _services[typeof(T)] = service;

    public static T Get<T>()
    {
        if (_services.TryGetValue(typeof(T), out var service))
        {
            return (T)service;
        }

        UnityEngine.Debug.LogWarning($"Сервис {typeof(T)} не найден в ServiceLocator!");
        return default;
    }
}