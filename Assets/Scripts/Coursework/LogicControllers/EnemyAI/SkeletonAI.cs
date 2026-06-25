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
        private AIState AIState = AIState.Patrol;

        [Header("Setting AI")]
        [SerializeField] private float _attackDistance = 5f;

        [Header("Vision")]
        [SerializeField] private float _radiusDetection = 25f;
        [SerializeField] private LayerMask _visionLayer;
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
        }

        private void CheckWallAhead()
        {
            RaycastHit2D hit = Physics2D.Raycast(
                _wallCheckOrigin.position,
                Vector2.right * Mathf.Sign(transform.localScale.x),
                _wallCheckDistance
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
                _ledgeCheckDistance
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
            if (target == null && targetLossTimer <= 0)
            {
                AIState = AIState.Patrol;
                return;
            }

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
            int count = Physics2D.OverlapCircle(
                transform.position,
                _radiusDetection,
                filter,
                targets
            );

            Collider2D closestTarget = null;
            float minSqrDistance = float.MaxValue;

            for (int i = 0; i < count; i++)
            {
                var potentialTarget = targets[i];

                float sqrDistance = (potentialTarget.transform.position - transform.position).sqrMagnitude;

                if (sqrDistance >= minSqrDistance)
                {
                    continue;
                }

                RaycastHit2D hit = Physics2D.Linecast(
                    transform.position,
                    potentialTarget.transform.position,
                    _enemiesLayer | _visionLayer
                );

                if (hit.collider != null && hit.collider.gameObject != potentialTarget.gameObject)
                {
                    continue;
                }

                minSqrDistance = sqrDistance;
                closestTarget = potentialTarget;
            }

            target = closestTarget != null ? closestTarget.transform : null;

            if (closestTarget != null)
            {
                target = closestTarget.transform;
                lastKnowPosTarget = target.position;
            }
            else
            {
                target = null;
            }
        }
    }

}