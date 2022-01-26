using UnityEngine;
using UnityEngine.Events;
using Sirenix.OdinInspector;
using MoreMountains.Feedbacks;

namespace Sangki.Player
{
    public class PlayerController : MonoBehaviour, IDamageable
    {
        #region VARIABLE
        public static PlayerController Instance;

        #region UNITY ACTION
        public event UnityAction<int> OnDamagedReceived;
        public event UnityAction OnPlayerAttack;
        public event UnityAction OnPlayerDied;
        public event UnityAction OnInteractable;
        #endregion

        #region COMPONENTS
        [Header("COMPONENTS")]
        [FoldoutGroup("COMPONENTS")]
        public AttackColliderSwitch attackColliderSwitch;
        [FoldoutGroup("COMPONENTS")]
        public Animator anim;

        [FoldoutGroup("COMPONENTS")]
        [SerializeField]
        private CharacterController characterController;
        [FoldoutGroup("COMPONENTS")]
        [SerializeField]
        private Rigidbody _rigidbody;
        [FoldoutGroup("COMPONENTS")]
        [SerializeField]
        private Transform shieldParent;
        #endregion

        #region STATS
        [Header("STATS")]
        [FoldoutGroup("STATS")]
        [SerializeField]
        private bool noHit;
        [FoldoutGroup("STATS")]
        public int attackPower;
        [FoldoutGroup("STATS")]
        public float attackComboInterval = 0.6f;
        [FoldoutGroup("STATS")]
        public int maxHealth;
        [FoldoutGroup("STATS")]
        [SerializeField]
        private int currentHealth;
        [FoldoutGroup("STATS")]
        [SerializeField]
        private float speed;
        [FoldoutGroup("STATS")]
        [SerializeField]
        private float jumpForce;
        [FoldoutGroup("STATS")]
        [SerializeField]
        private float jumpIntervalDelay = 1;
        [FoldoutGroup("STATS")]
        [SerializeField]
        private float blinkTime;
        [FoldoutGroup("STATS")]
        [SerializeField]
        private float shieldRotateSpeed = 1f;
        [FoldoutGroup("STATS")]
        [SerializeField]
        private float chargeTime = 2f;
        [FoldoutGroup("STATS")]
        [SerializeField]
        private float parryingTime = 0.6f;
        [FoldoutGroup("STATS")]
        [SerializeField]
        private float stepSize = 0.5f;
        #endregion

        #region FEEDBACKS
        [Header("FEEDBACKS")]
        [FoldoutGroup("FEEDBACKS")]
        [SerializeField]
        private MMFeedbacks feedback_Jump;
        [FoldoutGroup("FEEDBACKS")]
        public MMFeedbacks feedback_Attack;
        [FoldoutGroup("FEEDBACKS")]
        [SerializeField]
        private MMFeedbacks feedback_Knockback;
        [FoldoutGroup("FEEDBACKS")]
        [SerializeField]
        private MMFeedbacks feedback_ShieldDefense;
        [FoldoutGroup("FEEDBACKS")]
        [SerializeField]
        private MMFeedbacks feedback_Parrying;
        [FoldoutGroup("FEEDBACKS")]
        [SerializeField]
        private MMFeedbacks feedback_Interacting;
        [FoldoutGroup("FEEDBACKS")]
        [SerializeField]
        private MMFeedbacks feedback_Death;
        #endregion

        #region PARTICLE
        [Header("PARTICLE")]
        [FoldoutGroup("PARTICLE")]
        [SerializeField]
        private ParticleSystem particle_Charging;
        [FoldoutGroup("PARTICLE")]
        [SerializeField]
        private ParticleSystem particle_ChargeComplete;
        [FoldoutGroup("PARTICLE")]
        [SerializeField]
        private ParticleSystem particle_RoundSlash;
        #endregion

        #region ELSE
        [Header("ELSE")]
        [FoldoutGroup("ELSE")]
        [SerializeField]
        private LayerMask _fieldLayer;
        #endregion

        #region ETC
        private Transform thisTransform;

        private Vector3 _Movement, _LerpMovement;

        [HideInInspector]
        public bool isDead;
        [SerializeField]
        private bool isOnContol = true,
                     isJump,
                     isAttack,
                     isFalling,
                     isOnGround,
                     isOnShield,
                     isDamaged,
                     isAttackedShield,
                     isChargingStart,
                     isCharging,
                     isCharged;

