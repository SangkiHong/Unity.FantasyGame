using BehaviorDesigner.Runtime.Tasks.Movement;
using DG.Tweening;
using MoreMountains.Feedbacks;
using Sangki.Player;
using Sangki.Object;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using Sirenix.OdinInspector;

namespace Sangki.Enemy
{
    [RequireComponent(typeof(NavMeshAgent))]
    [RequireComponent(typeof(Animator))]
    public class EnemyContoller : MonoBehaviour
    {
        #region VARIABLE
        #region ABILITY
        private enum EnemyType { Human, Generic }
        private enum EnemyClass { Normal, Archer, Wizard }
        private enum EnemyState { Idle, Patrol, Seek, Chase, Attack, Dead }
        [Serializable]
        public struct DamageAbility
        {
            public DamageTrigger damageTrigger;
            public int damageAmount;
        }

        [SerializeField]
        private EnemyType enemyType;
        [ShowIf("enemyType", EnemyType.Human)]
        [SerializeField]
        private EnemyClass enemyClass;
        [SerializeField]
        private EnemyState enemyState;

        #region ENEMY STATS
        [Header("ENEMY STATS")]
        [FoldoutGroup("ENEMY ABILITY")]
        [SerializeField]
        private int healthPoint = 3;
        [FoldoutGroup("ENEMY ABILITY")]
        [SerializeField]
        private float lookSpeed = 1f;
        [FoldoutGroup("ENEMY ABILITY")]
        [SerializeField]
        private float blinkTime = 0.5f;
        [FoldoutGroup("ENEMY ABILITY")]
        [SerializeField]
        private float navMeshLinkSpeed = 0.5f;

        [Header("ENEMY ATTACK")]
        [FoldoutGroup("ENEMY ABILITY")]
        [SerializeField]
        private float attackCooldown = 2.5f;
        [FoldoutGroup("ENEMY ABILITY")]
        [SerializeField]
        private DamageAbility[] damageAbilities;
        #endregion
        #endregion

        #region SEEK AND WONDER
        [Header("SEEK AND WONDER")]
        [FoldoutGroup("SEEK AND WONDER")]
        [SerializeField]
        private float seekDistance = 10f;
        [FoldoutGroup("SEEK AND WONDER")]
        [SerializeField]
        private float seekIdleDuration = 5f;

        [Header("SEARCH PLAYER RADER")]
        [FoldoutGroup("SEEK AND WONDER")]
        [SerializeField]
        private bool debugRader;
        [FoldoutGroup("SEEK AND WONDER")]
        [SerializeField]
        private string targetTag = "Player";
        [FoldoutGroup("SEEK AND WONDER")]
        public LayerMask objectLayerMask;
        [FoldoutGroup("SEEK AND WONDER")]
        [SerializeField]
        private LayerMask ignoreLayerMask;
        [FoldoutGroup("SEEK AND WONDER")]
        [SerializeField]
        private float fieldOfViewAngle = 90;
        [FoldoutGroup("SEEK AND WONDER")]
        [SerializeField]
        private float viewDistance = 1000;
        [FoldoutGroup("SEEK AND WONDER")]
        [SerializeField]
        private Vector3 offset;
        [FoldoutGroup("SEEK AND WONDER")]
        [SerializeField]
        private Vector3 targetOffset;
        [FoldoutGroup("SEEK AND WONDER")]
        [SerializeField]
        private bool usePhysics2D;
        [FoldoutGroup("SEEK AND WONDER")]
        [SerializeField]
        private float angleOffset2D;
        [FoldoutGroup("SEEK AND WONDER")]
        [SerializeField]
        private bool useTargetBone;
        [FoldoutGroup("SEEK AND WONDER")]
        [ShowIf("useTargetBone")]
        [SerializeField]
        private HumanBodyBones targetBone;
        #endregion

