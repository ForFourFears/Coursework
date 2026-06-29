using Coursework.LogicControllers.CharactersControllers;
using UnityEngine;
using UnityEngine.UI;

namespace Coursework.UI
{
    public class HealthBar : MonoBehaviour, ISceneInitializable
    {
        [SerializeField] private Image _fillBar;
        [SerializeField, Range(0, 1)] private float _minFill;
        [SerializeField, Range(0, 1)] private float _maxFill = 1;

        private bool isInitialized;
        private bool isSubscribed;

        private PlayerController player;

        private void OnValidate()
        {
            if (_minFill >= _maxFill)
            {
                if (_minFill >= _maxFill && _minFill != 0)
                {
                    _minFill = _maxFill - 0.1f;
                }
                else
                {
                    _maxFill = _minFill + 0.1f;
                }
            }
        }

        public void Initialize()
        {
            if (G.Player.TryGetComponent(out PlayerController playerController))
            {
                player = playerController;
                isInitialized = true;
            }

            OnEnable();
        }

        private void OnEnable()
        {
            if (!isInitialized || isSubscribed) return;
            
            player.Health.HealthChanged += OnHealthChanged;

            isSubscribed = true;
        }

        private void OnDisable()
        {
            if (!isInitialized || !isSubscribed) return;

            player.Health.HealthChanged -= OnHealthChanged;

            isSubscribed = false;
        }

        private void OnHealthChanged(float health, float maxHealth, float delta)
        {
            if (maxHealth == 0) return;
            _fillBar.fillAmount = Mathf.Lerp(_minFill, _maxFill, health / maxHealth);
        }
    }
}