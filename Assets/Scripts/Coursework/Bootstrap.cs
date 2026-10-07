using UnityEngine;
using UnityEngine.Events;

namespace Coursework
{
    public class Bootstrap : MonoBehaviour
    {
        public UnityEvent onPostInitialize;
        

        private void Awake()
        {

            
            onPostInitialize?.Invoke();
        }
    }
}