        private float fixedDeltaTime, 
                      shieldLayerWeight, 
                      blinkTimer, 
                      chargeTimer,
                      parryingTimer,
                      jumpIntervalTimer,
                      fallingTimer,
                      attackComboTimer;

        private readonly string _Tag_DamageMelee = "DamageMelee"; 
        private readonly string _Tag_DamageObject = "DamageObject"; 
        private readonly string _ObjectPool_SwordImpactGold = "SwordImpactGold"; 
        private readonly string _Anim_Para_isMove = "isMove";
        private readonly string _Anim_Para_isAttack = "isAttack"; 
        private readonly string _Anim_Para_isParrying = "isParrying"; 
        private readonly string _Anim_Para_isFall = "Falling"; 
        private readonly string _Anim_Para_ComboReady = "ComboReady"; 
        private readonly string _Anim_Para_MoveBlend = "MoveBlend";
        private readonly string _Anim_Para_Jump = "Jump"; 
        private readonly string _Anim_Para_Land = "Land"; 
        private readonly string _Anim_Para_SwordAttack = "SwordAttack";
        private readonly string _Anim_Para_ChargingAttack = "ChargingAttack"; 
        private readonly string _Anim_Para_ChargingEnd = "ChargingEnd"; 
        private readonly string _Anim_Para_Parrying = "Parrying"; 
        private readonly string _Anim_Para_AttackCombo = "AttackCombo";
        #endregion
        #endregion

        #region AWAKE, FIXEDUPDATE
        private void Awake()
        {
            if (PlayerController.Instance != null) Destroy(this);
            else Instance = this;

            thisTransform = this.transform;
            fixedDeltaTime = Time.fixedDeltaTime;
            currentHealth = maxHealth;
        }

        private void FixedUpdate()
        {
            isOnGround = IsCheckGrounded();

            if (!isDead && isOnContol)
            {
                // Delay after Falling
                if (isOnGround)
                {
                    if (isJump || isFalling)
                    {
                        if (isJump)
                        {
                            jumpIntervalTimer = jumpIntervalDelay;
                            isJump = false;
                        }
                        if (isFalling) isFalling = false;
                        anim.SetTrigger(_Anim_Para_Land); // Landing Animation
                    }
                    if (fallingTimer > 0) fallingTimer = 0;
                }
                // Check falling
                else
                {
                    if (!isJump)
                    {
                        if (fallingTimer < 0.45f)
                        {
                            fallingTimer += fixedDeltaTime;
                        }
                        else
                        {
                            isFalling = true;
                            fallingTimer = 0;
                            anim.SetTrigger(_Anim_Para_isFall);
                        }
                    }
                }

                // Movement
                if (_Movement.x != 0 || _Movement.z != 0)
                {
                    // Animation
                    if (!anim.GetBool(_Anim_Para_isMove)) anim.SetBool(_Anim_Para_isMove, true);

                    if (!anim.GetBool(_Anim_Para_isAttack) && !isAttackedShield && !isDamaged)
                    {
                        // Movement Motion Blend
                        MoveMotionBlend();

                        // Rotate
                        Rotate();

                        // Movement
                        if (!isCharged)
                        {
                            if (!isOnShield) _Movement *= speed * fixedDeltaTime;
                            else _Movement *= speed * 0.2f * fixedDeltaTime;

                            _rigidbody.MovePosition(thisTransform.position + _Movement);
                        }
                    }
                }
                else
                {
                    // Animation
                    if (anim.GetBool(_Anim_Para_isMove)) anim.SetBool(_Anim_Para_isMove, false);
                }

                if (Input.GetKey(KeyCode.Space)) Jump();

                // Shield Motion
                if (!anim.GetBool(_Anim_Para_isAttack) && !anim.GetBool(_Anim_Para_isParrying) && !isDamaged)
                {
                    if (isOnShield)
                    {
                        if (shieldLayerWeight < 1)
                        {
                            shieldLayerWeight += fixedDeltaTime * 5f;
                            anim.SetLayerWeight(1, shieldLayerWeight);
                        }

                        // Parrying Timer
                        if (parryingTimer < parryingTime) parryingTimer += fixedDeltaTime;
                    }
                    else
                    {
                        if (shieldLayerWeight > 0)
                        {
                            shieldLayerWeight -= fixedDeltaTime * 5f;
                            anim.SetLayerWeight(1, shieldLayerWeight);
                        }
                    }
                }

                // Jump Interval Timer
                if (jumpIntervalTimer > 0) jumpIntervalTimer -= fixedDeltaTime;

                // Damage Blink
                if (isDamaged)
                {
                    blinkTimer += fixedDeltaTime;

                    if (blinkTimer >= blinkTime)
                    {
                        isDamaged = false;
                        blinkTimer = 0;
                    }
                }

                // Charging Attack Timer
                if (!isOnShield && isCharging && !isCharged && !isAttack)
                {
                    if (chargeTime > chargeTimer)
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
                        anim.SetTrigger(_Anim_Para_ChargingAttack);
                        particle_ChargeComplete.Play();
                    }
                }

                // Combo Attack Timer
                if (!isAttack && anim.GetBool(_Anim_Para_ComboReady))
                {
                    if (attackComboTimer < attackComboInterval)
                        attackComboTimer += fixedDeltaTime;
                    else // Initialize
                    {
                        InitializeAttack();
                    }
                }
            }
        }
        #endregion

