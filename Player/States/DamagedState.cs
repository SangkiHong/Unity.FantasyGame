using SK.Player;
using UnityEngine;

namespace SK.States
{
    public class DamagedState : State
    {
        private readonly Player.PlayerController _player;
        private readonly StateMachine _stateMachine;

        private CharacterController _characterController;

        private float blinkTimer;
        private bool isDamaged;

        public DamagedState(Player.PlayerController player, StateMachine stateMachine)
        {
            _player = player;
            _stateMachine = stateMachine;
            _characterController = _player.characterController;
        }

        public override void Enter()
        {
            base.Enter();
            blinkTimer = 0;
            isDamaged = true;
            Manager.InputManager.Instance.SetControlState(true);

            // Knockback
            _characterController.SimpleMove(Vector3.forward * -5); 
        }

        public override void FixedTick()
        {
            base.FixedTick();

            // Damage Blink
            if (isDamaged)
            {
                blinkTimer += _player.fixedDeltaTime;

                if (blinkTimer >= _player.BlinkTime)
                {
                    isDamaged = false;
                    blinkTimer = 0;
                    _stateMachine.ChangeState(_stateMachine.STATE_Locomotion);
                }
            }
        }

        public override void Exit()
        {
            base.Exit();
            Manager.InputManager.Instance.SetControlState(true);
        }
    }
}