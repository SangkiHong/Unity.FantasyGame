using UnityEngine;
using UnityEngine.AI;
using SK.Combat;

namespace SK.AISystem
{
    // by.���_�ʿ� ������Ʈ_220517
    #region RequireComponent
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(NavMeshAgent))]
    [RequireComponent(typeof(Combat.Combat))]
    [RequireComponent(typeof(SearchRadar))]
    #endregion
    public class Monster : Unit
    {
        // ���� ����׿� string
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

        // by.���_���� ���� �ӽ�
        private StateMachine _stateMachine;

        // by.���_���� ������ Ʈ������ ĳ��
        internal Transform thisTransform;

        // by.���_��Ÿ �ð��� ������ ����
        internal float _deltaTime, _fixedDeltaTime;

        // by.���_�ִϸ����� �Ķ���� ���� ����_220519
        private float _moveBlend, _sideways, _walkSpeed;

        private void Awake()
        {
            // by.���_���۷��� �ʱ� �Ҵ�_220517
            if (!searchRadar) searchRadar = GetComponent<SearchRadar>();
            if (!combat) combat = GetComponent<Combat.Combat>();

            // by.���_���� �̺�Ʈ ȣ�� �� ����Ǵ� �Լ� �Ҵ�
            //combat.OnAttack += TryAttack;

            // by.���_���� �ӽ� ����_220516
            _stateMachine = new StateMachine(this);

            // by.���_���� �ӽ� Idle ���·� �ʱ�ȭ_220516
            _stateMachine.ChangeState(_stateMachine.idleState);

            // by.���_���� �ʱ� �Ҵ�
            thisTransform = transform;
        }

        private void OnEnable()
        {
            // NavMeshAgent�� �ӵ��� info�� speed�� ���� ����
            navAgent.speed = info.MovementSpeed;

            // HP �ʱ�ȭ
            currentHp = info.MaxHp;
        }

        public override void FixedTick()
        {
            base.FixedTick();

            // by.���_��Ÿ �ð� ����_220517
            _fixedDeltaTime = Time.fixedDeltaTime;

            // by.���_�Ҵ�� ���� ������Ʈ_220517
            _stateMachine.currentState?.FixedTick();
        }

        public override void Tick()
        {
            base.Tick();// by.���_���� ��Ÿ �ð� ����_220517

            _deltaTime = Time.deltaTime;

            // by.���_�Ҵ�� ���� ������Ʈ_220517
            _stateMachine.currentState?.Tick();

            // by.���_�ִϸ��̼� ���� ���� ������Ʈ_220519
            AnimateMove();
        }

        public override void OnDamage(Unit attacker, int attackPower)
        {
            // �ǰ� �ִϸ��̼� ���
            //anim.CrossFade(AnimatorParams.AnimPara_Damaged, 0.2f);

            // ������ ������ Ÿ������ ����
            if (searchRadar.Target != attacker.gameObject)
                searchRadar.SetTarget(attacker.gameObject);

            _stateMachine.ChangeState(_stateMachine.combatState);
        }

        public override void OnDead()
        {
            // �� �Ŵ����� ���� ��ųʸ����� ����
            //ObjectPool.Instance.PushObject(gameObject);

            // ȿ�� ���
            //SK.Effect.EffectManager.INSTANCE.PlayEffect(1001, transform);

            //QuestManager.INSTANCE.AddMonsterCount(Info.UnitID);
        }

        // by.���_NavMeshAgent�� ���¿� ���� �ִϸ��̼� ���� ���� ������Ʈ_220519
        private void AnimateMove()
        {
            _walkSpeed = navAgent.velocity.magnitude / navAgent.speed;

            if (navAgent.velocity.sqrMagnitude <= 0.03f || navAgent.remainingDistance <= 0.15f)
            {
                var moveBlend = anim.GetFloat(Strings.AnimPara_MoveBlend);

                // ��ǥ ������ �αٿ� ���� �� ���� ���� �����ϸ� ����
                _moveBlend = moveBlend > 0 ? moveBlend - 0.07f : 0;
            }
            else
            {
                // �̵� ���̸� ���� ���� ���� ����
                _moveBlend = walkAnimSpeed * _walkSpeed + _deltaTime;
            }

            // ���� ���� 1�� ���� ���ϵ��� ��
            if (_moveBlend > 1) _moveBlend = 1;

            // ���� ������ �ִϸ����� �Ķ���Ϳ� �Ҵ�
            anim.SetFloat(Strings.AnimPara_MoveBlend, _moveBlend);
        }

        private void OnDisable()
        {
            // by.���_�Ҵ�� �̺�Ʈ �Լ� ����
            //combat.OnAttack -= TryAttack;

            // �� �Ŵ����� ���� ��ųʸ����� ����
            //ObjectPool.Instance.PushObject(gameObject);
        }
    }
}