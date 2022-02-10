using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using DarkTonic.MasterAudio;
using DG.Tweening;
using MoreMountains.Feedbacks;
using Sirenix.OdinInspector;
using Sangki.Enemy;
using Sangki.Manager;
using Sangki.States;
using Sangki.Utility;

namespace Sangki.Player
{
    public class PlayerController : MonoBehaviour, IDamageable
    {
        #region VARIABLE
        public static PlayerController Instance;

        [SerializeField]
        private bool onDebug;
        #region UNITY ACTION
        public event UnityAction<int> OnDamagedReceived;
        public event UnityAction OnPlayerAttack;
        public event UnityAction OnPlayerDied;
        public event UnityAction OnInteractable;
        #endregion

        #region COMPONENTS
        [Header("COMPONENTS")]
        [FoldoutGroup("COMPONENTS")]
        public Transform thisTransform;
        [FoldoutGroup("COMPONENTS")]
        public Animator anim;
        [FoldoutGroup("COMPONENTS")]
        public Rigidbody thisRigidbody;
        [FoldoutGroup("COMPONENTS")]
        public AttackColliderSwitch attackColliderSwitch;
        [FoldoutGroup("COMPONENTS")]
        public TargetingSystem targetingSystem;
        [FoldoutGroup("COMPONENTS")]
        [SerializeField]
        private CharacterController characterController;
        [FoldoutGroup("COMPONENTS")]
        [SerializeField]
        private Transform shieldParent;
        #endregion

        #region PLAYER STATES
        [SerializeField]
        public string CurrentState;
        public StateMachine stateMachine;
        public StandingState STATE_Standing;
        public JumpState STATE_Jump;
        public AttackState STATE_Attack;
        public DamagedState STATE_Damaged;
        public DeadState STATE_Dead;
        #endregion

        #region STATS
        [Header("STATS")]
        [FoldoutGroup("STATS")]
        [SerializeField]
        private int currentHealth;
        [FoldoutGroup("STATS")]
        [SerializeField]
        private PlayerData data;
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

        #region PARTICLE
        [Header("PARTICLE")]
        [FoldoutGroup("PARTICLE")]
        public ParticleSystem particle_Charging;
        [FoldoutGroup("PARTICLE")]
        public ParticleSystem particle_ChargeComplete;
        [FoldoutGroup("PARTICLE")]
        public ParticleSystem particle_RoundSlash;
        #endregion

        #region ELSE
        [Header("ELSE")]
        [FoldoutGroup("ELSE")]
        [SerializeField]
        private LayerMask _fieldLayer;
        #endregion

        #region Properties
        public int AttackPower => data.attackPower;
        public float AttackComboInterval => data.attackComboInterval;
        public int MaxHealth => data.maxHealth;
        public float Speed => data.speed;
        public float SpeedOnShield => data.speedOnShield;
        public float SpeedOnTargeting => data.speedOnTargeting;
        public float JumpForce => data.jumpForce;
        public float JumpIntervalDelay => data.jumpIntervalDelay;
        public float DiveRollingForce => data.diveRollingForce;
        public float BlinkTime => data.blinkTime;
        public float ShieldRotateSpeed => data.shieldRotateSpeed;
        public float ChargeTime => data.chargeTime;
        public float ParryingTime => data.parryingTime;
        public float StepSize => data.stepSize;
        #endregion

        #region ETC
        private Vector3 m_Movement, m_LerpMovement, m_RollingDir;

        [HideInInspector]
        public float fixedDeltaTime, jumpIntervalTimer;
        //[HideInInspector]
        public bool isDead, isCharged, isChargingStart, isCharging, isOnShield;
        //[HideInInspector]
        public bool isOnControl = true, isOnGround;

        private float shieldLayerWeight, chargeTimer, parryingTimer,
                      attackComboTimer, fallingTimer;
        [SerializeField]
        private bool isRolling, isFalling;

