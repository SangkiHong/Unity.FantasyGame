using System;
using UnityEngine;
using UnityEngine.Events;
using MoreMountains.Feedbacks;

namespace Sangki.Player
{
    public class PlayerController : MonoBehaviour
    {
        #region VARIABLE
        public static PlayerController Instance;

        public event UnityAction<int> OnDamagedReceived;
        public event UnityAction OnPlayerAttack;
        public event UnityAction OnPlayerDied;

        [Header("COMPONENTS")]
        public AttackColliderSwitch attackColliderSwitch;
        public Animator anim;

        [SerializeField]
        private CharacterController characterController;
        [SerializeField]
        private Rigidbody _rigidbody;
        [SerializeField]
        private Transform shieldParent;

        [Header("STATS")]
        [SerializeField]
        private bool noHit;
        public int attackPower;
        public int maxHealth;
        [SerializeField]
        private int currentHealth;
        [SerializeField]
        private float speed;
        [SerializeField]
        private float jumpForce; 
        [SerializeField]
        private float blinkTime;
        [SerializeField]
        private float shieldRotateSpeed = 1f;
        [SerializeField]
        private float chargeTime = 2f;
        [SerializeField]
        private float parryingTime = 0.6f;
        [SerializeField]
        private float stepSize = 0.5f;

        [Header("ELSE")]
        [SerializeField]
        private LayerMask _fieldLayer;

        [Header("FEEDBACKS")]
        [SerializeField]
        private MMFeedbacks feedback_Jump;
        public MMFeedbacks feedback_Attack;
        [SerializeField]
        private MMFeedbacks feedback_Knockback; 
        [SerializeField]
        private MMFeedbacks feedback_ShieldDefense;
        [SerializeField]
        private MMFeedbacks feedback_Parrying;
        [SerializeField]
        private MMFeedbacks feedback_Death;

        [Header("PARTICLE")]
        [SerializeField]
        private ParticleSystem particle_Charging;
        [SerializeField]
        private ParticleSystem particle_ChargeComplete;

        private Transform thisTransform;

        private Vector3 _Movement, _LerpMovement;

        [SerializeField]
        public bool isDead;

        private bool isOnContol = true, 
                     isOnGround,
                     isOnShield,
                     isDamaged,
                     isAttack,
                     isAttackedShield,
                     isChargingStart,
                     isCharging,
                     isCharged,
                     isParrying;

        private float fixedDeltaTime, 
                      shieldLayerWeight, 
                      blinkTimer, 
                      chargeTimer,
                      parryingTimer;

