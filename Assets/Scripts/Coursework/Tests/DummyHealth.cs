using Coursework.LogicControllers.CharactersControllers;
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
        _healthSystem.HealthChanged += LogDamage;
    }

    private void OnDisable()
    {
        _healthSystem.HealthChanged -= LogDamage;
    }

    public void TakeDamage(float damage)
    {
        
        _healthSystem.Health -= damage;

    }

    private void LogDamage(float health, float maxHealth, float delta)
    {
        Debug.Log($"<color=red>[MANNEQUIN]</color> Получен урон: {Mathf.Abs(delta)}. ");
        Debug.Log($"Текущее ХП: {health}/{maxHealth}");

        if (health <= 0)
        {
            Debug.Log("<color=black><b>[MANNEQUIN] УБИТ!</b></color>");
        }
    }
}