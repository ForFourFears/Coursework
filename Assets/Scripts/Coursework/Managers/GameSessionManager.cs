using UnityEngine;
using Coursework.ScriptableObjects;
using Coursework.EnumsCreatures.Knight;

namespace Coursework.Managers
{
    public class GameSessionManager : MonoBehaviour, ISceneInitializable
    {
        public static GameSessionManager Instance { get; private set; }

        [Header("Original Prefabs from Project")]
        [SerializeField] private KnightConfig _knightConfigPrefab;

        public IEntityDataHandler<KnightStates, KnightActions> KnightData => knightRuntimeConfig;

        private KnightConfig knightRuntimeConfig;

        public Vector3? RespawnPosition { get; set; }

        public void Initialize()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            InitializeConfigs();
        }

        private void InitializeConfigs()
        {
            if (_knightConfigPrefab == null)
            {
                Debug.LogError("GameSessionManager: Назначьте префаб KnightConfig в инспекторе!");
                return;
            }

            knightRuntimeConfig = Instantiate(_knightConfigPrefab);
        }

        public void AddDashCharge()
        {
            if (knightRuntimeConfig[KnightActions.Dash] is KnightDashAction dashAction)
            {
                dashAction.NumberOfDashCharges++;
                Debug.Log($"Рыцарь прокачан! Макс. зарядов дэша: {dashAction.NumberOfDashCharges}");
            }
        }

        public void AddJumpCharge()
        {
            if (knightRuntimeConfig[KnightActions.Jump] is KnightJumpAction jumpAction)
            {
                jumpAction.NumberOfJumps++;
                Debug.Log($"Рыцарь прокачан! Макс. зарядов прыжка: {jumpAction.NumberOfJumps}");
            }
        }
    }
}