using UnityEngine;

namespace SK.AISystem
{
    /*
        �ۼ���: ȫ���
        ����: Ž���� Ÿ���� ������ �Ÿ�(���� ���� �Ÿ�)��ŭ ������� ������ �߰�
        �ۼ���: 22�� 5�� 17��
    */

    public class ChaseState : StateBase
    {
        // ������
        public ChaseState(Monster monster, StateMachine stateMachine)
        {
            _monster = monster;
            _machine = stateMachine;
        }

        // ���� ������ ������ ����
        private readonly Monster _monster;

        // ���� �ӽ��� ������ ����
        private readonly StateMachine _machine;

        // Ÿ���� ��ġ�� ������ ����
        private Vector3 _targetPos;

        // Ÿ�ٰ��� �Ÿ��� ��� ���� ���� ��ġ ����
        private Vector3 _transformPos;

        // Ÿ�ٰ��� �Ÿ��� ������ ����
        private float _targetDistance;

        // ���� ���� �Ÿ��� ������ ����
        private float _combatDistance;

        // ���� ���Խ� ȣ��Ǵ� �Լ�
        public override void StateInit()
        {
            // NavMeshAgent�� ������Ʈ �簡��
            _monster.navAgent.Warp(_monster.thisTransform.position);
            _monster.navAgent.isStopped = false;

            // �ִϸ����� �Ķ���͸� ���� ������ ����
            _monster.anim.SetBool(Strings.AnimPara_OnCombat, true);

            // ���� ���� �Ÿ��� �� ���� ���
            if (_combatDistance == 0)
                _combatDistance = Mathf.Pow(_monster.combat.combatDistance, 2);
        }

        public override void FixedTick()
        {
            // Ÿ���� �Ҵ�Ǿ� �ִ� ���
            if (_monster.searchRadar.Target)
            {
                // Ÿ���� ��ġ ��, Ʈ�������� ��ġ ���� ������ ����
                _targetPos = _monster.searchRadar.Target.transform.position;
                _transformPos = _monster.thisTransform.position;

                // �Ÿ� ����ϱ� ���� ��ǥ ���
                var x = Mathf.Pow(_targetPos.x - _transformPos.x, 2);
                var y = Mathf.Pow(_targetPos.y - _transformPos.y, 2);
                var z = Mathf.Pow(_targetPos.z - _transformPos.z, 2);

                // Ÿ�ٰ��� �Ÿ� ������Ʈ�Ͽ� ������ ����
                _targetDistance = x + y + z;

                // Ÿ�ٰ��� �Ÿ��� ������ ������ �Ÿ��� �Ǹ� ���� ���·� ��ȯ
                if (_targetDistance < _combatDistance)
                {
                    _machine.ChangeState(_machine.combatState);
                    return;
                }

                // NavMeshAgent�� ��ǥ ��ġ�� �����ϴ� �Լ� ȣ��
                _monster.navAgent.SetDestination(_targetPos);
            }
            else
                _machine.ChangeState(_machine.patrolState);
        }
    }
}