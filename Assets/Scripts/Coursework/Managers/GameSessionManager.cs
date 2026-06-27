using UnityEngine;
using Coursework.ScriptableObjects;
using Coursework.EnumsCreatures.Knight;

namespace Coursework.Managers
{
    public class GameSessionManager : MonoBehaviour
    {
        public static GameSessionManager Instance { get; private set; }

        [Header("Original Prefabs from Project")]
        [SerializeField] private KnightConfig _knightConfigPrefab;

        // Это то, с чем будет работать вся игра в рантайме
        public IEntityDataHandler<KnightStates, KnightActions> KnightRuntimeData => _knightRuntimeConfig;

        private KnightConfig _knightRuntimeConfig;

        private void Awake()
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

            // Наш честный глубокий клон через JSON
            string json = JsonUtility.ToJson(_knightConfigPrefab);
            _knightRuntimeConfig = ScriptableObject.CreateInstance<KnightConfig>();
            JsonUtility.FromJsonOverwrite(json, _knightRuntimeConfig);

            // Метод OnEnable внутри _knightRuntimeConfig выполнится автоматически 
            // и соберет новые словари для склонированных экшенов
        }

        // Пример метода для зоны апгрейда
        public void UpgradeKnightDashCharges()
        {
            // Достаем экшен через твой индексатор и кастим к конкретному классу
            if (_knightRuntimeConfig[KnightActions.Dash] is KnightDashAction dashAction)
            {
                dashAction.NumberOfDashCharges++;
                Debug.Log($"Рыцарь прокачан! Макс. зарядов дэша: {dashAction.NumberOfDashCharges}");
            }
        }
    }
}