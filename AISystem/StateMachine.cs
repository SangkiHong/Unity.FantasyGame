namespace SK.AISystem
{
    public class StateMachine
    {
        // by.���_�Ҵ�� ���� ����_220516
        public StateBase currentState;
        
        // by.���_���� ���� Ŭ����_220516
        internal Monster thisMonster;

        // by.���_���� ���µ�_220516
        internal IdleState idleState;
        internal PatrolState patrolState;
        internal ChaseState chaseState;
        internal CombatState combatState;

        // by.���_���� �ӽ� ������_220517
        public StateMachine(Monster monster) 
        {
            // ������ ������ ����
            thisMonster = monster;

            // ����, ����, �߰�, ���� ���� ����
            idleState = new IdleState(thisMonster, this);
            patrolState = new PatrolState(thisMonster, this);
            chaseState = new ChaseState(thisMonster, this);
            combatState = new CombatState(thisMonster, this);
        }

        // by.���_���� ����_220516
        public void ChangeState(StateBase targetState)
        {
            // ���� ���°� �Ҵ�Ǿ� �ִٸ�, ���� ������ StateExit �Լ� ����
            if (currentState != null)
                currentState.StateExit();

            // Ÿ�� ���¸� ���� ���� ������ �Ҵ�
            currentState = targetState;
            // ����� ������ StateInit �Լ� ����
            currentState.StateInit();

            thisMonster.currentState = targetState.GetType().Name;
        }

        // by.���_���� ���°� �Ҵ�Ǿ��� ���� ���� ���θ� ��ȯ_220516
        public bool IsAssigned() { return currentState != null; }
    }
}
