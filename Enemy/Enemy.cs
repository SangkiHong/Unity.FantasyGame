using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using Sirenix.OdinInspector;
using EPOOutline;
using MoreMountains.Feedbacks;
using BehaviorDesigner.Runtime.Tasks.Movement;
using DG.Tweening;

namespace Sangki.Enemy
{
    public enum EnemyClass { Normal, Archer, Wizard }
    public enum EnemyState { Idle, Patrol, Seek, Chase, Attack, Dead }

    public abstract class Enemy : MonoBehaviour
    {
        [System.Serializable]
        public struct DamageAbility
        {
            public Object.DamageTrigger damageTrigger;
            public int damageAmount;
        }

        [SerializeField]
        private EnemyClass enemyClass;
        internal EnemyState enemyState;

        #region ENEMY STATS
        [Header("ENEMY STATS")]
        [FoldoutGroup("ENEMY ABILITY")]
        [SerializeField]
        public int healthPoint = 3;
        [FoldoutGroup("ENEMY ABILITY")]
        [SerializeField]
        public float lookSpeed = 1f;
        [FoldoutGroup("ENEMY ABILITY")]
        [SerializeField]
        public float blinkTime = 0.5f;
        [FoldoutGroup("ENEMY ABILITY")]
        [SerializeField]
        public float navMeshLinkSpeed = 0.5f;
        [FoldoutGroup("ENEMY ABILITY")]
        [SerializeField]
        public bool isBoss;

        [Header("ENEMY ATTACK")]
        [FoldoutGroup("ENEMY ABILITY")]
        [SerializeField]
        public float attackCooldown = 2.5f;
        [FoldoutGroup("ENEMY ABILITY")]
        [SerializeField]
        public float attackStepsize = 0.5f;
        [FoldoutGroup("ENEMY ABILITY")]
        [SerializeField]
        public float dodgeChance = 0.3f;
        [FoldoutGroup("ENEMY ABILITY")]
        [SerializeField]
        public float dodgeAngle = 30;
        [FoldoutGroup("ENEMY ABILITY")]
        [SerializeField]
        public float dodgeDistance = 5f;
        [FoldoutGroup("ENEMY ABILITY")]
        [SerializeField]
        public bool canCounterAttack;
        [FoldoutGroup("ENEMY ABILITY")]
        [ShowIf("canCounterAttack")]
        [SerializeField]
        public float counterattackChance = 0.3f;
        [FoldoutGroup("ENEMY ABILITY")]
        [SerializeField]
        public DamageAbility[] damageAbilities;
        [ShowIf("isBoss")]
        [FoldoutGroup("ENEMY ABILITY")]
        [SerializeField]
        public EnemyBoss enemyBoss;
        [ShowIf("isBoss")]
        [FoldoutGroup("ENEMY ABILITY")]
        [SerializeField]
        public float bossSkillCooldown = 6f;
        #endregion

        #region SEEK AND WONDER
        [Header("SEEK AND WONDER")]
        [FoldoutGroup("SEEK AND WONDER")]
        [SerializeField]
        public float seekDistance = 10f;
        [FoldoutGroup("SEEK AND WONDER")]
        [SerializeField]
        public float seekIdleDuration = 5f;

        [Header("SEARCH PLAYER FindTarget")]
        [FoldoutGroup("SEEK AND WONDER")]
        [SerializeField]
        public bool debugRader;
        [FoldoutGroup("SEEK AND WONDER")]
        public LayerMask objectLayerMask;
        [FoldoutGroup("SEEK AND WONDER")]
        [SerializeField]
        public LayerMask ignoreLayerMask;
        [FoldoutGroup("SEEK AND WONDER")]
        [SerializeField]
        public float fieldOfViewAngle = 90;
        [FoldoutGroup("SEEK AND WONDER")]
        [SerializeField]
        public float viewDistance = 1000;
        [FoldoutGroup("SEEK AND WONDER")]
        [SerializeField]
        public Vector3 offset;
        [FoldoutGroup("SEEK AND WONDER")]
        [SerializeField]
        public Vector3 targetOffset;
        [FoldoutGroup("SEEK AND WONDER")]
        [SerializeField]
        public bool usePhysics2D;
        [FoldoutGroup("SEEK AND WONDER")]
        [SerializeField]
        public float angleOffset2D;
        [FoldoutGroup("SEEK AND WONDER")]
        [SerializeField]
        public bool useTargetBone;
        [FoldoutGroup("SEEK AND WONDER")]
        [ShowIf("useTargetBone")]
        [SerializeField]
        public HumanBodyBones targetBone;
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
        [FoldoutGroup("COMPONENTS")]
        [SerializeField]
        private Outlinable outline;
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

