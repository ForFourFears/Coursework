using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Coursework.LogicControllers.AttackSystems
{
    public interface IHealth
    {
        public float MaxHealth { get; }
        public float Health { get; }

        public event Action<float> OnHealthDecreased; //Передаю значение урона.
        public event Action<float> OnHealthIncreased; //Передаю значение лечения.
    }

    public class HealthSystem : IHealth
    {
        public float MaxHealth { get; private set;  }

        private float health;
        public float Health
        {
            get
            {
                return health;
            }

            set
            {
                value = Mathf.Clamp(value, 0, MaxHealth);
                if (value > health)
                {
                    float heal = value - health;
                    health = value;
                    OnHealthIncreased?.Invoke(heal);
                }
                else if (value < health)
                {
                    float damage = health - value;
                    Debug.Log($"Получен урон: {damage}");
                    health = value;
                    OnHealthDecreased?.Invoke(damage);
                }
            }
        }

        public event Action<float> OnHealthDecreased;
        public event Action<float> OnHealthIncreased;

        public HealthSystem(float maxHealth, float health)
        {
            MaxHealth = maxHealth;
            this.health = health;
        }
    }
}
