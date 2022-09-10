using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SK.AISystem
{
    /*
        �ۼ���: ȫ���
        ����: Ư�� ���� ���� �ȿ��� ������ ��ġ�� �̵��ϸ� Ÿ���� ����
        �ۼ���: 22�� 5�� 17��
    */
    
    public class PatrolState : StateBase
    {
        // by.���_������_220517
        public PatrolState(Monster monster, StateMachine stateMachine)
        {
            _monster = monster;
            _machine = stateMachine;
        }

        // by.���_���� ������ ������ ����_220517
        private readonly Monster _monster;

        // by.���_���� �ӽ��� ������ ����_220517
        private readonly StateMachine _machine;

        // by.���_������ ��ǥ ������ ������ ����_220517
        private Vector3 _patrolPos;

        // by.���_���� ���Խ� ȣ��Ǵ� �Լ�_220517
        public override void StateInit()
        {
            // SearchRadar ������Ʈ�� ���� ���� ���� �� ������ ��ġ�� ������
            _patrolPos = _monster.searchRadar.GetPatrolPoint();

            // NavMeshAgent�� ��ǥ ��ġ�� �����ϴ� �Լ� ȣ��
            _monster.navAgent.SetDestination(_patrolPos);

            // �ִϸ����� �Ķ���͸� ������ ������ ����
            _monster.anim.SetBool(Strings.AnimPara_OnCombat, false);
        }

        public override void FixedTick()
        {
            // by.���_���� �� Ÿ�� Ž��_220517
            if (_monster.searchRadar.FindTarget())
            {
                // Ÿ���� �߰��� ��� �߰� ���·� ��ȯ
                _machine.ChangeState(_machine.chaseState);
                return;
            }

            // by.���_���� ��ǥ������ ������ ��� ���� ���·� ��ȯ_220518
            if (_monster.navAgent.remainingDistance < 0.1f)
                _machine.ChangeState(_machine.idleState);
        }

        // by.���_���¸� �������� �� ȣ��Ǵ� �Լ�_220517
        public override void StateExit()
        {

        }
    }
}