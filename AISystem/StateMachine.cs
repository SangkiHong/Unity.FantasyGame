namespace SK.AISystem
{
    public class StateMachine
    {
        // by.상기_할당된 현재 상태_220516
        public StateBase currentState;
        
        // by.상기_몬스터 유닛 클래스_220516
        internal Monster thisMonster;

        // by.상기_몬스터 상태들_220516
        internal IdleState idleState;
        internal PatrolState patrolState;
        internal ChaseState chaseState;
        internal CombatState combatState;

        // by.상기_상태 머신 생성자_220517
        public StateMachine(Monster monster) 
        {
            // 유닛을 변수에 저장
            thisMonster = monster;

            // 유휴, 순찰, 추격, 전투 상태 생성
            idleState = new IdleState(thisMonster, this);
            patrolState = new PatrolState(thisMonster, this);
            chaseState = new ChaseState(thisMonster, this);
            combatState = new CombatState(thisMonster, this);
        }

        // by.상기_상태 변경_220516
        public void ChangeState(StateBase targetState)
        {
            // 현재 상태가 할당되어 있다면, 현재 상태의 StateExit 함수 실행
            if (currentState != null)
                currentState.StateExit();

            // 타겟 상태를 현재 상태 변수에 할당
            currentState = targetState;
            // 변경된 상태의 StateInit 함수 실행
            currentState.StateInit();

            thisMonster.currentState = targetState.GetType().Name;
        }

        // by.상기_현재 상태가 할당되었는 지에 대한 여부를 반환_220516
        public bool IsAssigned() { return currentState != null; }
    }
}