        #region STRINGS
        private readonly int m_Anim_Para_SwordAttack = Animator.StringToHash("SwordAttack");
        private readonly int m_Anim_Para_isParrying = Animator.StringToHash("isParrying"); 
        private readonly int m_Anim_Para_ComboReady = Animator.StringToHash("ComboReady"); 
        private readonly int m_Anim_Para_MoveBlend = Animator.StringToHash("MoveBlend");
        private readonly int m_Anim_Para_Sidewalk = Animator.StringToHash("Sidewalk");
        private readonly int m_Anim_Para_ChargingAttack = Animator.StringToHash("ChargingAttack"); 
        private readonly int m_Anim_Para_Parrying = Animator.StringToHash("Parrying"); 
        private readonly int m_Anim_Param_AttackCombo = Animator.StringToHash("AttackCombo");
        private readonly int m_Anim_Para_DiveRolling = Animator.StringToHash("DiveRolling");
        private readonly int m_Anim_Para_Falling = Animator.StringToHash("Falling");
        private readonly int m_Anim_Para_Land = Animator.StringToHash("Land");
        private readonly string m_ObjectPool_SwordImpactGold = "SwordImpactGold";
        private readonly string m_Sound_FootSound = "FX_FootSound";
        private readonly string m_Tag_DamageMelee = "DamageMelee";
        private readonly string m_Tag_DamageObject = "DamageObject";
        #endregion

        #endregion
        #endregion

        #region UNITY CALLBACK METHOD
        private void Awake()
        {
            if (PlayerController.Instance != null) Destroy(this);
            else Instance = this;

            thisTransform = this.transform;
            fixedDeltaTime = Time.fixedDeltaTime;
            currentHealth = MaxHealth;

            stateMachine = new StateMachine();
            STATE_Standing = new StandingState(this, stateMachine);
            STATE_Jump = new JumpState(this, stateMachine);
            STATE_Attack = new AttackState(this, stateMachine);
            STATE_Damaged = new DamagedState(this, stateMachine);
            STATE_Dead = new DeadState(this, stateMachine);

            stateMachine.Initialize(STATE_Standing);
        }

        private void OnDestroy()
        {
            GameManager.Instance.OnGameStateChanged -= OnGameStateChanged;
        }

        private void Start()
        {
            GameManager.Instance.OnGameStateChanged += OnGameStateChanged;
        }

        private void Update()
        {
            stateMachine.CurrentState.HandleInput();

            stateMachine.CurrentState.LogicUpdate();
        }

        private void FixedUpdate()
        {
            isOnGround = IsCheckGrounded();

            if (!isDead)
            {
                if (stateMachine.CurrentState != STATE_Attack && 
                    stateMachine.CurrentState != STATE_Damaged)
                {
#if UNITY_EDITOR
                    GetKeyboard();
#endif
                    Movements();

                    Shield();

                    RunTimer();
                }
            }

            stateMachine.CurrentState.PhysicsUpdate();
        }
        #endregion

        #region MOVEMENT & CONTROL
        private void Movements()
        {
            if (isRolling)
            {
                // Moving Toward Diving Direction
                thisRigidbody.MovePosition(thisTransform.position + (m_RollingDir * Speed * fixedDeltaTime));

                // Rotating Toward Diving Direction
                thisTransform.rotation = Quaternion.LookRotation(Vector3.RotateTowards(transform.forward, m_RollingDir, 0.3f, 0));
                return;
            }

            if (m_Movement.x != 0 || m_Movement.z != 0)
            {
                // Motion Blend
                MoveMotionBlend();

                // Rotate
                Rotate();

                // Movement
                if (!isCharged)
                {
                    if (isOnShield)
                        m_Movement *= Speed * SpeedOnShield * fixedDeltaTime;
                    else if (targetingSystem.IsOnTargeting)
                        m_Movement *= Speed * SpeedOnTargeting * fixedDeltaTime;
                    else
                        m_Movement *= Speed * fixedDeltaTime;

                    thisRigidbody.MovePosition(thisTransform.position + m_Movement);
                }
            }
            else
            {
                if (targetingSystem.IsOnTargeting)
                {
                    anim.SetFloat(m_Anim_Para_MoveBlend, 0);
                    anim.SetFloat(m_Anim_Para_Sidewalk, 0);
                }
                float move = anim.GetFloat(m_Anim_Para_MoveBlend);
                if (move > 0)
                {
                    move -= fixedDeltaTime * 7;
                    anim.SetFloat(m_Anim_Para_MoveBlend, move >= 0 ? move : 0);
                }
            }
        }

        private void MoveMotionBlend()
        {
            // Targeting Mode
            if (targetingSystem.IsOnTargeting)
            {
                anim.SetFloat(m_Anim_Para_Sidewalk, m_Movement.x);
                anim.SetFloat(m_Anim_Para_MoveBlend, m_Movement.x > 0 ? m_Movement.x * -1 : m_Movement.x);
            }
            // Non-Targeting Mode
            else
            {
                Vector2 move = new Vector2();
                move.x = m_Movement.x;
                move.y = m_Movement.z;
                var magnitude = move.SqrMagnitude();
                if (magnitude > 0.9f) magnitude = 1;

                if (isOnShield || isCharged) anim.SetFloat(m_Anim_Para_MoveBlend, magnitude * 0.15f);
                else anim.SetFloat(m_Anim_Para_MoveBlend, magnitude);
            }
        }

