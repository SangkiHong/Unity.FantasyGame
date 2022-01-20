using BehaviorDesigner.Runtime.Tasks.Movement;
using DG.Tweening;
using MoreMountains.Feedbacks;
using Sangki.Player;
using Sangki.Object;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;
using Sirenix.OdinInspector;

namespace Sangki.Enemy
{
    [RequireComponent(typeof(NavMeshAgent))]
    [RequireComponent(typeof(Animator))]
    public class EnemyContoller : MonoBehaviour, IDamageable
    {
        #region VARIABLE
        #region ABILITY
        [SerializeField]
        private bool noHit;
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
        [FoldoutGroup("ENEMY ABILITY")]
        [SerializeField]
        private bool isBoss;

        [Header("ENEMY ATTACK")]
        [FoldoutGroup("ENEMY ABILITY")]
        [SerializeField]
        private float attackCooldown = 2.5f;
        [FoldoutGroup("ENEMY ABILITY")]
        [SerializeField]
        private float attackStepsize = 0.5f;
        [FoldoutGroup("ENEMY ABILITY")]
        [SerializeField]
        private float dodgeChance = 0.3f;
        [FoldoutGroup("ENEMY ABILITY")]
        [SerializeField]
        private float dodgeAngle = 30;
        [FoldoutGroup("ENEMY ABILITY")]
        [SerializeField]
        private float dodgeDistance = 5f;
        [FoldoutGroup("ENEMY ABILITY")]
        [SerializeField]
        private float counterattackChance = 0.3f;
        [FoldoutGroup("ENEMY ABILITY")]
        [SerializeField]
        private DamageAbility[] damageAbilities;
        [ShowIf("isBoss")]
        [FoldoutGroup("ENEMY ABILITY")]
        [SerializeField]
        private EnemyBoss enemyBoss;
        [ShowIf("isBoss")]
        [FoldoutGroup("ENEMY ABILITY")]
        [SerializeField]
        private float bossSkillCooldown = 6f;
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
        private EnemyStatsUI enemyHealthBar;
        private Collider thisCollider;
        private WaitForSeconds ws_State;
        private Rigidbody _rigidbody;
        private Sequence shotArrowSequence;
        private NavMeshHit navHit;
        private Vector3 randomDirection;

        private readonly string _Tag_DamageMelee = "DamageMelee";
        private readonly string _Tag_DamageObject = "DamageObject";
        private readonly string _String_EnemyStatsUI = "EnemyStatsUI"; 
        private readonly string _String_Arrow = "Arrow"; 
        private readonly string _String_Fireball = "Fireball";

        private float stateTime, blinkTimer, attackTimer, seekIdleTimer, skillTimer;
        private float defaultSpeed, defaultStopDist, attackDist, targetDist, targetWeight, layerChangeSpeed;
        private int currentHealth, targetLayer;

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
                    m_AnimPara_Spell,
                    m_AnimPara_Dodge,
                    m_AnimPara_Counterattack;
        private bool isDead, 
                     isDamaged,
                     isDodge,
                     isNavMeshLink,
                     isOnShotLine,
                     isChangeLayerWeight,
                     isCastingSkill;
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
            if (!_rigidbody) _rigidbody = this.GetComponent<Rigidbody>();

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

