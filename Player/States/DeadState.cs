using Sangki.Player;
using UnityEngine;
using EPOOutline;

namespace Sangki.States
{
    public class DeadState : State
    {
        private readonly int m_Anim_Para_Dead = Animator.StringToHash("Dead");
        public DeadState(PlayerController player, StateMachine stateMachine) : base(player, stateMachine)
        {
        }

        public override void Enter()
        {
            base.Enter();
            player.anim.SetTrigger(m_Anim_Para_Dead);
            Outlinable[] outlinables = player.GetComponentsInChildren<Outlinable>();
            foreach (var outline in outlinables)
            {
                outline.enabled = false;
            }
            outlinables = null;
        }
    }
}