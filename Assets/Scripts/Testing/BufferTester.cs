using UnityEngine;
using Coursework.LogicControllers.ActionBuffers;
using Coursework.EnumsCreatures.Knight;

namespace Coursework
{
    public class BufferTester : MonoBehaviour
    {
        private void Start()
        {
            Debug.Log("=== ЗАПУСК ТЕСТОВ БУФЕРА ВВОДА ===");

            Test_AddAction();
            Test_Chronology_FIFO();
            Test_Expiration();

            Debug.Log("=== ТЕСТИРОВАНИЕ ЗАВЕРШЕНО ===");
        }

        private void Test_AddAction()
        {
            ActionBuffer buffer = new();
            buffer.AddAction(KnightActions.Jump, 1.0f);

            if (buffer.ActionRequests.Count == 1 && buffer.GetNewestActionRequest().Action == KnightActions.Jump)
            {
                Debug.Log("✅ Тест 1 (Добавление): УСПЕШНО. Действие Jump добавлено.");
            }
            else
            {
                Debug.LogError("❌ Тест 1 (Добавление): ПРОВАЛ!");
            }
        }

        private void Test_Chronology_FIFO()
        {
            ActionBuffer buffer = new();

            // Игрок нажал Атаку, а через кадр Прыжок
            buffer.AddAction(KnightActions.Attack, 1.0f);
            buffer.AddAction(KnightActions.Jump, 1.0f);

            // Проверяем, что первым вызовется именно старое действие (Атака)
            KnightActions firstAction = buffer.GetOldestActionRequest().Action;

            if (firstAction == KnightActions.Attack)
            {
                Debug.Log("✅ Тест 2 (Хронология FIFO): УСПЕШНО. Первой извлечена Атака.");
            }
            else
            {
                Debug.LogError($"❌ Тест 2 (Хронология FIFO): ПРОВАЛ! Ожидалась Атака, но буфер вернул {firstAction}. " +
                               $"Замени GetNewestActionRequest на GetOldestActionRequest в контроллере!");
            }
        }

        private void Test_Expiration()
        {
            ActionBuffer buffer = new();
            buffer.AddAction(KnightActions.Jump, 0.5f);

            // Искусственно прокручиваем время вперед на 0.6 секунд
            buffer.Update(0.6f);

            if (buffer.ActionRequests.Count == 0)
            {
                Debug.Log("✅ Тест 3 (Время жизни): УСПЕШНО. Устаревшее действие удалено.");
            }
            else
            {
                Debug.LogError("❌ Тест 3 (Время жизни): ПРОВАЛ! Экшен остался в буфере.");
            }
        }
    }
}