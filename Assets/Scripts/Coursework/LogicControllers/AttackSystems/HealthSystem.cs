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

        public event Action<float, float, float> HealthChanged; //Current, Max, Delta
    }

    public interface IMutableHealth : IHealth
    {
        public void ApplyDamage(float amount);
        public void ApplyHealing(float amount);
    }

    public class HealthSystem : IMutableHealth
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
                if (Mathf.Approximately(health, clamped)) return;
                float delta = clamped - health;
                health = clamped;
                HealthChanged?.Invoke(health, MaxHealth, delta);
            }
        }

        public event Action<float, float, float> HealthChanged;

        public HealthSystem(float health, float maxHealth)
        {
            this.health = health;
            MaxHealth = maxHealth;
        }

        public void ApplyDamage(float amount)
        {
            if (amount <= 0) return;
            Health -= amount;
        }

        public void ApplyHealing(float amount)
        {
            if (amount <= 0) return;
            Health += amount;
        }
    }
}
