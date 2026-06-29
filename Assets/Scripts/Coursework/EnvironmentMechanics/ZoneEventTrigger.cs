using Coursework.Managers;
using UnityEngine;
using UnityEngine.Events;


namespace Coursework.EnvironmentMechanics
{
    public class ZoneEventTrigger : MonoBehaviour
    {
        [SerializeField] private UnityEvent _onEnter;

        [SerializeField] private string _triggerTag;

        [SerializeField] private bool _isOneTime = true;

        private bool isActivated;

        private void OnTriggerEnter2D(Collider2D collider)
        {
            if ((!_isOneTime || !isActivated) && collider.gameObject.CompareTag(_triggerTag))
            {
                _onEnter?.Invoke();
                isActivated = true;
            }
        }

        public void AddJumpCharge()
        {
            GameSessionManager.Instance.AddJumpCharge();
        }

        public void AddDashCharge()
        {
            GameSessionManager.Instance.AddDashCharge();
        }

        public void AddRespawn()
        {
            GameSessionManager.Instance.RespawnPosition = G.Player.transform.position;
            Debug.Log("+Resp");
        }
    }
}