        private void Rotate()
        {
            // Targeting Mode
            if (targetingSystem.IsOnTargeting && !isRolling)
            {
                RotateToTarget(targetingSystem.TargetObject.transform.position, 0);

                return;
            }
            // Non-Targeting Mode
            if (!isOnShield)
            {
                thisTransform.rotation = Quaternion.LookRotation(Vector3.RotateTowards(transform.forward, m_Movement, 0.3f, 0));
            }
            else
            {
                m_LerpMovement = Vector3.Lerp(transform.forward, m_Movement, Time.deltaTime * ShieldRotateSpeed);
                thisTransform.rotation = Quaternion.LookRotation(m_LerpMovement);
            }
        }

        public void Move(Vector2 newMovement)
        {
            if (isOnControl)
            {
                m_Movement.x = newMovement.x;
                m_Movement.z = newMovement.y;
            }
        }

        private void Shield()
        {
            if (!anim.GetBool(m_Anim_Para_isParrying))
            {
                if (isOnShield)
                {
                    // Shield Motion On
                    if (shieldLayerWeight < 1)
                    {
                        shieldLayerWeight += fixedDeltaTime * 5f;
                        anim.SetLayerWeight(1, shieldLayerWeight);
                    }

                    // Parrying Timer
                    if (parryingTimer < ParryingTime) parryingTimer += fixedDeltaTime;
                }
                else
                {
                    // Shield Motion Off
                    if (shieldLayerWeight > 0)
                    {
                        shieldLayerWeight -= fixedDeltaTime * 5f;
                        anim.SetLayerWeight(1, shieldLayerWeight);
                    }

                    // Charging Attack Timer
                    if (isCharging && !isCharged)
                    {
                        if (ChargeTime > chargeTimer)
                        {
                            chargeTimer += fixedDeltaTime;

                            if (chargeTimer > 0.3f && !isChargingStart)
                            {
                                isChargingStart = true;
                                particle_Charging.Play();
                            }
                        }
                        else
                        {
                            isCharged = true;
                            isChargingStart = false;
                            anim.SetTrigger(m_Anim_Para_ChargingAttack);
                            particle_ChargeComplete.Play();
                        }
                    }
                }
            }
        }

        public void OnEndRolling()
        {
            isOnControl = true;
            isRolling = false;
        }
        #endregion

        #region BUTTONS
        public void Button_Jump()
        {
            if (isRolling || !isOnControl || !isOnGround ||
                stateMachine.CurrentState == STATE_Jump ||
                stateMachine.CurrentState == STATE_Damaged)
                return;

            if (!targetingSystem.IsOnTargeting) // Jumping
            {
                stateMachine.ChangeState(STATE_Jump);
            }
            else // Dive Rolling
            {
                // Initialize
                isOnControl = false;
                isRolling = true;
                anim.SetTrigger(m_Anim_Para_DiveRolling);

                // Set Dive Direction
                if (targetingSystem.IsOnTargeting) // Set off Targeting
                {
                    m_RollingDir = m_Movement * DiveRollingForce;
                }
                else
                    m_RollingDir = thisTransform.forward * DiveRollingForce;
                
            }
        }

        public void Button_Attack(bool isOn)
        {
            if (!isOnGround || stateMachine.CurrentState == STATE_Damaged || stateMachine.CurrentState == STATE_Jump)
                return;

            if (isOn)
            {
                isCharged = false;
                isCharging = true;
            }
            else
            {
                isCharging = false;
                isChargingStart = false;

                if (isOnGround)
                {
                    if (!isOnShield)
                    {
                        if (stateMachine.CurrentState != STATE_Attack)
                        {
                            stateMachine.ChangeState(STATE_Attack);

                            OnPlayerAttack?.Invoke();
                        }
                    }
                }

                chargeTimer = 0;
            }
        }

        public void Button_Shield(bool isOn)
        {
            if (isOnGround)
            {
                isOnShield = isOn;

                // Initialize
                if (!isOn)
                {
                    parryingTimer = 0;
                }
            }
        }

        public void Button_Interacting()
        {
            if (stateMachine.CurrentState == STATE_Jump ||
                stateMachine.CurrentState == STATE_Damaged ||
                stateMachine.CurrentState == STATE_Dead) return;

            if (OnInteractable != null)
            {
                OnInteractable.Invoke();
                feedback_Interacting.PlayFeedbacks();
            }
        }

