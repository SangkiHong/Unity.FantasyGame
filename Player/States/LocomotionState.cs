﻿using UnityEngine;
using SK.Player;
using SK.Manager;

namespace SK.States
{
    public class LocomotionState : State
    {
        private readonly Player.PlayerController _player;
        private readonly StateMachine _stateMachine;
        private readonly Transform _transform;
        private readonly CharacterController _characterController;
        private readonly Animator _anim;

        private Vector3 _movement, _lerpMovement, _dodgeDir;
        private readonly float _speed;
        public float _speedOnShield;
        public float _speedOnTargeting;
        public float _shieldRotateSpeed;

        private bool _isDodge, _isCharged, _isOnShield;

        public LocomotionState(Player.PlayerController player, StateMachine stateMachine)
        {
            _player = player;
            _stateMachine = stateMachine;
            _transform = _player.thisTransform;
            _anim = _player.anim;
            _characterController = _player.characterController;

            _speed = _player.playerData.speed;
            _speedOnShield = _player.playerData.speedOnShield;
            _speedOnTargeting = _player.playerData.speedOnTargeting;
            _shieldRotateSpeed = _player.playerData.shieldRotateSpeed;
        }

        #region STATE EVENT METHOD
        public override void Enter()
        {
            base.Enter();
        }

        public override void FixedTick()
        {
            base.FixedTick();

            // 인풋 값 업데이트
            _movement.x = InputManager.Instance.Movement.x;
            _movement.z = InputManager.Instance.Movement.y;

            if (_isDodge) Dodge(); // 닷지 중인 경우
            else Movements(); // 일반 이동

            _player.movemnt = _movement;
        }
        #endregion

        #region MOVEMENT & CONTROL
        private void Movements()
        {
            if (_movement.x != 0 || _movement.z != 0)
            {
                // 애니메이션 블랜드
                MoveMotionBlend();

                // 회전
                Rotate();

                // 움직임
                if (!_isCharged)
                {
                    if (_isOnShield)
                        _movement *= _speed * _speedOnShield * _player.fixedDeltaTime;
                    else if (_player.targetingSystem.IsOnTargeting)
                        _movement *= _speed * _speedOnTargeting * _player.fixedDeltaTime;
                    else
                        _movement *= _speed * _player.fixedDeltaTime;

                    _characterController.SimpleMove(_movement);
                }
            }
            else
            {
                if (_player.targetingSystem.IsOnTargeting)
                {
                    _anim.SetFloat(Strings.AnimPara_MoveBlend, 0);
                    _anim.SetFloat(Strings.AnimPara_Sidewalk, 0);
                }
                float move = _anim.GetFloat(Strings.AnimPara_MoveBlend);
                if (move > 0)
                {
                    move -= _player.fixedDeltaTime * 7;
                    _anim.SetFloat(Strings.AnimPara_MoveBlend, move >= 0 ? move : 0);
                }
            }
        }

        private void MoveMotionBlend()
        {
            // 타겟팅 모드
            if (_player.targetingSystem.IsOnTargeting)
            {
                _anim.SetFloat(Strings.AnimPara_Sidewalk, _movement.x);
                _anim.SetFloat(Strings.AnimPara_MoveBlend, _movement.x > 0 ? _movement.x * -1 : _movement.x);
            }
            // 타겟팅 모드 아닌경우
            else
            {
                Vector2 move = new Vector2();
                move.x = _movement.x;
                move.y = _movement.z;
                var magnitude = move.SqrMagnitude();
                if (magnitude > 0.9f) magnitude = 1;

                if (_isOnShield || _isCharged) _anim.SetFloat(Strings.AnimPara_MoveBlend, magnitude * 0.15f);
                else _anim.SetFloat(Strings.AnimPara_MoveBlend, magnitude);
            }
        }

        private void Rotate()
        {
            // 타겟팅 모드
            if (_player.targetingSystem.IsOnTargeting && !_isDodge)
            {
                _player.RotateToTarget(_player.targetingSystem.TargetObject.transform.position, 0);

                return;
            }
            // 타겟팅 모드 아닌경우
            if (!_isOnShield)
            {
                _transform.rotation = Quaternion.LookRotation(Vector3.RotateTowards(_transform.forward, _movement, 0.3f, 0));
            }
            else
            {
                _lerpMovement = Vector3.Lerp(_transform.forward, _movement, Time.deltaTime * _shieldRotateSpeed);
                _transform.rotation = Quaternion.LookRotation(_lerpMovement);
            }
        }

        public void ExecuteDodge(Vector3 direction)
        {

        }

        private void Dodge()
        {
            // 닷지 위치로 이동
            _characterController.SimpleMove(_dodgeDir * _speed * _player.fixedDeltaTime);

            // 닷지 방향으로 회전
            _transform.rotation = Quaternion.LookRotation(Vector3.RotateTowards(_transform.forward, _dodgeDir, 0.3f, 0));
        }

        public void OnDodgeEnd()
        {
            InputManager.Instance.SetControlState(true);
            _isDodge = false;
        }
        #endregion
    }
}