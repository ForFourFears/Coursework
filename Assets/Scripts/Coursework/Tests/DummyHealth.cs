using Coursework.LogicControllers;
using Coursework.LogicControllers.AttackSystems;
using UnityEngine;

public class DummyHealth : MonoBehaviour, IDamageable
{
    private HealthSystem _healthSystem;

    private void Awake()
    {
        // Создаем систему здоровья для манекена (например, 100 ХП)
        _healthSystem = new HealthSystem(100f, 100f);
    }

    private void OnEnable()
    {
        // Подписываемся на события, чтобы проверить их работу в консоли
        _healthSystem.OnHealthDecreased += LogDamage;
    }

    private void OnDisable()
    {
        _healthSystem.OnHealthDecreased -= LogDamage;
    }

    public void TakeDamage(float damage)
    {
        // Передаем урон в систему здоровья манекена
        _healthSystem.Health -= damage;
    }

    private void LogDamage(float damageDealt)
    {
        Debug.Log($"<color=red>[MANNEQUIN]</color> Получен урон: {damageDealt}. " +
                  $"Текущее ХП: {_healthSystem.Health}/{_healthSystem.MaxHealth}");

        if (_healthSystem.Health <= 0)
        {
            Debug.Log("<color=black><b>[MANNEQUIN] УБИТ!</b></color>");
        }
    }
}