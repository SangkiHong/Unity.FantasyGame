
using Sangki.Player;
using UnityEngine;

namespace Sangki.States
{
    public class JumpState : State
    {
        private readonly int m_Anim_Para_isAttack = Animator.StringToHash("isAttack");
        private readonly int m_Anim_Para_Jump = Animator.StringToHash("Jump");
        private readonly int m_Anim_Para_Land = Animator.StringToHash("Land");
        private float intervalTimer;
        private bool isJump;

        public JumpState(PlayerController player, StateMachine stateMachine) : base(player, stateMachine)
        {
        }

        public override void Enter()
        {
            base.Enter();
            Jump();
        }

        public override void Exit()
        {
            base.Exit();

            if (isJump)
            {
                isJump = false;
            }
        }

        public override void PhysicsUpdate()
        {
            base.PhysicsUpdate();
            if (isJump)
            {
                if (intervalTimer < 0.4f)
                {
                    intervalTimer += player.fixedDeltaTime;
                }
                else
                {
                    if (player.isOnGround)
                    {
                        player.anim.SetTrigger(m_Anim_Para_Land); // Landing Animation
                        stateMachine.ChangeState(player.STATE_Standing);
                    }
                }
            }
        }

        private void Jump()
        {
            if (!isJump && player.jumpIntervalTimer <= 0 && player.isOnGround && player.isOnControl)
            {
                if (!player.anim.GetBool(m_Anim_Para_isAttack))
                {
                    isJump = true;
                    // Animation
                    player.anim.SetTrigger(m_Anim_Para_Jump);

                    player.thisRigidbody.AddForce(Vector3.up * (player.JumpForce * -Physics.gravity.y));

                    // Feedback
                    player.feedback_Jump?.PlayFeedbacks();

                    // Jump Interval Timer
                    player.jumpIntervalTimer = player.JumpIntervalDelay;

                    // Initialize
                    intervalTimer = 0;
                    player.InitializeAttack();
                    if (player.isOnShield)
                    {
                        player.anim.SetLayerWeight(1, 0);
                        player.isOnShield = false;
                    }
                }
            }
        }
    }
}