        #region COMPONENTS
        [Header("COMPONENTS")]
        [FoldoutGroup("COMPONENTS")]
        [SerializeField]
        private Animator anim;
        [FoldoutGroup("COMPONENTS")]
        [SerializeField]
        private NavMeshAgent navAgent;
        [FoldoutGroup("COMPONENTS")]
        [SerializeField]
        private AttackColliderSwitch attackColliderSwitch;
        [ShowIf("enemyType", EnemyType.Human)]
        [FoldoutGroup("COMPONENTS")]
        [SerializeField]
        private RuntimeAnimatorController archerAC, wizardAC;
        [FoldoutGroup("COMPONENTS")]
        [SerializeField]
        private bool canShotProjectile;
        [ShowIf("canShotProjectile")]
        [FoldoutGroup("COMPONENTS")]
        [SerializeField]
        private LineRenderer shotLineRenderer;
        [ShowIf("canShotProjectile")]
        [FoldoutGroup("COMPONENTS")]
        [SerializeField]
        private Transform arrowLineStartPos;
        #endregion

        #region EQUIPMENT
        [ShowIf("enemyType", EnemyType.Human)]
        [FoldoutGroup("EQUIPMENT")]
        [SerializeField]
        private GameObject[] equipments;
        #endregion

        #region FEEDBACK
        [Header("FEEDBACK")]
        [FoldoutGroup("FEEDBACK")]
        [SerializeField]
        private MMFeedbacks feedback_Attack;
        [FoldoutGroup("FEEDBACK")]
        [SerializeField]
        private MMFeedbacks feedback_Knockback;
        [FoldoutGroup("FEEDBACK")]
        [SerializeField]
        private MMFeedbacks feedback_Dead;
        #endregion

        #region ETC
        private GameObject targetObject;
        private Transform thisTransform;
        private EnemyHealthBar enemyHealthBar;
        private Collider thisCollider;
        private WaitForSeconds ws_State;
        private NavMeshHit navHit;
        private Vector3 randomDirection, arrowLineEndPos;

        private readonly string _Tag_DamageMelee = "DamageMelee";
        private readonly string _Tag_DamageObject = "DamageObject";
        private readonly string _String_EnemyHP = "EnemyHP"; 
        private readonly string _String_Arrow = "Arrow"; 
        private readonly string _String_Fireball = "Fireball"; 

        private int m_AnimPara_isMove,
                    m_AnimPara_MoveBlend,
                    m_AnimPara_MeleeSpeed,
                    m_AnimPara_Attack,
                    m_AnimPara_isFight,
                    m_AnimPara_isAttack,
                    m_AnimPara_Dead,
                    m_AnimPara_Jump,
                    m_AnimPara_Aimming,
                    m_AnimPara_ShotArrow,
                    m_AnimPara_Spell;

        private float stateTime, blinkTimer, attackTimer, seekIdleTimer;
        private float defaultSpeed, defaultStopDist, attackDist, targetWeight, layerChangeSpeed;
        private int currentHealth, targetLayer;
        private bool isDead, isDamaged, isHealthBarAttached, isNavMeshLink, isOnShotLine, isChangeLayerWeight;
        #endregion
        #endregion

