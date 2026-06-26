using Coursework.EnumsCreatures.Skeleton;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Coursework.LogicControllers.EnemyAI
{
    enum AIState
    {
        Patrol = 0,
        Chase,
        Attack
    }

    [RequireComponent(typeof(IBaseController<SkeletonActions>))]
    public class SkeletonAI : MonoBehaviour
    {

        [SerializeField] private IBaseController<SkeletonActions> _controller;
        private AIState AIState;

        [Header("Setting AI")]
        [SerializeField] private float _chaseStopDistanceX = 0.2f;
        [SerializeField] private float _attackDistance = 5f;

        [Header("Patrol Settings")]
        [SerializeField] private float _patrolWaitTime = 2f;
        private float _patrolDirection = 1f;
        private float _patrolWaitTimer;

        [Header("Vision")]
        [SerializeField] private Transform _visionOrigin;
        [SerializeField] private Vector2 _targetOffset = new(0f, 1f);
        [SerializeField] private float _radiusDetection = 25f;
        [SerializeField] private LayerMask _obstacleLayer;
        [SerializeField] private LayerMask _enemiesLayer;
        [SerializeField] private float _detectionInterval = 0.2f;
        [SerializeField] private float _targetLossTime = 2f;
        private ContactFilter2D filter;

        [Header("Wall Ahead Check")]
        [SerializeField] private Transform _wallCheckOrigin;
        [SerializeField] private float _wallCheckDistance;
        public bool HasWallAhead { get; private set; }

        [Header("Ground Ahead Check")]
        [SerializeField] private Transform _ledgeCheckOrigin;
        [SerializeField] private float _ledgeCheckDistance;
        public bool HasGroundAhead { get; private set; }

        //Для обнаружения цели
        private WaitForSeconds detectionWait;
        private Coroutine detectionCorutine;
        private Transform target;
        private Vector2 lastKnowPosTarget;
        private float targetLossTimer;
        private readonly List<Collider2D> targets = new(5);

        private void Awake()
        {
            detectionWait = new(_detectionInterval);

            filter.useLayerMask = true;
            filter.layerMask = _enemiesLayer;
        }

        private void OnEnable()
        {
            detectionCorutine = StartCoroutine(DetectionCoritine());
        }

        private void OnDisable()
        {
            if (detectionCorutine != null)
            {
                StopCoroutine(detectionCorutine);
                detectionCorutine = null;
            }
        }

        void Start()
        {
            _controller ??= GetComponent<IBaseController<SkeletonActions>>();
        }


        void FixedUpdate()
        {
            CheckGroundAhead();
            CheckWallAhead();

            if (target != null) targetLossTimer = _targetLossTime;
            else targetLossTimer = Mathf.Max(0, targetLossTimer - Time.fixedDeltaTime);

            UpdateAIState();
            ExecuteAIState();
        }

        private void CheckWallAhead()
        {
            RaycastHit2D hit = Physics2D.Raycast(
                _wallCheckOrigin.position,
                Vector2.right * Mathf.Sign(transform.localScale.x),
                _wallCheckDistance,
                _obstacleLayer
            );

            if (hit.normal == Vector2.zero)
            {
                HasWallAhead = false;
                return;
            }

            float normalAngle = Vector2.Angle(Vector2.up, hit.normal);
            HasWallAhead = normalAngle > _controller.MaxSlopeAngle;
        }

        private void CheckGroundAhead()
        {
            RaycastHit2D hit = Physics2D.Raycast(
                _ledgeCheckOrigin.position,
                Vector2.down,
                _ledgeCheckDistance,
                _obstacleLayer
            );

            if (hit.normal == Vector2.zero)
            {
                HasGroundAhead = false;
                return;
            }

            float normalAngle = Vector2.Angle(Vector2.up, hit.normal);
            HasGroundAhead = normalAngle <= _controller.MaxSlopeAngle;
        }

        private void UpdateAIState()
        {
            AIState previousState = AIState;
            if (target == null && targetLossTimer <= 0)
            {
                AIState = AIState.Patrol;
            }
            else
            {
                Vector2 currentPos = transform.position;
                Vector2 currentDestination = target != null ? target.position : lastKnowPosTarget;
                float sqrDistanceToTarget = (currentDestination - currentPos).sqrMagnitude;

                if (target != null && sqrDistanceToTarget <= Mathf.Pow(_attackDistance, 2))
                {
                    AIState = AIState.Attack;
                }
                else
                {
                    AIState = AIState.Chase;
                }
            }

            if (previousState == AIState.Patrol && AIState == AIState.Chase)
            {
                _controller.TryExecuteAction(SkeletonActions.React);
            }
        }

        private void ExecuteAIState()
        {
            switch (AIState)
            {
                case AIState.Patrol:
                    HandlePatrol();
                    break;

                case AIState.Chase:
                    HandleChase();
                    break;

                case AIState.Attack:
                    HandleAttack();
                    break;
            }
        }

        private void HandlePatrol()
        {
            if (_patrolWaitTimer > 0)
            {
                _patrolWaitTimer -= Time.fixedDeltaTime;
                _controller.MoveInput = Vector2.zero;
                return;
            }

            if (HasWallAhead || !HasGroundAhead)
            {
                _patrolDirection *= -1f;
                _patrolWaitTimer = _patrolWaitTime;

                // Передаем новое направление сразу, чтобы контроллер успел развернуть скелета
                _controller.MoveInput = new Vector2(_patrolDirection, 0f);
                return;
            }

            _controller.MoveInput = new Vector2(_patrolDirection, 0f);
        }

        private void HandleChase()
        {
            Vector2 currentDestination = target != null ? (Vector2)target.position : lastKnowPosTarget;

            float distanceX = Mathf.Abs(currentDestination.x - transform.position.x);
            float directionX = Mathf.Sign(currentDestination.x - transform.position.x);

            // 1. Проверка мёртвой зоны
            if (distanceX < _chaseStopDistanceX)
            {
                _controller.MoveInput = Vector2.zero;

                if (transform.localScale.x != 0 && Mathf.Sign(transform.localScale.x) != directionX)
                {
                    _controller.MoveInput = new Vector2(directionX, 0f);
                }
                return;
            }

            // 2. Проверка препятствий (стены и обрывы)
            if ((HasWallAhead || !HasGroundAhead) && Mathf.Sign(transform.localScale.x) == directionX)
            {
                _controller.MoveInput = Vector2.zero;
                return;
            }

            // 3. Блокировка движения, если тело ещё не развёрнуто в нужную сторону
            if (transform.localScale.x != 0)
            {
                float facingSign = Mathf.Sign(transform.localScale.x);

                if (facingSign != directionX)
                {
                    _controller.MoveInput = new Vector2(directionX, 0f);
                    return;
                }
            }

            // 4. Обычное движение преследования
            _controller.MoveInput = new Vector2(directionX, 0f);
        }

        private void HandleAttack()
        {
            // Во время атаки продолжаем передавать направление на игрока в MoveInput.
            // Если в контроллере/ActionExecutionSystem заблокирован поворот во время атаки — 
            // скелет не развернется визуально, но будет хранить верное намерение.
            if (target != null)
            {
                float directionX = Mathf.Sign(target.position.x - transform.position.x);
                _controller.MoveInput = new Vector2(directionX, 0f);
            }
            else
            {
                _controller.MoveInput = Vector2.zero;
            }

            // Пытаемся нанести удар каждый физический кадр
            _controller.TryExecuteAction(SkeletonActions.Attack);
        }

        private IEnumerator DetectionCoritine()
        {
            while(true)
            {
                FindTarget();
                yield return detectionWait;
            }
        }

        private void FindTarget()
        {
            Vector3 eyePosition = _visionOrigin != null ? _visionOrigin.position : transform.position;

            int count = Physics2D.OverlapCircle(
                eyePosition,
                _radiusDetection,
                filter,
                targets
            );

            Collider2D closestTarget = null;
            float minSqrDistance = float.MaxValue;

            for (int i = 0; i < count; i++)
            {
                var potentialTarget = targets[i];

                // Считаем точку на теле цели (ноги + наш оффсет)
                Vector3 targetBodyPosition = potentialTarget.transform.position + (Vector3)_targetOffset;

                // Расстояние теперь считаем от глаз скелета до "тела" цели
                float sqrDistance = (targetBodyPosition - eyePosition).sqrMagnitude;

                if (sqrDistance >= minSqrDistance)
                {
                    continue;
                }

                // Луч зрения пускаем из глаз скелета точно в смещенную точку на теле игрока
                RaycastHit2D hit = Physics2D.Linecast(
                    eyePosition,
                    targetBodyPosition,
                    _enemiesLayer | _obstacleLayer
                );

                if (hit.collider != null && hit.collider.gameObject != potentialTarget.gameObject)
                {
                    continue;
                }

                minSqrDistance = sqrDistance;
                closestTarget = potentialTarget;
            }

            if (closestTarget != null)
            {
                target = closestTarget.transform;
                // Запоминаем позицию ног для перемещения, но для проверок используем оффсет
                lastKnowPosTarget = target.position;
            }
            else
            {
                target = null;
            }
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Vector3 eyePosition = _visionOrigin != null ? _visionOrigin.position : transform.position;

            Gizmos.color = target != null ? Color.red : Color.green;
            Gizmos.DrawWireSphere(eyePosition, _radiusDetection);

            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(eyePosition, _attackDistance);

            if (target != null)
            {
                Vector3 targetBodyPosition = target.position + (Vector3)_targetOffset;

                Gizmos.color = Color.red;
                Gizmos.DrawLine(eyePosition, targetBodyPosition);
                Gizmos.DrawWireCube(targetBodyPosition, Vector3.one * 0.3f);
            }
            else if (targetLossTimer > 0)
            {
                Vector3 lastKnowBodyPos = (Vector3)lastKnowPosTarget + (Vector3)_targetOffset;

                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(eyePosition, lastKnowBodyPos);
                Gizmos.DrawWireSphere(lastKnowBodyPos, 0.2f);
            }

            if (_wallCheckOrigin != null)
            {
                Gizmos.color = HasWallAhead ? Color.red : Color.cyan;
                Vector3 wallDirection = Vector2.right * (Mathf.Sign(transform.localScale.x) * _wallCheckDistance);
                Gizmos.DrawRay(_wallCheckOrigin.position, wallDirection);
            }

            if (_ledgeCheckOrigin != null)
            {
                Gizmos.color = HasGroundAhead ? Color.cyan : Color.red;
                Gizmos.DrawRay(_ledgeCheckOrigin.position, Vector3.down * _ledgeCheckDistance);
            }

            if (_controller != null && _controller.MoveInput != Vector2.zero)
            {
                Gizmos.color = Color.blue;
                Vector3 moveDir = new Vector3(_controller.MoveInput.x, _controller.MoveInput.y, 0f).normalized;
                Gizmos.DrawRay(transform.position + Vector3.up, moveDir * 1.5f);
            }
        }
#endif
    }
}