        #region CONTROLL
        public virtual void Move(Vector2 newMovement)
        {
            if (isOnContol && !isAttack)
            {
                _Movement.x = newMovement.x;
                _Movement.z = newMovement.y;
            }
        }

        public void Jump()
        {
            if (!isJump && jumpIntervalTimer <= 0 && isOnGround && isOnContol)
            {
                if (!anim.GetBool(_Anim_Para_isAttack))
                {
                    isJump = true;

                    // Animation
                    anim.SetTrigger(_Anim_Para_Jump);

                    _rigidbody.AddForce(Vector3.up * (jumpForce * -Physics.gravity.y));

                    // Feedback
                    feedback_Jump?.PlayFeedbacks();

                    // Combo Initialized
                    InitializeAttack();

                    // 쉴드 시 해제
                    if (isOnShield)
                    {
                        anim.SetLayerWeight(1, 0);
                        isOnShield = false;
                    }
                }
            }
        }

        private void Rotate()
        {
            if (!isOnShield) 
            {
                thisTransform.rotation = Quaternion.LookRotation(Vector3.RotateTowards(transform.forward, _Movement, 0.3f, 0));
            }
            else
            {
                _LerpMovement = Vector3.Lerp(transform.forward, _Movement, Time.deltaTime * shieldRotateSpeed);
                thisTransform.rotation = Quaternion.LookRotation(_LerpMovement);
            }
        }

        private void MoveMotionBlend()
        {
            Vector2 move = new Vector2();
            move.x = _Movement.x;
            move.y = _Movement.z;
            var magnitude = move.SqrMagnitude();
            if (magnitude > 0.9f) magnitude = 1;

            if (isOnShield || isCharged) anim.SetFloat(_Anim_Para_MoveBlend, magnitude * 0.15f);
            else anim.SetFloat(_Anim_Para_MoveBlend, magnitude);
        }
        #endregion

        #region ATTACK & DAMAGE
        public void Attack(bool isPush)
        {
            if (!isAttack && !isDamaged)
            {
                if (isPush)
                {
                    isCharged = false;
                    isCharging = true;
                }
                else
                {
                    isAttack = true;
                    isCharging = false;
                    isChargingStart = false;

                    if (isOnGround)
                    {
                        if (!isOnShield)
                        {
                            // 360 degree Attack
                            if (isCharged)
                            {
                                isCharged = false;
                                anim.SetTrigger(_Anim_Para_ChargingEnd);
                                particle_RoundSlash.Play();
                            }
                            // Normal Attack
                            else
                            {
                                anim.SetTrigger(_Anim_Para_SwordAttack);
                            }

                            OnPlayerAttack?.Invoke();
                        }
                    }

                    chargeTimer = 0;
                }
            }
        }

        public void ShieldBlock(bool isBlock)
        {
            if (isOnGround)
            {
                isOnShield = isBlock;

                // 초기화
                if (!isBlock)
                {
                    parryingTimer = 0;
                }
            }
        }

