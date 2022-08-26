using UnityEngine;
using SK.Player;
using SK.Manager;

namespace SK.States
{
    public class JumpState : State
    {
        private readonly PlayerController _player;
        private readonly StateMachine _stateMachine;

        private readonly Animator _aim;
        private readonly CharacterController _characterController;

        private Vector3 _direction;
        private float _elapsed;

        public bool ButtonOn { get; private set; }

        public JumpState(PlayerController player, StateMachine stateMachine)
        {
            _player = player;
            _stateMachine = stateMachine;
            _aim = player.anim;
            _characterController = _player.characterController;
        }

        public override void Enter()
        {
            base.Enter();

            ButtonOn = true;

            // 애니메이션 초기화
            _aim.SetLayerWeight(1, 0);
            _aim.ResetTrigger(Strings.AnimPara_Land);
            Jump();
        }

        public override void Exit()
        {
            base.Exit();
        }

        public override void FixedTick()
        {
            base.FixedTick();

            var jumpPoint = _elapsed / _player.JumpTime;
            float fixedTime = _player.fixedDeltaTime;


            // 점프 중인 경우 가장 높이 올라갔을 때부터 착지 여부 확인
            if ((ButtonOn && 0.3f < jumpPoint && jumpPoint < 0.5f) || jumpPoint <= 0.3f)
            {
                Vector2 dir = InputManager.Instance.Movement;
                _direction.x = dir.x;
                _direction.y = EasingFunction.EaseOutCubic(0, 1, jumpPoint * 2);
                _direction.z = dir.y;
                _direction *= _player.JumpForce * fixedTime;
            }
            else 
            {
                if (_direction.y > Physics.gravity.y) 
                    _direction.y += fixedTime * jumpPoint * Physics.gravity.y;

                if (_player.IsOnGround) 
                {
                    // 초기화
                    _elapsed = 0;

                    _aim.SetTrigger(Strings.AnimPara_Land); // Landing Animation
                    _stateMachine.ChangeState(_stateMachine.STATE_Locomotion);

                    _aim.SetTrigger(Strings.AnimPara_Land);

                    // 사운드 효과
                    //AudioManager.Instance.PlayAudio(Strings.Audio_FX_Player_Land, _transform);
                    return;
                }
            }

            if (jumpPoint < 1)
                _elapsed += fixedTime;
            else
                _elapsed = _player.JumpTime;

            _characterController.Move(_direction);
        }

        private void Jump()
        {
            if (_player.jumpIntervalTimer <= 0 && _player.IsOnGround && InputManager.Instance.IsOnControl) {
                if (!_aim.GetBool(Strings.AnimPara_OnAttack)) {
                    // 애니메이션
                    _aim.SetTrigger(Strings.AnimPara_Jump);

                    // 점프 시 피드백 실행
                    _player.feedback_Jump?.PlayFeedbacks();

                    // 점프 후 착지 판정 사이의 간격 타이머
                    _player.jumpIntervalTimer = _player.JumpIntervalDelay;

                    // 정프 방향으로 회전
                    Vector2 dir = InputManager.Instance.Movement;
                    if (dir != Vector2.zero)
                    {
                        _direction.x = dir.x;
                        _direction.y = 0;
                        _direction.z = dir.y;
                        _player.thisTransform.rotation = Quaternion.LookRotation(_direction, Vector3.up);
                    }
                }
            }
        }

        public void StopJump() => ButtonOn = false;
    }
}