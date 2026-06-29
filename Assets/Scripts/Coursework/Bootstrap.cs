

using System.Collections.Generic;
using UnityEngine;

namespace Coursework
{
    public interface ISceneInitializable
    {
        public void Initialize();

        //Не забудь добавить эти флаги в классы, которые реалиуют этот интерфейс, чтобы не было проблем.
        //private bool isInitialized;
        //private bool isSubscribed;
    }

    public class Bootstrap : MonoBehaviour
    {
        [SerializeField] private List<GameObject> _objectsToInitialize;

        private void Awake()
        {
            foreach (var obj in _objectsToInitialize)
            {
                if (obj == null) continue;

                // Находим ВСЕ компоненты с интерфейсом на объекте
                var initializables = obj.GetComponents<ISceneInitializable>();

                // Инициализируем их по очереди
                foreach (var initializable in initializables)
                {
                    initializable.Initialize();
                }
            }
        }
    }
}
