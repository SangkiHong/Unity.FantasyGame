using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using MoreMountains.Feedbacks;
using BehaviorDesigner.Runtime.Tasks.Movement;
using Sangki.Scripts;

public class EnemyDummy : MonoBehaviour
{
    [SerializeField]
    private bool isStatic = true;

    #region VARIABLE
    private enum EnemyState { Idle, Patrol, Seek, Chase, Attack, Dead }

    [SerializeField]
    private EnemyState enemyState;

    [Header("ENEMY ABILITY")]
    [SerializeField]
    private int healthPoint = 3;
    [SerializeField]
    private float attackCooldown = 2.5f;
    [SerializeField]
    private float lookSpeed = 3f;
    [SerializeField]
    private float blinkTime = 0.5f;

    [Header("SEEK AND WONDER")]
    [SerializeField]
    private float seekDistance = 10f;
    [SerializeField]
    private float seekIdleDuration = 5f;

    [Header("SEARCH PLAYER RADER")]
    [SerializeField]
    private bool debugRader;
    [SerializeField]
    private string targetTag = "Player";
    public LayerMask objectLayerMask;
    [SerializeField]
    private LayerMask ignoreLayerMask;
    [SerializeField]
    private float fieldOfViewAngle = 80;
    [SerializeField]
    private float viewDistance = 12.5f;
    [SerializeField]
    private Vector3 offset;
    [SerializeField]
    private Vector3 targetOffset;
    [SerializeField]
    private bool usePhysics2D;
    [SerializeField]
    private float angleOffset2D;
    [SerializeField]
    private bool useTargetBone;
    [SerializeField]
    private HumanBodyBones targetBone;

    [Header("COMPONENTS")]
    [SerializeField]
    private Animator animator;
    [SerializeField]
    private NavMeshAgent navAgent;
    [SerializeField]
    private AttackColliderSwitch attackColliderSwitch;

    [Header("FEEDBACK")]
    [SerializeField]
    private MMFeedbacks feedback_Attack;
    [SerializeField]
    private MMFeedbacks feedback_Damaged;
    [SerializeField]
    private MMFeedbacks feedback_Dead;

    private GameObject targetObject;
    private Transform thisTransform;
    private Collider thisCollider;
    private EnemyHealthBar enemyHealthBar;
    private WaitForSeconds ws_State;
    private WaitForSeconds ws;
    private NavMeshHit navHit;
    private Vector3 randomDirection;

    private readonly string m_Tag_Damage = "Damage";
    private readonly string m_String_EnemyHP = "EnemyHP";

    private int m_AnimPara_isMove,
                m_AnimPara_isFight,
                m_AnimPara_isAttack,
                m_AnimPara_Attack;

    private float stateTime, blinkTimer, attackTimer, seekIdleTimer;
    private float defaultSpeed, defaultStopDist;
    private int currentHealth;
    private bool isDead, isDamaged, isHealthBarAttached;
    #endregion

    private void Awake()
    {
        thisTransform = this.transform;
        stateTime = 0.3f;
        ws_State = new WaitForSeconds(stateTime);
        ws = new WaitForSeconds(0.7f);
        defaultSpeed = navAgent.speed;
        defaultStopDist = navAgent.stoppingDistance;
        m_AnimPara_isMove = animator.GetParameter(1).nameHash;
        m_AnimPara_isAttack = animator.GetParameter(2).nameHash;
        m_AnimPara_isFight = animator.GetParameter(3).nameHash;
        m_AnimPara_Attack = animator.GetParameter(4).nameHash;
        thisCollider = this.GetComponent<Collider>();
    }

    private void OnEnable()
    {
        // INITIALIZE
        StopAllCoroutines();
        currentHealth = healthPoint;
        navAgent.speed = defaultSpeed;
        navAgent.stoppingDistance = defaultStopDist;
        navAgent.isStopped = false;
        isDead = false;
        isDamaged = false;
        isHealthBarAttached = false;
        blinkTimer = 0;
        attackTimer = 0;
        seekIdleTimer = 0;
        enemyState = EnemyState.Seek;
        animator.SetBool(m_AnimPara_isMove, false);
        animator.SetBool(m_AnimPara_isFight, false);
        animator.SetBool(m_AnimPara_isAttack, false);
        animator.SetBool(animator.GetParameter(0).nameHash, isStatic);
        thisCollider.enabled = true;

        if (enemyHealthBar)
        {
            enemyHealthBar.Unssaign();
            enemyHealthBar = null;
        }

        if (!isStatic) StartCoroutine(StateCoroutine());
    }