                    m_AnimPara_Aimming = anim.GetParameter(11).nameHash;
                    m_AnimPara_ShotArrow = anim.GetParameter(12).nameHash;
                }
                else if (enemyClass == EnemyClass.Wizard)
                {
                    equipments[2].SetActive(true);
                    anim.runtimeAnimatorController = wizardAC;
                    m_AnimPara_Spell = anim.GetParameter(11).nameHash;
                }
            }
            if (isBoss)
            {
                if (enemyBoss.boss == EnemyBoss.BossName.BudKing) m_AnimPara_Spell = anim.GetParameter(9).nameHash;
            }

            m_AnimPara_MoveBlend = anim.GetParameter(0).nameHash;
            m_AnimPara_Attack = anim.GetParameter(1).nameHash;
            m_AnimPara_isMove = anim.GetParameter(3).nameHash;
            m_AnimPara_isFight = anim.GetParameter(4).nameHash;
            m_AnimPara_isAttack = anim.GetParameter(5).nameHash;
            m_AnimPara_Dead = anim.GetParameter(6).nameHash;
            m_AnimPara_Jump = anim.GetParameter(7).nameHash;
            m_AnimPara_MeleeSpeed = anim.GetParameter(8).nameHash;
            m_AnimPara_Dodge = anim.GetParameter(9).nameHash;
            m_AnimPara_Counterattack = anim.GetParameter(10).nameHash;

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
            blinkTimer = 0;
            attackTimer = attackCooldown * 0.7f;
            seekIdleTimer = 0;
            skillTimer = 0;
            enemyState = EnemyState.Seek;
            anim.SetFloat(m_AnimPara_MoveBlend, 0);
            anim.SetBool(m_AnimPara_isMove, false);
            anim.SetBool(m_AnimPara_isFight, false);
            anim.SetBool(m_AnimPara_isAttack, false);
            thisCollider.enabled = true;


            if (enemyHealthBar) 
            {
                enemyHealthBar.Unassign();
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

            PlayerController.Instance.OnPlayerAttack += DodgeAttack;

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
                            targetDist = Vector3.Distance(thisTransform.position, targetObject.transform.position);
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
                                if (!isDamaged && !isCastingSkill) attackTimer += stateTime;
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

                // Blinker Timer
                if (isDamaged)
                {
                    blinkTimer += stateTime;

                    if (blinkTimer >= blinkTime)
                    {
                        isDamaged = false;
                        blinkTimer = 0;
                    }
                }

                //Boss Skill
                if (isBoss && !isCastingSkill && !anim.GetBool(m_AnimPara_isAttack))
                {
                    if (enemyState == EnemyState.Chase || enemyState == EnemyState.Attack)
                    {
                        if (skillTimer < bossSkillCooldown)
                        {
                            skillTimer += stateTime;
                        }
                        else
                        {
                            StopDuringCastingSkill();
                            anim.SetTrigger(m_AnimPara_Spell);
                        }
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
                    shotLineRenderer.SetPosition(1, arrowLineStartPos.position + arrowLineStartPos.forward * 20);
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

            if (enemyState == EnemyState.Chase && !isDead && !isDodge)
            {
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
                if (!isDead && !isDamaged && !noHit)
                {
                    if (other.gameObject.layer == 9) // Player Attack
                    {
                        Damage(PlayerController.Instance.attackPower);
                    }
                    else // Object Damage
                    {
                        Damage(1);
                    }
                }
            }
        }

        public void Damage(int damageAmount)
        {
            currentHealth -= damageAmount;
            isDamaged = true;
            navAgent.isStopped = true;
            attackColliderSwitch.isCancel = true;
            feedback_Attack?.StopFeedbacks();

            CancelAttack();
            
            if (currentHealth > 0)
            {
                feedback_Knockback.StopFeedbacks();
                feedback_Knockback.PlayFeedbacks();

                if (enemyState != EnemyState.Attack)
                {
                    anim.SetBool(m_AnimPara_isFight, true);
                    targetObject = PlayerController.Instance.gameObject;
                    navAgent.speed = defaultSpeed;
                    navAgent.stoppingDistance = defaultStopDist;
                    enemyState = EnemyState.Attack;
                }
                StickEnemyUI(true, true);
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
                    enemyHealthBar.Unassign();
                    enemyHealthBar = null;
                }
            }

            if (enemyHealthBar) enemyHealthBar.UpdateState(currentHealth);
        }
        #endregion

        #region ATTACK or BEHAVIOR
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

                if (shotArrowSequence == null)
                {
                    shotArrowSequence = DOTween.Sequence()
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
                            PoolManager.instance.GetObject(_String_Arrow, arrowLineStartPos.position, arrowLineStartPos.eulerAngles);

                          }
                      })
                      .AppendInterval(0.4f)
                      .AppendCallback(() =>
                      {
                          ChangeLayerWeight(1, 0, 2f);
                          anim.SetBool(m_AnimPara_isAttack, false);
                      })
                      .SetRecyclable().SetAutoKill(false); // Tween Reuse
                    shotArrowSequence.Play();
                }
                else
                {
                    shotArrowSequence.Restart(); // Tween Reuse
                }
                
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
            // Step
            navAgent.Move(thisTransform.forward * attackStepsize);
        }

        public void SpellAttack()
        {
            // Fireball
            PoolManager.instance.GetObject(_String_Fireball, arrowLineStartPos.position, arrowLineStartPos.eulerAngles);
        }

        public void CastSkill()
        {
            if (isBoss)enemyBoss.BossSkill();
        }

        public void DodgeAttack()
        {
            if (!isDodge && enemyState == EnemyState.Attack && targetDist <= navAgent.stoppingDistance + 0.1f)
            {
                if (UnityEngine.Random.value < dodgeChance)
                {
                    // Cancel Archer aiming
                    if (enemyClass == EnemyClass.Archer)
                    {
                        CancelAttack();
                    }

                    // NavMesh의 길이 있는지 파악 후 위치로 닷지
                    if (NavMesh.SamplePosition(GetDodgePoint(dodgeAngle), out navHit, dodgeChance, NavMesh.AllAreas))
                    {
                        isDodge = true;
                        navAgent.isStopped = true;
                        anim.SetBool(m_AnimPara_isAttack, false);
                        thisTransform.DOMove(navHit.position, 1f)
                            .SetEase(Ease.OutCubic)
                            .OnComplete(() =>
                            {
                                isDodge = false;
                                navAgent.Warp(thisTransform.position);
                                navAgent.isStopped = false;

                                // 닷지 후 반격
                                if (enemyClass == EnemyClass.Normal && UnityEngine.Random.value < counterattackChance)
                                {
                                    navAgent.isStopped = false;
                                    navAgent.Warp(thisTransform.position);
                                    navAgent.SetDestination(targetObject.transform.position);

                                    anim.SetBool(m_AnimPara_isAttack, true);
                                    anim.SetTrigger(m_AnimPara_Counterattack);
                                }
                            });
                        anim.SetTrigger(m_AnimPara_Dodge);
                    }
                }
            }
        }

        private void CancelAttack()
        {
            if (anim.GetBool(m_AnimPara_isAttack))
            {
                anim.SetBool(m_AnimPara_isAttack, false);

                if (isChangeLayerWeight)
                {
                    isChangeLayerWeight = false;
                    anim.SetLayerWeight(1, 0);
                }
                if (shotArrowSequence != null)
                {
                    shotArrowSequence.Pause();
                    shotLineRenderer.gameObject.SetActive(false);
                    isOnShotLine = false;
                }
            }
        }
        #endregion

        #region UTILITY
        // in Casting Skill
        public void StopDuringCastingSkill(bool isStop = true)
        {
            if (isStop)
            {
                isCastingSkill = true;
                navAgent.isStopped = true;
                anim.SetBool(m_AnimPara_isFight, true);
                anim.SetBool(m_AnimPara_isMove, false);
            }
            else
            {
                isCastingSkill = false;
                navAgent.isStopped = false;
                anim.SetBool(m_AnimPara_isFight, false);
                anim.SetBool(m_AnimPara_isMove, true);
                skillTimer = 0;
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

                StickEnemyUI(true, false);
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

        // GET DODGE POINT
        private Vector3 GetDodgePoint(float angle)
        {
            float randomVal = UnityEngine.Random.Range(-1f, 1f); // 반원에서 방향을 랜덤 값으로 정함
            angle = randomVal * angle; // 0 ~ angle 사이의 각을 구함

            return thisTransform.position + (thisTransform.rotation * Quaternion.Euler(0, angle, 0)) * (Vector3.forward * -dodgeDistance);
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

        private void StickEnemyUI(bool toStick, bool isHealthBar = true)
        {
            if (toStick)
            {
                if (!enemyHealthBar)
                    enemyHealthBar = UIPoolManager.instance.GetObject(_String_EnemyStatsUI, thisTransform.position).GetComponent<EnemyStatsUI>();
                
                if (isHealthBar) enemyHealthBar.Assign(thisTransform, healthPoint);
                else enemyHealthBar.Assign(thisTransform, true);
            }
            else
            {
                if (enemyHealthBar) enemyHealthBar.Unassign();
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

            PlayerController.Instance.OnPlayerAttack -= DodgeAttack;
        }
        #endregion
    }
}
