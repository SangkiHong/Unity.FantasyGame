using SK.Player;
using UnityEngine;

namespace SK.States
{
    public class AttackState : State
    {
        private readonly PlayerController _player;
        private readonly StateMachine _stateMachine;
        private readonly Animator _anim;

        private readonly float _chargingCompleteTime; // 차지 완료 시간
        private readonly float _chargingStartTime = 0.7f; // 시작되는 시간
        private readonly float _comboAttackTime = 0.7f; // 연속 공격 가능 시간
        private float _attackButtonTime, _lastAttackTime;
        private bool _onAttack, _attackCheck, _isCharging, _isCharged;

        private readonly int _attackCount;
        private int _attackIndex;

        public AttackState(PlayerController player, StateMachine stateMachine)
        {
            _player = player;
            _stateMachine = stateMachine;
            _anim = _player.anim;
            _chargingCompleteTime = _player.playerData.chargeTime;
            _attackCount = _player.attacks.Length;
        }

        public override void Enter()
        {
            base.Enter();

            // 콤보 가능 시간 보다 지체된 경우 공격 인덱스 초기화
            if (_lastAttackTime + _comboAttackTime < Time.time)
                _attackIndex = 0;

            _isCharging = _isCharged = false;
            _onAttack = _attackCheck = false;
            _player.anim.ResetTrigger(Strings.AnimPara_ChargingEnd);
            _attackButtonTime = Time.time;
        }

        public override void FixedTick()
        {
            base.FixedTick();

            // 차징 시작
            if (!_isCharging && _attackButtonTime + _chargingStartTime > Time.time)
            {
                _isCharging = true;
            }

            // 차징 공격 준비 완료
            if (_isCharging && !_isCharged && _attackButtonTime + _chargingCompleteTime > Time.time)
            {
                _isCharged = true;
            }

            // 공격 중인 경우
            if (_onAttack && !_attackCheck && _anim.GetBool(Strings.AnimPara_OnAttack))
            {
                _attackCheck = true;
                return;
            }

            if (_attackCheck && !_anim.GetBool(Strings.AnimPara_OnAttack))
                _stateMachine.ChangeState(_stateMachine.STATE_Locomotion);
        }

        public override void Exit()
        {
            base.Exit();
            _isCharging = _isCharged = false;
            _onAttack = _attackCheck = false;
            _lastAttackTime = Time.time;
        }

        public void Attack()
        {
            Debug.Log($"ATTACK");
            _onAttack = true;
            // Normal Attack
            if (!_isCharged) NormalAttack();
            // Charging Attack
            else ChargingAttack();
        }

        private void NormalAttack()
        {
            Debug.Log($"ATTACK NORMAL");
            _attackIndex %= _attackCount;
            _player.combat.BeginAttack(_player.attacks[_attackIndex++]);
            //particle_ChargeComplete.Play();
            //if (player.particle_Charging.isPlaying) player.particle_Charging.Stop();
        }

        private void ChargingAttack()
        {
            Debug.Log($"ATTACK CHARGING");
            _player.anim.SetTrigger(Strings.AnimPara_ChargingEnd);
            //player.particle_RoundSlash.Play();
        }
    }
}