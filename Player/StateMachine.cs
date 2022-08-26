using UnityEngine;

namespace SK.States
{
    public class StateMachine
    {
        public LocomotionState STATE_Locomotion;
        public JumpState STATE_Jump;
        public AttackState STATE_Attack;
        public DamagedState STATE_Damaged;
        public ShieldState STATE_Shield;
        public DeadState STATE_Dead;

        public StateMachine(Player.PlayerController player)
        {
            STATE_Locomotion = new LocomotionState(player, this);
            STATE_Jump = new JumpState(player, this);
            STATE_Attack = new AttackState(player, this);
            STATE_Damaged = new DamagedState(player, this);
            STATE_Shield = new ShieldState(player, this);
            STATE_Dead = new DeadState(player, this);

            Initialize(STATE_Locomotion);
        }

        public State CurrentState { get; private set; }

        public void Initialize(State state)
        {
            CurrentState = state;
            state.Enter();
            Player.PlayerController.Instance.CurrentState = state.ToString();
        }

        public void ChangeState(State newState)
        {
            if (CurrentState == newState) return;

            CurrentState.Exit();

            CurrentState = newState;
            newState.Enter();
            Player.PlayerController.Instance.CurrentState = newState.ToString();
        }
    }
}