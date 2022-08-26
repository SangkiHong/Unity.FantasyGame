using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using DarkTonic.MasterAudio;
using DG.Tweening;
using MoreMountains.Feedbacks;
using Sirenix.OdinInspector;
using SK.FSM;
using SK.Manager;
using SK.States;
using SK.Utility;

namespace SK.Player
{
    public class PlayerController : MonoBehaviour, IDamageable
    {
        public static PlayerController Instance;

        [SerializeField]
        private bool onDebug;

        #region VARIABLE
        #region UNITY ACTION
        public event UnityAction<int> OnDamagedReceived;
        public event UnityAction OnPlayerAttack;
        public event UnityAction OnPlayerDied;
        public event UnityAction OnInteractable;
        #endregion

        #region COMPONENTS
        [Header("COMPONENTS")]
        [FoldoutGroup("COMPONENTS")]
        [SerializeField] internal Transform thisTransform;
        [FoldoutGroup("COMPONENTS")]
        [SerializeField] internal Animator anim;
        [FoldoutGroup("COMPONENTS")]
        [SerializeField] internal CharacterController characterController;
        [FoldoutGroup("COMPONENTS")]
        [SerializeField] internal TargetingSystem targetingSystem;
        [FoldoutGroup("COMPONENTS")]
        [SerializeField] private Transform shieldParent;
        #endregion

        #region INFO
        [Header("INFO")]
        [SerializeField] internal string CurrentState;
        [SerializeField] internal Vector3 movemnt;
        #endregion

        #region PLAYER STATES
        [Header("PLAYER STATES")]
        [SerializeField] internal bool canDamage;
        [SerializeField] private bool _isDead, _isOnGround, _isFalling;
        #endregion

        #region STATS
        [Header("STATS")]
        [FoldoutGroup("STATS")]
        [SerializeField] internal PlayerData playerData;
        [FoldoutGroup("STATS")]
        [SerializeField] private int currentHealth;
        #endregion

        #region FEEDBACKS
        [Header("FEEDBACKS")]
        [FoldoutGroup("FEEDBACKS")]
        public MMFeedbacks feedback_Jump;
        [FoldoutGroup("FEEDBACKS")]
        public MMFeedbacks feedbackm_Attack;
        [FoldoutGroup("FEEDBACKS")]
        public MMFeedbacks feedback_Knockback;
        [FoldoutGroup("FEEDBACKS")]
        public MMFeedbacks feedback_ShieldDefense;
        [FoldoutGroup("FEEDBACKS")]
        public MMFeedbacks feedback_Parrying;
        [FoldoutGroup("FEEDBACKS")]
        public MMFeedbacks feedback_Interacting;
        [FoldoutGroup("FEEDBACKS")]
        public MMFeedbacks feedback_Death;
        #endregion

        #region ELSE
        [Header("ELSE")]
        [FoldoutGroup("ELSE")]
        [SerializeField] private LayerMask _fieldLayer;
        #endregion

        #region PROPERTIES
        public bool IsDead => _isDead;
        public bool IsOnGround => _isOnGround;
        public int AttackPower => playerData.attackPower;
        public int MaxHealth => playerData.maxHealth;
        public float JumpTime => playerData.jumpTime;
        public float JumpForce => playerData.jumpForce;
        public float JumpIntervalDelay => playerData.jumpIntervalDelay;
        public float DiveRollingForce => playerData.diveRollingForce;
        public float BlinkTime => playerData.blinkTime;
        public float ParryingTime => playerData.parryingTime;
        #endregion

        #region ETC
        private StateMachine stateMachine;

        internal float deltaTime, fixedDeltaTime, jumpIntervalTimer;
        private float fallingTimer;
        #endregion
        #endregion

        #region 유니티 이벤트 함수
        private void Awake()
        {
            if (Instance != null) Destroy(this);
            else Instance = this;

            thisTransform = this.transform;
            currentHealth = MaxHealth;

            stateMachine = new StateMachine(this);
        }