        #region AWAKE, ONENABLE
        private void Awake()
        {
            thisTransform = this.transform;
            stateTime = 0.3f;
            ws_State = new WaitForSeconds(stateTime);
            defaultSpeed = navAgent.speed;
            defaultStopDist = navAgent.stoppingDistance;

            if (!anim) anim = this.GetComponent<Animator>();
            if (!navAgent) navAgent = this.GetComponent<NavMeshAgent>();
            if (!thisCollider) thisCollider = this.GetComponent<Collider>();

            if (enemyType == EnemyType.Human)
            {
                if (enemyClass == EnemyClass.Normal)
                {
                    equipments[0].SetActive(true);
                }
                else if (enemyClass == EnemyClass.Archer)
                {
                    equipments[1].SetActive(true);
                    anim.runtimeAnimatorController = archerAC;

                    m_AnimPara_Aimming = anim.GetParameter(9).nameHash;
                    m_AnimPara_ShotArrow = anim.GetParameter(10).nameHash;
                }
                else if (enemyClass == EnemyClass.Wizard)
                {
                    equipments[2].SetActive(true);
                    anim.runtimeAnimatorController = wizardAC;
                    m_AnimPara_Spell = anim.GetParameter(9).nameHash;
                }
            }

            m_AnimPara_MoveBlend = anim.GetParameter(0).nameHash;
            m_AnimPara_Attack = anim.GetParameter(1).nameHash;
            m_AnimPara_isMove = anim.GetParameter(3).nameHash;
            m_AnimPara_isFight = anim.GetParameter(4).nameHash;
            m_AnimPara_isAttack = anim.GetParameter(5).nameHash;
            m_AnimPara_Dead = anim.GetParameter(6).nameHash;
            m_AnimPara_Jump = anim.GetParameter(7).nameHash;
            m_AnimPara_MeleeSpeed = anim.GetParameter(8).nameHash;

            for (int i = 0; i < damageAbilities.Length; i++)
            {
                damageAbilities[i].damageTrigger.damageAmount = damageAbilities[i].damageAmount;
            }
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
            attackTimer = attackCooldown * 0.7f;
            seekIdleTimer = 0; 
            enemyState = EnemyState.Seek;
            anim.SetFloat(m_AnimPara_MoveBlend, 0);
            anim.SetBool(m_AnimPara_isMove, false);
            anim.SetBool(m_AnimPara_isFight, false);
            anim.SetBool(m_AnimPara_isAttack, false);
            thisCollider.enabled = true;

            if (enemyHealthBar) 
            {
                enemyHealthBar.Unssaign();
                enemyHealthBar = null;
            }

            if (enemyType == EnemyType.Human)
            {
                if (enemyClass == EnemyClass.Normal)
                {
                    attackDist = navAgent.stoppingDistance + 0.1f;
                    anim.SetFloat(m_AnimPara_MeleeSpeed, 1.1f);
                }
                else if (enemyClass == EnemyClass.Archer)
                {
                    attackDist = 12;
                    anim.SetFloat(m_AnimPara_MeleeSpeed, 1f);
                }
                else if (enemyClass == EnemyClass.Wizard)
                {
                    attackDist = 10;
                    anim.SetFloat(m_AnimPara_MeleeSpeed, 0.9f);
                }
            }
            else
            {
                attackDist = navAgent.stoppingDistance + 0.1f;
                anim.SetFloat(m_AnimPara_MeleeSpeed, 1f);
            }

            StartCoroutine(StateCoroutine());
        }
        #endregion

