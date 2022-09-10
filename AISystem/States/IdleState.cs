using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SK.AISystem
{
    /*
        �ۼ���: ȫ���
        ����: AI�� ���ڸ����� �þ� ���� ���� Ÿ���� �ִ��� üũ��
        �ۼ���: 22�� 5�� 17��
    */

    public class IdleState : StateBase
    {
        // by.���_������_220517
        public IdleState(Monster monster, StateMachine stateMachine)
        {
            _monster = monster;
            _machine = stateMachine;
        }

        // by.���_���� ������ ������ ����_220517
        private readonly Monster _monster;

        // by.���_���� �ӽ��� ������ ����_220517
        private readonly StateMachine _machine;

        private float _elapsed;

        // by.���_���� ���Խ� ȣ��Ǵ� �Լ�_220517
        public override void StateInit()
        {
            _elapsed = 0;
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

            // by.���_���� ���� ���� �ð��� 0 �̻��� ��쿡�� ���� �ð� ���Ŀ� �ٽ� ���� ���·� ��ȯ ��_220517
            if (_monster.searchRadar.idleDuration > 0 && _elapsed < _monster.searchRadar.idleDuration)
                _elapsed += _monster._fixedDeltaTime;
            else
                _machine.ChangeState(_machine.patrolState);
        }
    }
}
