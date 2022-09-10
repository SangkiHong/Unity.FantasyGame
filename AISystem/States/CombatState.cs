using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SK.AISystem
{
    /*
        �ۼ���: ȫ���
        ����: Ư�� �Ÿ� �ȿ� Ÿ���� ������ Ư�� �������� ������
        �ۼ���: 22�� 5�� 17��
    */

    public class CombatState : StateBase
    {
        // by.���_������_220517
        public CombatState(Monster monster, StateMachine stateMachine)
        {
            _monster = monster;
            _machine = stateMachine;
        }

        // by.���_���� ������ ������ ����_220517
        private readonly Monster _monster;

        // by.���_���� �ӽ��� ������ ����_220517
        private readonly StateMachine _machine;

        // ���� ������ ������ ����_220519
        private Combat.AttackData _currentAttack;

        // ���� �迭�� �ε���_220519
        private int _attackIndex; 
        private float _targetDistance;
        private float _elapsed;

        // by.���_���� ���Խ� ȣ��Ǵ� �Լ�_220517
        public override void StateInit()
        {
            // NavMeshAgent�� ������Ʈ ����
            _monster.navAgent.Warp(_monster.thisTransform.position);
            _monster.navAgent.isStopped = true;
            // �ʱ�ȭ
            _elapsed = 0;
        }

        public override void FixedTick()
        {
            // by.���_Ÿ���� �Ҵ�Ǿ� �ִ� ���_220517
            if (_monster.searchRadar.Target)
            {
                // Ÿ�ٰ��� �Ÿ� ������Ʈ�Ͽ� ������ ����
                _targetDistance = Vector3.Distance(_monster.searchRadar.Target.transform.position, _monster.thisTransform.position);

                // Ÿ�ٰ��� �Ÿ��� ������ ������ �Ÿ����� +1 �� �־����� �߰� ���·� ��ȯ
                if (_targetDistance > _monster.combat.attackDistance)
                    _machine.ChangeState(_machine.chaseState);
            }
        }

        public override void Tick()
        {
            // by.���_Ÿ���� �ٶ󺸴� �Լ�
            RotateToTarget();

            // by.���_���� ��Ÿ�� ���ݿ� ���� ���� �Լ� ȣ��_220517
            if (_elapsed < _monster.combat.attackCooltime)
                _elapsed += _monster._deltaTime;
            else
            {
                if (_monster.normalAttacks.Length > 0)
                {
                    // ���� ������ ������ �Ҵ�
                    _currentAttack = _monster.normalAttacks[_attackIndex];

                    // ���� �ε����� 0 ~ ���� �迭 ���� -1 ������ ���� ��ȯ��
                    _attackIndex = (_attackIndex + 1) % _monster.normalAttacks.Length;

                    // ������ �ִϸ��̼� �̸��� �޾� �ִϸ��̼� �����ϸ� Ʈ�������� 0.2�� ������
                    _monster.anim.CrossFade(_currentAttack.animName, 0.2f);

                    _monster.combat.BeginAttack(_currentAttack);
                }
                _elapsed = 0;
            }
        }

        // by.���_���¸� �������� �� ȣ��Ǵ� �Լ�_220517
        public override void StateExit()
        {

        }

        private void RotateToTarget()
        {
            Vector3 dir = (_monster.searchRadar.Target.transform.position - _monster.thisTransform.position).normalized;
            dir.y = 0;
            _monster.thisTransform.rotation = 
                Quaternion.Lerp(_monster.thisTransform.rotation, Quaternion.LookRotation(dir), _monster._deltaTime * _monster.combat.lookTargetSpeed);
        }
    }
}
