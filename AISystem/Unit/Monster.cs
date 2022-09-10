using UnityEngine;
using UnityEngine.AI;
using SK.Combat;

namespace SK.AISystem
{
    // by.상기_필요 컴포넌트_220517
    #region RequireComponent
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(NavMeshAgent))]
    [RequireComponent(typeof(Combat.Combat))]
    [RequireComponent(typeof(SearchRadar))]
    #endregion
    public class Monster : Unit
    {
        // 상태 디버그용 string
        [SerializeField] internal string currentState;

        [Header("Reference")]
        [SerializeField] internal NavMeshAgent navAgent;
        [SerializeField] internal Animator anim;
        [SerializeField] internal SearchRadar searchRadar;
        [SerializeField] internal Combat.Combat combat;

        [Header("Value")]
        [SerializeField] private float walkAnimSpeed;

        [Header("Attack")]
        [SerializeField] internal AttackData[] normalAttacks;

        // by.상기_몬스터 상태 머신
        private StateMachine _stateMachine;

        // by.상기_현재 몬스터의 트랜스폼 캐싱
        internal Transform thisTransform;

        // by.상기_델타 시간을 저장할 변수
        internal float _deltaTime, _fixedDeltaTime;

        // by.상기_애니메이터 파라미터 관련 변수_220519
        private float _moveBlend, _sideways, _walkSpeed;

        private void Awake()
        {
            // by.상기_레퍼런스 초기 할당_220517
            if (!searchRadar) searchRadar = GetComponent<SearchRadar>();
            if (!combat) combat = GetComponent<Combat.Combat>();

            // by.상기_공격 이벤트 호출 시 실행되는 함수 할당
            //combat.OnAttack += TryAttack;

            // by.상기_상태 머신 생성_220516
            _stateMachine = new StateMachine(this);

            // by.상기_상태 머신 Idle 상태로 초기화_220516
            _stateMachine.ChangeState(_stateMachine.idleState);

            // by.상기_변수 초기 할당
            thisTransform = transform;
        }

        private void OnEnable()
        {
            // NavMeshAgent의 속도를 info의 speed에 따라 설정
            navAgent.speed = info.MovementSpeed;

            // HP 초기화
            currentHp = info.MaxHp;
        }

        public override void FixedTick()
        {
            base.FixedTick();

            // by.상기_델타 시간 저장_220517
            _fixedDeltaTime = Time.fixedDeltaTime;

            // by.상기_할당된 상태 업데이트_220517
            _stateMachine.currentState?.FixedTick();
        }

        public override void Tick()
        {
            base.Tick();// by.상기_고정 델타 시간 저장_220517

            _deltaTime = Time.deltaTime;

            // by.상기_할당된 상태 업데이트_220517
            _stateMachine.currentState?.Tick();

            // by.상기_애니메이션 동작 상태 업데이트_220519
            AnimateMove();
        }

        public override void OnDamage(Unit attacker, int attackPower)
        {
            // 피격 애니메이션 재생
            //anim.CrossFade(AnimatorParams.AnimPara_Damaged, 0.2f);

            // 공격한 유닛을 타겟으로 지정
            if (searchRadar.Target != attacker.gameObject)
                searchRadar.SetTarget(attacker.gameObject);

            _stateMachine.ChangeState(_stateMachine.combatState);
        }

        public override void OnDead()
        {
            // 씬 매니저의 유닛 딕셔너리에서 제거
            //ObjectPool.Instance.PushObject(gameObject);

            // 효과 재생
            //SK.Effect.EffectManager.INSTANCE.PlayEffect(1001, transform);

            //QuestManager.INSTANCE.AddMonsterCount(Info.UnitID);
        }

        // by.상기_NavMeshAgent의 상태에 따라 애니메이션 동작 상태 업데이트_220519
        private void AnimateMove()
        {
            _walkSpeed = navAgent.velocity.magnitude / navAgent.speed;

            if (navAgent.velocity.sqrMagnitude <= 0.03f || navAgent.remainingDistance <= 0.15f)
            {
                var moveBlend = anim.GetFloat(Strings.AnimPara_MoveBlend);

                // 목표 지점에 부근에 도달 시 블랜드 값이 감소하며 정지
                _moveBlend = moveBlend > 0 ? moveBlend - 0.07f : 0;
            }
            else
            {
                // 이동 중이면 블랜드 변수 값을 증가
                _moveBlend = walkAnimSpeed * _walkSpeed + _deltaTime;
            }

            // 블랜드 값은 1을 넘지 못하도록 함
            if (_moveBlend > 1) _moveBlend = 1;

            // 블랜드 변수를 애니메이터 파라미터에 할당
            anim.SetFloat(Strings.AnimPara_MoveBlend, _moveBlend);
        }

        private void OnDisable()
        {
            // by.상기_할당된 이벤트 함수 해제
            //combat.OnAttack -= TryAttack;

            // 씬 매니저의 유닛 딕셔너리에서 제거
            //ObjectPool.Instance.PushObject(gameObject);
        }
    }
}