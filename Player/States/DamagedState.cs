using Sangki.Player;
using UnityEngine;

namespace Sangki.States
{
    public class DamagedState : State
    {
        private float blinkTimer;
        private bool isDamaged;

        public DamagedState(PlayerController player, StateMachine stateMachine) : base(player, stateMachine)
        {
        }

        public override void Enter()
        {
            base.Enter();
            blinkTimer = 0;
            isDamaged = true;
            player.isOnControl = true;

            // Knockback
            player.thisRigidbody.AddRelativeForce(Vector3.forward * -5, ForceMode.Impulse); 
        }

        public override void PhysicsUpdate()
        {
            base.PhysicsUpdate();

            // Damage Blink
            if (isDamaged)
            {
                blinkTimer += player.fixedDeltaTime;

                if (blinkTimer >= player.BlinkTime)
                {
                    isDamaged = false;
                    blinkTimer = 0;
                    stateMachine.ChangeState(player.STATE_Standing);
                }
            }
        }

        public override void Exit()
        {
            base.Exit();
            player.isOnControl = true;
        }
    }
}