        public void Button_Targeting(bool isOn) => targetingSystem.SetTargeting(isOn);

        private void GetKeyboard()
        {
            if (Input.GetAxis("Horizontal") != 0)
            {
                m_Movement.x = Input.GetAxis("Horizontal");
            }
            if (Input.GetAxis("Vertical") != 0)
            {
                m_Movement.z = Input.GetAxis("Vertical");
            }
            if (Input.GetKeyUp(KeyCode.Z)) Button_Targeting(true);
            if (Input.GetKeyUp(KeyCode.X)) Button_Targeting(false);

            if (Input.GetKeyDown(KeyCode.A)) Button_Attack(true);
            if (Input.GetKeyUp(KeyCode.A)) Button_Attack(false);

            if (Input.GetKeyDown(KeyCode.S)) Button_Shield(true);
            if (Input.GetKeyUp(KeyCode.S)) Button_Shield(false);

            if (Input.GetKeyDown(KeyCode.Space)) Button_Jump();
        }
        #endregion

        #region ATTACK
        public void MeleeAttack()
        {
            // Feedback
            feedbackm_Attack?.PlayFeedbacks();

            // Collider On
            attackColliderSwitch.DoAttack();

            // Step
            thisRigidbody.MovePosition(thisTransform.position + thisTransform.forward * StepSize);

            // Combo
            attackComboTimer = 0;
            int combo = anim.GetInteger(m_Anim_Param_AttackCombo);
            anim.SetInteger(m_Anim_Param_AttackCombo, ++combo);
            if (combo >= 3) anim.SetInteger(m_Anim_Param_AttackCombo, 0);
            anim.SetBool(m_Anim_Para_ComboReady, true);
        }

        public void OnAttackEnd()
        {
            attackColliderSwitch.EndAttack();
            stateMachine.ChangeState(STATE_Standing);
        }

        public void InitializeAttack()
        {
            chargeTimer = 0;
            attackComboTimer = 0;

            isChargingStart = false;
            isCharging = false;
            isCharged = false;

            anim.ResetTrigger(m_Anim_Para_ChargingAttack);
            anim.ResetTrigger(m_Anim_Para_SwordAttack);
            anim.SetBool(m_Anim_Para_ComboReady, false);
            anim.SetInteger(m_Anim_Param_AttackCombo, 0);

            if (feedbackm_Attack.IsPlaying) feedbackm_Attack?.StopFeedbacks();
        }
        #endregion

        #region DAMAGE & SHIELD
        public void OnDamageTrigger(GameObject triggerObejct, int damageAmount)
        {
            if (isDead && stateMachine.CurrentState == STATE_Damaged) return;

            if (!anim.GetBool(m_Anim_Para_isParrying) && !onDebug)
            {
                bool isEnemy = triggerObejct.CompareTag(m_Tag_DamageMelee),
                     isDamageObejct = triggerObejct.CompareTag(m_Tag_DamageObject);

                if (isEnemy || isDamageObejct)
                {
                    if (isOnShield)
                    {
                        // 적의 방향 각도에 따른 쉴드 처리 또는 패링
                        if (IsSuccessShield(isEnemy, triggerObejct.transform.position - thisTransform.position)) return;
                    }

                    if (!targetingSystem.IsOnTargeting) RotateToTarget(triggerObejct.transform.position, 0.5f);
                    
                    Damage(damageAmount);
                }
            }
        }

        public void Damage(int damageAmount)
        {
            InitializeAttack();

            isOnShield = false;
            isRolling = false;
            attackColliderSwitch.isCancel = true;

            ResetAllTriggers();

            shieldLayerWeight = 0;
            anim.SetLayerWeight(1, 0);

            currentHealth -= damageAmount;

            // Event
            if (OnDamagedReceived != null) OnDamagedReceived(currentHealth);

            // Damaged
            if (currentHealth > 0)
            {
                stateMachine.ChangeState(STATE_Damaged);
                feedback_Knockback.PlayFeedbacks();
            }
            // Died
            else
            {
                isDead = true;
                isOnControl = false;
                stateMachine.ChangeState(STATE_Dead);
                feedback_Death.PlayFeedbacks();
            }
        }