        #region SOUND
        [FoldoutGroup("SOUND")]
        [SerializeField]
        private string hitSound;
        [FoldoutGroup("SOUND")]
        [SerializeField]
        private string dieSound;
        #endregion

        #region PROPERTY
        public Outlinable Outline => outline;
        #endregion
        
        #region ETC
        internal GameObject targetObject;
        internal Transform thisTransform;
        internal EnemyStatsUI enemyHealthBar;
        internal Collider thisCollider;
        internal WaitForSeconds ws_State;
        internal Rigidbody _rigidbody;
        internal Sequence shotArrowSequence;
        internal NavMeshHit navHit;
        internal Vector3 randomDirection;

        internal readonly string _ObjectPool_ImpactParticle = "SwordImpactOrange";
        internal readonly string _Tag_Player = "Player";
        internal readonly string _Tag_DamageMelee = "DamageMelee";
        internal readonly string _Tag_DamageObject = "DamageObject";
        internal readonly string _String_EnemyStatsUI = "EnemyStatsUI";
        internal readonly string _String_Arrow = "Arrow";
        internal readonly string _String_Fireball = "Fireball";

        internal float stateTime, blinkTimer, attackTimer, seekIdleTimer, skillTimer;
        internal float defaultSpeed, defaultStopDist, attackDist, targetDist, targetWeight, layerChangeSpeed;
        internal int currentHealth, targetLayer;

        internal int m_AnimPara_isMove,
                    m_AnimPara_MoveBlend,
                    m_AnimPara_MeleeSpeed,
                    m_AnimPara_Attack,
                    m_AnimPara_isFight,
                    m_AnimPara_isAttack,
                    m_AnimPara_Dead,
                    m_AnimPara_Jump,
                    m_AnimPara_Aimming,
                    m_AnimPara_ShotArrow,
                    m_AnimPara_CastSkill,
                    m_AnimPara_Dodge,
                    m_AnimPara_Counterattack;

        [HideInInspector]
        public bool isDead;

        internal bool isDamaged, isDodge, isNavMeshLink, 
                      isOnShotLine, isChangeLayerWeight, isCastingSkill;
#endregion


        public void Awake()
        {
            thisTransform = this.transform;
        }

        public abstract void Attack();

        public abstract void Die();

        public void DodgeAttack()
        {
            if (!isDodge && enemyState == EnemyState.Attack && targetDist <= navAgent.stoppingDistance + 0.1f)
            {
                if (Random.value < dodgeChance)
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
                                if (canCounterAttack && Random.value < counterattackChance)
                                {
                                    CounterAttack();
                                }
                            });
                        anim.SetTrigger(m_AnimPara_Dodge);
                    }
                }
            }
        }

        public void CounterAttack()
        {
            navAgent.isStopped = false;
            navAgent.Warp(thisTransform.position);
            navAgent.SetDestination(targetObject.transform.position);

            anim.SetBool(m_AnimPara_isAttack, true);
            anim.SetTrigger(m_AnimPara_Counterattack);
        }


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
        private void FindTarget()
        {
            if (targetObject = MovementUtility.WithinSight(thisTransform, offset, fieldOfViewAngle, viewDistance, PlayerController.Instance.gameObject, targetOffset, ignoreLayerMask, useTargetBone, targetBone))
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

        private void OnDestroy()
        {
            MovementUtility.ClearCache();

            Player.PlayerController.Instance.OnPlayerAttack -= DodgeAttack;
        }
    }
}