        private readonly string _Tag_DamageMelee = "DamageMelee"; 
        private readonly string _Tag_DamageObject = "DamageObject"; 
        private readonly string _ObjectPool_SwordImpactGold = "SwordImpactGold"; 
        private readonly string _Anim_Para_isMove = "isMove";
        private readonly string _Anim_Para_isAttack = "isAttack"; 
        private readonly string _Anim_Para_MoveBlend = "MoveBlend";
        private readonly string _Anim_Para_Jump = "Jump"; 
        private readonly string _Anim_Para_SwordAttack = "SwordAttack";
        private readonly string _Anim_Para_ChargingAttack = "ChargingAttack"; 
        private readonly string _Anim_Para_ChargingEnd = "ChargingEnd"; 
        private readonly string _Anim_Para_Parrying = "Parrying";
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
                // 움직임 관련
                if (_Movement.x != 0 || _Movement.z != 0)
                {
                    // Animation
                    if (!anim.GetBool(_Anim_Para_isMove)) anim.SetBool(_Anim_Para_isMove, true);

                    if (!anim.GetBool(_Anim_Para_isAttack) && !isAttackedShield && !isDamaged)
                    {
                        float x, z;
                        x = _Movement.x;
                        z = _Movement.z;
                        if (x < 0) x = -x;
                        if (z < 0) z = -z;
                        if (x >= z)
                        {
                            if (isOnShield || isCharged) anim.SetFloat(_Anim_Para_MoveBlend, x * 0.15f);
                            else anim.SetFloat(_Anim_Para_MoveBlend, x);
                        }
                        else
                        {
                            if (isOnShield || isCharged) anim.SetFloat(_Anim_Para_MoveBlend, z * 0.15f);
                            else anim.SetFloat(_Anim_Para_MoveBlend, z);
                        }

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

                // 쉴드 모션
                if (!anim.GetBool(_Anim_Para_isAttack) && !isParrying && !isDamaged)
                {
                    if (isOnShield)
                    {
                        if (shieldLayerWeight < 1)
                        {
                            shieldLayerWeight += fixedDeltaTime * 5f;
                            anim.SetLayerWeight(1, shieldLayerWeight);
                        }

                        // 패링 타이머
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

                // 블링크
                if (isDamaged)
                {
                    blinkTimer += fixedDeltaTime;

                    if (blinkTimer >= blinkTime)
                    {
                        isDamaged = false;
                        blinkTimer = 0;
                    }
                }

                // 차징 공격 타이머
                if (!isOnShield && isCharging && !isCharged && !anim.GetBool(_Anim_Para_isAttack))
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
            }
        }
        #endregion

        #region CONTROLL
        public virtual void Move(Vector2 newMovement)
        {
            if (isOnContol && !anim.GetBool(_Anim_Para_isAttack))
            {
                _Movement.x = newMovement.x;
                _Movement.z = newMovement.y;
            }
        }

        public void Jump()
        {
            if (isOnGround && isOnContol && !anim.GetBool(_Anim_Para_isAttack))
            {
                // Animation
                anim.SetTrigger(_Anim_Para_Jump);

                _rigidbody.AddForce(Vector3.up * (jumpForce * -Physics.gravity.y));

                // Feedback
                feedback_Jump?.PlayFeedbacks();

                // 쉴드 시 해제
                if (isOnShield) 
                {
                    anim.SetLayerWeight(1, 0);
                    isOnShield = false;
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

        private bool IsCheckGrounded()
        {
            // CharacterController.IsGrounded가 true라면 Raycast를 사용하지 않고 판정 종료
            if (characterController.isGrounded) return true;
            // 발사하는 광선의 초기 위치와 방향
            // 약간 신체에 박혀 있는 위치로부터 발사하지 않으면 제대로 판정할 수 없을 때가 있다.
            var maxDistance = 0.2f;

            var ray = new Ray(this.transform.position + Vector3.up * 0.1f, Vector3.down * maxDistance);

            Debug.DrawRay(transform.position + Vector3.up * 0.1f, Vector3.down * maxDistance, Color.red);

            return Physics.Raycast(ray, maxDistance, _fieldLayer);
        }
        #endregion

        #region ATTACK & DAMAGE
        public void Attack(bool isPush)
        {
            if (!isAttack)
            {
                if (isPush)
                {
                    isCharged = false;
                    isCharging = true;
                }
                else
                {
                    isChargingStart = false;
                    isCharging = false;

                    if (isOnGround)
                    {
                        if (!isOnShield && !isDamaged)
                        {
                            // 360 degree Attack
                            if (isCharged)
                            {
                                isCharged = false;
                                anim.SetTrigger(_Anim_Para_ChargingEnd);
                            }
                            // Normal Attack
                            else
                            {
                                isAttack = true;
                                anim.SetTrigger(_Anim_Para_SwordAttack);
                            }

                            OnPlayerAttack?.Invoke();
                        }
                    }

                    chargeTimer = 0;
                }
            }
        }

        public void AbleToComboAttack()
        {
            isAttack = false;
            isParrying = false;
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
        }

        public void OnDamageTrigger(GameObject triggerObejct, int damageAmount)
        {
            if (!isDead && !isDamaged && !isParrying && !isAttackedShield && !noHit)
            {
                if (triggerObejct.CompareTag(_Tag_DamageMelee) || triggerObejct.CompareTag(_Tag_DamageObject))
                {
                    if (isOnShield)
                    {
                        if (!isAttack)
                        {
                            // 적의 방향 각도에 따른 쉴드 처리
                            var enemyAngle = Vector3.Dot(thisTransform.forward, Vector3.Normalize(triggerObejct.transform.position - thisTransform.position));
                            if (enemyAngle > 0)
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
                                        isAttack = true;
                                        isParrying = true;
                                        return;
                                    }
                                }
                                // 일반 쉴드
                                feedback_ShieldDefense.PlayFeedbacks();
                                return;
                            }
                        }
                    }

                    // 피해 입음
                    isDamaged = true;
                    isCharging = false;
                    isCharged = false;
                    isAttack = false;
                    attackColliderSwitch.isCancel = true;
                    shieldLayerWeight = 0;
                    anim.SetLayerWeight(1, 0);
                    feedback_Attack.StopFeedbacks();
                    RotateToDamage(triggerObejct.transform);
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
                        isAttack = false;
                        isParrying = false;
                        isOnContol = false;
                        feedback_Death.PlayFeedbacks();
                    }
                }
            }
        }
        #endregion

        #region UTILITY
        private void RotateToDamage(Transform target)
        {
            //thisTransform.rotation = Quaternion.LookRotation(Vector3.RotateTowards(transform.forward, target.position, 1, 0));
            Vector3 dirToTarget = target.position - thisTransform.position;
            dirToTarget = Quaternion.LookRotation(dirToTarget, Vector3.up).eulerAngles;
            dirToTarget.x = 0; dirToTarget.z = 0;
            thisTransform.rotation = Quaternion.Euler(dirToTarget);
        }
        #endregion
    }
}
