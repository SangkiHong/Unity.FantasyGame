using Sangki.Player;
using UnityEngine;

namespace Sangki.States
{
    public class StandingState : State
    {
        private readonly int m_Anim_Para_isMove = Animator.StringToHash("isMove");

        public StandingState(PlayerController player, StateMachine stateMachine) : base(player, stateMachine)
        {
        }

        public override void Enter()
        {
            base.Enter(); 
            player.anim.SetBool(m_Anim_Para_isMove, false);
        }
    }
}