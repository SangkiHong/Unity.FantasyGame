using Sangki.Player;
using UnityEngine;

namespace Sangki.States
{
    public class AttackState : State
    {
        private readonly int m_Anim_Para_isAttack = Animator.StringToHash("isAttack");
        private readonly int m_Anim_Para_SwordAttack = Animator.StringToHash("SwordAttack");
        private readonly int m_Anim_Para_ChargingEnd = Animator.StringToHash("ChargingEnd");

        private float intervalTimer;
        private bool isAttack;

        public AttackState(PlayerController player, StateMachine stateMachine) : base(player, stateMachine)
        {
        }

        public override void Enter()
        {
            base.Enter();

            if (!isAttack)
            {
                isAttack = true;
                intervalTimer = 0;

                // Normal Attack
                if (!player.isCharged) NormalAttack();
                // 360 degree Attack
                else
                {
                    player.isCharged = false;
                    player.anim.SetTrigger(m_Anim_Para_ChargingEnd);
                    player.particle_RoundSlash.Play();
                }
            }
        }

        public override void PhysicsUpdate()
        {
            base.PhysicsUpdate();
            if (isAttack && !player.anim.GetBool(m_Anim_Para_isAttack))
            {
                if (intervalTimer < 0.5f)
                    intervalTimer += player.fixedDeltaTime;
                else
                {
                    stateMachine.ChangeState(player.STATE_Standing);
                }
            }
        }

        public override void Exit()
        {
            base.Exit();
            isAttack = false;
        }

        private void NormalAttack()
        {
            intervalTimer = 0;
            player.anim.SetTrigger(m_Anim_Para_SwordAttack);
            if (player.particle_Charging.isPlaying) player.particle_Charging.Stop();
        }
    }
}