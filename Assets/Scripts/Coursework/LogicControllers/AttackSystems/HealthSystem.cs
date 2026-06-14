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

        public event Action<float, float> HealthChanged;
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
                float clamped = Mathf.Clamp(value, 0, MaxHealth);
                if (health == clamped) return;

                health = clamped;
                HealthChanged?.Invoke(health, MaxHealth);
            }
        }

        public event Action<float, float> HealthChanged;

        public HealthSystem(float health, float maxHealth)
        {
            this.health = health;
            MaxHealth = maxHealth;
        }
    }
}