        private bool IsSuccessShield(bool isEnemy, Vector3 targetDir)
        {
            var enemyAngle = Vector3.Angle(targetDir, thisTransform.forward);
            if (enemyAngle < 90)
            {
                if (isEnemy)
                {
                    // 패링 작동
                    if (parryingTimer < ParryingTime && shieldLayerWeight < 0.5f)
                    {
                        shieldLayerWeight = 0;
                        anim.SetLayerWeight(1, 0);
                        anim.SetTrigger(m_Anim_Para_Parrying);
                        feedback_Parrying?.PlayFeedbacks();
                        // FX
                        PoolManager.instance.GetObject(m_ObjectPool_SwordImpactGold, shieldParent.position);

                        isOnShield = false;
                    }
                    // 일반 쉴드
                    else feedback_ShieldDefense?.PlayFeedbacks();
                }

                return true;
            }
            return false;
        }

        private void ResetAllTriggers()
        {
            anim.ResetTrigger(m_Anim_Para_SwordAttack);
            anim.ResetTrigger(m_Anim_Para_ChargingAttack);
            anim.ResetTrigger(m_Anim_Para_Parrying);
            anim.ResetTrigger(m_Anim_Para_Falling);
            anim.ResetTrigger(m_Anim_Para_DiveRolling);
            anim.ResetTrigger(m_Anim_Para_Land);
        }
        #endregion

        #region PUBLIC METHOD
        public void FootSound() => MasterAudio.PlaySound(m_Sound_FootSound);
        #endregion

        #region UTILITY
        private void RunTimer()
        { 
            // Combo Attack Timer
            if (anim.GetBool(m_Anim_Para_ComboReady))
            {
                if (attackComboTimer < AttackComboInterval)
                    attackComboTimer += fixedDeltaTime;
                else // Initialize
                {
                    InitializeAttack();
                }
            }

            // Jump Interval Timer
            if (jumpIntervalTimer > 0) jumpIntervalTimer -= fixedDeltaTime;

            // Falling Timer
            if (!isOnGround)
            {
                if (!isFalling)
                {
                    if (fallingTimer < 0.55f)
                    {
                        fallingTimer += fixedDeltaTime;
                    }
                    else
                    {
                        isFalling = true;
                        fallingTimer = 0;
                        anim.SetTrigger(m_Anim_Para_Falling); // Falling Animation
                    }
                }
            }
            else
            {
                if (isFalling)
                {
                    isFalling = false;
                    anim.SetTrigger(m_Anim_Para_Land); // Landing Animation
                }
                if (fallingTimer > 0) fallingTimer = 0;
            }
        }

        private bool IsCheckGrounded()
        {
            // CharacterController.IsGrounded가 true라면 Raycast를 사용하지 않고 판정 종료
            if (characterController.isGrounded) return true;

            // 발사하는 광선의 초기 위치와 방향
            // 약간 신체에 박혀 있는 위치로부터 발사하지 않으면 제대로 판정할 수 없을 때가 있다.
            var maxDistance = 0.2f;

            var ray1 = new Ray(thisTransform.position - (thisTransform.forward * 0.3f), Vector3.down * maxDistance);

            Debug.DrawRay(thisTransform.position - (thisTransform.forward * 0.3f), Vector3.down * maxDistance, Color.green);

            return Physics.Raycast(ray1, maxDistance, _fieldLayer);
        }

        private void RotateToTarget(Vector3 targetPos, float duration)
        {
            Vector3 dirToTarget = targetPos - thisTransform.position;
            dirToTarget = Quaternion.LookRotation(dirToTarget, Vector3.up).eulerAngles;
            dirToTarget.x = 0; dirToTarget.z = 0;

            if (duration > 0) thisTransform.DORotate(dirToTarget, duration);
            else thisTransform.rotation = Quaternion.Euler(dirToTarget);
        }
                
        private void OnGameStateChanged(GameState newGameState)
        {
            enabled = newGameState == GameState.GamePlay;

            anim.enabled = enabled;
            thisRigidbody.useGravity = enabled;
            if (enabled) thisRigidbody.constraints = RigidbodyConstraints.FreezeRotation;
            else thisRigidbody.constraints = RigidbodyConstraints.FreezeAll;

            if (particle_Charging.isPlaying) particle_Charging.Pause(true);
            else if (particle_Charging.isPaused) particle_Charging.Play();

            if (particle_ChargeComplete.isPlaying) particle_ChargeComplete.Pause(true);
            else if (particle_ChargeComplete.isPaused) particle_ChargeComplete.Play();

            if (particle_RoundSlash.isPlaying) particle_RoundSlash.Pause(true);
            else if (particle_RoundSlash.isPaused) particle_RoundSlash.Play();
        }
        #endregion
    }
}