        #region LOOP, UPDATE
        private IEnumerator StateCoroutine()
        {
            yield return ws_State;

            while (!isDead)
            {
                switch (enemyState)
                {
                    case EnemyState.Idle:
                        if (!PlayerController.Instance.isDead) Rader();
                        break;
                    case EnemyState.Patrol:
                        if (!PlayerController.Instance.isDead) Rader();
                        break;
                    case EnemyState.Seek:
                        if (!PlayerController.Instance.isDead) Rader();
                        if (!navAgent.hasPath)
                        {
                            if (seekIdleTimer > seekIdleDuration)
                            {
                                Vector3 randomPos = SeekAndWonder(seekDistance, 6);
                                navAgent.SetDestination(randomPos);
                                anim.SetBool(m_AnimPara_isMove, true);
                                seekIdleTimer = 0;
                                navAgent.speed = 2;
                                navAgent.stoppingDistance = 0;
                            }
                            else
                            {
                                if (anim.GetBool(m_AnimPara_isMove)) anim.SetBool(m_AnimPara_isMove, false);
                                seekIdleTimer += stateTime;
                            }
                        }
                        break;
                    case EnemyState.Chase:
                        if (targetObject)
                        {
                            if (!CheckPlayerAlive()) break;

                            if (!anim.GetBool(m_AnimPara_isAttack))
                            {
                                if (navAgent.isStopped) navAgent.isStopped = false;
                                if (!navAgent.pathPending)
                                {
                                    if (!isDamaged)
                                    {
                                        navAgent.SetDestination(targetObject.transform.position);
                                    }
                                }
                                if (navAgent.remainingDistance != 0 && navAgent.remainingDistance <= attackDist)
                                {
                                    enemyState = EnemyState.Attack;
                                    anim.SetBool(m_AnimPara_isFight, true);
                                    anim.SetBool(m_AnimPara_isMove, false);
                                    break;
                                }
                                // Animation
                                if (!anim.GetBool(m_AnimPara_isMove)) anim.SetBool(m_AnimPara_isMove, true);
                            }
                        }
                        break;
                    case EnemyState.Attack:
                        if (!anim.GetBool(m_AnimPara_isAttack))
                        {
                            if (!CheckPlayerAlive()) break;

                            if (!navAgent.isStopped) navAgent.isStopped = true;

                            // TARGET 과 거리 측정 후 STATE 재설정(공격 중이 아닐 때)
                            float targetDist = Vector3.Distance(thisTransform.position, targetObject.transform.position);
                            if (targetDist > attackDist)
                            {
                                anim.SetBool(m_AnimPara_isMove, true);
                                enemyState = EnemyState.Chase;
                                break;
                            }

                            if (anim.GetBool(m_AnimPara_isMove)) anim.SetBool(m_AnimPara_isMove, false);

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

                                    if (enemyType == EnemyType.Human)
                                    {
                                        if (targetDist <= navAgent.stoppingDistance + 0.1f)
                                        {
                                            Attacks();
                                        }
                                        else
                                        {
                                            if (enemyClass == EnemyClass.Archer)
                                            {
                                                Attacks(1);
                                            }
                                            else if (enemyClass == EnemyClass.Wizard)
                                            {
                                                Attacks(2);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        Attacks();
                                    }
                                }
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

                if (isOnShotLine)
                {
                    shotLineRenderer.SetPosition(0, arrowLineStartPos.position);
                    arrowLineEndPos = targetObject.transform.position;
                    arrowLineEndPos.y = arrowLineStartPos.position.y;
                    shotLineRenderer.SetPosition(1, arrowLineEndPos);
                }
            }
            else
            {
                if (navAgent.velocity.sqrMagnitude >= 0.1f * 0.1f && navAgent.remainingDistance <= 0.1f)
                {
                    anim.SetBool(m_AnimPara_isMove, false); // 걷는 애니메이션 중지
                }
                else if (navAgent.desiredVelocity.sqrMagnitude >= 0.1f * 0.1f)
                {
                    // 에이전트의 이동방향
                    Vector3 direction = navAgent.desiredVelocity;
                    // 회전각도(쿼터니언) 산출
                    Quaternion targetAngle = Quaternion.LookRotation(direction);
                    // 선형보간 함수를 이용해 부드러운 회전
                    thisTransform.rotation = Quaternion.Slerp(thisTransform.rotation, targetAngle, Time.deltaTime * 8.0f);
                }

                // MoveBlend
                float moveBlend = anim.GetFloat(m_AnimPara_MoveBlend);
                if (navAgent.velocity.magnitude > navAgent.speed - 0.1f)
                {
                    if (moveBlend < 0.95f) anim.SetFloat(m_AnimPara_MoveBlend, moveBlend + 0.01f);
                }
                else
                {
                    if (moveBlend > 0.2f) anim.SetFloat(m_AnimPara_MoveBlend, moveBlend - 0.01f);
                }
            }

            if (navAgent.isOnOffMeshLink && !isNavMeshLink)
            {
                isNavMeshLink = true;
                navAgent.speed *= navMeshLinkSpeed;
                anim.SetTrigger(m_AnimPara_Jump);
            }
            else if (!navAgent.isOnOffMeshLink && isNavMeshLink)
            {
                isNavMeshLink = false;
                navAgent.velocity = Vector3.zero;
                navAgent.speed = defaultSpeed;
            }

            if (isChangeLayerWeight)
            {
                float cw = anim.GetLayerWeight(targetLayer);
                if (cw != targetWeight)
                {
                    var currentWeight = Mathf.Lerp(cw, targetWeight, Time.deltaTime * layerChangeSpeed);
                    anim.SetLayerWeight(1, currentWeight);
                }
                else
                {
                    isChangeLayerWeight = false;
                }
            }
        }
        #endregion

        #region TRIGGER
        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag(_Tag_DamageMelee) || other.CompareTag(_Tag_DamageObject))
            {
                if (!isDead && !isDamaged)
                {
                    if (other.gameObject.layer == 9) // Player Attack
                    {
                        currentHealth -= PlayerController.Instance.attackPower;
                    }
                    else if (other.gameObject.layer == 10) // Object Damage
                    {
                        currentHealth -= 1;
                    }
                    navAgent.isStopped = true;
                    isDamaged = true;
                    attackColliderSwitch.isCancel = true;
                    feedback_Attack?.StopFeedbacks();

                    if (currentHealth > 0)
                    {
                        feedback_Knockback.PlayFeedbacks();

                        if (enemyState != EnemyState.Attack)
                        {
                            anim.SetBool(m_AnimPara_isFight, true);
                            targetObject = PlayerController.Instance.gameObject;
                            navAgent.speed = defaultSpeed;
                            navAgent.stoppingDistance = defaultStopDist;
                            enemyState = EnemyState.Attack;
                        }

                        if (!isHealthBarAttached)
                        {
                            isHealthBarAttached = true;

                            enemyHealthBar = UIPoolManager.instance.GetObject(_String_EnemyHP, thisTransform.position).GetComponent<EnemyHealthBar>();
                            enemyHealthBar.Assign(thisTransform, healthPoint);
                        }
                    }
                    else
                    {
                        isDead = true;
                        enemyState = EnemyState.Dead;
                        anim.SetTrigger(m_AnimPara_Dead);
                        feedback_Dead?.PlayFeedbacks();
                        thisCollider.enabled = false;
                        if (enemyHealthBar)
                        {
                            enemyHealthBar.Unssaign();
                            enemyHealthBar = null;
                            isHealthBarAttached = false;
                        }
                    }

                    if (isHealthBarAttached) enemyHealthBar.UpdateState(currentHealth);
                }
            }
        }
        #endregion

        #region ATTACK
        private void Attacks(int type = 0)
        {
            // MELLEE
            if (type == 0)
            {
                anim.SetTrigger(m_AnimPara_Attack);
            }
            // SHOT ARROW
            if (type == 1)
            {
                anim.SetBool(m_AnimPara_isAttack, true);
                ChangeLayerWeight(1, 1, 5f);
                anim.SetTrigger(m_AnimPara_Aimming);
                shotLineRenderer.gameObject.SetActive(true);
                isOnShotLine = true;
                Sequence shotArrow = DOTween.Sequence();
                shotArrow
                    .AppendInterval(2f)
                    .AppendCallback(() => 
                    {
                        if (!isDamaged)
                        {
                            shotLineRenderer.SetPosition(1, arrowLineStartPos.position);
                            shotLineRenderer.gameObject.SetActive(false);
                            isOnShotLine = false;
                            anim.SetTrigger(m_AnimPara_ShotArrow);

                            // Arrow
                            PoolManager.instance.GetObject(_String_Arrow, arrowLineStartPos.position, arrowLineStartPos.rotation);

                        }
                    })
                    .AppendInterval(0.4f)
                    .AppendCallback(()=>
                    {
                        ChangeLayerWeight(1, 0, 2f);
                        anim.SetBool(m_AnimPara_isAttack, false);
                    });
            }
            // SPELL
            if (type == 2)
            {
                anim.SetTrigger(m_AnimPara_Spell);
            }
        }

        public void MeleeAttack()
        {
            // Feedback
            feedback_Attack?.PlayFeedbacks();
            // Collider On
            attackColliderSwitch?.DoAttack();
        }

        public void SpellAttack()
        {
            // Fireball
            PoolManager.instance.GetObject(_String_Fireball, arrowLineStartPos.position, arrowLineStartPos.rotation);
        }
        #endregion

        #region UTILITY
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
            randomDirection = UnityEngine.Random.insideUnitSphere * distance;

            randomDirection += thisTransform.position;

            NavMesh.SamplePosition(randomDirection, out navHit, distance, NavMesh.AllAreas);

            return navHit.position;
        }

        // FIGHT MODE 시 타겟 바라보기
        private void FollowTarget()
        {
            Vector3 dir = targetObject.transform.position - thisTransform.position;
            thisTransform.rotation = Quaternion.Lerp(thisTransform.rotation, Quaternion.LookRotation(dir), Time.deltaTime * lookSpeed);
        }

        private void ChangeLayerWeight(int layerNum, float to, float speed)
        {
            targetLayer = layerNum;
            targetWeight = to;
            layerChangeSpeed = speed;
            isChangeLayerWeight = true;
        }

        private bool CheckPlayerAlive()
        {
            if (PlayerController.Instance.isDead)
            {
                anim.SetBool(m_AnimPara_isFight, false);
                navAgent.ResetPath();
                enemyState = EnemyState.Seek;
                return false;
            }
            else
            {
                return true;
            }
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
        #endregion
    }
}