    private IEnumerator StateCoroutine()
    {
        while (!isDead)
        {
            switch (enemyState)
            {
                case EnemyState.Idle:
                    Rader();
                    break;
                case EnemyState.Patrol:
                    Rader();
                    break;
                case EnemyState.Seek:
                    Rader();
                    if (!navAgent.hasPath)
                    {
                        if (seekIdleTimer > seekIdleDuration)
                        {
                            Vector3 randomPos = SeekAndWonder(seekDistance, 6);
                            navAgent.SetDestination(randomPos);
                            animator.SetBool(m_AnimPara_isMove, true);
                            seekIdleTimer = 0;
                            navAgent.speed = 2;
                            navAgent.stoppingDistance = 0;
                        }
                        else
                        {
                            if (animator.GetBool(m_AnimPara_isMove)) animator.SetBool(m_AnimPara_isMove, false);
                            seekIdleTimer += stateTime;
                        }
                    }
                    break;
                case EnemyState.Chase:
                    if (targetObject)
                    {
                        if (!animator.GetBool(m_AnimPara_isAttack))
                        {
                            if (!navAgent.pathPending)
                            {
                                if (!isDamaged)
                                {
                                    navAgent.SetDestination(targetObject.transform.position);
                                }
                            }
                            if (navAgent.remainingDistance != 0 && navAgent.remainingDistance <= navAgent.stoppingDistance + 0.1f)
                            {
                                enemyState = EnemyState.Attack;
                                animator.SetBool(m_AnimPara_isFight, true);
                                animator.SetBool(m_AnimPara_isMove, false);
                                break;
                            }
                            if (viewDistance < navAgent.remainingDistance)
                            {
                                navAgent.destination = thisTransform.position;
                                enemyState = EnemyState.Seek;
                            }
                            // Animation
                            if (!animator.GetBool(m_AnimPara_isMove)) animator.SetBool(m_AnimPara_isMove, true);
                        }
                    }
                    break;
                case EnemyState.Attack:
                    if (!animator.GetBool(m_AnimPara_isAttack))
                    {
                        // Attack Cooldown
                        if (attackCooldown > attackTimer)
                        {
                            if (!isDamaged) attackTimer += stateTime;
                        }
                        // Do Attack
                        else
                        {
                            if (!isDamaged)
                            {
                                attackTimer = 0;
                                animator.SetTrigger(m_AnimPara_Attack);

                                // Feedback
                                feedback_Attack?.PlayFeedbacks();

                                // Collider On
                                attackColliderSwitch?.DoAttack();
                            }
                        }

                        // TARGET 과 거리 측정 후 STATE 재설정(공격 중이 아닐 때)
                        if (Vector3.Distance(thisTransform.position, targetObject.transform.position) > navAgent.stoppingDistance)
                        {
                            animator.SetBool(m_AnimPara_isMove, true);
                            enemyState = EnemyState.Chase;
                            break;
                        }
                    }
                    break;
                case EnemyState.Dead:

                    break;
                default:
                    break;
            }

            if (isDamaged)
            {
                blinkTimer += stateTime;

                if (blinkTimer >= blinkTime)
                {
                    isDamaged = false;
                    blinkTimer = 0;
                }
            }

            yield return ws_State;
        }
    }

    private void Update()
    {
        if (enemyState == EnemyState.Attack)
        {
            FollowTarget(); // FIGHT MODE 시 타겟 바라보기
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(m_Tag_Damage))
        {
            if (!isDead && !isDamaged)
            {
                currentHealth -= 1;
                if (currentHealth > 0)
                {
                    isDamaged = true;
                    feedback_Damaged.PlayFeedbacks();
                    StartCoroutine(BlinkTimer());

                    if (!isHealthBarAttached)
                    {
                        isHealthBarAttached = true;

                        enemyHealthBar = UIPoolManager.instance.GetObject(m_String_EnemyHP, transform.position).GetComponent<EnemyHealthBar>();
                        enemyHealthBar.Assign(transform, healthPoint);
                    }
                }
                // DEAD
                else
                {
                    isDead = true;

                    enemyState = EnemyState.Dead;
                    feedback_Dead.PlayFeedbacks();
                    thisCollider.enabled = false;

                    if (isHealthBarAttached)
                    {
                        isHealthBarAttached = false;
                        enemyHealthBar.Unssaign();
                        enemyHealthBar = null;
                    }
                }

                if (isHealthBarAttached) enemyHealthBar.UpdateState(currentHealth);
            }
        }
    }


    // TARGET SEARCH RADER
    private void Rader()
    {
        if (targetObject = MovementUtility.WithinSight(thisTransform, offset, fieldOfViewAngle, viewDistance, GameObject.FindGameObjectWithTag(targetTag), targetOffset, ignoreLayerMask, useTargetBone, targetBone))
        {
            navAgent.speed = defaultSpeed;
            navAgent.stoppingDistance = defaultStopDist;
            enemyState = EnemyState.Chase;
        }
    }

    // SEEK AND WONDER
    private Vector3 SeekAndWonder(float distance, int layermask)
    {
        randomDirection = Random.insideUnitSphere * distance;

        randomDirection += thisTransform.position;

        NavMesh.SamplePosition(randomDirection, out navHit, distance, NavMesh.AllAreas);

        return navHit.position;
    }

    IEnumerator BlinkTimer()
    {
        yield return ws;
        isDamaged = false;
    }

    // FIGHT MODE 시 타겟 바라보기
    private void FollowTarget()
    {
        Vector3 dir = targetObject.transform.position - thisTransform.position;
        thisTransform.rotation = Quaternion.Lerp(thisTransform.rotation, Quaternion.LookRotation(dir), Time.deltaTime * lookSpeed);
    }

    // Draw the line of sight representation within the scene window
    private void OnDrawGizmos()
    {
        if (debugRader) MovementUtility.DrawLineOfSight(transform, offset, fieldOfViewAngle, angleOffset2D, viewDistance, usePhysics2D);
    }
    private void OnDisable()
    {
        MovementUtility.ClearCache();
    }
}