        private void Start()
        {
            // 인풋 함수 할당
            InputManager.Instance.Input_Button_Jump += Button_Jump;
            InputManager.Instance.Input_Button_Dodge += Button_Dodge;
            InputManager.Instance.Input_Button_Attack += Button_Attack;
            InputManager.Instance.Input_Button_Shield += Button_Shield;
            InputManager.Instance.Input_Button_Interacting += Button_Interacting;
            InputManager.Instance.Input_Button_Targeting += Button_Targeting;
        }

        public void Tick()
        {
            deltaTime = Time.deltaTime;

            if (stateMachine.CurrentState != null)
                stateMachine.CurrentState.Tick();
        }

        public void FixedTick()
        {
            fixedDeltaTime = Time.fixedDeltaTime;

            CheckGrounded();

            if (!IsDead)
            {
                if (stateMachine.CurrentState != stateMachine.STATE_Attack && 
                    stateMachine.CurrentState != stateMachine.STATE_Damaged)
                {
                    RunTimer();
                }
            }

            if (stateMachine.CurrentState != null)
                stateMachine.CurrentState.FixedTick();
        }
        #endregion

        #region 컨트롤
        public void Button_Jump(InputState inputState)
        {
            // 버튼을 눌렀을 경우
            if (inputState == InputState.onStart)
            {
                if (!IsOnGround || stateMachine.CurrentState != stateMachine.STATE_Locomotion)
                    return;

                // 타겟팅 중이 아닌 경우 일반 점프
                if (!targetingSystem.IsOnTargeting)
                    stateMachine.ChangeState(stateMachine.STATE_Jump);
                else
                { // 타겟팅 중인 경우 닷지 작동{
                  // Initialize
                    InputManager.Instance.SetControlState(false);

                    // 애니메이션
                    //anim.SetTrigger(Strings.AnimPara_Dodge);

                    // 닷지 방향(캐릭터 전방)
                    if (targetingSystem.IsOnTargeting)
                        stateMachine.STATE_Locomotion.ExecuteDodge(thisTransform.forward);
                }
            }
            // 버튼을 뗐을 경우
            else if (inputState == InputState.onEnd)
            {
                if (stateMachine.STATE_Jump.ButtonOn)
                    stateMachine.STATE_Jump.StopJump();
            }
        }

        public void Button_Attack(InputState inputState)
        {
            if (!IsOnGround || stateMachine.CurrentState == stateMachine.STATE_Damaged || 
                stateMachine.CurrentState == stateMachine.STATE_Jump)
                return;

            OnPlayerAttack?.Invoke();
        }

        public void Button_Dodge()
        {
            if (!IsOnGround || stateMachine.CurrentState != stateMachine.STATE_Locomotion)
                return;

        }

        public void Button_Shield(InputState inputState)
        {
            if (IsOnGround)
            {
                
            }
        }

        public void Button_Interacting(InputState inputState)
        {
            if (stateMachine.CurrentState == stateMachine.STATE_Jump ||
                stateMachine.CurrentState == stateMachine.STATE_Damaged ||
                stateMachine.CurrentState == stateMachine.STATE_Dead) return;

            if (OnInteractable != null)
            {
                OnInteractable.Invoke();
                feedback_Interacting.PlayFeedbacks();
            }
        }

        public void Button_Targeting(InputState inputState)
        {
            if (inputState == InputState.onStart)
                targetingSystem.SetTargeting(true);
            else if (inputState == InputState.onEnd)
                targetingSystem.SetTargeting(false);
        }
        #endregion

        #region 데미지
        public void Damage(int damageAmount)
        {
            /*isOnShield = false;
            isRolling = false;
            //attackColliderSwitch.isCancel = true;

            ResetAllTriggers();

            shieldLayerWeight = 0;
            anim.SetLayerWeight(1, 0);

            currentHealth -= damageAmount;

            // Event
            if (OnDamagedReceived != null) OnDamagedReceived(currentHealth);

            // Damaged
            if (currentHealth > 0)
            {
                stateMachine.ChangeState(stateMachine.STATE_Damaged);
                feedback_Knockback.PlayFeedbacks();
            }
            // Died
            else
            {
                isDead = true;
                isOnControl = false;
                stateMachine.ChangeState(stateMachine.STATE_Dead);
                feedback_Death.PlayFeedbacks();
            }*/
        }