        public void MeleeAttack()
        {
            // Feedback
            feedback_Attack?.PlayFeedbacks();

            // Collider On
            attackColliderSwitch.DoAttack();

            // Step
            _rigidbody.MovePosition(thisTransform.position + thisTransform.forward * stepSize);

            // Combo
            attackComboTimer = 0;
            int combo = anim.GetInteger(_Anim_Para_AttackCombo);
            anim.SetInteger(_Anim_Para_AttackCombo, ++combo);
            if (combo >= 3) anim.SetInteger(_Anim_Para_AttackCombo, 0);
            anim.SetBool(_Anim_Para_ComboReady, true);
        }

        public void OnAttackEnd() => isAttack = false;

        private void InitializeAttack()
        {
            isCharged = false;
            isChargingStart = false;
            anim.ResetTrigger(_Anim_Para_ChargingAttack);
            anim.ResetTrigger(_Anim_Para_SwordAttack);
            anim.SetBool(_Anim_Para_ComboReady, false);
            anim.SetInteger(_Anim_Para_AttackCombo, 0);

            chargeTimer = 0;
            attackComboTimer = 0;
        }

        public void OnDamageTrigger(GameObject triggerObejct, int damageAmount)
        {
            if (!isDead && !isDamaged && !anim.GetBool(_Anim_Para_isParrying) && !isAttackedShield && !noHit)
            {
                if (triggerObejct.CompareTag(_Tag_DamageMelee) || triggerObejct.CompareTag(_Tag_DamageObject))
                {
                    if (isOnShield)
                    {
                        if (!isAttack)
                        {
                            // 적의 방향 각도에 따른 쉴드 처리
                            Vector3 targetDir = triggerObejct.transform.position - thisTransform.position;
                            var enemyAngle = Vector3.Angle(targetDir, thisTransform.forward);
                            if (enemyAngle < 90)
                            {
                                if (triggerObejct.CompareTag(_Tag_DamageMelee))
                                {
                                    // 패링 작동
                                    if (parryingTimer < parryingTime && shieldLayerWeight < 0.5f)
                                    {
                                        shieldLayerWeight = 0;
                                        anim.SetLayerWeight(1, 0);
                                        anim.SetTrigger(_Anim_Para_Parrying);
                                        feedback_Parrying.PlayFeedbacks();
                                        // FX
                                        PoolManager.instance.GetObject(_ObjectPool_SwordImpactGold, shieldParent.position);

                                        isOnShield = false;
                                        return;
                                    }
                                }
                                // 일반 쉴드
                                feedback_ShieldDefense.PlayFeedbacks();
                                return;
                            }
                        }
                    }

                    RotateToDamage(triggerObejct.transform);
                    Damage(damageAmount);
                }
            }
        }

        public void Damage(int damageAmount)
        { 
            // 피해 입음
            isDamaged = true;
            isCharging = false;
            isCharged = false;
            isAttack = false;
            InitializeAttack();
            attackColliderSwitch.isCancel = true;
            shieldLayerWeight = 0;
            anim.SetLayerWeight(1, 0);
            feedback_Attack.StopFeedbacks();
            _rigidbody.AddRelativeForce(Vector3.forward * -5, ForceMode.Impulse);

            currentHealth -= damageAmount;

            // Event
            if (OnDamagedReceived != null)
                OnDamagedReceived(currentHealth);

            // Damaged
            if (currentHealth > 0)
            {
                feedback_Knockback.PlayFeedbacks();
            }
            // Died
            else
            {
                isDead = true;
                isOnContol = false;
                feedback_Death.PlayFeedbacks();
            }
        }
        #endregion

        #region PUBLIC METHOD
        public void Interacting() // Y Button
        {
            if (OnInteractable != null && !isJump)
            {
                OnInteractable.Invoke();
                feedback_Interacting.PlayFeedbacks();
            }
        }
        #endregion

        #region UTILITY
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

        private void RotateToDamage(Transform target)
        {
            //thisTransform.rotation = Quaternion.LookRotation(Vector3.RotateTowards(transform.forward, target.position, 1, 0));
            Vector3 dirToTarget = target.position - thisTransform.position;
            dirToTarget = Quaternion.LookRotation(dirToTarget, Vector3.up).eulerAngles;
            dirToTarget.x = 0; dirToTarget.z = 0;
            thisTransform.rotation = Quaternion.Euler(dirToTarget);
        }

        // return -180 ~ 180 Degree
        private static float GetAngle(Vector3 vStart, Vector3 vEnd)
        {
            Vector3 v = vEnd - vStart;

            return Mathf.Atan2(v.z, v.x) * Mathf.Rad2Deg;
        }
        #endregion
    }
}
