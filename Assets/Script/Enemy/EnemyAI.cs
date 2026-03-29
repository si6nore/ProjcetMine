using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Health))]
[RequireComponent(typeof(WeaponSystem))]
public class EnemyAI : MonoBehaviour
{
    enum State { Patrol, Chase, Attack, Dead }
    State _state = State.Patrol;

    [Header("탐지")]
    public float sightRange = 20f;
    public float sightAngle = 120f;
    public float attackRange = 12f;

    [Header("순찰")]
    public float patrolRadius = 15f;
    public float patrolWait = 2f;

    [Header("사격")]
    public float fireRate = 1.5f;

    [Header("이동 속도")]
    public float patrolSpeed = 2f;
    public float chaseSpeed = 4.5f;

    // ── 내부 상태 ─────────────────────────
    NavMeshAgent _agent;
    Health _health;
    Animator _animator;
    Transform _player;
    WeaponSystem _weapon;

    float _nextFireTime = 0f;
    float _patrolTimer = 0f;
    Vector3 _patrolOrigin;

    // 플레이어의 HitBox 캐시
    HitBox[] _playerHitBoxes;

    void Start()
    {
        _agent = GetComponent<NavMeshAgent>();
        _health = GetComponent<Health>();
        _weapon = GetComponent<WeaponSystem>();
        _animator = GetComponentInChildren<Animator>();
        _patrolOrigin = transform.position;

        GameObject p = GameObject.FindWithTag("Player");
        if (p != null)
        {
            _player = p.transform;
            // 플레이어 HitBox 미리 캐시
            _playerHitBoxes = p.GetComponentsInChildren<HitBox>();
        }

        SetNextPatrolPoint();
    }

    void Update()
    {
        if (_health.IsDead)
        {
            if (_state != State.Dead)
            {
                _state = State.Dead;
                SetStopped(true);
                SetAnim(false);
            }
            return;
        }

        switch (_state)
        {
            case State.Patrol: UpdatePatrol(); break;
            case State.Chase:  UpdateChase();  break;
            case State.Attack: UpdateAttack(); break;
        }

        UpdateAnimation();
    }

    void UpdatePatrol()
    {
        _agent.speed = patrolSpeed;

        if (!_agent.pathPending && _agent.remainingDistance <= _agent.stoppingDistance)
        {
            _patrolTimer += Time.deltaTime;
            if (_patrolTimer >= patrolWait)
            {
                _patrolTimer = 0f;
                SetNextPatrolPoint();
            }
        }

        if (CanSeePlayer())
            _state = State.Chase;
    }

    void UpdateChase()
    {
        _agent.speed = chaseSpeed;

        if (_player == null) { _state = State.Patrol; return; }

        float dist = Vector3.Distance(transform.position, _player.position);

        if (dist <= attackRange)
        {
            SetStopped(true);
            _state = State.Attack;
            return;
        }

        if (dist > sightRange * 1.5f)
        {
            _state = State.Patrol;
            SetNextPatrolPoint();
            return;
        }

        SetStopped(false);
        if (_agent.isOnNavMesh)
            _agent.SetDestination(_player.position);
    }

    void UpdateAttack()
    {
        if (_player == null) { _state = State.Patrol; return; }

        float dist = Vector3.Distance(transform.position, _player.position);

        Vector3 dir = (_player.position - transform.position).normalized;
        dir.y = 0f;

        if (dir != Vector3.zero)
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                Quaternion.LookRotation(dir),
                Time.deltaTime * 5f
            );

        if (dist > attackRange * 1.2f)
        {
            SetStopped(false);
            _state = State.Chase;
            return;
        }

        if (_weapon != null && Time.time >= _nextFireTime)
        {
            _nextFireTime = Time.time + (1f / fireRate);

            // 플레이어 HitBox 중 랜덤 하나를 목표로 발사
            if (_playerHitBoxes != null && _playerHitBoxes.Length > 0)
            {
                HitBox randomHitBox = _playerHitBoxes[Random.Range(0, _playerHitBoxes.Length)];
                _weapon.TryFire(transform, randomHitBox.transform.position);
            }
            else
            {
                // HitBox가 없으면 플레이어 중심으로 발사
                _weapon.TryFire(transform, _player.position);
            }
        }
    }

    void UpdateAnimation()
    {
        if (_animator == null) return;

        Vector3 flatVel = new Vector3(_agent.velocity.x, 0f, _agent.velocity.z);
        bool isMoving = flatVel.magnitude > 0.1f;

        _animator.SetBool("IsRunning", isMoving);
    }

    bool CanSeePlayer()
    {
        if (_player == null) return false;

        Vector3 origin    = transform.position + Vector3.up * 1.5f;
        Vector3 targetPos = _player.position + Vector3.up * 1f;
        Vector3 dir       = (targetPos - origin).normalized;
        float dist        = Vector3.Distance(origin, targetPos);

        if (dist > sightRange) return false;

        float angle = Vector3.Angle(transform.forward, dir);
        if (angle > sightAngle * 0.5f) return false;

        if (Physics.Raycast(origin, dir, out RaycastHit hit, dist))
        {
            if (hit.collider.CompareTag("Player") ||
                hit.transform.root == _player.root)
                return true;
        }

        return false;
    }

    void SetNextPatrolPoint()
    {
        Vector2 rand   = Random.insideUnitCircle * patrolRadius;
        Vector3 target = _patrolOrigin + new Vector3(rand.x, 0f, rand.y);

        if (NavMesh.SamplePosition(target, out NavMeshHit hit, patrolRadius, NavMesh.AllAreas))
        {
            SetStopped(false);
            if (_agent.isOnNavMesh)
                _agent.SetDestination(hit.position);
        }
    }

    void SetAnim(bool isRunning)
    {
        if (_animator != null)
            _animator.SetBool("IsRunning", isRunning);
    }

    void SetStopped(bool stopped)
    {
        if (_agent != null && _agent.isOnNavMesh)
            _agent.isStopped = stopped;
    }
}