        /*public void OnDamageTrigger(GameObject triggerObejct, int damageAmount)
        {
            if (onDebug && !canDamage && isDead && stateMachine.CurrentState == stateMachine.STATE_Damaged) return;

            bool isDamageObejct = triggerObejct.CompareTag(Strings.Tag_DamageObject);

            if (isDamageObejct)
            {
                //if (isOnShield)
                {
                    // 적의 방향 각도에 따른 쉴드 처리 또는 패링
                    //if (IsSuccessShield(isEnemy, triggerObejct.transform.position - thisTransform.position)) return;
                }

                if (!targetingSystem.IsOnTargeting) RotateToTarget(triggerObejct.transform.position, 0.5f);

                Damage(damageAmount);
            }
        }*/

        private void ResetAllTriggers()
        {
            anim.ResetTrigger(Strings.AnimPara_Attack);
            anim.ResetTrigger(Strings.AnimPara_Parrying);
            anim.ResetTrigger(Strings.AnimPara_Falling);
            anim.ResetTrigger(Strings.AnimPara_Dodge);
            anim.ResetTrigger(Strings.AnimPara_Land);
        }
        #endregion

        #region 애니메이션
        public void FootSound() => MasterAudio.PlaySound(Strings.Sound_FootSound);
        #endregion

        #region UTILITY
        private void RunTimer()
        { 
            // Jump Interval Timer
            if (jumpIntervalTimer > 0) jumpIntervalTimer -= fixedDeltaTime;

            // Falling Timer
            if (!IsOnGround)
            {
                if (!_isFalling)
                {
                    if (fallingTimer < 0.55f)
                    {
                        fallingTimer += fixedDeltaTime;
                    }
                    else
                    {
                        _isFalling = true;
                        fallingTimer = 0;
                        anim.SetTrigger(Strings.AnimPara_Falling); // Falling Animation
                    }
                }
            }
            else
            {
                if (_isFalling)
                {
                    _isFalling = false;
                    anim.SetTrigger(Strings.AnimPara_Land); // Landing Animation
                }
                if (fallingTimer > 0) fallingTimer = 0;
            }
        }

        private void CheckGrounded()
        {
            // CharacterController.IsGrounded가 true라면 Raycast를 사용하지 않고 판정 종료
            if (characterController.isGrounded) _isOnGround = true;

            // 발사하는 광선의 초기 위치와 방향
            // 약간 신체에 박혀 있는 위치로부터 발사하지 않으면 제대로 판정할 수 없을 때가 있다.
            var maxDistance = 0.2f;

            var ray1 = new Ray(thisTransform.position - (thisTransform.forward * 0.3f), Vector3.down * maxDistance);

            Debug.DrawRay(thisTransform.position - (thisTransform.forward * 0.3f), Vector3.down * maxDistance, Color.green);

            _isOnGround = Physics.Raycast(ray1, maxDistance, _fieldLayer);

            if (!_isOnGround && stateMachine.CurrentState == stateMachine.STATE_Locomotion)
            {
                Vector3 slipDirection = thisTransform.forward + (Vector3.down * 10);
                characterController.Move(slipDirection * fixedDeltaTime);
            }
        }

        internal void RotateToTarget(Vector3 targetPos, float duration)
        {
            Vector3 dirToTarget = targetPos - thisTransform.position;
            dirToTarget = Quaternion.LookRotation(dirToTarget, Vector3.up).eulerAngles;
            dirToTarget.x = 0; dirToTarget.z = 0;

            if (duration > 0) thisTransform.DORotate(dirToTarget, duration);
            else thisTransform.rotation = Quaternion.Euler(dirToTarget);
        }
        #endregion
    }
}